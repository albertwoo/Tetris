namespace rec Server.Dtos.Game

open System
open Tetris.Core


type NewRecord =
    { PlayerName: string
      PlayerPassword: string
      GameEvents: TetrisEvent list
      Score: int
      RecordDate: DateTime }

type GameBoard =
    { OnlineCount: int
      TopRanks: RecordBriefInfo list }

type RecordBriefInfo =
    { Id: int
      PlayerName: string
      Score: int
      RecordDate: DateTime }
