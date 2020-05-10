module Tetris.Server.Handlers.RobotChecker

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
            let fobit = RequestErrors.FORBIDDEN "Robot check failed" nxt ctx
            
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
                        if result then return! nxt ctx
                        else return! fobit
                    | _ ->
                        return! fobit
            else
                return! fobit
        }
