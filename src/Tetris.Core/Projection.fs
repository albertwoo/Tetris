// Update state based on event
module Tetris.Core.Projection

open System
open Utils


let operateBlock block operation =
    match operation with
    | Operation.RotateClockWise -> rotate block
    | Operation.MoveLeft        -> move moveL block 
    | Operation.MoveRight       -> move moveR block 
    | Operation.MoveDown        -> move moveD block 


let updateMovingBlock playground event =
    match event, playground.MovingBlock with
    | Event.NewBlock block, _ -> Some block
    | Event.NewOperation _, None -> None
    | Event.NewOperation operation, Some block ->
        match operateBlock block operation with
        | CollidedWithSquares playground.SquaresGrid -> Some block
        | updatedBlock -> Some updatedBlock


let rec updatePredictionBlock playground block =
    match operateBlock block Operation.MoveDown with
    | CollidedWithSquares playground.SquaresGrid -> block
    | updatedBlock -> updatePredictionBlock playground updatedBlock


let private updateGrid playground event =
    let updateSquaresGrid block (grid: Grid) =
        getBlockSquares block
        |> List.iter (fun square -> grid.set(square, Used))
        grid

    match playground.MovingBlock, event with
    | Some movingBlock, Event.NewBlock _ ->
        let grid =
            match operateBlock movingBlock Operation.MoveDown with
            | CollidedWithSquares playground.SquaresGrid -> updateSquaresGrid movingBlock playground.SquaresGrid
            | _ -> playground.SquaresGrid
            |> function Grid x -> x

        let targetYs = 
            getBlockSquares movingBlock 
            |> Seq.groupBy (fun s -> s.Y) 
            |> Seq.map fst 
            |> Seq.sort

        let mutable count = 0
        for y in targetYs do
            let row = grid.[y]
            let shouldEliminate = row |> Seq.filter (function Used -> true | _ -> false) |> Seq.length |> (=) playground.Size.Width
            if shouldEliminate then
                count <- count + 1
                for y' in [y..(-1)..1] do
                    grid.[y'] <- grid.[y'-1]
                grid.[0] <- [| for x in [1..playground.Size.Width] -> NotUsed |]
        count
    | _ ->
        0


let updatePlayground playground evt =
    let movingBlock = updateMovingBlock playground evt
    let eliminatedRowsNum = updateGrid playground evt
    let predictionBlock = movingBlock |> Option.map (updatePredictionBlock playground)
    
    let isGameOver =
        match movingBlock with
        | Some (CollidedWithSquares playground.SquaresGrid) -> true
        | _ -> false
    
    let score =
        let scale = Math.Pow(2., float (eliminatedRowsNum / 2)) |> int
        playground.Score + eliminatedRowsNum * scale * 10
        
    { playground with
        IsGameOver = isGameOver
        Score = score
        MovingBlock = movingBlock
        PredictionBlock = predictionBlock }
