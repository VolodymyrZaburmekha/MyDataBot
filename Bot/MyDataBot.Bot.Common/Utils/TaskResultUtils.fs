module MyDataBot.Bot.Common.Utils.TaskResultUtils

open System.Runtime.InteropServices.JavaScript
open System.Threading.Tasks
open FsToolkit.ErrorHandling

let onErrorDoTask (f: 'TError -> Task) (taskResult: TaskResult<'TResult, 'TError>) =
    task {
        let! result = taskResult

        match result with
        | Ok ok -> return Ok ok
        | Error e ->
            return!
                task {
                    do! f e
                    return Error e
                }
    }

let onErrorDo (f: 'TError -> unit) (taskResult: TaskResult<'TResult, 'TError>) =
    task {
        let! result = taskResult

        match result with
        | Ok ok -> return Ok ok
        | Error e ->
            f e
            return Error e
    }
