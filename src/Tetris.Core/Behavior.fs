// create new event based on event and state
module Tetris.Core.Behavior

open Utils


let play playground event =
    [
        yield event
        yield!
            match event with
            | Event.NewBlock _ -> []
            | Event.NewOperation operation ->
                match playground.MovingBlock with
                | None -> []
                | Some movingBlock ->
                    [
                        match Projection.updateBlock movingBlock operation with
                        | CollidedWithSquares playground.RemainSquares
                        | CollidedWithSquares playground.BottomBorder ->
                            match operation with
                            | Operation.MoveDown
                            | Operation.RotateClockWise -> 
                                generateRamdomBlock(playground.Size.Width / 2 - 2) 
                                |> Event.NewBlock
                            | _ -> 
                                ()
                        | _ ->
                            ()
                    ]
    ]


let moveToEnd playground operation =
    match playground.MovingBlock with
    | None -> []
    | Some block ->
        match operation with
        | Operation.MoveDown | Operation.MoveLeft | Operation.MoveRight ->
            let rec loop block evts =
                match Projection.updateBlock block operation with
                | CollidedWithSquares playground.RemainSquares
                | CollidedWithSquares playground.LeftBorder
                | CollidedWithSquares playground.RightBorder
                | CollidedWithSquares playground.BottomBorder -> 
                    evts
                | newBlock ->
                    loop newBlock (evts@[ operation ])
            loop block []
        | _ ->
            []
