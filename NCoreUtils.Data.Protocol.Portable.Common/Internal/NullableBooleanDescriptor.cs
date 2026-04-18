using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(bool?))]
public sealed partial class NullableBooleanDescriptor : ITypeDescriptor
{
    object ITypeDescriptor.Parse(string value)
        => Parse(value)!;

    public IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<bool?, bool>>)(e => e.HasValue)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<bool?, bool>>)(e => e!.Value)).Body).Member
    };

    public bool IsArithmetic => true;

    public bool IsEnum => false;

    public bool IsValue => true;

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(bool?));

    public static bool? Parse(string value)
        => string.IsNullOrEmpty(value)
            ? default(bool?)
            : bool.Parse(value);

    public string Stringify(object? value) => value switch
    {
        null => default!,
        bool b => b.ToString(CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to boolean.")
    };

    public Expression CreateNotEqual(Expression self, Expression right)
        => right.Type == typeof(bool?)
            ? Expression.NotEqual(self, right)
            : right.Type == typeof(bool)
                ? Expression.NotEqual(self, Expression.Convert(right, typeof(bool?)))
                : throw new InvalidOperationException($"Cannot create NotEqual expression from bool? and {right.Type}.");

    public Expression CreateGreaterThan(Expression self, Expression right)
        => right.Type == typeof(bool?)
            ? Expression.GreaterThan(self, right)
            : right.Type == typeof(bool)
                ? Expression.GreaterThan(self, Expression.Convert(right, typeof(bool?)))
                : throw new InvalidOperationException($"Cannot create GreaterThan expression from bool? and {right.Type}.");

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(bool?)
            ? Expression.GreaterThanOrEqual(self, right)
            : right.Type == typeof(bool)
                ? Expression.GreaterThanOrEqual(self, Expression.Convert(right, typeof(bool?)))
                : throw new InvalidOperationException($"Cannot create GreaterThanOrEqual expression from bool? and {right.Type}.");

    public Expression CreateLessThan(Expression self, Expression right)
        => right.Type == typeof(bool?)
            ? Expression.LessThan(self, right)
            : right.Type == typeof(bool)
                ? Expression.LessThan(self, Expression.Convert(right, typeof(bool?)))
                : throw new InvalidOperationException($"Cannot create LessThan expression from bool? and {right.Type}.");

    public Expression CreateLessThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(bool?)
            ? Expression.LessThanOrEqual(self, right)
            : right.Type == typeof(bool)
                ? Expression.LessThanOrEqual(self, Expression.Convert(right, typeof(bool?)))
                : throw new InvalidOperationException($"Cannot create LessThanOrEqual expression from bool? and {right.Type}.");

    public Expression CreateAdd(Expression self, Expression right)
        => Expression.Add(self, right);

    public Expression CreateSubtract(Expression self, Expression right)
        => Expression.Subtract(self, right);
}