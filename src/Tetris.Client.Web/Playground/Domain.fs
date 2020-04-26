namespace Tetris.Client.Web.Playground

open Tetris.Core
open System


type State =
    { Events: TetrisEvent list
      Playground: Playground
      StartTime: DateTime option
      IsReplaying: bool
      IsViewMode: bool }


type Msg =
    | Start
    | Tick
    | NewEvent of Event
    | MoveToEnd of Operation
    | ReplayEvent of eventIndex: int
