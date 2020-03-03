module Client.Startup

open Elmish
open Elmish.React
open Fable.Core.JsInterop

#if DEBUG
// open Elmish.Debug
open Elmish.HMR
#endif

importAll "@babel/polyfill"
importAll "core-js"

Program.mkProgram App.States.init App.States.update App.Views.render
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "root"
#if DEBUG
//|> Program.withDebugger
#endif
|> Program.run