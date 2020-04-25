module Client.App.States

open Elmish
open Client


let init () =
    { ErrorInfo = None
      GameBoard = Deferred.NotStartYet
      SelectedRankInfo = None
      IsPlaying = false
      ReplayingData = Deferred.NotStartYet
      PlagroundState = None }
    , Cmd.batch [
        Cmd.ofMsg (GetGameBoard AsyncOperation.Start)
        Cmd.ofSub(fun dispatch ->
            Browser.Dom.window.setInterval(
                fun _ -> 
                    dispatch PingServer
                    dispatch (GetGameBoard AsyncOperation.Start)
                , 10000
            )
            |> ignore
        )
      ]


let update msg state =
    match msg with
    | OnError e -> { state with ErrorInfo = e }, Cmd.none

    | PingServer ->
        state
        , Http.postJson "/api/game/ping" ""
          |> Http.handleAsync (fun _ -> Pong) (Some >> OnError)
          |> Cmd.OfAsync.result
    | Pong -> state, Cmd.none

    | GetGameBoard AsyncOperation.Start ->
        let gameboard =
            match state.GameBoard with
            | DeferredValue x -> Deferred.Reloading x
            | _ -> Deferred.Loading
        { state with GameBoard = gameboard }
        , Http.get "/api/game/board"
          |> Http.handleAsyncOperation GetGameBoard
          |> Cmd.OfAsync.result
    | GetGameBoard (AsyncOperation.Finished data) ->
        { state with GameBoard = Deferred.Loaded data }
        , Cmd.none
    | GetGameBoard (AsyncOperation.Failed e) ->
        let gameboard =
            match state.GameBoard with
            | DeferredValue x -> Deferred.ReloadingFailed (x, e)
            | _ -> Deferred.LoadFailed e
        { state with GameBoard = gameboard }
        , Cmd.none

    | SelectRankInfo s -> { state with SelectedRankInfo = s }, Cmd.none


    | GetRecordDetail AsyncOperation.Start ->
        match state.SelectedRankInfo with
        | None -> state, Cmd.none
        | Some rank ->
            { state with ReplayingData = Deferred.Loading }
            , Http.get (sprintf "/api/player/%s/record/%d/events" rank.PlayerName rank.Id)
              |> Http.handleAsyncOperation GetRecordDetail
              |> Cmd.OfAsync.result
    | GetRecordDetail (AsyncOperation.Finished data) ->
        { state with ReplayingData = Deferred.Loaded data }
        , Cmd.none
    | GetRecordDetail (AsyncOperation.Failed e) ->
        { state with ReplayingData = Deferred.LoadFailed e }
        , Cmd.none

    | StartReplay -> 
        state
        , Cmd.ofMsg (GetRecordDetail AsyncOperation.Start)
    | StopReplay ->
        { state with ReplayingData = Deferred.NotStartYet }
        , Cmd.none

    | StartPlay -> 
        let newS, newC = Playground.States.init()
        { state with 
            IsPlaying = true
            PlagroundState = Some newS }
        , Cmd.batch [
            Cmd.map PlaygroundMsg newC
            Cmd.ofMsg (Playground.Start |> PlaygroundMsg)
          ]
    | StopPlay -> 
        { state with 
            IsPlaying = false
            PlagroundState = None }, Cmd.none

    | PlaygroundMsg msg' ->
        match state.PlagroundState with
        | None -> state, Cmd.none
        | Some s ->
            let newS, newCmd = Playground.States.update msg' s
            { state with PlagroundState = Some newS }
            , Cmd.map PlaygroundMsg newCmd
