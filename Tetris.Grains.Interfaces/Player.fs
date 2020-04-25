namespace rec Tetris.Grains.Interfaces

open System.Threading.Tasks
open Orleans


type IPlayerGrain =
    inherit IGrainWithStringKey
    abstract member InitCredential: Password -> Task<unit>
    abstract member AddRecord: Password * Record -> Task<Result<unit, AddRecordError>>
    abstract member GetRecord: RecordId -> Task<Record option>


[<RequireQualifiedAccess>]
type AddRecordError =
    | PasswordMissMatch
