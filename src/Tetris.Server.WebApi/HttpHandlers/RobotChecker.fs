﻿module Tetris.Server.HttpHandlers.RobotChecker

open System
open Giraffe
open FSharp.Control.Tasks
open Orleans

open Tetris.Server.WebApi.Grain.Interfaces
open Tetris.Server.WebApi.Dtos


let getChecker: HttpHandler =
    fun nxt ctx ->
        task {
            let factory = ctx.GetService<IGrainFactory>()
            let id = Guid.NewGuid()
            let robotChecker = factory.GetGrain<IRobotCheckerGrain>(id)
            let! base64 = robotChecker.GetCheckerImage()
            let! expireDate = robotChecker.GetExpireDate()
            let data = { Id = id; Base64ImageSource = base64; ExpireDate = expireDate }
            return! json data nxt ctx
        }


let checkHeader: HttpHandler =
    fun nxt ctx ->
        task {
            let forbit x = RequestErrors.FORBIDDEN (sprintf "Robot check failed %s" x) nxt ctx
            
            if ctx.Request.Headers.ContainsKey RobotCheckerIdKey &&
               ctx.Request.Headers.ContainsKey RobotCheckerValueKey
            then
                match Guid.TryParse(ctx.Request.Headers.Item(RobotCheckerIdKey).ToString()),
                      System.Single.TryParse(ctx.Request.Headers.Item(RobotCheckerValueKey).ToString())
                    with
                    | (true, id), (true, value)->
                        let factory = ctx.GetService<IGrainFactory>()
                        let robotChecker = factory.GetGrain<IRobotCheckerGrain>(id)
                        let! result = robotChecker.Check value
                        match result with
                        | ValidateResult.Valid -> return! nxt ctx
                        | x -> return! forbit (string x)
                    | _ ->
                        return! forbit ""
            else
                return! forbit ""
        }
