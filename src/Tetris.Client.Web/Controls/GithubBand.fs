[<RequireQualifiedAccess>]
module Tetris.Client.Web.Controls.GithubBand

open Fable.React
open Fable.React.Props
open Tetris.Client.Web.Controls


let view =
    div </> [
        Classes [ Tw.``fixed``; Tw.``top-0``; Tw.``right-0`` ]
        Children [
            a </> [
                Text "GITHUB"
                Classes [ 
                    Tw.``bg-github-color``; Tw.uppercase; Tw.``text-gray-lighter``
                    Tw.``text-center``; Tw.``py-01``; Tw.``text-xs``; Tw.``shadow-lg``
                    Tw.block; Tw.``hover:bg-brand``
                ]
                Style [ 
                    Transform "rotate(60deg)"
                    Width "200px"
                    Margin "22px -70px 0 0"
                ]
                Href "https://github.com/albertwoo/tetris"
            ]
        ]
    ]
