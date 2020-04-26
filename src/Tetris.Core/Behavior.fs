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
                        | CollidedWithBorderBottom playground.Border ->
                            match operation with
                            | Operation.MoveDown
                            | Operation.RotateClockWise -> 
                                generateRamdomBlock(playground.Border.Width / 2 - 2) 
                                |> Event.NewBlock
                            | _ -> 
                                ()
                        | _ ->
                            ()
                    ]
    ]


let rec moveToEnd playground operation evts =
    match playground.MovingBlock with
    | None -> playground, evts
    | Some block ->
        match operation with
        | Operation.MoveDown | Operation.MoveLeft | Operation.MoveRight ->
            match Projection.updateBlock block operation with
            | CollidedWithSquares playground.RemainSquares
            | CollidedWithBorderLeft playground.Border
            | CollidedWithBorderRight playground.Border
            | CollidedWithBorderBottom playground.Border -> 
                playground, evts
            | _ ->
                let newEvents = play playground (Event.NewOperation operation)
                let newPlayground = newEvents |> List.fold Projection.updatePlayground playground
                moveToEnd newPlayground operation (evts@newEvents)
        | _ ->
            playground, evts
