// Update state based on event
module Tetris.Core.Projection

open Utils


let updateBlock block operation =
    match operation with
    | Operation.RotateClockWise -> rotate block
    | Operation.MoveLeft        -> move moveL block 
    | Operation.MoveRight       -> move moveR block 
    | Operation.MoveDown        -> move moveD block 


let updateMovingBlock border remainSquares movingBlock event =
    match event, movingBlock with
    | Event.NewBlock block, _ -> Some block
    | Event.NewOperation _, None -> None
    | Event.NewOperation operation, Some block ->
        match updateBlock block operation with
        | CollidedWithSquares remainSquares
        | CollidedWithBorderLeft border
        | CollidedWithBorderRight border
        | CollidedWithBorderBottom border -> Some block
        | updatedBlock -> Some updatedBlock


let rec updatePredictionBlock border remainSquares block =
    match updateBlock block Operation.MoveDown with
    | CollidedWithSquares remainSquares
    | CollidedWithBorderBottom border -> block
    | updatedBlock -> updatePredictionBlock border remainSquares updatedBlock


let updateRemainSquares border reaminSquares movingBlock event =
    match movingBlock, event with
    | Some movingBlock, Event.NewBlock _ ->
        let allSquares = 
            match updateBlock movingBlock Operation.MoveDown with
            | CollidedWithSquares reaminSquares
            | CollidedWithBorderBottom border -> reaminSquares@(getBlockSquares movingBlock)
            | _ -> reaminSquares
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
    | _ -> 
        reaminSquares


let updatePlayground playground evt =
    let movingBlock = updateMovingBlock playground.Border playground.RemainSquares playground.MovingBlock evt
    let remainSquares = updateRemainSquares playground.Border playground.RemainSquares playground.MovingBlock evt
    let predictionBlock = movingBlock |> Option.map (updatePredictionBlock playground.Border remainSquares)
    
    let isGameOver =
        match playground.MovingBlock with
        | Some (CollidedWithSquares playground.RemainSquares) -> true
        | _ -> false
    
    let score =
        playground.Score 
        + (
            (remainSquares |> Seq.groupBy (fun s -> s.Y) |> Seq.length) // curent rows number
            -
            (playground.RemainSquares |> Seq.groupBy (fun s -> s.Y) |> Seq.length) // old rows number
          ) 
          * 10
        
    {
        IsGameOver = isGameOver
        Score = score
        Border = playground.Border
        MovingBlock = movingBlock
        PredictionBlock = predictionBlock
        RemainSquares = remainSquares
    }
