using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

#pragma warning disable IDE0060

internal class DefaultMembersEmitter
{
    private static ref readonly IntDesc InCond(bool condition, in IntDesc a, in IntDesc b)
    {
        if (condition)
        {
            return ref a;
        }
        return ref b;
    }

    private static bool TryGetNextSize(int size, out int nextSize)
    {
        switch (size)
        {
            case 2:
                nextSize = 4;
                return true;
            case 4:
                nextSize = 8;
                return true;
            default:
                nextSize = default;
                return false;
        }
    }

    private static bool TryGetCommonIntDesc(in IntDesc a, in IntDesc b, out IntDesc common)
    {
        var nullable = a.Nullable || b.Nullable;
        if (a.Signed == b.Signed)
        {
            common = new(Math.Max(a.Size, b.Size), a.Signed, nullable);
            return true;
        }
        ref readonly IntDesc s = ref InCond(a.Signed, in a, in b);
        ref readonly IntDesc u = ref InCond(a.Signed, in b, in a);
        if (s.Size > u.Size)
        {
            common = new(s.Size, signed: true, nullable: nullable);
            return true;
        }
        if (TryGetNextSize(Math.Max(s.Size, u.Size), out var size))
        {
            common = new(size, signed: true, nullable: nullable);
            return true;
        }
        common = default;
        return false;
    }

    private IntegerTypeSymbols IntSymbols { get; }

    public DefaultMembersEmitter(IntegerTypeSymbols intSymbols)
        => IntSymbols = intSymbols;

    private bool IsInteger(ITypeSymbol type, out IntDesc desc)
    {
        if (type.SpecialType == SpecialType.System_Int16)
        {
            desc = new(2, true, false);
            return true;
        }
        if (type.SpecialType == SpecialType.System_Int32)
        {
            desc = new(4, true, false);
            return true;
        }
        if (type.SpecialType == SpecialType.System_Int64)
        {
            desc = new(8, true, false);
            return true;
        }
        if (type.SpecialType == SpecialType.System_UInt16)
        {
            desc = new(2, false, false);
            return true;
        }
        if (type.SpecialType == SpecialType.System_UInt32)
        {
            desc = new(4, false, false);
            return true;
        }
        if (type.SpecialType == SpecialType.System_UInt64)
        {
            desc = new(8, false, false);
            return true;
        }
        if (SymbolEqualityComparer.Default.Equals(type, IntSymbols.NullableInt16))
        {
            desc = new(2, true, true);
            return true;
        }
        if (SymbolEqualityComparer.Default.Equals(type, IntSymbols.NullableInt32))
        {
            desc = new(4, true, true);
            return true;
        }
        if (SymbolEqualityComparer.Default.Equals(type, IntSymbols.NullableInt64))
        {
            desc = new(8, true, true);
            return true;
        }
        if (SymbolEqualityComparer.Default.Equals(type, IntSymbols.NullableUInt16))
        {
            desc = new(2, false, true);
            return true;
        }
        if (SymbolEqualityComparer.Default.Equals(type, IntSymbols.NullableUInt32))
        {
            desc = new(4, false, true);
            return true;
        }
        if (SymbolEqualityComparer.Default.Equals(type, IntSymbols.NullableUInt64))
        {
            desc = new(8, false, true);
            return true;
        }
        desc = default;
        return false;
    }


    private static string EmitBox(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateBox)
        {
            return @$"public sealed class Box
    {{
        public readonly {target.TargetFullName} Value;

        public Box({target.TargetFullName} value) => Value = value;

        public override string ToString() => $""{{{{{{Value}}}}}}"";
    }}";
        }
        return string.Empty;
    }

    private static string EmitBoxValueField(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateBoxValueField)
        {
            return @$"private FieldInfo BoxValueField {{ get; }} = (FieldInfo)((MemberExpression)((Expression<Func<Box, {target.TargetFullName}>>)(e => e.Value)).Body).Member;";
        }
        return string.Empty;
    }

    private static string EmitBoxNullable(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateBoxNullable)
        {
            if (target.TargetTypeSymbol.IsValueType)
            {
                if (target.IsTargetNullable)
                {
                    return $"public{target.Modifier} object? BoxNullable(object value) => throw new InvalidOperationException(\"Unable to create nullable from nullable.\");";
                }
                return $"public{target.Modifier} object? BoxNullable(object value) => ({target.TargetFullName}?)({target.TargetFullName})value;";
            }
            return $"public{target.Modifier} object? BoxNullable(object value) => throw new InvalidOperationException(\"Unable to create nullable from reference type.\");";
        }
        return string.Empty;
    }

    private static string EmitType(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateType)
        {
            return @$"[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public{target.Modifier} Type Type => typeof({target.TargetFullName});";
        }
        return string.Empty;
    }

    private static string EmitArrayOfType(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateArrayOfType)
        {
            return @$"[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public{target.Modifier} Type ArrayOfType => typeof({target.TargetFullName}[]);";
        }
        return string.Empty;
    }

    private static string EmitEnumerableOfType(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateEnumerableOfType)
        {
            return @$"[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public{target.Modifier} Type EnumerableOfType => typeof(IEnumerable<{target.TargetFullName}>);";
        }
        return string.Empty;
    }

    private static string EmitProperties(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateProperties)
        {
            if (target.IsTargetNullable)
            {
                return $@"public IReadOnlyList<PropertyInfo> Properties {{ get; }} = new PropertyInfo[]
    {{
        (PropertyInfo)((MemberExpression)((Expression<Func<{target.TargetFullName}, {target.NonNullableTargetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>>)(e => e!.Value)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<{target.TargetFullName}, bool>>)(e => e.HasValue)).Body).Member
    }};";
            }
            return "public IReadOnlyList<PropertyInfo> Properties => Array.Empty<PropertyInfo>();";
        }
        return string.Empty;
    }

    private static string EmitCreateAdd(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateAdd)
        {
            return "public Expression CreateAdd(Expression self, Expression right) => throw new NotSupportedException();";
        }
        return string.Empty;
    }

    private static string EmitCreateAndAlso(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateAndAlso)
        {
            return "public Expression CreateAndAlso(Expression self, Expression right) => throw new NotSupportedException();";
        }
        return string.Empty;
    }

    private static string EmitCreateBoxedConstant(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateBoxedConstant)
        {
            return @$"public{target.Modifier} Expression CreateBoxedConstant(object? value) => Expression.Field(
        Expression.Constant(new Box(({target.TargetFullName})value!)),
        BoxValueField
    );";
        }
        return string.Empty;
    }

    private static string EmitCreateDivide(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateDivide)
        {
            return "public Expression CreateDivide(Expression self, Expression right) => throw new NotSupportedException();";
        }
        return string.Empty;
    }

    private static string EmitCreateGreaterThan(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateGreaterThan)
        {
            return "public Expression CreateGreaterThan(Expression self, Expression right) => throw new NotSupportedException();";
        }
        return string.Empty;
    }

    private static string EmitCreateGreaterThanOrEqual(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateGreaterThanOrEqual)
        {
            return "public Expression CreateGreaterThanOrEqual(Expression self, Expression right) => throw new NotSupportedException();";
        }
        return string.Empty;
    }

    private static string EmitCreateLessThan(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateLessThan)
        {
            return "public Expression CreateLessThan(Expression self, Expression right) => throw new NotSupportedException();";
        }
        return string.Empty;
    }

    private static string EmitCreateLessThanOrEqual(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateLessThanOrEqual)
        {
            return "public Expression CreateLessThanOrEqual(Expression self, Expression right) => throw new NotSupportedException();";
        }
        return string.Empty;
    }

    private static string EmitCreateModulo(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateModulo)
        {
            return "public Expression CreateModulo(Expression self, Expression right) => throw new NotSupportedException();";
        }
        return string.Empty;
    }

    private static string EmitCreateMultiply(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateMultiply)
        {
            return "public Expression CreateMultiply(Expression self, Expression right) => throw new NotSupportedException();";
        }
        return string.Empty;
    }

    private static string EmitCreateOrElse(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateOrElse)
        {
            return "public Expression CreateOrElse(Expression self, Expression right) => throw new NotSupportedException();";
        }
        return string.Empty;
    }

    private static string EmitCreateSubtract(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateSubtract)
        {
            return "public Expression CreateSubtract(Expression self, Expression right) => throw new NotSupportedException();";
        }
        return string.Empty;
    }

    private static string EmitCreateEqual(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateEqual)
        {
            if (!target.TargetTypeSymbol.IsValueType)
            {
                return "public Expression CreateEqual(Expression self, Expression right) => Expression.Equal(self, right);";
            }
            if (target.IsTargetNullable)
            {
                return @$"public Expression CreateEqual(Expression self, Expression right)
        => right.Type == typeof({target.TargetFullName})
            ? Expression.Equal(self, right)
            : right.Type == typeof({target.NonNullableTargetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})
                ? Expression.Equal(self, Expression.Convert(right, typeof({target.TargetFullName})))
                : throw new InvalidOperationException($""Cannot create Equal expression from {target.TargetFullName} and {{right.Type}}."");";
            }
            return @$"public Expression CreateEqual(Expression self, Expression right)
        => right.Type == typeof({target.TargetFullName})
            ? Expression.Equal(self, right)
            : right.Type == typeof({target.NullableTargetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})
                ? Expression.Equal(Expression.Convert(self, typeof({target.NullableTargetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})), right)
                : throw new InvalidOperationException($""Cannot create Equal expression from {target.TargetFullName} and {{right.Type}}."");";
        }
        return string.Empty;
    }

    private static string EmitCreateNotEqual(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateCreateNotEqual)
        {
            if (!target.TargetTypeSymbol.IsValueType)
            {
                return "public Expression CreateNotEqual(Expression self, Expression right) => Expression.NotEqual(self, right);";
            }
            if (target.IsTargetNullable)
            {
                return @$"public Expression CreateNotEqual(Expression self, Expression right)
        => right.Type == typeof({target.TargetFullName})
            ? Expression.NotEqual(self, right)
            : right.Type == typeof({target.NonNullableTargetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})
                ? Expression.NotEqual(self, Expression.Convert(right, typeof({target.TargetFullName})))
                : throw new InvalidOperationException($""Cannot create NotEqual expression from {target.TargetFullName} and {{right.Type}}."");";
            }
            return @$"public Expression CreateNotEqual(Expression self, Expression right)
        => right.Type == typeof({target.TargetFullName})
            ? Expression.NotEqual(self, right)
            : right.Type == typeof({target.NullableTargetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})
                ? Expression.NotEqual(Expression.Convert(self, typeof({target.NullableTargetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})), right)
                : throw new InvalidOperationException($""Cannot create NotEqual expression from {target.TargetFullName} and {{right.Type}}."");";
        }
        return string.Empty;
    }

    private static string EmitIsEnumerable(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateIsEnumerable)
        {
            return @"public bool IsEnumerable([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }";
        }
        return string.Empty;
    }

    private static string EmitIsArray(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateIsArray)
        {
            return @"public bool IsArray([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }";
        }
        return string.Empty;
    }

    private static string EmitIsLambda(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateIsLambda)
        {
            return @"public bool IsLambda([MaybeNullWhen(false)] out Type argType, [MaybeNullWhen(false)] out Type resType)
    {
        argType = default;
        resType = default;
        return false;
    }";
        }
        return string.Empty;
    }

    private static string EmitIsMaybe(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateIsMaybe)
        {
            return @"public bool IsMaybe([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }";
        }
        return string.Empty;
    }

    private static string EmitIsNullable(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateIsNullable)
        {
            if (target.IsTargetNullable)
            {
                return @$"public{target.Modifier} bool IsNullable([MaybeNullWhen(false)] out Type elementType)
    {{
        elementType = typeof({target.NonNullableTargetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)});
        return true;
    }}";
            }
            return @$"public{target.Modifier} bool IsNullable([MaybeNullWhen(false)] out Type elementType)
    {{
        elementType = default;
        return false;
    }}";
        }
        return string.Empty;
    }

    private static string EmitTryGetEnumFactory(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateTryGetEnumFactory)
        {
            return @"public bool TryGetEnumFactory([MaybeNullWhen(false)] out IEnumFactory enumFactory)
    {
        enumFactory = default;
        return false;
    }";
        }
        return string.Empty;
    }

    private static string EmitEnumerableAnyMethod(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateEnumerableAnyMethod)
        {
            return $"public{target.Modifier} MethodInfo EnumerableAnyMethod {{ get; }} = ReflectionHelpers.GetMethod<IEnumerable<{target.TargetFullName}>, Func<{target.TargetFullName}, bool>, bool>(Enumerable.Any);";
        }
        return string.Empty;
    }

    private static string EmitEnumerableAllMethod(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateEnumerableAllMethod)
        {
            return $"public{target.Modifier} MethodInfo EnumerableAllMethod {{ get; }} = ReflectionHelpers.GetMethod<IEnumerable<{target.TargetFullName}>, Func<{target.TargetFullName}, bool>, bool>(Enumerable.All);";
        }
        return string.Empty;
    }

    private static string EmitEnumerableContainsMethod(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateEnumerableContainsMethod)
        {
            return $"public{target.Modifier} MethodInfo EnumerableContainsMethod {{ get; }} = ReflectionHelpers.GetMethod<IEnumerable<{target.TargetFullName}>, {target.TargetFullName}, bool>(Enumerable.Contains);";
        }
        return string.Empty;
    }

    private static string EmitAccept(BuiltInDescriptorTarget target, GenerationOptions opts)
    {
        if (opts.GenerateAccept)
        {
            return $"public{target.Modifier} void Accept(IDataTypeVisitor visitor) => visitor.Visit<{target.TargetFullName}>();";
        }
        return string.Empty;
    }

    private static string EmitUnifyExpressionTypesCase(BuiltInDescriptorTarget target, ITypeSymbol common, ITypeSymbol type)
    {
        var typeFullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var commonFullName = common.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (SymbolEqualityComparer.Default.Equals(target.TargetTypeSymbol, common))
        {
            if (SymbolEqualityComparer.Default.Equals(type, common))
            {
                return $@"if (typeof({typeFullName}) == right.Type)
        {{
            return (self, right);
        }}";
            }
            return $@"if (typeof({typeFullName}) == right.Type)
        {{
            return (self, Expression.Convert(right, typeof({commonFullName})));
        }}";
        }
        else if (SymbolEqualityComparer.Default.Equals(type, common))
        {
            return $@"if (typeof({typeFullName}) == right.Type)
        {{
            return (Expression.Convert(self, typeof({commonFullName})), right);
        }}";
        }
        return $@"if (typeof({typeFullName}) == right.Type)
        {{
            return (
                Expression.Convert(self, typeof({commonFullName})),
                Expression.Convert(right, typeof({commonFullName}))
            );
        }}";
    }

    private string EmitUnifyExpressionTypes(BuiltInDescriptorTarget target)
    {
        if (IsInteger(target.TargetTypeSymbol, out var desc))
        {
            var cases0 = new List<(ITypeSymbol RightType, ITypeSymbol CommonType)>();
            foreach (var ity in IntSymbols)
            {
                if (IsInteger(ity, out var idesc) && TryGetCommonIntDesc(in desc, in idesc, out var common))
                {
                    cases0.Add((ity, IntSymbols[in common]));
                }
            }
            var cases = cases0
                .Select(tup => EmitUnifyExpressionTypesCase(target, tup.CommonType, tup.RightType));
            return @$"protected override (Expression Left, Expression Right) UnifyExpressionTypes(Expression self, Expression right)
    {{
        {string.Join("\n        ", cases)}
        throw new InvalidOperationException($""Unable to unify types {target.TargetFullName} and {{right.Type}}."");
    }}";
        }
        return string.Empty;
    }

    public string EmitDefaultMembers(BuiltInDescriptorTarget target, GenerationOptions opts)
        => @$"#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public partial class {target.TypeSymbol.Name}
{{
    {EmitBox(target, opts)}
    {EmitBoxValueField(target, opts)}
    {EmitBoxNullable(target, opts)}
    {EmitType(target, opts)}
    {EmitArrayOfType(target, opts)}
    {EmitEnumerableOfType(target, opts)}
    {EmitProperties(target, opts)}
    {EmitCreateAdd(target, opts)}
    {EmitCreateAndAlso(target, opts)}
    {EmitCreateBoxedConstant(target, opts)}
    {EmitCreateDivide(target, opts)}
    {EmitCreateGreaterThan(target, opts)}
    {EmitCreateGreaterThanOrEqual(target, opts)}
    {EmitCreateLessThan(target, opts)}
    {EmitCreateLessThanOrEqual(target, opts)}
    {EmitCreateModulo(target, opts)}
    {EmitCreateMultiply(target, opts)}
    {EmitCreateOrElse(target, opts)}
    {EmitCreateSubtract(target, opts)}
    {EmitCreateEqual(target, opts)}
    {EmitCreateNotEqual(target, opts)}
    {EmitIsEnumerable(target, opts)}
    {EmitIsArray(target, opts)}
    {EmitIsLambda(target, opts)}
    {EmitIsMaybe(target, opts)}
    {EmitIsNullable(target, opts)}
    {EmitTryGetEnumFactory(target, opts)}
    {EmitEnumerableAnyMethod(target, opts)}
    {EmitEnumerableAllMethod(target, opts)}
    {EmitEnumerableContainsMethod(target, opts)}
    {EmitAccept(target, opts)}
    {EmitUnifyExpressionTypes(target)}
}}";
}
#pragma warning restore IDE0060