namespace rec Client.App

open Client
open Server.Dtos.Game


type State =
    { ErrorInfo: ClientError option
      GameBoard: Deferred<GameBoard>
      SelectedRankInfo: RecordBriefInfo option
      IsPlaying: bool
      ReplayingData: Deferred<RecordEvents>
      PlagroundState: Playground.State option }


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

    | PlaygroundMsg of Playground.Msg
