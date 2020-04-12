namespace rec Server.Grains.Interfaces

open System
open System.Threading.Tasks
open Orleans
open Tetris.Core


type IPlayerGrain =
    inherit IGrainWithStringKey
    abstract member AddCredential: PlayerCredential -> Task<unit>
    abstract member AddRecord: Record -> Task<unit>


[<CLIMutable>]
type PlayerState =
    { NickName: string
      Password: string
      Records: Record list }

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
