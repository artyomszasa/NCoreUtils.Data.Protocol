using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCoreUtils.Data.Protocol.Internal;
using Xunit;

namespace NCoreUtils.Data.Protocol.Unit;

public class ReparseTests
{
    private class ServiceProviderAndScope : IDisposable
    {
        public ServiceProvider ServiceProvider { get; }

        public IServiceScope Scope { get; }

        public ServiceProviderAndScope(ServiceProvider serviceProvider, IServiceScope scope)
        {
            ServiceProvider = serviceProvider;
            Scope = scope;
        }

        public void Dispose()
        {
            Scope.Dispose();
            ServiceProvider.Dispose();
        }
    }

    private static ServiceProviderAndScope CreateExpressionBuilderAndParser(IDataUtils util, out DefaultDataQueryExpressionBuilder expressionBuilder, out ExpressionParser expressionParser)
    {
        var serviceProvider = new ServiceCollection()
            .AddTransient(typeof(ILogger<>), typeof(DummyLogger<>))
            .AddCommonDataQueryClientServices()
            .AddCommonDataQueryServerServices()
            .AddSingleton(util)
            .BuildServiceProvider(true);
        var scope = serviceProvider.CreateScope();
        expressionBuilder = (DefaultDataQueryExpressionBuilder)scope.ServiceProvider.GetRequiredService<IDataQueryExpressionBuilder>();
        expressionParser = scope.ServiceProvider.GetRequiredService<ExpressionParser>();
        return new(serviceProvider, scope);
    }

    private static void RunPortableAndReflection(Action<IDataUtils> action)
    {
        {
            var util = new PortableDataUtils(GeneratedContext.Singleton);
            action(util);
        }
        {
            var util = new ReflectionDataUtils();
            action(util);
        }
    }

    [Theory]
    [InlineData("e => e <= dateTimeOffset(637767216000000000)")]
    public void DateTimeOffsetTests(string raw) => RunPortableAndReflection(util =>
    {
        using var _ = CreateExpressionBuilderAndParser(util, out var ebuilder, out var eparser);
        var expression = ebuilder.BuildExpression(typeof(DateTimeOffset), raw, out var ast);
        var actual = eparser.ParseExpression(expression);
        Assert.Equal(ast, actual);
    });

    [Theory]
    [InlineData(typeof(int), "e => includes(array(1,2,3), e)")]
    [InlineData(typeof(string), "e => includes(array(\"1\",\"2\",\"3\"), e)")]
    [InlineData(typeof(AOrB), "e => includes(array(\"A\",\"B\"), e)")]
    [InlineData(typeof(Item), "e => includes(array(1,2,3), e.Num) && e.Str = \"xxx\"")]
    public void ArrayOfTests([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type itemType, string raw) => RunPortableAndReflection(util =>
    {
        using var _ = CreateExpressionBuilderAndParser(util, out var ebuilder, out var eparser);
        var expression = ebuilder.BuildExpression(itemType, raw, out var ast);
        var actual = eparser.ParseExpression(expression);
        Assert.Equal(ast, actual);
    });
}