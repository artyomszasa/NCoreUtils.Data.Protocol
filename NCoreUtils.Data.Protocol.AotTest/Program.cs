using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Data.Protocol;

internal class Program
{
    private static void Main(string[] args)
    {
        using var services = new ServiceCollection()
            .AddLogging(b => b.ClearProviders().SetMinimumLevel(LogLevel.Debug).AddSimpleConsole(o => o.SingleLine = true))
            .AddDataQueryServices(DataQueryContext.Singleton)
            .BuildServiceProvider(true);
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var builder = scope.ServiceProvider.GetRequiredService<IDataQueryExpressionBuilder>();
        var parser = scope.ServiceProvider.GetRequiredService<ExpressionParser>();
        foreach (var arg in args)
        {
            var expression = builder.BuildExpression(typeof(DataEntity), arg);
            // logger.LogInformation("Expression: {Expression}", expression);
            Console.WriteLine("----");
            Console.WriteLine(expression);
            var ast = parser.ParseExpression(expression);
            Console.WriteLine(ast);
        }
    }
}