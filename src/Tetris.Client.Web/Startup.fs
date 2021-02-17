module Tetris.Client.Web.Startup

open Elmish
open Elmish.React
open Fable.Core.JsInterop


importAll "@babel/polyfill"
importAll "core-js"

Program.mkProgram App.States.init App.States.update App.Views.render
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "root"
|> Program.run