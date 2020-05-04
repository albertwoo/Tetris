namespace rec Tetris.Client.Web.App

open Tetris.Client.Web
open Tetris.Server.WebApi.Dtos.Game


type State =
    { Context: ClientContext 
      ErrorInfo: ClientError option
      GameBoard: Deferred<GameBoard>
      SelectedRankInfo: RecordBriefInfo option
      Plaground: PlaygroundState
      UploadingState: Deferred<unit> }


type Msg =
    | OnError of ClientError option
    
    | GetTranslations of Lang * AsyncOperation<Map<string, string>>

    | PingServer
    | Pong
    
    | GetGameBoard of AsyncOperation<GameBoard>
    | SelectRankInfo of RecordBriefInfo option

    | StartReplay
    | GetRecordDetail of AsyncOperation<RecordEvents>
    | StopReplay

    | StartPlay
    | PausePlay
    | ReStartPlay
    | StopPlay
    | ClosePlay

    | UploadRecord of Controls.RobotCheckerValue * NewRecord * AsyncOperation<unit>

    | PlaygroundMsg of Playground.Msg

    | OnWindowHide


[<RequireQualifiedAccess>]
type PlaygroundState =
    | Replaying of Deferred<Playground.State>
    | Playing of Playground.State
    | Paused of Playground.State
    | Submiting of Playground.State
    | Closed
