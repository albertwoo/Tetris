module Tetris.Server.WebApi.Common.Json

open Thoth.Json.Net

let fromJson ty str = Decode.Auto.LowLevel.fromString(str, ty)
let toJson obj = Encode.Auto.toString(4, obj) 
