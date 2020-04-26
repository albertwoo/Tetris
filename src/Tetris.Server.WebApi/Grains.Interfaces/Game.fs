namespace rec Tetris.Server.WebApi.Grain.Interfaces

open System
open System.Threading.Tasks
open Orleans
open Tetris.Core


type IGameBoardGrain =
    inherit IGrainWithIntegerKey
    abstract member Ping: IP -> Task<unit>
    abstract member AddRecord: Record -> Task<unit>
    abstract member GetState: unit -> Task<GameBoardState>


type IPlayerGrain =
    inherit IGrainWithStringKey
    abstract member InitCredential: Password -> Task<unit>
    abstract member AddRecord: Password * Record -> Task<Result<RecordId, AddRecordError>>
    abstract member GetRecord: RecordId -> Task<Record option>


[<RequireQualifiedAccess>]
type AddRecordError =
    | PasswordMissMatch


// ===========================================================================================


[<CLIMutable>]
type GameBoardState =
  { OnlineIPs: Map<IP, DateTime>
    Ranks: Record list }
  static member defaultValue =
      { OnlineIPs = Map.empty
        Ranks = [] }

[<CLIMutable>]
type PlayerState =
    { Name: string
      Password: string
      TopRecord: Record option }
    static member defaultState =
        { Name = ""
          Password = ""
          TopRecord = None }


type Password = string
type RecordId = int
type IP = string


[<CLIMutable>]
type Record =
    { Id: int
      PlayerName: string
      GameEvents: TetrisEvent list
      Score: int
      TimeCostInMs: int
      RecordDate: DateTime }
