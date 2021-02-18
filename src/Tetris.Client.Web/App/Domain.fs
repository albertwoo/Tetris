namespace rec Tetris.Client.Web.App

open Tetris.Client.Web
open Tetris.Server.WebApi.Dtos.Game


type State =
    { Context: ClientContext 
      ErrorInfo: ClientError option
      GameBoard: Deferred<GameBoard>
      SelectedSeason: SeasonBriefInfo option
      SelectedRankInfo: RecordBriefInfo option
      Plaground: PlaygroundState
      UploadingState: Deferred<unit> }


type Msg =
    | OnError of ClientError option
    
    | GetTranslations of Lang * AsyncOperation<Map<string, string>>
    | GetGameBoard of AsyncOperation<GameBoard>
    | GetRecordDetail of AsyncOperation<RecordEvents>
    | UploadRecord of Controls.RobotCheckerValue * NewRecord * AsyncOperation<unit>

    | PingServer
    | Pong
    
    | SelectRankInfo of RecordBriefInfo option

    | ControlPlayground of PlayMsg
    | PlaygroundMsg of Playground.Msg

    | OnWindowHide

    | GotoPreSeason
    | GotoPosSeason


[<RequireQualifiedAccess>]
type PlaygroundState =
    | Replaying of replayState: Deferred<Playground.State> * oldState: Playground.State option
    | Playing of Playground.State
    | Paused of Playground.State
    | Submiting of Playground.State
    | Closed


[<RequireQualifiedAccess>]
type PlayMsg =
    | StartReplay
    | StopReplay

    | StartPlay
    | PausePlay
    | ReStartPlay
    | StopPlay
    | ClosePlay
