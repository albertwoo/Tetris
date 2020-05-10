module Tetris.Client.Web.App.States

open Elmish
open Fable.SimpleHttp
open Fun.Result

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

    let cmd =
        Cmd.batch [
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

    state, cmd


let update msg state =
    match msg with
    | OnError e -> { state with ErrorInfo = e }, Cmd.none

    | PingServer ->
        state
        , Http.postJson "/api/game/ping" ""
          |> Http.handleAsync (fun _ -> Pong) (Some >> OnError)
          |> Cmd.OfAsync.result

    | Pong -> state, Cmd.none

    | GetTranslations (lang, op) -> Handlers.getTranslations (lang, op) state
    | GetGameBoard op -> Handlers.getGameBoard op state
    | GetRecordDetail op -> Handlers.getRecord op state

    | SelectRankInfo s -> { state with SelectedRankInfo = s }, Cmd.none

    | ControlPlayground msg -> HandlePlayMsg.play msg state

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

    | UploadRecord (checker, record, op) -> Handlers.uploadRecord (checker, record, op) state

    | OnWindowHide ->
        match state.Plaground with
            | PlaygroundState.Replaying _ -> ()
            | _ -> Utils.setCachedPlayingState state
        state, Cmd.none
