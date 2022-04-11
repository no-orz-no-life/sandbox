open System
open System.Diagnostics
open System.IO
open System.Text
open System.Globalization

let rec eachLines (sr:StreamReader) = seq {
    match sr.ReadLine() with
    | null -> ()
    | line -> yield line; yield! eachLines sr
}

let rec visit path = seq {
    if Directory.Exists(path) then
        for file in Directory.GetFiles(path) do
            yield file
        for dir in Directory.GetDirectories(path) do
            yield! visit dir
    else
        yield Path.GetFullPath(path)
}

let also f v = f v ; v

type Info = 
| Text of string
| Timestamp of DateTime

type Filter = 
| Regex of RegularExpressions.Regex
| Mapping of (string->(string*string) option)

let parse filter mapper sr = 
    let eachKV (sr:StreamReader) =  seq {
        for line in eachLines sr do
            match filter with 
            | Regex re ->                
                match re.Match(line) with
                | null -> ()
                | m -> yield (m.Groups.[1].Value, m.Groups.[2].Value)
            | Mapping f ->
                let it = f line
                if Option.isSome(it) then
                    yield (Option.get(it))
    }
    eachKV sr
    |> Seq.map mapper
    |> Map.ofSeq

let parseExiftags sr = 
    let re = RegularExpressions.Regex("^([^:]+): (.+)$")
    let mapKV (k, v) = 
        let newV = 
            match k with
            | "Image Created" ->
                match DateTime.TryParseExact(v, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None) with
                | true, d -> Timestamp d
                | _ -> Text v
            | k -> Text v
        (k, newV)
    parse (Regex re) mapKV sr

let parseFfmpeg sr = 
    let re = RegularExpressions.Regex("^([^:]+): (.+)$")
    let mapKV (k, v:string) = 
        let newV = 
            match k with
            | "    creation_time   " ->
                DateTime.Parse(v, null, DateTimeStyles.RoundtripKind)
                |> Timestamp
            | k -> Text v
        (k, newV)
    parse (Regex re) mapKV sr

let parseDcraw sr = 
    let re = RegularExpressions.Regex("^([^:]+): (.+)$")
    let mapKV (k, v:string) = 
        let newV = 
            match k with
            | "Timestamp" ->
                let normalized = 
                    if v.[8] = ' ' then
                        sprintf "%s0%s" (v.Substring(0, 8)) (v.Substring(9))
                    else
                        v
                match DateTime.TryParseExact(normalized, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture,  DateTimeStyles.None) with
                | true, d -> Timestamp d
                | _ -> Text v
            | k -> Text v
        (k, newV)
    parse (Regex re) mapKV sr

let testExiftags () = 
    use fs = new FileStream("test/out.exiftags.txt", FileMode.Open, FileAccess.Read)
    use sr = new StreamReader(fs)
    parseExiftags sr


let testFfprobe () = 
    use fs = new FileStream("test/out.ffprobe.txt", FileMode.Open, FileAccess.Read)
    use sr = new StreamReader(fs)
    parseFfmpeg sr

let testDcraw () = 
    use fs = new FileStream("test/out.dcraw-v-i.txt", FileMode.Open, FileAccess.Read)
    use sr = new StreamReader(fs)
    parseDcraw sr


let processExiftags path = 
    use proc = new Process()
    let startInfo = ProcessStartInfo("exiftags", path)
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true
    proc.StartInfo <- startInfo
    proc.Start() |> ignore

    parseExiftags proc.StandardOutput
    |> also (fun v ->
        proc.WaitForExit()
    )
    |> Map.find("Image Created")

let processDcraw path = 
    use proc = new Process()
    let startInfo = ProcessStartInfo("dcraw", (sprintf "-i -v %s" path))
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true
    proc.StartInfo <- startInfo
    proc.Start() |> ignore

    parseDcraw proc.StandardOutput
    |> also (fun v ->
        proc.WaitForExit()
    )
    |> Map.find("Timestamp")

let processFfmpeg path = 
    use proc = new Process()
    let startInfo = ProcessStartInfo("ffprobe", path)
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardError <- true
    proc.StartInfo <- startInfo
    proc.Start() |> ignore

    parseFfmpeg proc.StandardError
    |> also (fun v ->
        proc.WaitForExit()
    )
    |> Map.find("    creation_time   ")

let processPath fileOrDirectory = 
    let getTimestamp info =
        match info with
        | Timestamp ts -> ts
        | info -> sprintf "%A ha Timestamp janai." info |> failwith
    visit fileOrDirectory
    |> Seq.map (fun path ->
        let extension = Path.GetExtension(path)
        try 
            match (extension.ToLower()) with
            | ".jpg" -> processExiftags path |> Some
            | ".mp4" -> processFfmpeg path |> Some
            | ".lrv" -> processFfmpeg path |> Some
            | ".avi" -> processFfmpeg path |> Some
            | ".orf" -> processDcraw path |> Some
            | ".arw" -> processDcraw path |> Some
            | ".mov" -> processFfmpeg path |> Some
            | ".3gp" -> processFfmpeg path |> Some
            | ".txt" -> None
            | ext -> (sprintf "%A shiranai" ext) |> failwith
            |> Option.map (fun ts -> (path, getTimestamp ts))
        with
        | ex -> printfn "# ERROR: %A: %A" path (ex.Message.Replace("\n", "/")); None
    )
    |> Seq.filter Option.isSome
    |> Seq.map Option.get
    
[<EntryPoint>]
let main args =
    let usage () = 
        printfn "usage: %s [src...] [dst]" "dotnet run"
        Environment.Exit(1)

    let parseArg args = seq {
        if Array.length args < 2 then
            usage ()
        let len = Array.length args
        let srcs, dsts = Array.splitAt (len - 1) args
        let dst = Array.exactlyOne dsts
        for src in srcs do
            yield (dst, src)
    }

    parseArg args
    |> Seq.fold (fun m (dst, src) ->
        processPath src
        |> Seq.fold (fun m (path, ts) ->
            let baseName = Path.GetFileName(path)
            let dirName = Path.GetDirectoryName(path)

            let dstBase = Path.Combine(dst, sprintf "%04d/%02d/%04d-%02d-%02d" (ts.Year) (ts.Month) (ts.Year) (ts.Month) (ts.Day))

            let newM = 
                if Set.contains dstBase m then
                    m
                else
                    printfn "mkdir -p %s" dstBase
                    Set.add dstBase m
            let dstName = Path.Combine(dstBase, sprintf "%04d%02d%02d-%02d%02d%02d-%s" (ts.Year) (ts.Month) (ts.Day) (ts.Hour) (ts.Minute) (ts.Second) baseName)

            printfn "mv -n %s %s" path dstName
            newM
        ) m
    ) Set.empty<string>
    |> ignore
    0