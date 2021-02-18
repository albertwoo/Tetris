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
              SelectedSeason = None
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
        , Http.postJson (sprintf "/api/game/ping/%s" (state.Context.ClientId.ToString())) ""
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
        | PlaygroundState.Replaying (DeferredValue s, oldState) ->
            let newS, newCmd = Playground.States.update msg' s
            { state with Plaground = PlaygroundState.Replaying (Deferred.Loaded newS, oldState) }
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

    | GotoPreSeason ->
        match state.GameBoard.Value with
        | None -> state, Cmd.none
        | Some b ->
            let id =
                match state.SelectedSeason with
                | None -> b.Seasons |> List.tryHead
                | Some s when s.Id > 1 -> b.Seasons |> List.tryFind (fun x -> x.Id = s.Id - 1)
                | x -> x
            { state with SelectedSeason = id; Plaground = PlaygroundState.Closed }, Cmd.none

    | GotoPosSeason ->
        match state.GameBoard.Value with
        | None -> state, Cmd.none
        | Some b ->
            let maxId = b.Seasons |> List.maxBy (fun x -> x.Id) |> fun x -> x.Id
            let id =
                match state.SelectedSeason with
                | None -> b.Seasons |> List.tryHead
                | Some s when s.Id < maxId -> b.Seasons |> List.tryFind (fun x -> x.Id = s.Id + 1)
                | x -> x
            { state with SelectedSeason = id; Plaground = PlaygroundState.Closed }, Cmd.none
