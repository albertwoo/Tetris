﻿module Tetris.Client.Web.App.OnlineInfoView

open System
open Feliz
open Tetris.Client.Web.Controls


let render (state: State) =
    let ranks = state.SelectedSeason |> Option.map (fun x -> x.Ranks) |> Option.defaultValue []
    Html.div [
        prop.classes [ 
            Tw.``bg-brand-dark``; Tw.``text-xs``; Tw.``py-01``; Tw.``text-center``
            Tw.``text-white``; Tw.``opacity-75``
            Tw.``fixed``; Tw.``top-0``; Tw.``right-0``; Tw.``left-0``
        ]
        prop.text (
            match state.GameBoard.Value with
            | Some gameboard -> 
                String.Format
                    (state.Context.Translate "App.OnlineInfo"
                    ,gameboard.OnlineCount 
                    ,(
                        match ranks with
                        | [] -> 0
                        | h::_ -> h.Score
                    ))
            | _ -> "..."
        )
    ]
