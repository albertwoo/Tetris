namespace Server.Grans

open Orleans
open Orleans.Runtime
open FSharp.Control.Tasks
open Microsoft.Extensions.Configuration
open LiteDB
open LiteDB.FSharp
open Fun.Result

open Tetris.Server.WebApi.Common
open Tetris.Server.WebApi.Grain.Interfaces


type PlayerGrain
    (
        [<PersistentState("PlayerState", Constants.LiteDbStore)>]
        state: IPersistentState<PlayerState>,
        grainFactory: IGrainFactory,
        configuration: IConfiguration
    ) as this =

    inherit Grain()

    // We may have a lot of records, so we did not keep it in the PlayerState directly
    // Save records history for future usage
    let db = new LiteDatabase(configuration.GetConnectionString(Constants.AppDbConnectionName), FSharpBsonMapper())

    member _.SafeState 
        with get() =
            if box state.State |> isNull then PlayerState.defaultState
            else state.State
        and set s =
            state.State <- s

    interface IPlayerGrain with
        member _.InitCredential password =
            task {
                match this.SafeState.Password with
                | SafeString _ -> return ()
                | NullOrEmptyString ->
                    this.SafeState <- { this.SafeState with Password = password }
            }

        member _.AddRecord (password, record) =
            task {
                do! (this :> IPlayerGrain).InitCredential(password)
                if this.SafeState.Password = password then
                    let records = db.GetCollection<Record>()
                    let id = records.Insert { record with PlayerName = this.GetPrimaryKeyString() }

                    let gameBoard = grainFactory.GetGrain<IGameBoardGrain>(int64 Constants.GameZone1)
                    do! gameBoard.AddRecord { record with Id = id.AsInt32 }
                    do! state.WriteStateAsync()
                    return Ok id.AsInt32

                else
                    return Error AddRecordError.PasswordMissMatch
            }

        member _.GetRecord (recordId) = 
            task {
                let records = db.GetCollection<Record>()
                let record = records.FindById(BsonValue(recordId))
                return 
                    if box record |> isNull then None
                    else Some record
            }
       