module Client.Startup

open Elmish
open Elmish.React

#if DEBUG
// open Elmish.Debug
open Elmish.HMR
#endif


Program.mkProgram App.States.init App.States.update App.Views.render
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "root"
#if DEBUG
//|> Program.withDebugger
#endif
|> Program.run