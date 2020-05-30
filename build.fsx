#r "paket:
nuget FSharp.Core
nuget Fake.Core.ReleaseNotes
nuget Fake.Core.Target
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.IO.Zip //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open System

open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators


Target.initEnvironment ()


let [<Literal>] TranslationFile = "Translation.lang"


let serverPath          = __SOURCE_DIRECTORY__ </> "src/Tetris.Server.WebApi"
let clientWebPath       = __SOURCE_DIRECTORY__ </> "src/Tetris.Client.Web"

let deployDir           = __SOURCE_DIRECTORY__ </> "deploy"
let publishDir          = deployDir </> "publish"
let clientDeployPath    = clientWebPath </> "deploy"


[<AutoOpen>]
module Utils =
    let platformTool tool winTool =
        let tool = if Environment.isUnix then tool else winTool
        match ProcessUtils.tryFindFileOnPath tool with
        | Some t -> t
        | _ -> failwith (tool + " was not found in path. ")


    let runTool cmd args workingDir =
        let arguments = args |> String.split ' ' |> Arguments.OfArgs
        Command.RawCommand (cmd, arguments)
        |> CreateProcess.fromCommand
        |> CreateProcess.withWorkingDirectory workingDir
        |> CreateProcess.ensureExitCode
        |> Proc.run
        |> ignore


    let node   = runTool (platformTool "node" "node.exe")
    let yarn   = runTool (platformTool "yarn" "yarn.cmd")
    let dotnet = runTool (platformTool "dotnet" "dotnet.exe")


    let openBrowser url =
        Command.ShellCommand url
        |> CreateProcess.fromCommand
        |> CreateProcess.ensureExitCodeWithMessage "opening browser failed"
        |> Proc.run
        |> ignore


    let clearDeployFolder targetDir =
        !! (targetDir </> "*/FSharp.Core.resources.dll")
        |> Seq.map Path.getDirectory
        |> Shell.deleteDirs

        !! (targetDir </> "*.pdb")
        ++ (targetDir </> "**/*.js.map")
        ++ (targetDir </> "**/*.css.map")
        |> Seq.iter Shell.rm_rf


    let translateI18n rootDir targetFile =
        let head = "Label,CN,EN"
        let readFile file =
            printfn "Processing translation file %s" file
            let rows = File.read file 
            rows
            |> Seq.tryHead
            |> function
                | Some x when x.Trim() = head -> rows |> Seq.skip 1
                | Some _ -> failwithf "Translation file(%s) head is not correct" file
                | None -> [] |> unbox<string seq>
        !!(rootDir </> "**/*.i18n")
        |> Seq.map readFile
        |> Seq.concat
        |> Seq.sort
        |> fun rows -> [ head; yield! rows ]
        |> File.write false targetFile


Target.create "Clean" <| fun _ ->
    [ publishDir
      clientDeployPath ]
    |> Shell.cleanDirs


Target.create "InstallPackages" <| fun _ ->
    printfn "Node version:"
    node "--version" clientWebPath
    printfn "Npm version:"
    yarn "--version" clientWebPath
    yarn "install" clientWebPath


Target.create "PrepareAssets" <| fun _ ->
    [
        clientWebPath
    ]
    |> List.iter (fun p -> Shell.copyDir clientDeployPath (p </> "public") FileFilter.allFiles)


Target.create "Translate" <| fun _ ->
    translateI18n "src" (publishDir </> TranslationFile)
    Shell.copyFile (serverPath </> TranslationFile) (publishDir </> TranslationFile)


Target.create "BuildClient" <| fun _ ->
    yarn "webpack -p" clientWebPath

Target.create "BuildServer" <| fun _ ->
    dotnet "build" serverPath


Target.create "RunClientWeb" <| fun _ ->
    let buildTailwind() = yarn "tailwind build ../Tetris.Client.Web/public/css/tailwind-source.css -o ../Tetris.Client.Web/public/css/tailwind-generated.css -c ../Tetris.Client.Web/tailwind.config.js" clientWebPath
    let buildTranslation() = translateI18n "src" (serverPath </> TranslationFile)
    [
        async {
            use _ = 
                ChangeWatcher.run 
                    (fun _ -> printfn "Rebuild tailwind..."; buildTailwind()) 
                    (!!(clientWebPath </> "public/css/tailwind-source.css")
                     ++(clientWebPath </> "tailwind.config.js"))
            buildTailwind()
            use _ =
                ChangeWatcher.run
                    (fun _ -> printfn "Rebuild translation file"; buildTranslation())
                    (!!("src" </> "**/*.i18n"))
            buildTranslation()
            yarn "webpack-dev-server" clientWebPath
        }
        async {
            do! Async.Sleep 10000
            openBrowser "http://localhost:8080"
        } 
    ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore


Target.create "RunServer" <| fun _ ->
    async {
        dotnet "watch run" serverPath
    }
    |> Async.RunSynchronously


Target.create "Test" <| fun _ ->
    dotnet "test /p:CollectCoverage=true" __SOURCE_DIRECTORY__


Target.create "Bundle" <| fun _ ->
    let publishArgs = sprintf "publish -c Release -o %s" publishDir
    dotnet publishArgs serverPath

    let clientDir = publishDir </> "wwwroot"
    Shell.copyDir clientDir clientDeployPath FileFilter.allFiles

    clearDeployFolder publishDir

    !!(publishDir </> "**/*.*")
    |> Zip.zip publishDir (deployDir </> DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".zip")


open Fake.Core.TargetOperators

"Clean"
    ==> "InstallPackages"
    ==> "Translate"
    ==> "PrepareAssets"
    ==> "BuildClient"
    // ==> "BuildServer"
    // ==> "Test"
    ==> "Bundle"

"Clean"
    ==> "PrepareAssets"
    ==> "RunClientWeb"

"Clean"
    ==> "Translate"
    ==> "RunServer"


Target.runOrDefaultWithArguments "Build"
