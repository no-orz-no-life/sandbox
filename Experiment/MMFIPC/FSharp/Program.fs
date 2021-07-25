open System.Collections.Generic
open System.Runtime.InteropServices

[<Literal>]
let LibRt = "librt.so.1"
[<Literal>]
let LibC = "libc.so.6"
[<Literal>]
let LibPthread = "libpthread.so.0"

type size_t = int64
type off_t = int64

[<DllImport(LibRt)>]
extern int shm_open(string name, int oflag, int mode)

[<DllImport(LibRt)>]
extern int shm_unlink(string name)

[<DllImport(LibC)>]
extern nativeint mmap(nativeint addr, size_t length, int prot, int flags, int fd, off_t offset)

[<DllImport(LibC)>]
extern int munmap(nativeint addr, size_t length)

[<DllImport(LibC)>]
extern int close(int fd)

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type timespec =
    val mutable tv_sec: int64
    val mutable tv_nsec: int64

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type sem_t =
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst=32)>] val mutable opaque: byte array

[<DllImport(LibPthread)>]
extern int sem_destroy(nativeint sem)

[<DllImport(LibPthread)>]
extern int sem_init(nativeint sem, int pshared, uint value)

[<DllImport(LibPthread)>]
extern int sem_post(nativeint sem)

[<DllImport(LibPthread)>]
extern int sem_wait(nativeint sem)

[<DllImport(LibPthread)>]
extern int sem_trywait(nativeint sem)

[<DllImport(LibPthread)>]
extern int sem_timedwait(nativeint  sem, timespec& abs_timeout)

[<DllImport(LibC)>]
extern void perror(string s)

type mqd_t = int
type ssize_t = int64

[<DllImport(LibRt)>]
extern mqd_t mq_open(string name, int oflag)

[<DllImport(LibRt)>]
extern ssize_t mq_receive(mqd_t mqdes, nativeint msg_ptr, size_t msg_len, nativeint msg_prio)

[<DllImport(LibRt)>]
extern ssize_t mq_timedreceivessize_t(mqd_t mqdes, nativeint msg_ptr, size_t msg_len, nativeint msg_prio, timespec& abs_timeout)


[<DllImport(LibRt)>]
extern int mq_close(mqd_t mqdes)

let O_RDWR = 2
let PROT_READ = 1
let PROT_WRITE = 2
let MAP_SHARED = 1

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

    let offsetShmName = Marshal.OffsetOf(typeof<QueueItem>, "shmName")
    let offsetPointer = Marshal.OffsetOf(typeof<SHMItem>, "pointer")
    let offsetSemAck = Marshal.OffsetOf(typeof<SHMItem>, "semAck")
    let offsetSemResponse = Marshal.OffsetOf(typeof<SHMItem>, "semResponse")
    let message = System.Text.Encoding.ASCII.GetBytes("ABCDE\x00")
    while true do
        match mq_receive(queue, buf, bufSize, 0n) with
        | n when n > 0L ->
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