module Server.Startup

open System
open System.IO
open Microsoft.Net.Http.Headers
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Primitives
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Fun.Result


let publicPath = 
    #if DEBUG
    Path.GetFullPath "../Client.Web/deploy"
    #else
    "wwwroot"
    #endif


let addServices (config: IConfiguration) (services: IServiceCollection) =
    services
        .AddCors()
        .AddGiraffe()
        .AddResponseCaching()
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
            )
        )
        .UseGiraffe Routes.all


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

[<EntryPoint>]
let main args =
    let config = buildConfiguration args

    try            
        Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun wb ->
                wb.CaptureStartupErrors(true)
                  .UseConfiguration(config)
                  .UseWebRoot(publicPath)
                  .Configure(Action<IApplicationBuilder> configureApp)
                  .ConfigureServices(addServices config)
                |> ignore
            )
            .Build()
            .Run()
    with ex ->
        raise ex

    1
