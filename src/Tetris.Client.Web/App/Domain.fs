namespace rec Tetris.Client.Web.App

open System
open Tetris.Client.Web
open Tetris.Server.WebApi.Dtos.Game


type State =
    { ErrorInfo: ClientError option
      GameBoard: Deferred<GameBoard>
      SelectedRankInfo: RecordBriefInfo option
      Plaground: PlaygroundState
      UploadingState: Deferred<unit>
      LastCachedTime: DateTime }


type Msg =
    | OnError of ClientError option
    
    | PingServer
    | Pong
    
    | GetGameBoard of AsyncOperation<GameBoard>
    | SelectRankInfo of RecordBriefInfo option

    | StartReplay
    | GetRecordDetail of AsyncOperation<RecordEvents>
    | StopReplay

    | StartPlay
    | StopPlay
    | ClosePlay

    | UploadRecord of Tetris.Client.Web.Controls.RobotCheckerValue * NewRecord * AsyncOperation<unit>

    | PlaygroundMsg of Playground.Msg


[<RequireQualifiedAccess>]
type PlaygroundState =
    | Replaying of Deferred<Playground.State>
    | Playing of Playground.State
    | Submiting of Playground.State
    | Closed
