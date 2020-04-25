namespace rec Tetris.Grains.Interfaces

open System
open Tetris.Core


[<CLIMutable>]
type GameBoardState =
  { OnlineIPs: Map<IP, DateTime>
    Ranks: Record list }
  static member defaultValue =
      { OnlineIPs = Map.empty
        Ranks = [] }

[<CLIMutable>]
type PlayerState =
    { NickName: string
      Password: string
      TopRecord: Record option }
    static member defaultState =
        { NickName = ""
          Password = ""
          TopRecord = None }


type Password = string
type RecordId = int
type IP = string


[<CLIMutable>]
type Record =
    { Id: int
      PlayerId: string
      GameEvents: TetrisEvent list
      Score: int
      RecordDate: DateTime }

[<CLIMutable>]
type TetrisEvent =
    { TimeStamp: DateTime
      Event: Event }
