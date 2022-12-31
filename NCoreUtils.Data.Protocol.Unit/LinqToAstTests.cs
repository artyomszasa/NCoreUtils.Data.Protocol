using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCoreUtils.Data.Protocol.Internal;
using Xunit;

namespace NCoreUtils.Data.Protocol.Unit;

public class LinqToAstTests
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

    public sealed class ArrayOfCases : IEnumerable<object[]>
    {
        private static readonly int[] _arraySeeds = new int[] { 2, 3, 4 };

        private static readonly List<int> _listSeeds = new() { 2, 3, 4 };

        private static readonly HashSet<int> _setSeeds = new() { 2, 3, 4 };

        private static readonly IEnumerable<int> _enumerableSeeds = Enumerable.Range(2, 3);

        private readonly IReadOnlyList<object[]> _cases = new object[][]
        {
            new object[] {
                (Expression<Func<Item, bool>>)(e => Enumerable.Contains(_arraySeeds, e.Num)),
                "a => includes(array(2, 3, 4), a.Num)"
            },
            new object[] {
                (Expression<Func<Item, bool>>)(e => Enumerable.Contains(_listSeeds, e.Num)),
                "a => includes(array(2, 3, 4), a.Num)"
            },
            new object[] {
                (Expression<Func<Item, bool>>)(e => Enumerable.Contains(_setSeeds, e.Num)),
                "a => includes(array(2, 3, 4), a.Num)"
            },
            new object[] {
                (Expression<Func<Item, bool>>)(e => Enumerable.Contains(_enumerableSeeds, e.Num)),
                "a => includes(array(2, 3, 4), a.Num)"
            }
        };

        public IEnumerator<object[]> GetEnumerator()
            => _cases.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private static ServiceProviderAndScope CreateExpressionParser(IDataUtils util, out ExpressionParser expressionParser)
    {
        var serviceProvider = new ServiceCollection()
            .AddTransient(typeof(ILogger<>), typeof(DummyLogger<>))
            .AddCommonDataQueryClientServices()
            .AddSingleton(util)
            .BuildServiceProvider(true);
        var scope = serviceProvider.CreateScope();
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
    [ClassData(typeof(ArrayOfCases))]
    public void ArrayOfTests(Expression<Func<Item, bool>> expr, string expected) => RunPortableAndReflection(util =>
    {
        using var _ = CreateExpressionParser(util, out var parser);
        var ast = parser.ParseExpression(expr);
        var actual = ast.ToString();
        Assert.Equal(expected, actual);
    });
}