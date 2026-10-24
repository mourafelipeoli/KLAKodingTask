using KodingTask.Api.Contracts;
using KodingTask.Api.Translations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KodingTask.Api.Hosting;

/// <summary>Every refusal keeps its code and gets its message in the visitor's language.</summary>
public sealed class TranslateErrorMessages(ServerTexts texts) : IAsyncResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: ErrorResponse error } result)
            result.Value = Translate(error, context.HttpContext, texts);
        return next();
    }

    public static ErrorResponse Translate(ErrorResponse error, HttpContext context, ServerTexts texts) =>
        error with { Message = texts.Translate(error.Message, LocaleResolver.Resolve(context)) };
}
