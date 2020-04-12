module Server.Common.Json

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Microsoft.FSharpLu.Json
open Microsoft.FSharpLu.Json.Compact.Strict


let jsonSettings =
    JsonSerializerSettings(
        ContractResolver = 
            RequireNonOptionalPropertiesContractResolver(
                NamingStrategy = 
                    CamelCaseNamingStrategy(
                        ProcessDictionaryKeys = false,
                        OverrideSpecifiedNames = true
                    )
            ),
        Converters = [| CompactUnionJsonConverter(true, true) |]
    )


let fromJson ty str = JsonConvert.DeserializeObject(str, ty, jsonSettings)
let toJson obj = JsonConvert.SerializeObject(obj, jsonSettings)
