namespace rec Client.App

open Client
open Server.Dtos.Game


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

    | UploadRecord of NewRecord
    | UploadedRecord

    | PlaygroundMsg of Playground.Msg


[<RequireQualifiedAccess>]
type PlaygroundState =
    | Replaying of Deferred<Playground.State>
    | Playing of Playground.State
    | Submiting of Playground.State
    | Closed
