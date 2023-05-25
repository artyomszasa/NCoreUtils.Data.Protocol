using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal class ProtocolContextEmitter
{
    private sealed class BooleanBox
    {
        public bool Value { get; set; }
    }

    private static Cache<TypeData, string> DescriptorEmitCache { get; } = new(TypeDataEmitEqualityComparer.Singleton);

    private HashSet<ITypeSymbol> BuiltInTypes { get; }

    public ProtocolContextEmitter(HashSet<ITypeSymbol> builtInTypes)
    {
        BuiltInTypes = builtInTypes;
    }

    private static string EmitEnumFactory(TypeData data)
    {
        if (data.IsEnum)
        {
            var first = new BooleanBox { Value = true };
            var fields = data.Symbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => f.IsStatic);
            return @$"public sealed class {data.SafeName}EnumFactory : global::NCoreUtils.Data.Protocol.Internal.IEnumFactory
        {{
            public static {data.SafeName}EnumFactory Singleton {{ get; }} = new {data.SafeName}EnumFactory();

            public object FromRawValue(string rawValue)
            {{
                {string.Join(string.Empty, fields.Select(field => EmitNameCheck(first, data, field)))}
                }} else {{
                    throw new global::System.InvalidOperationException($""Unable to parse {{rawValue}} as {data.FullName}."");
                }}
            }}
        }}";
        }
        return string.Empty;

        static string EmitNameCheck(BooleanBox first, TypeData data, IFieldSymbol field)
        {
            if (first.Value)
            {
                first.Value = false;
                return @$"if (global::System.StringComparer.InvariantCultureIgnoreCase.Equals(""{field.Name}"", rawValue) || rawValue == ""{field.ConstantValue}"")
                {{
                    return {data.FullName}.{field.Name};";
            }
            else
            {
                return @$"
                }}
                else if (global::System.StringComparer.InvariantCultureIgnoreCase.Equals(""{field.Name}"", rawValue) || rawValue == ""{field.ConstantValue}"")
                {{
                    return {data.FullName}.{field.Name};";
            }
        }
    }

    private static string EmitCreateEqualMethodBodyForValueType(TypeData data)
    {
        return @$"right.Type.Equals(typeof({data.FullName}))
            ? Expression.Equal(self, right)
            : right.Type.Equals(typeof({data.FullName}?))
                ? Expression.Equal(Expression.Convert(self, typeof({data.FullName}?)), right)
                : throw new global::System.InvalidOperationException($""Cannot create AndAlso expression from {data.FullName} and {{right.Type}}."")";
    }

    private static string EmitCreateEqualMethodBodyForNullableType(TypeData data, ITypeSymbol t)
    {
        return @$"right.Type.Equals(typeof({data.FullName}))
            ? Expression.Equal(self, right)
            : right.Type.Equals(typeof({t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}))
                ? Expression.Equal(self, Expression.Convert(right, typeof({data.FullName})))
                : throw new global::System.InvalidOperationException($""Cannot create AndAlso expression from {data.FullName} and {{right.Type}}."")";
    }

    private static string EmitCreateEqualMethodBody(TypeData data) => data switch
    {
        { IsValueType: true, IsNullable: true, NullableType: var t } => EmitCreateEqualMethodBodyForNullableType(data, t),
        { IsValueType: true } => EmitCreateEqualMethodBodyForValueType(data),
        _ => $"Expression.Equal(self, right)"
    };

    private static string EmitCreateNotEqualMethodBodyForValueType(TypeData data)
    {
        return @$"right.Type.Equals(typeof({data.FullName}))
            ? Expression.NotEqual(self, right)
            : right.Type.Equals(typeof({data.FullName}?))
                ? Expression.NotEqual(Expression.Convert(self, typeof({data.FullName}?)), right)
                : throw new global::System.InvalidOperationException($""Cannot create AndAlso expression from {data.FullName} and {{right.Type}}."")";
    }

    private static string EmitCreateNotEqualMethodBodyForNullableType(TypeData data, ITypeSymbol t)
    {
        return @$"right.Type.Equals(typeof({data.FullName}))
            ? Expression.NotEqual(self, right)
            : right.Type.Equals(typeof({t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}))
                ? Expression.NotEqual(self, Expression.Convert(right, typeof({data.FullName})))
                : throw new global::System.InvalidOperationException($""Cannot create AndAlso expression from {data.FullName} and {{right.Type}}."")";
    }

    private static string EmitCreateNotEqualMethodBody(TypeData data) => data switch
    {
        { IsValueType: true, IsNullable: true, NullableType: var t } => EmitCreateNotEqualMethodBodyForNullableType(data, t),
        { IsValueType: true } => EmitCreateNotEqualMethodBodyForValueType(data),
        _ => $"Expression.NotEqual(self, right)"
    };

    private static string EmitIsAssignableToBody(TypeData data)
    {
        var bases = new List<ITypeSymbol> { data.Symbol };
        var baseType = data.Symbol.BaseType;
        while (baseType is not null && baseType.SpecialType != SpecialType.System_Object && baseType.SpecialType != SpecialType.System_ValueType && baseType.SpecialType != SpecialType.System_Enum)
        {
            bases.Add(baseType);
            baseType = baseType.BaseType;
        }
        return string.Join(" || ", bases.Select(t => $" typeof({t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}).Equals(baseType)"));
    }

    private static string EmitPropertyValue(TypeData data, IPropertySymbol p)
        => $"(PropertyInfo)((MemberExpression)((Expression<global::System.Func<{data.FullName}, {p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>>)(e => e!.{p.Name}!)).Body).Member";

    private static string EmitPropertyValues(TypeData data)
    {
        if (data.IsEnumerable)
        {
            return string.Empty;
        }
        return string.Join(",\n            ", data.Properties.Select(p => EmitPropertyValue(data, p)));
    }

    private static string EmitStringify(TypeData data)
    {
        if (data.IsEnum)
        {
            var fields = data.Symbol.GetMembers().OfType<IFieldSymbol>().Where(f => f.IsStatic);
            return @$"public string? Stringify(object? value) => value switch
            {{
                null => default,
                {string.Join(",\n                ", fields.Select(field => EmitEnumCase(data, field)))},
                _ => default
            }};";
        }
        return @$"public string? Stringify(object? value)
            => throw new global::System.NotSupportedException(""{data.FullName} cannot be coverted to literal."");";

        static string EmitEnumCase(TypeData data, IFieldSymbol field)
            => $"{data.FullName}.{field.Name} => \"{field.Name}\"";
    }

    private static string EmitDescriptorImpl(TypeData data)
    {
        return @$"
    [global::NCoreUtils.Data.Protocol.Internal.DescribedTypeAttribute(typeof({data.FullName}))]
    public sealed class {data.SafeName}Descriptor : global::NCoreUtils.Data.Protocol.Internal.ITypeDescriptor
    {{
        {EmitEnumFactory(data)}

        public sealed class Box
        {{
            public readonly {data.NullName} Value;

            public Box({data.NullName} value) => Value = value;
        }}

        private static readonly PropertyInfo[] _properties = new PropertyInfo[]
        {{
            {EmitPropertyValues(data)}
        }};

        private FieldInfo BoxValueField {{ get; }} = (FieldInfo)((MemberExpression)((Expression<global::System.Func<Box, {data.NullName}>>)(e => e.Value)).Body).Member;

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public global::System.Type Type
        {{
            {(data.IsLambda ? "[UnconditionalSuppressMessage(\"Trimming\", \"IL2026\")]" : string.Empty)}
            get => typeof({data.FullName});
        }}

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public global::System.Type ArrayOfType => typeof({data.FullName}[]);

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public global::System.Type EnumerableOfType => typeof(global::System.Collections.Generic.IEnumerable<{data.FullName}>);

        public global::System.Collections.Generic.IReadOnlyList<PropertyInfo> Properties => _properties;

        public bool IsArithmetic => false;

        public bool IsEnum => {(data.Symbol.TypeKind == TypeKind.Enum ? "true" : "false")};

        public bool IsValue => {(data.Symbol.IsValueType ? "true" : "false")};

        public object? BoxNullable(object value)
            => throw new global::System.NotSupportedException();

        public bool IsAssignableTo(global::System.Type baseType)
            => {EmitIsAssignableToBody(data)};

        public bool IsEnumerable([MaybeNullWhen(false)] out global::System.Type elementType)
        {{
            elementType = {(data.IsEnumerable ? $"typeof({data.ElementTypeFullName})" : "default")};
            return {(data.IsEnumerable ? "true" : "false")};
        }}

        public bool IsArray([MaybeNullWhen(false)] out global::System.Type elementType)
        {{
            elementType = {(data.IsArray ? $"typeof({data.ElementTypeFullName})" : "default")};
            return {(data.IsArray ? "true" : "false")};
        }}

        public bool IsLambda([MaybeNullWhen(false)] out global::System.Type argType, [MaybeNullWhen(false)] out global::System.Type resType)
        {{
            argType = {(data.IsLambda ? $"typeof({data.LambdaArgTypeFullName})" : "default")};
            resType = {(data.IsLambda ? $"typeof({data.LambdaResTypeFullName})" : "default")};
            return {(data.IsLambda ? "true" : "false")};
        }}

        public bool IsMaybe([MaybeNullWhen(false)] out global::System.Type elementType)
        {{
            elementType = default;
            return false;
        }}

        public bool IsNullable([MaybeNullWhen(false)] out global::System.Type elementType)
        {{
            elementType = {(data.NullableType is null ? "default" : $"typeof({data.NullableType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})")};
            return {(data.IsNullable ? "true" : "false")};
        }}

        public object Parse(string value)
            => throw new global::System.NotSupportedException(""{data.FullName} cannot be coverted from literal."");

        {EmitStringify(data)}

        public bool TryGetEnumFactory([MaybeNullWhen(false)] out global::NCoreUtils.Data.Protocol.Internal.IEnumFactory enumFactory)
        {{
            enumFactory = {(data.IsEnum ? $"{data.SafeName}EnumFactory.Singleton" : "default")};
            return {(data.IsEnum ? "true" : "false")};
        }}

        public Expression CreateBoxedConstant(object? value)
            => Expression.Field(
                Expression.Constant(new Box(({data.NullName})value!)),
                BoxValueField
            );

        public Expression CreateAdd(Expression self, Expression right)
            => throw new global::System.NotSupportedException();

        public Expression CreateAndAlso(Expression self, Expression right)
            => throw new global::System.NotSupportedException();

        public Expression CreateDivide(Expression self, Expression right)
            => throw new global::System.NotSupportedException();

        public Expression CreateEqual(Expression self, Expression right)
            => {EmitCreateEqualMethodBody(data)};

        public Expression CreateGreaterThan(Expression self, Expression right)
            => throw new global::System.NotSupportedException();

        public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
            => throw new global::System.NotSupportedException();

        public Expression CreateLessThan(Expression self, Expression right)
            => throw new global::System.NotSupportedException();

        public Expression CreateLessThanOrEqual(Expression self, Expression right)
            => throw new global::System.NotSupportedException();

        public Expression CreateModulo(Expression self, Expression right)
            => throw new global::System.NotSupportedException();

        public Expression CreateMultiply(Expression self, Expression right)
            => throw new global::System.NotSupportedException();

        public Expression CreateNotEqual(Expression self, Expression right)
            => {EmitCreateNotEqualMethodBody(data)};

        public Expression CreateOrElse(Expression self, Expression right)
            => throw new global::System.NotSupportedException();

        public Expression CreateSubtract(Expression self, Expression right)
            => throw new global::System.NotSupportedException();

        public MethodInfo EnumerableAnyMethod {{ get; }} = GetMethod<global::System.Collections.Generic.IEnumerable<{data.FullName}>, global::System.Func<{data.FullName}, bool>, bool>(global::System.Linq.Enumerable.Any);

        public MethodInfo EnumerableAllMethod {{ get; }} = GetMethod<global::System.Collections.Generic.IEnumerable<{data.FullName}>, global::System.Func<{data.FullName}, bool>, bool>(global::System.Linq.Enumerable.All);

        public MethodInfo EnumerableContainsMethod {{ get; }} = GetMethod<global::System.Collections.Generic.IEnumerable<{data.FullName}>, {data.FullName}, bool>(global::System.Linq.Enumerable.Contains);

        public void Accept(global::NCoreUtils.Data.Protocol.Internal.IDataTypeVisitor visitor)
            => visitor.Visit<{data.FullName}>();
    }}";
    }

    private static string EmitDescriptor(TypeData data)
        => DescriptorEmitCache.GetOrAdd(data, EmitDescriptorImpl);

    public string EmitContext(
        string @namespace,
        string name,
        string visibility,
        IEnumerable<TypeData> types0,
        IEnumerable<(string ArgType, string ResType)> lambdaTypes0,
        Action<Diagnostic> reportDiagnostic)
    {
        try
        {
            var types = types0.ToList();
            var lambdaTypes = new HashSet<(string ArgType, string ResType)>(lambdaTypes0);
            foreach (var data in types)
            {
                foreach (var bitype in BuiltInTypes)
                {
                    var bitname = bitype.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    lambdaTypes.Add((data.FullName, bitname));
                    lambdaTypes.Add((bitname, data.FullName));
                }
            }
            return $@"#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace {@namespace}
{{
{visibility} partial class {name} : global::NCoreUtils.Data.Protocol.IPortableDataContext
{{
    {string.Join("\n\n    ", types.Select(EmitDescriptor))}

    private static readonly global::NCoreUtils.Data.Protocol.Internal.ITypeDescriptor[] _descriptors = new global::NCoreUtils.Data.Protocol.Internal.ITypeDescriptor[]
    {{
        {string.Join(",\n        ", types.Select(data => $"new {data.SafeName}Descriptor()"))}
    }};

    private static readonly (global::System.Type ArgType, global::System.Type ResType, global::System.Type LambdaType)[] _lambdaTypes = new (global::System.Type ArgType, global::System.Type ResType, global::System.Type LambdaType)[]
    {{
        {string.Join(",\n        ", lambdaTypes.Select(data => $"(typeof({data.ArgType}), typeof({data.ResType}), typeof(global::System.Func<{data.ArgType}, {data.ResType}>))"))}
    }};

    public static global::NCoreUtils.Data.Protocol.IPortableDataContext Singleton {{ get; }} = new {name}();

    private static MethodInfo GetMethod<TArg1, TArg2, TResult>(global::System.Func<TArg1, TArg2, TResult> func)
        => func.Method;

    public global::System.Collections.Generic.IEnumerable<global::NCoreUtils.Data.Protocol.Internal.ITypeDescriptor> GetTypeDescriptors() => _descriptors;

    public global::System.Collections.Generic.IEnumerable<(global::System.Type ArgType, global::System.Type ResType, global::System.Type LambdaType)> GetLambdaTypes() => _lambdaTypes;
}}
}}";
        }
        catch (Exception exn)
        {
            reportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "NCU0000",
                    "An exception was thrown by the ProtocolContextEmitter generator",
                    "An exception was thrown by the ProtocolContextEmitter generator: '{0}'",
                    "ProtocolContextEmitter",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                Location.None,
                exn.ToString()
            ));
            return exn.ToString();
        }
    }
}