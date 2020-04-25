module Server.Common.Json

open System.Threading.Tasks
open FSharp.Control.Tasks
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Microsoft.FSharpLu.Json
open Microsoft.FSharpLu.Json.Compact.Strict


let private jsonSettings =
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


let private serializer = JsonSerializer.Create jsonSettings

let private formatting = Compact.TupleAsArraySettings.formatting

let private Utf8EncodingWithoutBom = System.Text.UTF8Encoding(false)
let private DefaultBufferSize = 1024
    

let fromJson ty str = JsonConvert.DeserializeObject(str, ty, jsonSettings)
let toJson obj = JsonConvert.SerializeObject(obj, jsonSettings)


/// A Giraffe serializer based on FSharpLu.Json
/// See https://github.com/giraffe-fsharp/Giraffe/blob/master/DOCUMENTATION.md#serialization
type FSharpLuJsonSerializer () =
    interface Giraffe.Serialization.Json.IJsonSerializer with
        member __.SerializeToString (o:'T) =
            JsonConvert.SerializeObject(o, formatting, jsonSettings)
            
        member __.SerializeToBytes<'T> (o: 'T) : byte array =
            JsonConvert.SerializeObject(o, formatting, jsonSettings)
            |> System.Text.Encoding.UTF8.GetBytes
            
        member __.SerializeToStreamAsync<'T> (o: 'T) (stream:System.IO.Stream) : Task =
            use sw = new System.IO.StreamWriter(stream, Utf8EncodingWithoutBom, DefaultBufferSize, true)
            use jw = new JsonTextWriter(sw, Formatting = formatting)
            serializer.Serialize(jw, o)
            Task.CompletedTask
            
        member __.Deserialize<'T> (json:string) :'T =
            JsonConvert.DeserializeObject<'T>(json, jsonSettings)
            
        member __.Deserialize<'T> (bytes:byte[]) :'T =
            let json = System.Text.Encoding.UTF8.GetString bytes
            JsonConvert.DeserializeObject<'T>(json, jsonSettings)
            
        member __.DeserializeAsync (stream: System.IO.Stream) : Task<'T> =
            task {
                use streamReader = new System.IO.StreamReader(stream)
                let! json = streamReader.ReadToEndAsync()
                return JsonConvert.DeserializeObject<'T>(json, jsonSettings)
            }
