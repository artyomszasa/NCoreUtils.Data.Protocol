using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace NCoreUtils.Data.Protocol.TypeInference;

public partial record TypeConstraints(
    ImmutableHashSet<CaseInsensitive> Members,
    ImmutableHashSet<Type> Interfaces,
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    [param: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    Type? Base,
    bool? IsNumeric,
    bool? IsNullable,
    bool? IsLambda,
    ImmutableList<(TypeUid TypeUid, string MemberName)> MemberOf
);

public partial record TypeConstraints
{
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append('{');
        var first = true;
        if (Base is not null)
        {
            builder.Append(':')
                .Append(Base.Name);
        }
        if (IsNumeric.HasValue)
        {
            AppendSeparator();
            builder.Append(IsNumeric.Value ? "num" : "non-num");
        }
        if (IsNullable.HasValue)
        {
            AppendSeparator();
            builder.Append(IsNullable.Value ? "null" : "non-null");
        }
        if (IsLambda.HasValue)
        {
            AppendSeparator();
            builder.Append(IsLambda.Value ? "lambda" : "non-lambda");
        }
        foreach (var member in Members)
        {
            AppendSeparator();
            builder.AppendFormat("member({0})", member.Value);
        }
        foreach (var @interface in Interfaces)
        {
            AppendSeparator();
            builder.AppendFormat(":{0}", @interface.Name);
        }
        foreach (var memberOf in MemberOf)
        {
            AppendSeparator();
            builder.AppendFormat("memberOf({0}, {1})", memberOf.TypeUid, memberOf.MemberName);
        }
        builder.Append('}');
        return builder.ToString();

        void AppendSeparator()
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append(", ");
            }
        }
    }
}

public partial record TypeConstraints
{
    private static ImmutableHashSet<Type> NumericTypes { get; } = ImmutableHashSet.CreateRange(new []
    {
        typeof(sbyte),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(byte),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(decimal),
        typeof(float),
        typeof(double),
        typeof(DateTimeOffset),
        typeof(sbyte?),
        typeof(short?),
        typeof(int?),
        typeof(long?),
        typeof(byte?),
        typeof(ushort?),
        typeof(uint?),
        typeof(ulong?),
        typeof(decimal?),
        typeof(float?),
        typeof(double?),
        typeof(DateTimeOffset?)
    });



    public static TypeConstraints Empty { get; } = new TypeConstraints(
        ImmutableHashSet<CaseInsensitive>.Empty,
        ImmutableHashSet<Type>.Empty,
        default,
        default,
        default,
        default,
        ImmutableList<(TypeUid TypeUid, string MemberName)>.Empty
    );

    public static TypeConstraints Numeric { get; } = Empty with { IsNumeric = true, IsLambda = false };

    public static TypeConstraints NotNumeric { get; } = Empty with { IsNumeric = false };

    public static TypeConstraints Nullable { get; } = Empty with { IsNullable = true, IsLambda = false };

    public static TypeConstraints Lambda { get; } = Empty with { IsLambda = true, IsNullable = false, IsNumeric = false };

    private static bool IsNullableType(Type type)
        => Internal.TypeExtensions.IsOptionalValue(type);

    private static bool IsEnumType(Type type)
        => type.IsEnum || (IsNullableType(type) && type.GetGenericArguments()[0].IsEnum);

    public static TypeConstraints HasMember(CaseInsensitive memberName)
        => Empty with { Members = ImmutableHashSet.Create(memberName) };

    public static TypeConstraints ImplementsInterface(Type @interface)
        => Empty with { Interfaces = ImmutableHashSet.Create(@interface) };

    public static TypeConstraints IsMemberOf(TypeUid ownerType, string memberName)
        => Empty with { MemberOf = ImmutableList.Create((ownerType, memberName)) };

    public static bool CheckMembers(IEnumerable<CaseInsensitive> members, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type candidateType, out TypeConstriantMismatch error)
    {
        foreach (var member in members)
        {
            if (candidateType.GetProperty(member.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy) is null)
            {
                error = new(
                    candidateType,
                    new TypeConstriantMismatchReason.MissingMember(member.Value)
                );
                return false;
            }
        }
        error = default;
        return true;
    }

    public static bool CheckInterfaces(IEnumerable<Type> interfaces, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type candidateType, out TypeConstriantMismatch error)
    {
        foreach (var @interface in interfaces)
        {
            if (!@interface.IsAssignableFrom(candidateType))
            {
                error = new(
                    candidateType,
                    new TypeConstriantMismatchReason.MissingInterfaceImplmentation(@interface)
                );
                return false;
            }
        }
        error = default;
        return true;
    }

    public static bool CheckBaseType(Type? baseType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type candidateType, out TypeConstriantMismatch error)
    {
        if (baseType is not null && !(baseType == candidateType || baseType.IsAssignableFrom(candidateType)))
        {
            error = new(
                candidateType,
                new TypeConstriantMismatchReason.IncompatibleType(baseType)
            );
            return false;
        }
        error = default;
        return true;
    }

    public static bool CheckNumericity(bool? numericity, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type candidateType, out TypeConstriantMismatch error)
    {
        if (numericity.HasValue)
        {
            switch (numericity.Value, NumericTypes.Contains(candidateType) || IsEnumType(candidateType))
            {
                case (true, false):
                    error = new(candidateType, TypeConstriantMismatchReason.NumericConstraint.Instance);
                    return false;
                case (false, true):
                    error = new(candidateType, TypeConstriantMismatchReason.NonNumericConstraint.Instance);
                    return false;
                default:
                    break;
            }
        }
        error = default;
        return true;
    }

    public static bool CheckNullability(bool? nullability, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type candidateType, out TypeConstriantMismatch error)
    {
        if (nullability.HasValue)
        {
            switch (nullability.Value, !candidateType.IsValueType || IsNullableType(candidateType))
            {
                case (true, false):
                    error = new(candidateType, TypeConstriantMismatchReason.NullableConstraint.Instance);
                    return false;
                case (false, true):
                    error = new(candidateType, TypeConstriantMismatchReason.NonNullableConstraint.Instance);
                    return false;
                default:
                    break;
            }
        }
        error = default;
        return true;
    }

    public static bool CheckLambda(bool? lambda, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type candidateType, out TypeConstriantMismatch error)
    {
        if (lambda.HasValue)
        {
            if (lambda.Value && !(candidateType.IsGenericType && candidateType.GetGenericTypeDefinition() == typeof(Func<,>)))
            {
                error = new(candidateType, TypeConstriantMismatchReason.LambdaConstraint.Instance);
                return false;
            }
        }
        error = default;
        return true;
    }
}