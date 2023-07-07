using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace MyDataBot.Bot.Data.Mappers;

public static class BaseFSharpTypesMapper
{
    public static FSharpOption<T> ToFsharpOption<T>(this T? val) where T : struct
        => val is { } notNullValue ? FSharpOption<T>.Some(notNullValue) : FSharpOption<T>.None;

    public static FSharpOption<T> ToFsharpOption<T>(this T? obj) where T : class
        => obj is { } notNullObj ? FSharpOption<T>.Some(notNullObj) : FSharpOption<T>.None;


    public static FSharpList<T> ToFsharpList<T>(this IEnumerable<T> elements) => ListModule.OfSeq(elements);
}