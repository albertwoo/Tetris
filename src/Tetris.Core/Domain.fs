namespace rec Tetris.Core

open System


type Playground =
    { IsGameOver: bool
      Score: int
      Size: Size
      BottomBorder: Square list
      MovingBlock: Block option
      PredictionBlock: Block option
      SquaresGrid: Grid }


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

type Point =
    | Used // In the future we can add payload for color etc.
    | NotUsed

type Grid = 
    private Grid of Point[][]
    with
    static member create width height = 
        Grid [| 
            for _ in 1..height ->
                [| for _ in 1..width -> NotUsed |] 
        |]
    static member value = function
        | Grid x -> x
    static member item(x, y) = function
        | Grid g ->
            try g.[y].[x] |> Some
            with _ -> None
    static member item (s: Square) = 
        Grid.item(s.X, s.Y)
    static member set square value = function
        | Grid g ->
            try g.[square.Y].[square.X] <- value
            with _ -> ()
