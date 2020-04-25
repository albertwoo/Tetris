module Client.App.OnlineInfo

open Fable.React
open Client.Controls


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
                sprintf "%d正在玩/最高分%d" 
                        gameboard.OnlineCount 
                        (
                            match gameboard.TopRanks with
                            | [] -> 0
                            | h::_ -> h.Score
                        )
            | _ -> "..."
        )
    ]
