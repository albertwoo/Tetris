module Client.App.States

open System
open Elmish


let fakeRankInfos =
    [
        for i in 0..10 do
            { Id = int64 i
              Name = sprintf "slaveoftime-%d" i
              Score = Random().Next(100, 10000)
              TimeCost = Random().Next(1000, 100000) }
    ]


let init () =
    { ErrorInfo = None
      OnlineInfo = None
      RankInfos = []
      SelectedRankInfo = None
      IsLoading = false
      IsPlaying = false
      IsReplying = false
      ReplyingData = None }
    , Cmd.batch [
        Cmd.ofMsg GetRankInfos
        Cmd.ofSub(fun dispatch ->
            // Simulate signalR
            Browser.Dom.window.setInterval(
                fun _ ->
                    { PlayerCount = Random().Next(10, 20)
                      HightestScore = Random().Next(100, 10000) }
                    |> GotOnlineInfo
                    |> dispatch
                , 1000
            )
            |> ignore
        )
      ]


let update msg state =
    match msg with
    | OnError e -> { state with ErrorInfo = e }, Cmd.none

    | GetRankInfos ->
        { state with IsLoading = true }
        , Cmd.ofMsg (GotRankInfos fakeRankInfos)

    | GotRankInfos data ->
        { state with
            IsLoading = false
            RankInfos = data |> List.sortByDescending (fun x -> x.Score) }
        , Cmd.none

    | GotOnlineInfo data -> { state with OnlineInfo = Some data }, Cmd.none

    | SelectRankInfo s -> { state with SelectedRankInfo = s }, Cmd.none

    | StartReply -> { state with IsReplying = true }, Cmd.none

    | GotReplyingData data ->
        { state with ReplyingData = Some data }
        , Cmd.none

    | StopReply ->
        { state with
            IsReplying = false
            ReplyingData = None }
        , Cmd.none

    | StartPlay -> { state with IsPlaying = true }, Cmd.none
    | StopPlay -> { state with IsPlaying = false }, Cmd.none
