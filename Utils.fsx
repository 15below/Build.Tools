[<AutoOpen>]
module Utils

open Microsoft.FSharp.Collections

type Map<'Key,'Value when 'Key : comparison> with
    member this.get (name: 'Key) =
        this |> Map.find name