module Tetris.Client.Web.App.States

open Elmish
open Fable.SimpleHttp
open Fun.Result

open Tetris.Server.WebApi.Dtos
open Tetris.Client.Web


let init () =
    let state =
        Utils.getCachedPlayingState() 
        |> Option.defaultValue
            { Context = ClientContext.defaultValue
              ErrorInfo = None
              GameBoard = Deferred.NotStartYet
              SelectedRankInfo = None
              Plaground =  PlaygroundState.Closed
              UploadingState = Deferred.NotStartYet }
    let lang =
        match Browser.Dom.window.location.hash with
        | SafeString s when s.Contains("lang=EN") -> Lang.EN
        | SafeString s when s.Contains("lang=CN") -> Lang.CN
        | _ -> state.Context.Lang
    state
    , Cmd.batch [
        if state.Context.Translations.IsEmpty || state.Context.Lang <> lang then
            Cmd.ofMsg (GetTranslations (lang, AsyncOperation.Start))
        Cmd.ofMsg (GetGameBoard AsyncOperation.Start)
        Cmd.ofSub (fun dispatch ->
            Browser.Dom.window.setInterval(
                fun _ -> 
                    dispatch PingServer
                    dispatch (GetGameBoard AsyncOperation.Start)
                , 10000
            )
            |> ignore
        )
        Cmd.ofSub (fun dispatch ->
            Browser.Dom.document.addEventListener
                ("visibilitychange"
                ,fun _ -> dispatch OnWindowHide)
        )
        match state.Plaground with
        | PlaygroundState.Playing _ ->
            Cmd.ofMsg (Playground.Tick |> PlaygroundMsg)
        | _ ->
            ()
      ]


let update msg state =
    match msg with
    | OnError e -> { state with ErrorInfo = e }, Cmd.none

    | GetTranslations (lang, AsyncOperation.Start) ->
        state
        , Http.get (sprintf "/api/translations/%A" lang)
          |> Http.handleAsyncOperation (fun x -> GetTranslations(lang, x))
          |> Cmd.OfAsync.result
    | GetTranslations (lang, AsyncOperation.Finished data) ->
        let context =
            { state.Context with
                Lang = lang
                Translations = data }
        let state = { state with Context = context }
        Utils.setCachedPlayingState state
        state, Cmd.none
    | GetTranslations (_, AsyncOperation.Failed e) ->
        state, Cmd.none

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
            | DeferredValue x -> Deferred.ReloadFailed (x, e)
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
    | PausePlay ->
        let state =
            { state with
                Plaground =
                    match state.Plaground with
                    | PlaygroundState.Playing s -> PlaygroundState.Paused s
                    | x -> x }
        Utils.setCachedPlayingState state
        state, Cmd.none
    | ReStartPlay ->
        match state.Plaground with
        | PlaygroundState.Paused s ->
            { state with Plaground = PlaygroundState.Playing s }
            , Cmd.ofMsg (Playground.Tick |> PlaygroundMsg)
        | _ ->
            state, Cmd.none
    | StopReplay ->
        { state with Plaground = PlaygroundState.Closed }
        , Cmd.none

    | StartPlay -> 
        let newS, newC = Playground.States.init()
        let newS = { newS with IsViewMode = false }
        { state with 
            Plaground = PlaygroundState.Playing newS
            UploadingState = Deferred.NotStartYet }
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
        Utils.setCachedPlayingState { state with Plaground = PlaygroundState.Closed }
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

    | UploadRecord (checker, record, AsyncOperation.Start) ->
        { state with UploadingState = Deferred.Loading }
        , Http.postJson "/api/player/record" record
          |> Http.headers [
                Header(RobotCheckerIdKey, checker.Id.ToString())
                Header(RobotCheckerValueKey, checker.Value.ToString())
          ]
          |> Http.handleAsync 
            (fun _ -> UploadRecord (checker, record, AsyncOperation.Finished()))
            (fun e -> UploadRecord (checker, record, AsyncOperation.Failed e))
          |> Cmd.OfAsync.result
    | UploadRecord (_, _, AsyncOperation.Finished ()) ->
        { state with UploadingState = Deferred.Loaded() }
        , Cmd.batch [
            Cmd.ofMsg ClosePlay
            Cmd.ofMsg (GetGameBoard AsyncOperation.Start)
          ]
    | UploadRecord (_, _, AsyncOperation.Failed e) ->
        { state with UploadingState = Deferred.LoadFailed e }
        , Cmd.ofMsg (OnError (Some e))

    | OnWindowHide ->
        Utils.setCachedPlayingState state
        state, Cmd.none
