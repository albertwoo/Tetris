namespace rec Tetris.Server.WebApi.Grain.Interfaces

open System
open System.Threading.Tasks
open Orleans


type IRobotCheckerGrain =
    abstract member Check: float32 -> Task<bool>
    abstract member GetCheckerImage: unit -> Task<Base64Image>
    abstract member GetExpireDate: unit -> Task<DateTime>
    inherit IGrainWithGuidKey


type Base64Image = string
