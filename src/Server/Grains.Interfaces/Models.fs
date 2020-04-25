namespace rec Server.Grains.Interfaces

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
      RecordDate: DateTime }
