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
                        match Projection.operateBlock movingBlock operation with
                        | CollidedWithSquares playground.SquaresGrid ->
                            match operation with
                            | Operation.MoveDown -> 
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
                match Projection.operateBlock block operation with
                | CollidedWithSquares playground.SquaresGrid -> 
                    evts
                | newBlock ->
                    loop newBlock (evts@[ operation ])
            loop block []
        | _ ->
            []
