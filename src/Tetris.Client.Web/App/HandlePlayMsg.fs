module Tetris.Client.Web.App.HandlePlayMsg

open Elmish
open Tetris.Client.Web


let play msg state =
    match msg with
    | PlayMsg.StartReplay -> 
        state
        , Cmd.ofMsg (GetRecordDetail AsyncOperation.Start)

    | PlayMsg.StopReplay ->
        { state with Plaground = PlaygroundState.Closed }
        , Cmd.none


    | PlayMsg.StartPlay ->
        let freshStart() =
            let newS, newC = Playground.States.init()
            newS, Cmd.batch [ newC; Cmd.ofMsg Playground.Start ]

        let newS, newC = 
            match Utils.getCachedPlayingState()with
            | Some state ->
                match state.Plaground with
                | PlaygroundState.Playing x
                | PlaygroundState.Paused x
                | PlaygroundState.Submiting x -> x, Cmd.ofMsg Playground.Tick
                | _ -> freshStart()
            | _ ->
                freshStart()

        let newS = { newS with IsViewMode = false }

        { state with 
            Plaground = PlaygroundState.Playing newS
            UploadingState = Deferred.NotStartYet }
        , Cmd.map PlaygroundMsg newC


    | PlayMsg.PausePlay ->
        let state =
            { state with
                Plaground =
                    match state.Plaground with
                    | PlaygroundState.Playing s -> PlaygroundState.Paused s
                    | x -> x }
        Utils.setCachedPlayingState state
        state, Cmd.none

    | PlayMsg.ReStartPlay ->
        match state.Plaground with
        | PlaygroundState.Paused s ->
            { state with Plaground = PlaygroundState.Playing s }
            , Cmd.ofMsg (Playground.Tick |> PlaygroundMsg)
        | _ ->
            state, Cmd.none

    | PlayMsg.StopPlay -> 
        { state with 
            Plaground =
                match state.Plaground with
                | PlaygroundState.Playing x -> PlaygroundState.Submiting x 
                | _ -> PlaygroundState.Closed }
        , Cmd.none

    | PlayMsg.ClosePlay ->
        Utils.setCachedPlayingState { state with Plaground = PlaygroundState.Closed }
        { state with Plaground = PlaygroundState.Closed }
        , Cmd.none
