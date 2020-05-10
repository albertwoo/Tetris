module Tetris.Server.Handlers.Translations

open System
open System.IO
open Giraffe
open FSharp.Control.Tasks
open Fun.Result


let getTranslations lang: HttpHandler =
    fun nxt ctx ->
        task {
            let lines = File.ReadAllLines("Translation.lang")
            let index = 
                lines
                |> Seq.tryHead
                |> Option.bind (fun head ->
                    head.Split ','
                    |> Seq.tryFindIndex (fun x -> x.Equals(lang, StringComparison.OrdinalIgnoreCase)))
            match index with
            | None -> return! RequestErrors.NOT_FOUND "" nxt ctx
            | Some index ->
                let translations =
                    lines
                    |> Seq.skip 1
                    |> Seq.choose (fun line ->
                        let columns = line.Split ','
                        option {
                            let! key = columns |> Seq.tryItem 0
                            let! text = columns |> Seq.tryItem index
                            return key, text
                        })
                    |> Map.ofSeq
                return! json translations nxt ctx

        }