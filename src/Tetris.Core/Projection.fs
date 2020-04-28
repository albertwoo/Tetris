// Update state based on event
module Tetris.Core.Projection

open System
open Utils


let updateBlock block operation =
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
        match updateBlock block operation with
        | CollidedWithSquares playground.RemainSquares 
        | CollidedWithSquares playground.LeftBorder 
        | CollidedWithSquares playground.RightBorder 
        | CollidedWithSquares playground.BottomBorder -> Some block
        | updatedBlock -> Some updatedBlock


let rec updatePredictionBlock playground block =
    match updateBlock block Operation.MoveDown with
    | CollidedWithSquares playground.RemainSquares 
    | CollidedWithSquares playground.BottomBorder -> block
    | updatedBlock -> updatePredictionBlock playground updatedBlock


let updateRemainSquares playground event =
    match playground.MovingBlock, event with
    | Some movingBlock, Event.NewBlock _ ->
        let allSquares =
            match updateBlock movingBlock Operation.MoveDown with
            | CollidedWithSquares playground.RemainSquares
            | CollidedWithSquares playground.BottomBorder -> playground.RemainSquares@(getBlockSquares movingBlock)
            | _ -> playground.RemainSquares
        let shouldEliminate row = row |> Seq.map (fun x -> x.X) |> Seq.distinct |> Seq.length |> (=) playground.Size.Width
        let allRows = allSquares |> Seq.groupBy (fun x -> x.Y)
        let rowsAfterEliminated = allRows |> Seq.filter (fun (_, row) -> shouldEliminate row |> not)
        let move delta row = row |> Seq.map (fun x -> { x with Y = x.Y + delta })
        {| 
            RemainSquares =
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
                    (playground.Size.Height, []) // Bottom row
                |> snd
            EliminatedRowsNum = Seq.length allRows - Seq.length rowsAfterEliminated
        |}
    | _ ->
        {| 
            RemainSquares = playground.RemainSquares
            EliminatedRowsNum = 0
        |}


let updatePlayground playground evt =
    let movingBlock = updateMovingBlock playground evt
    let remainSquares = updateRemainSquares playground evt
    let predictionBlock = movingBlock |> Option.map (updatePredictionBlock { playground with RemainSquares = remainSquares.RemainSquares })
    
    let isGameOver =
        match playground.MovingBlock with
        | Some (CollidedWithSquares playground.RemainSquares) -> true
        | _ -> false
    
    let score =
        let scale = Math.Pow(2., float (remainSquares.EliminatedRowsNum / 2)) |> int
        playground.Score + remainSquares.EliminatedRowsNum * scale * 10
        
    { playground with
        IsGameOver = isGameOver
        Score = score
        MovingBlock = movingBlock
        PredictionBlock = predictionBlock
        RemainSquares = remainSquares.RemainSquares }
