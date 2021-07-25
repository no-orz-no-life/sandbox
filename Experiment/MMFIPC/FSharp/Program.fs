open System.Collections.Generic
open System.Runtime.InteropServices

open Posix.Posix

let queueName = "/frei0r.memorymap.mq"

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type SHMItem = 
    val mutable width: uint
    val mutable height: uint
    val mutable semAck: sem_t
    val mutable semResponse: sem_t
    val mutable pointer: nativeint

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type QueueItem =
    val mutable size: size_t
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst=32)>]
    val mutable shmName: byte array

[<EntryPoint>]
let main argv =

    let queue = mq_open(queueName, O_RDWR)
    printfn "queue: %A" queue

    let bufSize = 8192L
    let buf = Marshal.AllocHGlobal(int bufSize)

    let shmems = Dictionary<string, nativeint>()
    let getPointer name size = 
        match shmems.TryGetValue(name) with 
        | (true, v) ->
            v
        | (false, _) ->
            let fd = shm_open(name, O_RDWR, 0)
            printfn "fd = %d" fd
            let ptr = mmap(0n, size, PROT_READ|||PROT_WRITE, MAP_SHARED, fd, 0L)
            close(fd) |> ignore
            shmems.Add(name, ptr)
            ptr          

    let inline timespecToMsec (ts:timespec) = 
        ts.tv_sec * 1000L + ts.tv_nsec / (1000L * 1000L)
    let inline timespecDiffMsec (tFrom:timespec) (tTo:timespec) =
        (timespecToMsec tTo) - (timespecToMsec tFrom)
    let inline msecToTimespec msec = 
        let mutable ts:timespec = Unchecked.defaultof<timespec>
        ts.tv_sec <- msec / 1000L
        ts.tv_nsec <- (msec % 1000L) * (1000L * 1000L) 
        ts
    let getTimeout lastReceived = 
        let mutable now:timespec = Unchecked.defaultof<timespec>
        timespec_get(&now, TIME_UTC) |> ignore
        if timespecDiffMsec lastReceived now > 1000L then
            (timespecToMsec now) + 100L
        else
            0L
        |> msecToTimespec

    let offsetShmName = Marshal.OffsetOf(typeof<QueueItem>, "shmName")
    let offsetPointer = Marshal.OffsetOf(typeof<SHMItem>, "pointer")
    let offsetSemAck = Marshal.OffsetOf(typeof<SHMItem>, "semAck")
    let offsetSemResponse = Marshal.OffsetOf(typeof<SHMItem>, "semResponse")
    let message = System.Text.Encoding.ASCII.GetBytes("ABCDE\x00")
    let mutable lastReceived:timespec = Unchecked.defaultof<timespec>
    while true do
        let mutable timeout = getTimeout lastReceived
        match mq_timedreceive(queue, buf, bufSize, 0n, &timeout) with
        | n when n > 0L ->
            let mutable now:timespec = Unchecked.defaultof<timespec>
            timespec_get(&now, TIME_UTC) |> ignore
            lastReceived <- now

            printfn "request received. sending ACK."
            let queueItem = Marshal.PtrToStructure<QueueItem>(buf)
            let shmName = Marshal.PtrToStringAnsi(buf + offsetShmName)

            let ptr = getPointer shmName (queueItem.size)
            let parameter = Marshal.PtrToStructure<SHMItem>(ptr)
            printfn "%A %A" parameter.width parameter.height
            let size = (nativeint parameter.width) * (nativeint parameter.height) * (nativeint sizeof<int32>)
            let pReq = ptr + offsetPointer
            let pRes = pReq + size

            let semAck = ptr + offsetSemAck
            let semResponse = ptr + offsetSemResponse

            sem_post(semAck) |> ignore

            Marshal.PtrToStringAnsi(pReq) |> printfn "request: %A"
            Marshal.Copy(message, 0, pRes, 6)
            printfn "sending response.."
            sem_post(semResponse) |> ignore            
        | 0L ->
            ()
        | n ->
            ()
    0 // return an integer exit code