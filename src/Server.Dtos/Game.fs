namespace rec Server.Dtos.Game

open System
open Tetris.Core


type NewRecord =
    { PlayerName: string
      PlayerPassword: string
      GameEvents: TetrisEvent list
      Score: int
      TimeCostInMs: int }

type GameBoard =
    { OnlineCount: int
      TopRanks: RecordBriefInfo list }

type RecordBriefInfo =
    { Id: int
      PlayerName: string
      Score: int
      TimeCostInMs: int
      RecordDate: DateTime }

type RecordEvents = TetrisEvent list
