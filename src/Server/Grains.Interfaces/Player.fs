namespace rec Server.Grains.Interfaces

open System
open System.Threading.Tasks
open Orleans
open Tetris.Core


type IPlayerGrain =
    inherit IGrainWithStringKey
    abstract member InitCredential: Password -> Task<unit>
    abstract member AddRecord: Password * Record -> Task<Result<unit, AddRecordError>>
    abstract member GetRecord: RecordId -> Task<Record option>


type Password = string
type RecordId = int

[<CLIMutable>]
type PlayerState =
    { NickName: string
      Password: string
      TopRecord: Record option }
    static member defaultState =
        { NickName = ""
          Password = ""
          TopRecord = None }

[<RequireQualifiedAccess>]
type PlayerEvent =
    | InitCredential of Password
    | NewRecord of Record


[<CLIMutable>]
type PlayerCredential =
    { NickName: string
      Password: string }

[<CLIMutable>]
type Record =
    { GameEvents: TetrisEvent list
      Score: int
      RecordDate: DateTime }

[<CLIMutable>]
type TetrisEvent =
    { TimeStamp: DateTime
      Event: Event }

[<RequireQualifiedAccess>]
type AddRecordError =
    | PasswordMissMatch
