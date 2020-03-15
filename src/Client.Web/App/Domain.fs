namespace rec Client.App

open Client
open Client.Common


type State =
    { ErrorInfo: ClientError option
      OnlineInfo: OnlineInfo option
      RankInfos: RankInfo list
      SelectedRankInfo: RankInfo option
      IsLoading: bool
      IsPlaying: bool
      IsReplying: bool
      ReplyingData: NeedDefine option
      PlagroundState: Playground.State option }


type Msg =
    | OnError of ClientError option
    | GetRankInfos
    | GotRankInfos of RankInfo list
    | GotOnlineInfo of OnlineInfo

    | SelectRankInfo of RankInfo option

    | StartReply
    | GotReplyingData of NeedDefine
    | StopReply

    | StartPlay
    | StopPlay    

    | PlaygroundMsg of Playground.Msg


type OnlineInfo =
    { PlayerCount: int
      HightestScore: int }

type RankInfo =
    { Id: int64
      Score: int
      TimeCost: int
      Name: string }
