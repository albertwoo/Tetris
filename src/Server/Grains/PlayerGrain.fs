namespace Server.Grans

open System.Threading.Tasks
open Orleans
open Server.Grains.Interfaces


type PlayerGrain() =
    inherit Grain<PlayerState>()
        
    interface IPlayerGrain with
        member this.AddRecord record = 
            this.State <- 
                { this.State with
                    Records = record::this.State.Records }
            Task.FromResult ()

        member this.AddCredential x =
            this.State <-
                { this.State with
                    NickName = x.NickName
                    Password = x.Password }
            Task.FromResult ()
