module Tetris.Client.Web.App.Utils

open Fun.Result
open Tetris.Client.Web.Http


let private CachePlayingStateKey = "CachePlayingStateKey"

let getCachedPlayingState() =
    option {
        let! str = 
            match Browser.Dom.window.localStorage.getItem CachePlayingStateKey with
            | SafeString str -> Some str
            | NullOrEmptyString -> None
        return! fromJson<State> str |> Result.toOption
    }

let setCachedPlayingState (state: State) =
    Browser.Dom.window.localStorage.setItem(CachePlayingStateKey, toJson state)
