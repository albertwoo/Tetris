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
                match playground.Blocks with
                | [] -> []
                | movingBlock::restBlocks ->
                    [
                        match Projection.updateBlock movingBlock operation with
                        | CollidedWithBlocks restBlocks
                        | CollidedWithBorderBottom playground.Border ->
                            match operation with
                            | Operation.MoveDown
                            | Operation.RotateClockWise -> generateRamdomBlock(playground.Border.Width / 2 - 2) |> Event.NewBlock
                            | _ -> ()
                        | _ ->
                            ()
                    ]
    ]
