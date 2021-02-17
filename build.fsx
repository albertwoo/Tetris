#r "nuget: Fake.Core.Process,5.20.0"
#r "nuget: Fake.IO.FileSystem,5.20.0"
#r "nuget: Fake.IO.Zip,5.20.0"
#r "nuget: BlackFox.Fake.BuildTask,0.1.3"

open System
open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open BlackFox.Fake


fsi.CommandLineArgs
|> Array.skip 1
|> BuildTask.setupContextFromArgv 


type Env = PROD | DEV


let [<Literal>] TranslationFile = "Translation.lang"


let serverPath          = __SOURCE_DIRECTORY__ </> "src/Tetris.Server.WebApi"
let clientWebPath       = __SOURCE_DIRECTORY__ </> "src/Tetris.Client.Web"

let deployDir           = __SOURCE_DIRECTORY__ </> "deploy"
let publishDir          = deployDir </> "publish"
let distProd            = "www" </> ".dist_prod"


[<AutoOpen>]
module Utils =
    let platformTool tool winTool =
        let tool = if Environment.isUnix then tool else winTool
        match ProcessUtils.tryFindFileOnPath tool with
        | Some t -> t
        | _ -> failwith (tool + " was not found in path. ")

    let run cmd args workingDir =
        let arguments = args |> String.split ' ' |> Arguments.OfArgs
        Command.RawCommand (cmd, arguments)
        |> CreateProcess.fromCommand
        |> CreateProcess.withWorkingDirectory workingDir
        |> CreateProcess.ensureExitCode
        |> Proc.run
        |> ignore

    let yarn   = run (platformTool "yarn" "yarn.cmd")
    let dotnet = run (platformTool "dotnet" "dotnet.exe") 

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

        
    // It will generate all source code for the target project in the dir and put the js under fablejs
    let runFable dir env watch =
        let mode = match watch with false -> "" | true -> " watch"
        let define = match env with PROD -> "" | DEV -> " --define DEBUG"
        dotnet (sprintf "fable%s . --outDir ./www/fablejs%s" mode define) dir

    let cleanGeneratedJs dir = Shell.cleanDir (dir </> "www/fablejs")

    let buildTailwindCss dir =
        printfn "Build client css"
        yarn "tailwindcss build css/app.css -o css/tailwind-generated.css" (dir </> "www")

    let serveDevJs dir =
        Shell.cleanDir (dir </> "www/.dist")
        yarn "parcel index.html --dist-dir .dist" (dir </> "www")

    let runBundle dir =
        Shell.cleanDir (dir </> distProd)
        yarn "parcel build index.html --dist-dir .dist_prod --public-url ./ --no-source-maps --no-cache" (dir </> "www")


let checkEnv =
    BuildTask.create "Check environment" [] {
        Shell.cleanDir publishDir

        yarn "--version" ""
        yarn "install" (clientWebPath </> "www")
        dotnet "tool restore" ""
    }

let translate =
    BuildTask.create "Translate" [] {
        translateI18n "src" (publishDir </> TranslationFile)
        Shell.copyFile (serverPath </> TranslationFile) (publishDir </> TranslationFile)
    }

let preBuildClient =
    BuildTask.create "PreBuildClient" [ translate; checkEnv ] {
        cleanGeneratedJs clientWebPath
        buildTailwindCss clientWebPath
    }

let runClientWeb =
    BuildTask.create "RunClientWeb" [ preBuildClient ] {
        runFable clientWebPath DEV false
        [
            async {
                runFable clientWebPath DEV true
            }
            async {
                serveDevJs clientWebPath
                printfn "Clean up..."
            }
        ]
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore
    }


let bundleClientProd =
    BuildTask.create "BundleClientProd" [ preBuildClient ] {
        Shell.cleanDir (clientWebPath </> distProd)
        runFable clientWebPath PROD false
        runBundle clientWebPath
    }


let buildServer =
    BuildTask.create "BuildServer" [ translate ] {
        dotnet "build" serverPath
    }


let test =
    BuildTask.create "Test" [] {
        dotnet "test /p:CollectCoverage=true" __SOURCE_DIRECTORY__
    }


let bundle =
    BuildTask.create "Bundle" [ checkEnv; bundleClientProd; buildServer ] {
        let publishArgs = sprintf "publish -c Release -o %s" publishDir
        dotnet publishArgs serverPath

        let clientDir = publishDir </> "www"
        Shell.copyDir clientDir (clientWebPath </> distProd) FileFilter.allFiles

        clearDeployFolder publishDir

        !!(publishDir </> "**/*.*")
        |> Zip.zip publishDir (deployDir </> DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".zip")
    }


BuildTask.runOrDefault bundle
