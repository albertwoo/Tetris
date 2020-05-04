module Tetris.Client.Web.App.Views

open Feliz
open Tetris.Client.Web.Controls


let private playButton (state: State, dispatch) =
    Html.div [
        prop.classes [ Tw.flex; Tw.``flex-col``; Tw.``items-center``; Tw.``py-04``; Tw.``mt-04`` ]
        prop.children [
            match state.Plaground with
            | PlaygroundState.Closed ->
                Button.render [
                    ButtonProp.Text (state.Context.Translate "App.StartPlay")
                    ButtonProp.OnClick (fun _ -> StartPlay |> dispatch)
                ]
            | PlaygroundState.Paused _ ->
                Html.button [
                    prop.onClick (fun e -> e.preventDefault(); ReStartPlay |> dispatch)
                    prop.classes [ 
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
    Html.div [
        prop.classes [
            Tw.``h-full``; Tw.``font-sans``
            match state.Plaground with
            | PlaygroundState.Closed -> Tw.``py-08``
            | _ -> ()
        ]
        prop.children [
            OnlineInfoView.render state
            GithubBand.view

            Html.div [
                prop.classes [
                    Tw.``h-full``; Tw.``w-full``; Tw.``mx-auto``; Tw.flex; Tw.``flex-col``; Tw.``justify-center``
                    match state.Plaground with
                    | PlaygroundState.Closed -> Tw.``overflow-auto``
                    | _ -> ()
                ]
                prop.style [ style.maxWidth 720 ]
                prop.children [
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
