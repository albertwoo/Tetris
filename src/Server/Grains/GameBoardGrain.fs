namespace Server.Grains

open System
open Orleans
open Orleans.Providers
open FSharp.Control.Tasks
open Server.Grains.Interfaces
open Server.Common


[<StorageProvider(ProviderName = Constants.LiteDbStore)>]
type GameBoardGrain (factory: IGrainFactory) =
    inherit Grain<GameState>()

    member _.State 
        with get() = 
            if box base.State = null then GameState.defaultValue
            else base.State
        and set x = base.State <- x

    member _.SaveState() = base.WriteStateAsync()

    interface IGameBoardGrain with
        member this.UpdateGame(evt) =
            task {
                let! newState =
                    match evt with
                    | GameEvent.AddRecord (playerCred, record) ->
                        task {
                            let player = factory.GetGrain<IPlayerGrain>(playerCred.NickName)
                            do! player.AddRecord record
                            let newRank = 
                                { PlayerName = playerCred.NickName
                                  TimeSpentInSec = 1
                                  Score = record.Score
                                  RecordTime = DateTime.Now }
                            return 
                                { this.State with Ranks = newRank::this.State.Ranks }
                        }
                    | GameEvent.IncreaseOnlineCount ->
                        task { return { this.State with OnlineCount = this.State.OnlineCount + uint32 1 } }
                    | GameEvent.DecreaseOnlineCount ->
                        task { return { this.State with OnlineCount = this.State.OnlineCount - uint32 1 } }
                this.State <- newState
                do! this.SaveState()
                return ()
            }

        member this.GetTopRanks count =
            task {
                return 
                    this.State.Ranks 
                    |> Seq.sortByDescending (fun x -> x.Score) 
                    |> fun x -> if Seq.length x < count then x else Seq.take count x
                    |> Seq.toList
            }

        member this.GetOnlineCount () =
            task { return int this.State.OnlineCount }