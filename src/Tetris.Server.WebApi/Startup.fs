module Tetris.Server.Startup

open System
open System.IO
open System.Net
open Microsoft.Net.Http.Headers
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Primitives
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Orleans
open Orleans.Hosting
open Orleans.Configuration
open Fun.Result
open Tetris.Server.WebApi.Common


let publicPath = 
    #if DEBUG
    Path.GetFullPath "../Tetris.Client.Web/deploy"
    #else
    "wwwroot"
    #endif


let buildConfiguration (args: string[]) =
    let env =
        match Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") with
        | SafeString x -> x
        | NullOrEmptyString -> "Production"

    ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
        .AddJsonFile(sprintf "appsettings.%s.json" env, optional = true)
        .AddCommandLine(args)
        .AddEnvironmentVariables()
        .Build()


let addServices (services: IServiceCollection) =
    services
        .AddCors()
        .AddGiraffe()
        .AddResponseCaching()
        .AddSingleton<Serialization.Json.IJsonSerializer>(Thoth.Json.Giraffe.ThothSerializer())
        .AddResponseCompression()
    |> ignore
    

let configureApp (app: IApplicationBuilder) =
    app
        .UseCors(fun builder -> builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin() |> ignore)
        .UseResponseCaching()
        .UseResponseCompression()
        .UseDefaultFiles() // comment this line for disable it for SSR support
        .UseStaticFiles(
            StaticFileOptions(
                OnPrepareResponse = fun ctx ->
                    ctx.Context.Response.Headers.[HeaderNames.CacheControl] <- StringValues (sprintf "public,max-age=%d" (60*60*24*30))
            ))
        .UseGiraffe Routes.all


let setOrleans (config: IConfiguration) (siloBuilder: ISiloBuilder) =
    siloBuilder
        .AddMemoryGrainStorageAsDefault()
        .AddLiteDbGrainStorage(Constants.LiteDbStore, config.GetConnectionString(Constants.AppDbConnectionName))
        .UseDashboard(fun opt -> 
            opt.Port <- config.GetValue<int>("App:OrleansDashboard:Port")
            opt.Username <- config.GetValue("App:OrleansDashboard:Username")
            opt.Password <- config.GetValue("App:OrleansDashboard:Password"))
        .UseLocalhostClustering()
        .Configure(fun (opts: ClusterOptions) ->
            opts.ClusterId <- "tetris-dev"
            opts.ServiceId <- "tetris-game")
        .Configure(fun (opts: EndpointOptions) -> opts.AdvertisedIPAddress <- IPAddress.Loopback)
        .ConfigureApplicationParts(fun parts ->
            parts
                .AddFromAppDomain()
                .WithReferences()
                .WithCodeGeneration()
            |> ignore)
    |> ignore


[<EntryPoint>]
let main args =
    let config = buildConfiguration args

    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun wb ->
            wb.CaptureStartupErrors(true)
                .UseConfiguration(config)
                .UseWebRoot(publicPath)
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(addServices)
            |> ignore)
        .UseOrleans(setOrleans config)
        .Build()
        .Run()

    1
