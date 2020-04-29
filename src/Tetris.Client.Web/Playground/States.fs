﻿module Tetris.Client.Web.Playground.States

open System
open Elmish
open Fun.Result
open Tetris.Core


let private defaultPlayground = Utils.createDefaultPlayground (18, 30)

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
            Cmd.ofMsg (Utils.generateRamdomBlock(state.Playground.Size.Width / 2 - 2) |> Event.NewBlock |> NewEvent)
            Cmd.ofMsg Tick
          ]

    | Tick ->
        state
        , if state.Playground.IsGameOver || state.IsReplaying 
          then Cmd.none
          else 
            Cmd.ofSub (fun dispatch ->
                let timeout =
                    match 1000 / Math.Max(state.Playground.Score / 500, 1) with
                    | LessEqual 50 -> 50
                    | x -> x
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
                |> Option.map (Projection.updatePredictionBlock state.Playground)
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
                    do! Async.Sleep (
                            match 20 / Math.Max(state.Events.Length / 500, 1) with
                            | LessEqual 1 -> 1
                            | x -> x
                        )
                    return ReplayEvent (index + 1)
                }
            )
