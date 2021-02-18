namespace rec Tetris.Server.WebApi.Grain.Interfaces

open System
open System.Threading.Tasks
open Orleans


type IGameBoardGrain =
    inherit IGrainWithIntegerKey
    abstract member Ping: IP -> Task<unit>
    abstract member AddRecord: SeasonId * Record -> Task<unit>
    abstract member GetState: unit -> Task<GameBoardState>


type IPlayerGrain =
    inherit IGrainWithStringKey
    abstract member InitCredential: Password -> Task<unit>
    abstract member AddRecord: Password * SeasonId * Record * TetrisEventStr -> Task<Result<RecordId, AddRecordError>>
    abstract member GetRecord: RecordId -> Task<Record option>
    abstract member GetRecordEvents: RecordId -> Task<TetrisEventStr option>


[<RequireQualifiedAccess>]
type AddRecordError =
    | PasswordMissMatch


// ===========================================================================================


[<CLIMutable>]
type GameBoardState =
    { OnlineIPs: Map<IP, DateTime>
      Ranks: Record list
      Seasons: Map<SeasonId, Season> option }
    static member defaultValue =
        { OnlineIPs = Map.empty
          Ranks = []
          Seasons =
            Map.empty
            |> Map.add 1
                { StartTime = DateTime.Now
                  Width = 10
                  Height = 22
                  Ranks = [] }
            |> Some }

[<CLIMutable>]
type PlayerState =
    { Name: string
      Password: string
      TopRecord: Record option }
    static member defaultValue =
        { Name = ""
          Password = ""
          TopRecord = None }


type Password = string
type RecordId = int
type IP = string
type TetrisEventStr = string
type SeasonId = int

[<CLIMutable>]
type Record =
    { Id: int
      PlayerName: string
      Score: int
      TimeCostInMs: int
      RecordDate: DateTime }

[<CLIMutable>]
type Season =
    { Width: int
      Height: int
      StartTime: DateTime
      Ranks: Record list }
