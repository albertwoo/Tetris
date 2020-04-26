module Tetris.Server.WebApi.Common.Json

open Newtonsoft.Json
open Microsoft.FSharpLu.Json
open Microsoft.FSharpLu.Json.Compact.Strict


let private jsonSettings =
    JsonSerializerSettings(
        ContractResolver = 
            RequireNonOptionalPropertiesContractResolver(
                //NamingStrategy =
                //    CamelCaseNamingStrategy(
                //        ProcessDictionaryKeys = false,
                //        OverrideSpecifiedNames = false
                //    )
            ),
        Converters = [| CompactUnionJsonConverter(true, true) |]
    )


let fromJson ty str = JsonConvert.DeserializeObject(str, ty, jsonSettings)
let toJson obj = JsonConvert.SerializeObject(obj, jsonSettings)
