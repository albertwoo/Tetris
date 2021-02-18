module Tetris.Server.HttpHandlers.GameBoard

open Giraffe
open FSharp.Control.Tasks
open Orleans

open Tetris.Server.WebApi.Common
open Tetris.Server.WebApi.Grain.Interfaces
open Tetris.Server.WebApi.Dtos
open Tetris.Server.WebApi.Dtos.Game


let updateOnline clientId: HttpHandler =
    fun nxt ctx ->
        task {
            let factory = ctx.GetService<IGrainFactory>()
            let gamebord = factory.GetGrain<IGameBoardGrain>(int64 Constants.GameZone1)
            do! gamebord.Ping clientId
            return! text "pong" nxt ctx
        }


let getGameBoardInfo: HttpHandler =
    fun nxt ctx ->
        task {
            let factory = ctx.GetService<IGrainFactory>()
            let gamebord = factory.GetGrain<IGameBoardGrain>(int64 Constants.GameZone1)
            let! state = gamebord.GetState()
            let board: GameBoard =
                { OnlineCount = state.OnlineIPs.Count
                  Seasons = 
                    state.Seasons
                    |> Option.defaultValue Map.empty
                    |> Map.toList
                    |> List.map (fun (i, s) ->
                        { Id = i
                          StartTime = s.StartTime
                          Width = s.Width
                          Height = s.Height
                          Ranks =
                            s.Ranks
                            |> List.map (fun x -> 
                                { Id = x.Id
                                  PlayerName = x.PlayerName
                                  Score = x.Score
                                  RecordDate = x.RecordDate; 
                                  TimeCostInMs = x.TimeCostInMs }) })
                    |> List.sortByDescending (fun x -> x.Id) }
            return! json board  nxt ctx
        }

