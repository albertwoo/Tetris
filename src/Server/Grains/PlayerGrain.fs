namespace Server.Grans

open System
open System.Collections.Generic
open Orleans
open Orleans.Runtime
open Orleans.Providers
open Orleans.EventSourcing
open Orleans.EventSourcing.CustomStorage
open FSharp.Control.Tasks
open CosmoStore
open Fun.Result

open Server.Common
open Server.Grains.Interfaces


[<LogConsistencyProvider(ProviderName = Constants.LiteDbLogStore)>]
type PlayerGrain
    (
        [<PersistentState("PlayerState", Constants.LiteDbStore)>] snapshot: IPersistentState<PlayerState>,
        [<PersistentState("PlayerState-Version", Constants.LiteDbStore)>] version: IPersistentState<int>
    ) as this =

    inherit JournaledGrain<PlayerState, PlayerEvent>()

    let eventStore: CosmoStore.EventStore<PlayerEvent, int64>  = 
        CosmoStore.LiteDb.EventStore.getEventStore
            { CosmoStore.LiteDb.Configuration.Empty with Name = "" }

    let streamId = "PlayerStream" + this.GetPrimaryKeyString()


    override _.TransitionState (state, evt) =
        snapshot.State <-
            match evt with
            | PlayerEvent.InitCredential p when String.IsNullOrEmpty(state.Password) -> { state with Password = p }
            | PlayerEvent.InitCredential _ -> state
            | PlayerEvent.NewRecord r ->
                match state.TopRecord with
                | Some t when t.Score < r.Score -> { state with TopRecord = Some r }
                | _ -> state


    interface ICustomStorageInterface<PlayerState, PlayerEvent> with
        member _.ReadStateFromStorage() =
            task {
                return KeyValuePair (version.State, snapshot.State)
            }

        member _.ApplyUpdatesToStorage (updates, expectedVersion) =
            task {
                try
                    do! snapshot.WriteStateAsync()
                    do!
                        updates
                        |> Seq.map (fun x ->
                            { Id = Guid.NewGuid()
                              CorrelationId = None 
                              CausationId = None
                              Name = ""
                              Data = x
                              Metadata = None }
                        )
                        |> Seq.toList
                        |> eventStore.AppendEvents streamId (ExpectedVersion.Exact (int64 expectedVersion))
                        |> Task.map ignore
                    return true
                with _ ->
                    return false
            }
       