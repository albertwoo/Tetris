module Tetris.Server.HttpHandlers.Player

open System
open Giraffe
open FSharp.Control.Tasks
open Orleans

open Tetris.Server.WebApi.Grain.Interfaces
open Tetris.Server.WebApi.Dtos.Game


let uploadRecord seasonId: HttpHandler =
    fun nxt ctx ->
        task {
            let! payload = ctx.BindJsonAsync<NewRecord>()

            if String.IsNullOrEmpty payload.PlayerName then
                return! RequestErrors.BAD_REQUEST "Player name cannot be empty" nxt ctx
            else
                let factory = ctx.GetService<IGrainFactory>()
                let player = factory.GetGrain<IPlayerGrain>(payload.PlayerName)
                let newRecord: Record =
                    { Id = 0
                      PlayerName = payload.PlayerName
                      Score = payload.Score
                      RecordDate = DateTime.Now
                      TimeCostInMs = payload.TimeCostInMs }
                let! id = player.AddRecord(payload.PlayerPassword, seasonId, newRecord, payload.GameEvents)
                match id with
                | Ok id   -> return! Successful.CREATED id nxt ctx
                | Error e -> return! RequestErrors.BAD_REQUEST e nxt ctx
        }


let getRecord  (playerName, recordId): HttpHandler =
    fun nxt ctx ->
        task {
            let factory = ctx.GetService<IGrainFactory>()
            let player = factory.GetGrain<IPlayerGrain>(playerName)
            let! record = player.GetRecord(recordId)
            match record with
            | Some x -> return! json x nxt ctx
            | None   -> return! RequestErrors.NOT_FOUND "" nxt ctx
        }


let getRecordEvents (playerName, recordId): HttpHandler =
    fun nxt ctx ->
        task {
            let factory = ctx.GetService<IGrainFactory>()
            let player = factory.GetGrain<IPlayerGrain>(playerName)
            let! record = player.GetRecordEvents(recordId)
            match record with
            | Some x -> return! text x nxt ctx
            | None   -> return! RequestErrors.NOT_FOUND "" nxt ctx
        }
