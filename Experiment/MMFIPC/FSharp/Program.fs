open System
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

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type Parameter = 
    val mutable width: uint
    val mutable height: uint
    val mutable sem_request: sem_t
    val mutable sem_ack: sem_t
    val mutable sem_response: sem_t

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

let getPointer name size =
    let fd = shm_open(name, O_RDWR, 0)
    printfn "fd = %d" fd
    let ptr = mmap(0n, size, PROT_READ|||PROT_WRITE, MAP_SHARED, fd, 0L)
    close(fd) |> ignore
    ptr
[<EntryPoint>]
let main argv =
    printfn "%d" (Marshal.SizeOf(typeof<sem_t>))
    printfn "%d" (Marshal.SizeOf(typeof<Parameter>))
    printfn "%d" (sizeof<Parameter>)

    let pParam = getPointer "frei0r.memorymap.parameter" ( (Marshal.SizeOf(typeof<Parameter>)) |> int64 )
    let param = Marshal.PtrToStructure<Parameter>(pParam)
    printfn "%d %d" param.width param.height
    let size = ((int param.width) * (int param.height) * sizeof<int>) |> int64

    let pReq = getPointer "frei0r.memorymap.request" size
    let pRes = getPointer "frei0r.memorymap.response" size

    let semRequest = pParam + Marshal.OffsetOf(typeof<Parameter>, "sem_request")
    let semAck = pParam + Marshal.OffsetOf(typeof<Parameter>, "sem_ack")
    let semResponse = pParam + Marshal.OffsetOf(typeof<Parameter>, "sem_response")

    let message = System.Text.Encoding.ASCII.GetBytes("ABCDE\x00")
    while true do
        match sem_trywait(semRequest) with
        | 0 ->
            printfn "request received. sending ACK."
            sem_post(semAck) |> ignore

            Marshal.PtrToStringAnsi(pReq) |> printfn "request: %A"
            Marshal.Copy(message, 0, pRes, 6)
            printfn "sending response.."
            sem_post(semResponse) |> ignore
        | n ->
            ()
    0 // return an integer exit code