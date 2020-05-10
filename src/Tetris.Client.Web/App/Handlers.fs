module Tetris.Client.Web.App.Handlers

open Elmish
open Fable.SimpleHttp

open Tetris.Server.WebApi.Dtos
open Tetris.Client.Web


let getTranslations msg state =
    match msg with
    | lang, AsyncOperation.Start ->
        state
        , Http.get (sprintf "/api/translations/%A?v=1" lang)
          |> Http.handleAsyncOperation (fun x -> GetTranslations(lang, x))
          |> Cmd.OfAsync.result

    | lang, AsyncOperation.Finished data ->
        let context =
            { state.Context with
                Lang = lang
                Translations = data }
        let state = { state with Context = context }
        Utils.setCachedPlayingState state
        state, Cmd.none

    | _, AsyncOperation.Failed e ->
        state, Cmd.none


let getGameBoard msg state =
    match msg with
    | AsyncOperation.Start ->
        let gameboard =
            match state.GameBoard with
            | DeferredValue x -> Deferred.Reloading x
            | _ -> Deferred.Loading
        { state with GameBoard = gameboard }
        , Http.get "/api/game/board"
          |> Http.handleAsyncOperation GetGameBoard
          |> Cmd.OfAsync.result

    | AsyncOperation.Finished data ->
        { state with GameBoard = Deferred.Loaded data }
        , Cmd.none

    | AsyncOperation.Failed e ->
        let gameboard =
            match state.GameBoard with
            | DeferredValue x -> Deferred.ReloadFailed (x, e)
            | _ -> Deferred.LoadFailed e
        { state with GameBoard = gameboard }
        , Cmd.none


let getRecord msg state =
    match msg with
    | AsyncOperation.Start ->
        match state.SelectedRankInfo with
        | None -> state, Cmd.none
        | Some rank ->
            { state with Plaground = PlaygroundState.Replaying Deferred.Loading }
            , Http.get (sprintf "/api/player/%s/record/%d/events" rank.PlayerName rank.Id)
              |> Http.handleAsyncOperation GetRecordDetail
              |> Cmd.OfAsync.result

    | AsyncOperation.Finished data ->
        let newS, newC = Playground.States.init()
        let newS = { newS with Events = data; IsViewMode = true }
        { state with Plaground = PlaygroundState.Replaying (Deferred.Loaded newS)  }
        , Cmd.batch [
            Cmd.map PlaygroundMsg newC
            Cmd.ofMsg (Playground.ReplayEvent 0 |> PlaygroundMsg)
          ]

    | AsyncOperation.Failed e ->
        { state with Plaground = PlaygroundState.Closed }
        , Cmd.none


let uploadRecord msg state =
    match msg with
    | checker: Controls.RobotCheckerValue, record, AsyncOperation.Start ->
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

    | _, _, AsyncOperation.Finished () ->
        { state with UploadingState = Deferred.Loaded() }
        , Cmd.batch [
            Cmd.ofMsg (PlayMsg.ClosePlay |> ControlPlayground)
            Cmd.ofMsg (GetGameBoard AsyncOperation.Start)
          ]

    | _, _, AsyncOperation.Failed e ->
        { state with UploadingState = Deferred.LoadFailed e }
        , Cmd.ofMsg (OnError (Some e))

