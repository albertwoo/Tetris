module Tetris.Client.Web.App.Views

open Fable.React
open Fable.React.Props
open Tetris.Client.Web.Controls


let private playButton dispatch =
    div </> [
        Classes [ Tw.flex; Tw.``flex-col``; Tw.``items-center``; Tw.``py-04``; Tw.``mt-04`` ]
        Children [
            Button.primary [
                Text "开始游戏"
                OnClick (fun _ -> StartPlay |> dispatch)
            ]
        ]
    ]


let render state dispatch =
    div </> [
        Classes [ Tw.``h-full``; Tw.``font-sans`` ]
        Children [
            OnlineInfoView.render state
            GithubBand.view

            div </> [
                Classes [ Tw.``h-full``; Tw.``w-full``; Tw.``mx-auto``; Tw.flex; Tw.``flex-col``; Tw.``justify-center`` ]
                Styles [ MaxWidth 720 ]
                Children [
                    match state.Plaground with
                    | PlaygroundState.Replaying _
                    | PlaygroundState.Playing _ ->
                        PlaygroundView.render state dispatch
                    | PlaygroundState.Submiting p ->
                        SubmitRecordView.render (p, dispatch)
                    | _ ->
                        HeaderView.view
                        RankView.render state dispatch
                        playButton dispatch
                ]
            ]

            match state.ErrorInfo with
            | Some e -> errorView e (Msg.OnError >> dispatch)
            | None -> ()
        ]
    ]
