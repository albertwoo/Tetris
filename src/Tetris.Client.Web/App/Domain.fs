namespace rec Client.App

open System
open Client
open Tetris.Server.WebApi.Dtos.Game


type State =
    { ErrorInfo: ClientError option
      GameBoard: Deferred<GameBoard>
      SelectedRankInfo: RecordBriefInfo option
      Plaground: PlaygroundState
      IsUploading: bool }


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

    | UploadRecord of RobotCheckerValue * NewRecord
    | UploadedRecord

    | PlaygroundMsg of Playground.Msg


[<RequireQualifiedAccess>]
type PlaygroundState =
    | Replaying of Deferred<Playground.State>
    | Playing of Playground.State
    | Submiting of Playground.State
    | Closed

type RobotCheckerValue =
    { Id: Guid
      Value: float }
