module Client.Playground.States

open System
open Elmish
open Fun.Result
open Tetris.Core


let defaultPlayground =
    {
        IsGameOver = false
        Score = 0
        Border = { Width = 15; Height = 30 }
        Blocks = []
        MovingBlock = None
        RemainSquares = []
    }

let init() =
    { Events = []
      Playground = defaultPlayground 
      StartTime = None
      IsReplaying = false }
    , Cmd.none


let update msg state =
    match msg with
    | Start ->
        { state with
            Events = []
            Playground = defaultPlayground
            StartTime = Some DateTime.Now }
        , Cmd.batch [
            Cmd.ofMsg (Utils.generateRamdomBlock() |> Event.NewBlock |> NewEvent)
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
                Events = state.Events@(newEvents |> List.map (fun e -> { TimeStamp = DateTime.Now; Event = e }))
                Playground = newEvents |> List.fold Projection.updatePlayground state.Playground }
            , Cmd.none

    | ReplayEvent index ->
        let isFinished = index + 1 = state.Events.Length
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
