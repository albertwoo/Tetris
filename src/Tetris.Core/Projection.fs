// Update state based on event
module Tetris.Core.Projection

open Utils


let updateBlock block operation =
    match operation with
    | Operation.RotateClockWise -> rotate block
    | Operation.MoveLeft        -> move block moveL
    | Operation.MoveRight       -> move block moveR
    | Operation.MoveDown        -> move block moveD


let updateBlocks border blocks event =
    match event, blocks with
    | Event.NewBlock block, _ -> block::blocks
    | Event.NewOperation _, [] -> []
    | Event.NewOperation operation, latestBlock::restBlocks ->
        match updateBlock latestBlock operation with
        | CollidedWithBlocks restBlocks
        | CollidedWithBorderLeft border
        | CollidedWithBorderRight border
        | CollidedWithBorderBottom border -> blocks
        | updatedBlock -> updatedBlock::restBlocks


let updateRemainSquares border blocks reaminSquares event =
    match event with
    | Event.NewOperation _ -> reaminSquares
    | Event.NewBlock _ ->
        let allSquares = blocks |> List.collect getBlockSquares
        let shouldEliminate row = row |> Seq.map (fun x -> x.X) |> Seq.distinct |> Seq.length |> (=) border.Width
        let allRows = allSquares |> Seq.groupBy (fun x -> x.Y)
        let rowsAfterEliminated = allRows |> Seq.filter (fun (_, row) ->  shouldEliminate row |> not)
        let move delta row = row |> Seq.map (fun x -> { x with Y = x.Y + delta })
        rowsAfterEliminated
        |> Seq.sortByDescending (fun (y, _) -> y)
        |> Seq.fold 
            (fun (y0, rows) (y, row) ->
                let delta = y0 - y - 1
                let currentIndex = y0 - 1
                match rows, delta > 0 with
                | [], true  -> currentIndex, row |> move delta |> Seq.toList
                | [], false -> currentIndex, row |> Seq.toList
                | _, true   -> currentIndex, [ yield! row |> move delta; yield! rows ]
                | _, false  -> currentIndex, [ yield! row; yield! rows ]
            )
            (border.Height, []) // Bottom row
        |> snd


let updatePlayground playground evt =
    let allBlocks = updateBlocks playground.Border playground.Blocks evt

    let movingBlock, currentBlocks =
        match allBlocks with
        | [] -> None, []
        | [x] -> Some x, []
        | x::rest -> Some x, rest

    let currentRemainSquares = updateRemainSquares playground.Border currentBlocks playground.RemainSquares evt

    {
        IsGameOver = 
            match movingBlock with
            | Some (CollidedWithBlocks currentBlocks) -> true
            | _ -> false
        Score = 
            playground.Score 
            + (
                (currentRemainSquares |> Seq.groupBy (fun s -> s.Y) |> Seq.length) // curent rows number
                -
                (playground.RemainSquares |> Seq.groupBy (fun s -> s.Y) |> Seq.length) // old rows number
              ) 
              * 10
        Border = playground.Border
        Blocks = allBlocks
        MovingBlock = movingBlock
        RemainSquares = currentRemainSquares
    }
