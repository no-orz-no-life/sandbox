open System
open System.IO
open System.Text
open System.Globalization

let rec eachLines (sr:StreamReader) = seq {
    match sr.ReadLine() with
    | null -> ()
    | line -> yield line; yield! eachLines sr
}

type Info = 
| Text of string
| Timestamp of DateTime

type Filter = 
| Regex of RegularExpressions.Regex
| Mapping of (string->(string->string) option)

let test filter = 
    use fs = new FileStream("test/out.dcraw-v-i.txt", FileMode.Open, FileAccess.Read)
    use sr = new StreamReader(fs)

    let eachKV (sr:StreamReader) =  seq {
        for line in eachLines sr do
            match filter with 
            | Regex re ->                
                match re.Match(line) with
                | null -> ()
                | m -> yield (m.Groups.[1].Value, m.Groups.[2].Value)
            | Mapping f ->
                f line 
                |> Option.iter (fun k v ->
                    yield (k, v)
                )
    }

    let mapKV (k, v) = 
        let newV = 
            match k with
            | "Timestamp" ->
                DateTime.ParseExact(v, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture)
                |> Timestamp
            | k -> Text k
        (k, newV)
    eachKV sr
    |> Seq.map mapKV
    |> Map.ofSeq
    |> Map.iter (fun k v ->
        printfn "%A => %A" k v
    )
let testDcraw () = 
    use fs = new FileStream("test/out.dcraw-v-i.txt", FileMode.Open, FileAccess.Read)
    use sr = new StreamReader(fs)

    let eachKV (sr:StreamReader) (re:RegularExpressions.Regex) =  seq {
        for line in eachLines sr do
            match re.Match(line) with
            | null -> ()
            | m -> yield (m.Groups.[1].Value, m.Groups.[2].Value)
    }

    let mapKV (k, v) = 
        let newV = 
            match k with
            | "Timestamp" ->
                DateTime.ParseExact(v, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture)
                |> Timestamp
            | k -> Text k
        (k, newV)

    let re = RegularExpressions.Regex("^([^:]+): (.+)$")

    eachKV sr re
    |> Seq.map mapKV
    |> Map.ofSeq
    |> Map.iter (fun k v ->
        printfn "%A => %A" k v
    )

let testExiftags () = 
    use fs = new FileStream("test/out.exiftags.txt", FileMode.Open, FileAccess.Read)
    use sr = new StreamReader(fs)
    ()

testDcraw ()
