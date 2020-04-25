namespace rec Server.Grains.Interfaces

open System.Threading.Tasks
open Orleans


type IGameBoardGrain =
    inherit IGrainWithIntegerKey
    abstract member Ping: IP -> Task<unit>
    abstract member AddRecord: Record -> Task<unit>
    abstract member GetState: unit -> Task<GameBoardState>
