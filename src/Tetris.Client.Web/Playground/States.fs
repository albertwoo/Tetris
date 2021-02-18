module Tetris.Client.Web.Playground.States

open System
open Elmish
open Fun.Result
open Tetris.Core


let private convertToTetrisEvent newEvents =
    newEvents 
    |> List.map (fun e -> { TimeStamp = DateTime.Now; Event = e })


let init size =
    { Events = []
      Playground = size |> Option.defaultValue (18, 30) |> Utils.createDefaultPlayground
      StartTime = None
      IsReplaying = false
      IsViewMode = true }
    , Cmd.none


let update msg state =
    match msg with
    | Start ->
        { state with
            Events = []
            Playground = Utils.createDefaultPlayground (state.Playground.Size.Width, state.Playground.Size.Height)
            StartTime = Some DateTime.Now }
        , Cmd.batch [
            Cmd.ofMsg (Utils.generateRamdomBlock(state.Playground.Size.Width / 2 - 2) |> Event.NewBlock |> NewEvent)
            Cmd.ofMsg Tick
          ]

    | Tick ->
        let cmd = 
            if state.Playground.IsGameOver || state.IsReplaying then 
                Cmd.none
            else 
                Cmd.ofSub (fun dispatch ->
                    let timeout =
                        match 1000 / Math.Max(state.Playground.Score / 600, 1) with
                        | LessEqual 100 -> 100
                        | x -> x
                    Browser.Dom.window.setTimeout(
                        fun _ ->
                            Operation.MoveDown |> Event.NewOperation |> NewEvent |> dispatch
                            Tick |> dispatch
                        , timeout
                    )
                    |> ignore
                )
        state, cmd 

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
            let newEvents = Behavior.moveToEnd state.Playground operation |> List.map Event.NewOperation
            let newPlayground = newEvents |> List.fold Projection.updatePlayground state.Playground
            { state with
                Events = state.Events@(convertToTetrisEvent newEvents)
                Playground = newPlayground }
            , Cmd.none

    | ReplayEvent index ->
        let isFinished = index + 1 >= state.Events.Length
        let length = 20
        let newPlayground =
            if isFinished then state.Playground
            else
                let playground =
                    if index = 0 then Utils.createDefaultPlayground (state.Playground.Size.Width, state.Playground.Size.Height)
                    else state.Playground
                state.Events 
                |> List.map (fun x -> x.Event) 
                |> Seq.skip index
                |> Seq.truncate length
                |> Seq.fold Projection.updatePlayground playground 
        let cmd =
            if isFinished then Cmd.none
            else
                Cmd.OfAsync.result(async {
                    do! Async.Sleep (
                            match 20 / Math.Max(state.Events.Length / 500, 1) with
                            | LessEqual 1 -> 1
                            | x -> x
                        )
                    return ReplayEvent (index + length)
                })
        { state with
            IsReplaying = not isFinished
            Playground = newPlayground }
        , cmd
