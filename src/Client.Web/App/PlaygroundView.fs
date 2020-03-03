module Client.App.PlaygroundView

open Fable.React
open Fable.React.Props
open Client.Controls


let render state dispatch =
    div </> [
        Children [
            // to do
            div </> [
                Classes [ Tw.flex; Tw.``flex-row``; Tw.``justify-center``; Tw.``items-center`` ]
                Children [
                    Button.primary [
                        Text "结束"
                        OnClick (fun _ -> StopPlay |> dispatch)
                    ]
                ]
            ]
        ]
    ]
