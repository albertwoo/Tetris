module Tetris.Client.Web.App.Views

open Fable.React
open Fable.React.Props
open Tetris.Client.Web.Controls


let heading =
    div </> [
        Children [
            h1 </> [
                Classes [
                    Tw.flex; Tw.``flex-row``; Tw.``items-center``; Tw.``justify-center``
                    Tw.``py-10``
                ]
                Children [
                    span </> [
                        Text "slaveoftime@"
                        Classes [ Tw.``text-xl``; Tw.``text-gray-lightest``; Tw.``opacity-50`` ]
                    ]
                    span </> [
                        Text "俄罗斯方块"
                        Classes [ 
                            Tw.``text-2xl``; Tw.``text-gray-lightest``; Tw.``opacity-75``; Tw.``font-bold`` 
                            Tw.``ml-04``
                        ]
                    ]
                ]
            ]    
        ]
    ]


let playButton dispatch =
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
            githubBrand

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
                        heading
                        RankView.render state dispatch
                        playButton dispatch
                ]
            ]

            match state.ErrorInfo with
            | Some e -> errorView e (Msg.OnError >> dispatch)
            | None -> ()
        ]
    ]
