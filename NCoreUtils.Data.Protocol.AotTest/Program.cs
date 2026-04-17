using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Data.Protocol;

internal class Program
{
    private sealed class DummyDisposable : IDisposable
    {
        public static DummyDisposable Singleton { get; } = new();

        public void Dispose() { /* noop */ }
    }

    private sealed class DummyLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => DummyDisposable.Singleton;

        public bool IsEnabled(LogLevel logLevel)
            => false;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        { /* noop */ }
    }

    private static void Main(string[] args)
    {
        var desc = DataQueryContext.Singleton.GetTypeDescriptors().FirstOrDefault(e => e.Type == typeof(DataEntity));

        using var services = new ServiceCollection()
            .AddSingleton(typeof(ILogger<>), typeof(DummyLogger<>))
            .AddDataQueryServices(DataQueryContext.Singleton)
            .BuildServiceProvider(true);
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var builder = scope.ServiceProvider.GetRequiredService<IDataQueryExpressionBuilder>();
        var parser = scope.ServiceProvider.GetRequiredService<ExpressionParser>();
        if (args.Length == 0)
        {
            var expression = builder.BuildExpression(typeof(DataEntity), "e => some(e.Meta, x => x = \"hu-HU\")");
            // logger.LogInformation("Expression: {Expression}", expression);
            Console.WriteLine("----");
            Console.WriteLine(expression);
            var ast = parser.ParseExpression(expression);
            Console.WriteLine(ast);
        }
        else
        {
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
}