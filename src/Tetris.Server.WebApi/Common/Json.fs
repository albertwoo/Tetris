module Tetris.Server.WebApi.Common.Json

open Thoth.Json.Net


let caseStrategy = CaseStrategy.PascalCase

let extraCoders = Extra.empty


let fromJson ty str = 
    match Decode.Auto.LowLevel.fromString(str, ty, caseStrategy, extraCoders) with
    | Ok x -> x
    | Error e -> failwith e

let toJson ty obj =
    let encoder = Encode.Auto.LowLevel.generateEncoderCached(ty, caseStrategy, extraCoders)
    encoder obj |> Encode.toString 4


let giraffeSerialier = Thoth.Json.Giraffe.ThothSerializer(caseStrategy, extraCoders)
