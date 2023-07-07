namespace MyDataBot.Bot.Common.ValueObjects.Ids

open System

type CorrelationId =
    | CorrelationId of Ulid

    member this.Value =
        match this with
        | CorrelationId v -> v

    member this.ValueStr = this.Value.ToString()
    static member fromStr(ulidStr: string) = CorrelationId(Ulid.Parse(ulidStr))
    