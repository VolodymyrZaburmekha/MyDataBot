module MyDataBot.Bot.Nats.Nats

open System
open System.Collections.Generic
open System.Text
open System.Text.Json
open System.Threading.Tasks
open NATS.Client
open NATS.Client.JetStream

module NatsEncoding =
    let encode (str: string) = Encoding.UTF8.GetBytes(str)

    let decode (bytes: byte[]) = Encoding.UTF8.GetString(bytes)

    let createStrMsg subject str = Msg(subject, str |> encode)

    let createObjectEncodedMsg subject object =
        Msg(subject, object |> JsonSerializer.Serialize |> encode)

let createConnection (url: string option) =
    ConnectionFactory()
    |> fun cf ->
        match url with
        | Some urlValue -> cf.CreateConnection(urlValue)
        | None -> cf.CreateConnection()

let createStream (connection: IConnection) streamName (subjects: string list) =
    let streamConfig =
        StreamConfiguration
            .Builder()
            .WithName(streamName)
            .AddSubjects(List<_>(subjects))
            .WithStorageType(StorageType.Memory)
            .Build()

    let management = connection.CreateJetStreamManagementContext()
    management.AddStream(streamConfig)

let createSubscriber
    (handlerFunc: 'Tdata -> Task<bool>)
    (errorHandler: Exception -> Task<bool>)
    (stream: IJetStream)
    subject
    queueName
    durableName
    =
    let config =
        PushSubscribeOptions.Builder()
        |> fun b ->
            match durableName with
            | Some durableValue -> b.WithDurable(durableValue)
            | None -> b
        |> fun b -> b.Build()

    let handler _ (msg: MsgHandlerEventArgs) =
        let _ =
            task {
                try
                    msg.Message.InProgress()
                    let dataStr = NatsEncoding.decode msg.Message.Data

                    let data =
                        JsonSerializer.Deserialize<'Tdata>(
                            dataStr,
                            JsonSerializerOptions(PropertyNameCaseInsensitive = true)
                        )

                    let! handled = handlerFunc data

                    if handled then msg.Message.Ack() else msg.Message.Nak()
                with :? Exception as ex ->
                    let! errorHandled = errorHandler ex

                    if errorHandled then
                        msg.Message.Ack()
                    else
                        msg.Message.Nak()

            }

        ()
        msg.Message.Ack()

    stream.PushSubscribeAsync(subject, queueName, handler, false, config)
