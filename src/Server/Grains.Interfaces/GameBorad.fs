namespace rec Server.Grains.Interfaces

open System
open System.Threading.Tasks
open Orleans


type IGameBoardGrain =
    inherit IGrainWithIntegerKey
    abstract member UpdateGame: GameEvent -> Task<unit>
    abstract member GetTopRanks: int -> Task<Rank list>
    abstract member GetOnlineCount: unit -> Task<int>


[<CLIMutable>]
type GameState =
    { OnlineCount: uint32
      Ranks: Rank list }
    static member defaultValue =
        { OnlineCount = uint32 0
          Ranks = [] }

type GameEvent =
    | IncreaseOnlineCount
    | DecreaseOnlineCount
    | AddRecord of PlayerCredential * Record
  
[<CLIMutable>] 
type Rank =
    { PlayerName: string
      Score: int
      TimeSpentInSec: int
      RecordTime: DateTime }
