[<RequireQualifiedAccess>]
module Tetris.Client.Web.Controls.GithubBand

open Feliz
open Tetris.Client.Web.Controls


let view =
    Html.div [
        prop.classes [ Tw.``fixed``; Tw.``top-0``; Tw.``right-0`` ]
        prop.children [
            Html.a [
                prop.text "GITHUB"
                prop.classes [ 
                    Tw.``bg-github-color``; Tw.uppercase; Tw.``text-gray-lighter``
                    Tw.``text-center``; Tw.``py-01``; Tw.``text-xs``; Tw.``shadow-lg``
                    Tw.block; Tw.``hover:bg-brand``
                ]
                prop.style [ 
                    style.transform [
                        transform.rotate 60
                    ]
                    style.width 200
                    style.margin (22, -70, 0, 0)
                ]
                prop.href "https://github.com/albertwoo/tetris"
            ]
        ]
    ]
