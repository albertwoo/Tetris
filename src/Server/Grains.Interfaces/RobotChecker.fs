namespace rec Server.Grains.Interfaces

open System.Threading.Tasks
open Orleans


type IRobotCheckerGrain =
    abstract member Check: float32 -> Task<bool>
    abstract member GetCheckerImage: unit -> Task<Base64Image>
    inherit IGrainWithGuidKey


type Base64Image = string
