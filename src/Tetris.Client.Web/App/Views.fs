module Tetris.Client.Web.App.Views

open Fable.React
open Fable.React.Props
open Tetris.Client.Web.Controls


let private playButton (state: State, dispatch) =
    div </> [
        Classes [ Tw.flex; Tw.``flex-col``; Tw.``items-center``; Tw.``py-04``; Tw.``mt-04`` ]
        Children [
            match state.Plaground with
            | PlaygroundState.Closed ->
                Button.primary [
                    Text (state.Context.Translate "App.StartPlay")
                    OnClick (fun _ -> StartPlay |> dispatch)
                ]
            | PlaygroundState.Paused _ ->
                button </> [
                    OnClick (fun e -> e.preventDefault(); ReStartPlay |> dispatch)
                    Classes [ 
                        Icons.``icon-play-circle``; Tw.``text-white``; Tw.``opacity-75``; Tw.``w-10``; Tw.``h-10``
                        Tw.``rounded-full``; Tw.``outline-none``; Tw.``bg-brand``
                        Tw.``hover:opacity-100``; Tw.``focus:opacity-100``; Tw.``focus:outline-none``
                    ]
                ]
            | _ ->
                ()
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
                        SubmitRecordView.render (state, p, dispatch)
                    | PlaygroundState.Closed  | PlaygroundState.Paused _ ->
                        HeaderView.render (state.Context, dispatch)
                        RankView.render state dispatch
                        playButton (state, dispatch)
                ]
            ]

            match state.ErrorInfo with
            | Some e -> errorView e (Msg.OnError >> dispatch)
            | None -> ()
        ]
    ]
