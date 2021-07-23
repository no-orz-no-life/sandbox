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
extern int sem_destroy(sem_t& sem)

[<DllImport(LibPthread)>]
extern int sem_init(sem_t&  sem, int pshared, uint value)

[<DllImport(LibPthread)>]
extern int sem_post(sem_t&  sem)

[<DllImport(LibPthread)>]
extern int sem_wait(sem_t&  sem)

[<DllImport(LibPthread)>]
extern int sem_trywait(sem_t&  sem)

[<DllImport(LibPthread)>]
extern int sem_timedwait(sem_t&  sem, timespec& abs_timeout)

[<DllImport(LibC)>]
extern void perror(string s)

let O_RDWR = 2
let PROT_READ = 1
let PROT_WRITE = 2
let MAP_SHARED = 1

[<EntryPoint>]
let main argv =
    printfn "%d" (Marshal.SizeOf(typeof<sem_t>))
    printfn "%d" (Marshal.SizeOf(typeof<Parameter>))
    printfn "%d" (sizeof<Parameter>)

    let fd0 = shm_open("frei0r.memorymap.parameter", O_RDWR, 0)
    printfn "fd0 = %d" fd0
    let ptr = mmap(0n, Marshal.SizeOf(typeof<Parameter>) |> int64, PROT_READ ||| PROT_WRITE, MAP_SHARED, fd0, 0L)
    printfn "ptr = %A, %A" ptr (ptr = -1n)
    close(fd0) |> ignore
    let param = Marshal.PtrToStructure<Parameter>(ptr)
    printfn "%d %d" param.width param.height

    0 // return an integer exit code