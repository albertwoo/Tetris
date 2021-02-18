namespace rec Tetris.Server.WebApi.Dtos.Game

open System
open Tetris.Core


type NewRecord =
    { PlayerName: string
      PlayerPassword: string
      GameEvents: string
      Score: int
      TimeCostInMs: int }

type GameBoard =
    { OnlineCount: int
      Seasons: SeasonBriefInfo list }

type SeasonBriefInfo =
    { Id: int
      Width: int
      Height: int
      StartTime: DateTime
      Ranks: RecordBriefInfo list }

type RecordBriefInfo =
    { Id: int
      PlayerName: string
      Score: int
      TimeCostInMs: int
      RecordDate: DateTime }

type RecordEvents = TetrisEvent list
