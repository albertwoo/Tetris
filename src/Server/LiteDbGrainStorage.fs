namespace Server

open System.Threading.Tasks
open FSharp.Control.Tasks
open Microsoft.Extensions.DependencyInjection
open Orleans
open Orleans.Storage
open Orleans.Runtime
open Orleans.Hosting
open LiteDB
open Server.Common.Json


[<CLIMutable>]
type GrainData =
    { Id: string
      Value: string }


type LiteDbGrainStorage (liteDbPath: string) =
    let createDb() = new LiteDatabase(liteDbPath)

    let getGrains (db: LiteDatabase) = db.GetCollection<GrainData>("Grains")
    let createGrainName grainType (ref: GrainReference) = sprintf "%s-%s" grainType (ref.ToKeyString())

    interface IGrainStorage with
        member _.ClearStateAsync(grainType: string, grainReference: GrainReference, grainState: IGrainState): Task = 
            task {
                use db = createDb()
                let grains = getGrains db
                grains.Delete(BsonValue(createGrainName grainType grainReference)) |> ignore
            } :> Task

        member _.WriteStateAsync(grainType: string, grainReference: GrainReference, grainState: IGrainState): Task = 
            task {
                use db = createDb()
                let grains = getGrains db
                let id = createGrainName grainType grainReference
                let newValue = { Id = id; Value = toJson grainState.State }
                if box (grains.FindById(BsonValue id)) = null then
                    grains.Insert(newValue) |> ignore
                else
                    grains.Update(newValue) |> ignore
            } :> Task

        member _.ReadStateAsync(grainType: string, grainReference: GrainReference, grainState: IGrainState): Task =
            task {
                use db = createDb()
                let grains = getGrains db
                let value = grains.FindById(BsonValue(createGrainName grainType grainReference))
                grainState.State <- 
                    if box value = null then null
                    else fromJson grainState.Type value.Value
            } :> Task


[<AutoOpen>]
module Extensions =
    type IServiceCollection with 
        member this.AddLiteDbGrainStorage(storageName, liteDbPath) =
            this.AddSingletonNamedService<IGrainStorage>(storageName, fun _ _ -> LiteDbGrainStorage(liteDbPath) :> IGrainStorage)

    type ISiloBuilder with
        member this.AddLiteDbGrainStorage(storageName, liteDbPath) =
            this.ConfigureServices(fun services -> 
                services.AddLiteDbGrainStorage(storageName, liteDbPath) 
                |> ignore
            )
