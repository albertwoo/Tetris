namespace rec Tetris.Core

open System


type Playground =
    { IsGameOver: bool
      Score: int
      Size: Size
      LeftBorder: Square list
      RightBorder: Square list
      BottomBorder: Square list
      MovingBlock: Block option
      PredictionBlock: Block option
      RemainSquares: Square list }


type Operation =
    | RotateClockWise
    | MoveLeft
    | MoveRight
    | MoveDown


type Event =
    | NewBlock of Block
    | NewOperation of Operation

type TetrisEvent =
    { TimeStamp: DateTime
      Event: Event }


type Block =
    { Squares: Square list
      CenterSquare: Square option }

type Square =
    { X: int
      Y: int }

type Size =
    { Width: int
      Height: int }

