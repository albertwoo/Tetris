namespace Tetris.Server.WebApi.Grain

open System
open System.Threading.Tasks
open Orleans
open Orleans.Runtime
open FSharp.Control.Tasks
open Fun.Result
open Tetris.Server.WebApi.Common
open Tetris.Server.WebApi.Grain.Interfaces


type GameBoardGrain
    (
        [<PersistentState("GameBoard", Constants.LiteDbStore)>] state: IPersistentState<GameBoardState>
    ) as this =

    inherit Grain()

    member _.SafeState 
        with get() =
            let compatibleState =
                if box state.State |> isNull then GameBoardState.defaultValue
                else 
                    if state.State.Ranks.IsEmpty then state.State
                    else { state.State with
                            Ranks = []
                            Seasons =
                                Map.empty
                                |> Map.add 1
                                    { StartTime = DateTime.Parse("2020/05/01")
                                      Width = 18
                                      Height = 30
                                      Ranks = state.State.Ranks }
                                |> Some }
            match compatibleState.Seasons with
            | Some ss when ss |> Map.containsKey 2 |> not ->
                { compatibleState with 
                    Seasons = 
                        ss
                        |> Map.add 2 
                            {
                                StartTime = DateTime.Parse("2021/02/18")
                                Width = 10
                                Height = 22
                                Ranks = []
                            }
                        |> Some }
            | _ ->
                compatibleState
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

        member _.AddRecord (seasonId, record) =
            task {
                let seasons =
                    option {
                        let! seasons = this.SafeState.Seasons
                        return
                            seasons
                            |> Map.tryFind seasonId
                            |> function
                                | None -> failwith $"Season {seasonId} is not found"
                                | Some season ->
                                    seasons
                                    |> Map.add seasonId
                                        { season with
                                            Ranks =
                                                record::season.Ranks
                                                |> List.sortByDescending (fun x -> x.Score)
                                                |> List.chunkBySize 10
                                                |> List.item 0 }
                    }

                this.SafeState <- { this.SafeState with Seasons = seasons }
                do! state.WriteStateAsync()
            }

        member _.GetState () =
            task {
                return this.SafeState
            }
