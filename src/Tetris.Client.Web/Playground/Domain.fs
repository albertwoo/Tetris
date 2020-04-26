namespace Client.Playground

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
    | ReplayEvent of eventIndex: int
