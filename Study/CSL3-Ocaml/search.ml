#use "metro.ml"
open Printf;;

let romaji_to_kanji_opt romaji ekimei_list =
    List.find_opt (fun e -> e.romaji = romaji) ekimei_list
let romaji_to_kanji romaji ekimei_list =
    romaji_to_kanji_opt romaji ekimei_list
    |> Option.map (fun it -> it.kanji)
    |> Option.value ~default:""

let get_ekikan_kyori ekimeiFrom ekimeiTo ekikan_list =
    match List.find_opt (fun e -> 
        (e.kiten = ekimeiFrom && e.shuten = ekimeiTo) ||
        (e.kiten = ekimeiTo && e.shuten = ekimeiFrom)) ekikan_list with
      Some({kyori=kyori}) -> kyori
    | None -> infinity

let kyori_wo_hyoji romajiFrom romajiTo = 
    let romaji_to_kanji_result romaji =
        romaji_to_kanji_opt romaji global_ekimei_list
        |> Option.map (fun it -> it.kanji)
        |> Option.to_result ~none:(sprintf "%s という駅は存在しません" romaji)
    in
    
    let get_result r = 
        match r with
          Ok(v) -> v
        | Error(v) -> v
    in

    let bind f  r = Result.bind r f in
    romaji_to_kanji_result romajiFrom
    |> bind (fun ekimeiFrom -> 
        romaji_to_kanji_result romajiTo
        |> bind (fun ekimeiTo ->
            match get_ekikan_kyori ekimeiFrom ekimeiTo global_ekikan_list with
              kyori when kyori < infinity -> Ok(sprintf "%s駅から%s駅までは%fkmです" ekimeiFrom ekimeiTo kyori)
            | infinity -> Error(sprintf "%s駅と%s駅はつながっていません" ekimeiFrom ekimeiTo)
        )
    )
    |> get_result
