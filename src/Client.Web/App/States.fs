module Client.App.States

open Elmish
open Client


let init () =
    { ErrorInfo = None
      GameBoard = Deferred.NotStartYet
      SelectedRankInfo = None
      Plaground = PlaygroundState.Closed
      IsUploading = false }
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
            { state with Plaground = PlaygroundState.Replaying Deferred.Loading }
            , Http.get (sprintf "/api/player/%s/record/%d/events" rank.PlayerName rank.Id)
              |> Http.handleAsyncOperation GetRecordDetail
              |> Cmd.OfAsync.result
    | GetRecordDetail (AsyncOperation.Finished data) ->
        let newS, newC = Playground.States.init()
        let newS = { newS with Events = data; IsViewMode = true }
        { state with Plaground = PlaygroundState.Replaying (Deferred.Loaded newS)  }
        , Cmd.batch [
            Cmd.map PlaygroundMsg newC
            Cmd.ofMsg (Playground.ReplayEvent 0 |> PlaygroundMsg)
          ]
    | GetRecordDetail (AsyncOperation.Failed e) ->
        { state with Plaground = PlaygroundState.Closed }
        , Cmd.none

    | StartReplay -> 
        state
        , Cmd.ofMsg (GetRecordDetail AsyncOperation.Start)
    | StopReplay ->
        { state with Plaground = PlaygroundState.Closed }
        , Cmd.none

    | StartPlay -> 
        let newS, newC = Playground.States.init()
        let newS = { newS with IsViewMode = false }
        { state with Plaground = PlaygroundState.Playing newS }
        , Cmd.batch [
            Cmd.map PlaygroundMsg newC
            Cmd.ofMsg (Playground.Start |> PlaygroundMsg)
          ]
    | StopPlay -> 
        { state with 
            Plaground =
                match state.Plaground with
                | PlaygroundState.Playing x -> PlaygroundState.Submiting x 
                | _ -> PlaygroundState.Closed }
        , Cmd.none
    | ClosePlay ->
        { state with Plaground = PlaygroundState.Closed }
        , Cmd.none

    | PlaygroundMsg msg' ->
        match state.Plaground with
        | PlaygroundState.Replaying (DeferredValue s) ->
            let newS, newCmd = Playground.States.update msg' s
            { state with Plaground = PlaygroundState.Replaying (Deferred.Loaded newS) }
            , Cmd.map PlaygroundMsg newCmd
        | PlaygroundState.Playing s -> 
            let newS, newCmd = Playground.States.update msg' s
            { state with Plaground = PlaygroundState.Playing newS }
            , Cmd.map PlaygroundMsg newCmd
        | _ ->
            state, Cmd.none

    | UploadRecord record ->
        { state with IsUploading = true }
        , Http.postJson "/api/player/record" record
          |> Http.handleAsync (fun _ -> UploadedRecord) (Some >> OnError)
          |> Cmd.OfAsync.result
    | UploadedRecord ->
        { state with IsUploading = false }
        , Cmd.batch [
            Cmd.ofMsg ClosePlay
            Cmd.ofMsg (GetGameBoard AsyncOperation.Start)
          ]
