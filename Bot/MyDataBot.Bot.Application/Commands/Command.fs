namespace MyDataBot.Bot.Application.Commands

open System
open MediatR
open MyDataBot.Bot.Common.ValueObjects.Ids

type Command<'data, 'response> =
    { CorrelationId: CorrelationId
      DateTimeUtc: DateTime
      Data: 'data }

    interface IRequest<'response>

module Command =
    let createFrom (c: Command<_, _>) (d: 'Tdata) : Command<'Tdata, _> =
        { CorrelationId = c.CorrelationId
          DateTimeUtc = c.DateTimeUtc
          Data = d }
