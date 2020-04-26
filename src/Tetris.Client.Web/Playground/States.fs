﻿module Tetris.Client.Web.Playground.States

open System
open Elmish
open Fun.Result
open Tetris.Core


let private defaultPlayground =
    {
        IsGameOver = false
        Score = 0
        Border = { Width = 18; Height = 30 }
        MovingBlock = None
        PredictionBlock = None
        RemainSquares = []
    }

let private convertToTetrisEvent newEvents =
    newEvents 
    |> List.map (fun e -> { TimeStamp = DateTime.Now; Event = e })


let init() =
    { Events = []
      Playground = defaultPlayground 
      StartTime = None
      IsReplaying = false
      IsViewMode = true }
    , Cmd.none


let update msg state =
    match msg with
    | Start ->
        { state with
            Events = []
            Playground = defaultPlayground
            StartTime = Some DateTime.Now }
        , Cmd.batch [
            Cmd.ofMsg (Utils.generateRamdomBlock(state.Playground.Border.Width / 2 - 2) |> Event.NewBlock |> NewEvent)
            Cmd.ofMsg Tick
          ]

    | Tick ->
        state
        , if state.Playground.IsGameOver || state.IsReplaying 
          then Cmd.none
          else 
            Cmd.ofSub (fun dispatch ->
                let timeout =
                    match state.Playground.Score with
                    | BetweenEqual 100 200 -> 800
                    | BetweenEqual 200 500 -> 500
                    | BetweenEqual 500 Int32.MaxValue -> 300
                    | _ -> 1000
                Browser.Dom.window.setTimeout(
                    fun _ ->
                        Operation.MoveDown |> Event.NewOperation |> NewEvent |> dispatch
                        Tick |> dispatch
                    , timeout
                )
                |> ignore
            )

    | NewEvent event ->
        if state.IsReplaying || state.Playground.IsGameOver 
        then state, Cmd.none
        else
            let newEvents = Behavior.play state.Playground event
            { state with
                Events = state.Events@(newEvents |> convertToTetrisEvent)
                Playground = newEvents |> List.fold Projection.updatePlayground state.Playground }
            , Cmd.none

    | MoveToEnd operation ->
        if state.IsReplaying || state.Playground.IsGameOver 
        then state, Cmd.none
        else
            let newOperations = Behavior.moveToEnd state.Playground operation
            let movingBlock =
                state.Playground.MovingBlock 
                |> Option.map (fun block -> List.fold Projection.updateBlock block newOperations)
            let predictionBlock =
                movingBlock
                |> Option.map (Projection.updatePredictionBlock state.Playground.Border state.Playground.RemainSquares)
            { state with
                Events = state.Events@(newOperations |> List.map Event.NewOperation |> convertToTetrisEvent)
                Playground = 
                    { state.Playground with 
                        MovingBlock = movingBlock
                        PredictionBlock = predictionBlock } }
            , Cmd.none

    | ReplayEvent index ->
        let isFinished = index + 1 >= state.Events.Length
        { state with
            IsReplaying = not isFinished
            Playground =
                if isFinished then state.Playground
                else
                    let playground =
                        if index = 0 then defaultPlayground
                        else state.Playground
                    state.Events 
                    |> List.map (fun x -> x.Event) 
                    |> Seq.item index 
                    |> Projection.updatePlayground playground }
        , if isFinished then Cmd.none
          else
            Cmd.OfAsync.result(
                async {
                    do! Async.Sleep 50
                    return ReplayEvent (index + 1)
                }
            )
