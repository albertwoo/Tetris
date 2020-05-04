module Tetris.Client.Web.App.OnlineInfoView

open System
open Fable.React
open Tetris.Client.Web.Controls


let render (state: State) =
    div </> [
        Classes [ 
            Tw.``bg-brand-dark``; Tw.``text-xs``; Tw.``py-01``; Tw.``text-center``
            Tw.``text-white``; Tw.``opacity-75``
            Tw.``fixed``; Tw.``top-0``; Tw.``right-0``; Tw.``left-0``
        ]
        Text (
            match state.GameBoard with
            | DeferredValue gameboard -> 
                String.Format
                    (state.Context.Translate "App.OnlineInfo"
                    ,gameboard.OnlineCount 
                    ,(
                        match gameboard.TopRanks with
                        | [] -> 0
                        | h::_ -> h.Score
                    ))
            | _ -> "..."
        )
    ]
