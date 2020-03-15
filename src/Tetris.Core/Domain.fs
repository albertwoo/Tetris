namespace rec Tetris.Core


type Playground =
    { IsGameOver: bool
      Score: int
      Border: Border
      Blocks: Block list
      MovingBlock: Block option
      RemainSquares: Square list }


type Operation =
    | RotateClockWise
    | MoveLeft
    | MoveRight
    | MoveDown


type Event =
    | NewBlock of Block
    | NewOperation of Operation


type Block =
    { Squares: Square list
      CenterSquare: Square option }

type Square =
    { X: int
      Y: int }

type Border =
    { Width: int
      Height: int }


