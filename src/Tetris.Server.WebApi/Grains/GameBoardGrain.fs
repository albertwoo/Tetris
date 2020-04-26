namespace Tetris.Server.WebApi.Grain

open System
open System.Threading.Tasks
open Orleans
open Orleans.Runtime
open FSharp.Control.Tasks
open Tetris.Server.WebApi.Common
open Tetris.Server.WebApi.Grain.Interfaces


type GameBoardGrain
    (
        [<PersistentState("GameBoard", Constants.LiteDbStore)>] state: IPersistentState<GameBoardState>
    ) as this =

    inherit Grain()

    member _.SafeState 
        with get() =
            if box state.State |> isNull then GameBoardState.defaultValue
            else state.State
        and set s =
            state.State <- s

    member _.RefreshOnline() =
        task {
            this.SafeState <-
                { this.SafeState with 
                    OnlineIPs = 
                        this.SafeState.OnlineIPs
                        |> Map.filter (fun _ t -> t.AddSeconds 10. > DateTime.Now) }
            do! state.WriteStateAsync()
        }

    override _.OnActivateAsync() =
        this.RegisterTimer
            (fun _ -> this.RefreshOnline() :> Task
            ,this.SafeState
            ,TimeSpan.FromSeconds 5.
            ,TimeSpan.FromSeconds 5.)
        |> ignore
        task { () } :> Task

    interface IGameBoardGrain with
        member _.Ping (ip) =
            task {
                this.SafeState <- { this.SafeState with OnlineIPs = this.SafeState.OnlineIPs.Add(ip, DateTime.Now) }
                do! state.WriteStateAsync()
                return ()
            }

        member _.AddRecord (record) =
            task {
                let ranks =
                    record::this.SafeState.Ranks
                    |> List.sortByDescending (fun x -> x.Score)
                    |> List.chunkBySize 10
                    |> List.item 0

                this.SafeState <- { this.SafeState with Ranks = ranks }
                do! state.WriteStateAsync()
            }

        member _.GetState () =
            task {
                return this.SafeState
            }
