using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NCoreUtils.Data.Protocol.Generator;

[Generator(LanguageNames.CSharp)]
public partial class ProtocolContextGeneratorV2 : IIncrementalGenerator
{
    private sealed class TypeDataEqualityComparer : IEqualityComparer<ITypeData>
    {
        public static TypeDataEqualityComparer Singleton { get; } = new();

        public bool Equals(ITypeData x, ITypeData y)
        {
            if (x is TypeDataV2 a && y is TypeDataV2 b)
            {
                return a.Equals(b);
            }
            return x.Equals(y);
        }

        public int GetHashCode(ITypeData obj)
            => obj.GetHashCode();
    }

    private readonly struct TargetOrError
        : IEquatable<TargetOrError>
    {
        public static implicit operator TargetOrError(DiagnosticData error) => new(error);

        // public static implicit operator TargetOrError(ProtocolContextTargetV2 target) => new(target);

        public ProtocolContextTargetV2? Target { get; }

        public DiagnosticData? Error { get; }

        private TargetOrError(ProtocolContextTargetV2? target, DiagnosticData? error)
        {
            if (target is null && error is null)
            {
                throw new InvalidOperationException("Either target or error must be not null.");
            }
            Target = target;
            Error = error;
        }

        public TargetOrError(ProtocolContextTargetV2 target) : this(target, default) { }

        public TargetOrError(DiagnosticData error) : this(default, error) { }

        public bool Equals(TargetOrError other)
        {
            if (Target is ProtocolContextTargetV2 target)
            {
                return other.Target is ProtocolContextTargetV2 otherTarget && target == otherTarget;
            }
            return false;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is TargetOrError other && Equals(other);

        public override int GetHashCode()
            => Target is ProtocolContextTargetV2 target
                ? target.GetHashCode() & (~1)
                : Error is DiagnosticData error
                    ? error.GetHashCode() | 1
                    : 0;
    }

    internal readonly struct LambdaTypeSymbols(ITypeSymbol argType, ITypeSymbol resType)
        : IEquatable<LambdaTypeSymbols>
    {
        public static bool operator==(LambdaTypeSymbols a, LambdaTypeSymbols b)
            => a.Equals(b);

        public static bool operator!=(LambdaTypeSymbols a, LambdaTypeSymbols b)
            => !a.Equals(b);

        public ITypeSymbol ArgType { get; } = argType;

        public ITypeSymbol ResType { get; } = resType;

        public bool Equals(LambdaTypeSymbols other)
            => SymbolEqualityComparer.Default.Equals(ArgType, other.ArgType)
                && SymbolEqualityComparer.Default.Equals(ResType, other.ResType);

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is LambdaTypeSymbols other && Equals(other);

        public override int GetHashCode()
            => SymbolEqualityComparer.Default.GetHashCode(ArgType)
                ^ SymbolEqualityComparer.Default.GetHashCode(ResType);
    }

    internal readonly struct TypeDescriptorWrapper(ITypeSymbol descriptorType, ITypeSymbol describedType)
        : IEquatable<TypeDescriptorWrapper>
    {
        public static bool operator==(TypeDescriptorWrapper a, TypeDescriptorWrapper b)
            => a.Equals(b);

        public static bool operator!=(TypeDescriptorWrapper a, TypeDescriptorWrapper b)
            => !a.Equals(b);

        public ITypeSymbol DescriptorType { get; } = descriptorType;

        public ITypeSymbol DescribedType { get; } = describedType;

        public bool Equals(TypeDescriptorWrapper other)
            => SymbolEqualityComparer.Default.Equals(DescriptorType, other.DescriptorType)
                && SymbolEqualityComparer.Default.Equals(DescribedType, other.DescribedType);

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is TypeDescriptorWrapper other && Equals(other);

        public override int GetHashCode()
            => SymbolEqualityComparer.Default.GetHashCode(DescriptorType)
                ^ SymbolEqualityComparer.Default.GetHashCode(DescribedType);
    }

    private static UTF8Encoding Utf8 { get; } = new(false);

    private static TypeSyntax PredefinedType(SyntaxKind kind)
        => SyntaxFactory.PredefinedType(SyntaxFactory.Token(kind));

    private static Dictionary<SpecialType, PrimitiveValueTypeData> CommonPrimitiveValueTypes { get; } = new()
    {
        { SpecialType.System_Boolean, new PrimitiveValueTypeData("bool", "Boolean", PredefinedType(SyntaxKind.BoolKeyword)) },
        // { SpecialType., new PrimitiveValueTypeData("global::System.Guid", "Guid") },
        { SpecialType.System_DateTime, new PrimitiveValueTypeData("global::System.DateTime", "DateTime") },
        // { SpecialType, new PrimitiveValueTypeData("global::System.DateTimeOffset", "DateTimeOffset") },
        { SpecialType.System_SByte, new PrimitiveValueTypeData("sbyte", "SByte", PredefinedType(SyntaxKind.SByteKeyword)) },
        { SpecialType.System_Int16, new PrimitiveValueTypeData("short", "Int16", PredefinedType(SyntaxKind.ShortKeyword)) },
        { SpecialType.System_Int32, new PrimitiveValueTypeData("int", "Int32", PredefinedType(SyntaxKind.IntKeyword)) },
        { SpecialType.System_Int64, new PrimitiveValueTypeData("long", "Int64", PredefinedType(SyntaxKind.LongKeyword)) },
        { SpecialType.System_Byte, new PrimitiveValueTypeData("byte", "Byte", PredefinedType(SyntaxKind.ByteKeyword)) },
        { SpecialType.System_UInt16, new PrimitiveValueTypeData("ushort", "UInt16", PredefinedType(SyntaxKind.UShortKeyword)) },
        { SpecialType.System_UInt32, new PrimitiveValueTypeData("uint", "UInt32", PredefinedType(SyntaxKind.UIntKeyword)) },
        { SpecialType.System_UInt64, new PrimitiveValueTypeData("ulong", "UInt64", PredefinedType(SyntaxKind.ULongKeyword)) },
        { SpecialType.System_Single, new PrimitiveValueTypeData("float", "Single", PredefinedType(SyntaxKind.FloatKeyword)) },
        { SpecialType.System_Double, new PrimitiveValueTypeData("double", "Double", PredefinedType(SyntaxKind.DoubleKeyword)) },
        { SpecialType.System_Decimal, new PrimitiveValueTypeData("decimal", "Decimal", PredefinedType(SyntaxKind.DecimalKeyword)) },
        { SpecialType.System_String, new PrimitiveValueTypeData("string", "String", PredefinedType(SyntaxKind.StringKeyword)) },
    };

    private static void AddPrimitiveValueType(Compilation compilation, Dictionary<ITypeSymbol, ITypeData> types, string qualifiedName, string name)
    {
        if (compilation.GetTypeByMetadataName(qualifiedName) is ITypeSymbol symbol)
        {
            // FIXME: cache
            var data = new PrimitiveValueTypeData($"global::{qualifiedName}", name);
            types.Add(symbol, data);
            types.Add(compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(symbol), new NullablePrimitiveValueTypeData(data));
        }
    }

    private static (SpecialType? TypeData, Action<Compilation, Dictionary<ITypeSymbol, ITypeData>>? Factory) GetPrimitiveType(ProtocolPrimitiveType key) => key switch
    {
        ProtocolPrimitiveType.Boolean => (SpecialType.System_Boolean, null),
        ProtocolPrimitiveType.Guid => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "System.Guid", "Guid")),
        ProtocolPrimitiveType.DateTime => (SpecialType.System_DateTime, null),
        ProtocolPrimitiveType.DateTimeOffset => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "System.DateTimeOffset", "DateTimeOffset")),
        ProtocolPrimitiveType.SByte => (SpecialType.System_SByte, null),
        ProtocolPrimitiveType.Int16 => (SpecialType.System_Int16, null),
        ProtocolPrimitiveType.Int32 => (SpecialType.System_Int32, null),
        ProtocolPrimitiveType.Int64 => (SpecialType.System_Int64, null),
        ProtocolPrimitiveType.Int128 => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "System.Int128", "Int128")),
        ProtocolPrimitiveType.Byte => (SpecialType.System_Byte, null),
        ProtocolPrimitiveType.UInt16 => (SpecialType.System_UInt16, null),
        ProtocolPrimitiveType.UInt32 => (SpecialType.System_UInt32, null),
        ProtocolPrimitiveType.UInt64 => (SpecialType.System_UInt64, null),
        ProtocolPrimitiveType.UInt128 => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "System.UInt128", "UInt128")),
        ProtocolPrimitiveType.Half => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "System.Half", "Half")),
        ProtocolPrimitiveType.Single => (SpecialType.System_Single, null),
        ProtocolPrimitiveType.Double => (SpecialType.System_Double, null),
        ProtocolPrimitiveType.Decimal => (SpecialType.System_Decimal, null),
        ProtocolPrimitiveType.DateOnly => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "System.DateOnly", "DateOnly")),
        ProtocolPrimitiveType.TimeOnly => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "System.TimeOnly", "TimeOnly")),
        ProtocolPrimitiveType.String => (SpecialType.System_String, null),
        _ => throw new InvalidOperationException("Unsupported protocol primitive value type.")
    };

    private void AddPrimitiveValueTypes(Compilation compilation, Dictionary<ITypeSymbol, ITypeData> types)
    {
        foreach (ProtocolPrimitiveType value in Enum.GetValues(typeof(ProtocolPrimitiveType)))
        {
            var (stype0, factory) = GetPrimitiveType(value);
            if (stype0 is SpecialType stype)
            {
                var data = CommonPrimitiveValueTypes[stype];
                var ctype = compilation.GetSpecialType(stype);
                types.Add(ctype, data);
                if (stype is not SpecialType.System_String)
                {
                    types.Add(compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(ctype), new NullablePrimitiveValueTypeData(data));
                }
            }
            else
            {
                factory!(compilation, types);
            }
        }
    }

    private HashSet<ITypeSymbol> GetBuiltInTypes(Compilation compilation, INamedTypeSymbol nullableT)
    {
        var valuePrimitives = new List<ITypeSymbol>
        {
            compilation.GetSpecialType(SpecialType.System_Boolean),
            compilation.GetTypeByMetadataName("System.Guid") ?? throw new InvalidOperationException("Unable to get type symbol for System.Guid."),
            compilation.GetTypeByMetadataName("System.DateTime") ?? throw new InvalidOperationException("Unable to get type symbol for System.DateTime."),
            compilation.GetTypeByMetadataName("System.DateTimeOffset") ?? throw new InvalidOperationException("Unable to get type symbol for System.DateTimeOffset."),
            compilation.GetSpecialType(SpecialType.System_SByte),
            compilation.GetSpecialType(SpecialType.System_Int16),
            compilation.GetSpecialType(SpecialType.System_Int32),
            compilation.GetSpecialType(SpecialType.System_Int64),
            compilation.GetSpecialType(SpecialType.System_Byte),
            compilation.GetSpecialType(SpecialType.System_UInt16),
            compilation.GetSpecialType(SpecialType.System_UInt32),
            compilation.GetSpecialType(SpecialType.System_UInt64),
            compilation.GetSpecialType(SpecialType.System_Single),
            compilation.GetSpecialType(SpecialType.System_Double),
            compilation.GetSpecialType(SpecialType.System_DateTime),
            compilation.GetSpecialType(SpecialType.System_Decimal),
        };

        // NOTE: DateOnly is only available on .NET5+
        if (compilation.GetTypeByMetadataName("System.DateOnly") is ITypeSymbol dateOnlyType)
        {
            valuePrimitives.Add(dateOnlyType);
        }

        // NOTE: TimeOnly is only available on .NET5+
        if (compilation.GetTypeByMetadataName("System.TimeOnly") is ITypeSymbol timeOnlyType)
        {
            valuePrimitives.Add(timeOnlyType);
        }

        // NOTE: Half is only available on .NET5+
        if (compilation.GetTypeByMetadataName("System.Half") is ITypeSymbol halfType)
        {
            valuePrimitives.Add(halfType);
        }

        // NOTE: Int128 is only available on .NET5+
        if (compilation.GetTypeByMetadataName("System.Int128") is ITypeSymbol i128Type)
        {
            valuePrimitives.Add(i128Type);
        }

        // NOTE: UInt128 is only available on .NET5+
        if (compilation.GetTypeByMetadataName("System.UInt128") is ITypeSymbol u128Type)
        {
            valuePrimitives.Add(u128Type);
        }

        return new HashSet<ITypeSymbol>(
            new ITypeSymbol[] { compilation.GetSpecialType(SpecialType.System_String) }
                .Concat(valuePrimitives)
                .Concat(valuePrimitives.Select(t => nullableT.Construct(t))),
            SymbolEqualityComparer.Default
        );
    }

    private static void AddTargetTypes(
        Queue<(ITypeSymbol Symbol, bool Root, bool FromProperty)> pending,
        CompilationContext compilation,
        GenMode mode,
        IDictionary<ITypeSymbol, ITypeData> targetTypes,
        HashSet<ITypeSymbol> opaqueTypes,
        HashSet<ITypeSymbol> builtin,
        Func<ITypeSymbol, string?> safeNameFactory,
        HashSet<ITypeSymbol> propertyReachedTypes,
        CancellationToken cancellationToken)
    {
        while (pending.TryDequeue(out var symbol, out var root, out var fromProperty))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (fromProperty)
            {
                propertyReachedTypes.Add(symbol);
            }
            if (targetTypes.ContainsKey(symbol) || builtin.Contains(symbol))
            {
                // NOTE: we still need to generate predicate/enumerable/array descriptors for property types and
                // roots (e.g. explicitly described types)..
                if (fromProperty || root)
                {
                    if (mode.HasFlag(GenMode.Array) && !compilation.IsCollection(symbol))
                    {
                        pending.Enqueue(compilation.CreateArrayTypeSymbol(symbol));
                    }
                    if (mode.HasFlag(GenMode.Enumerable) && !compilation.IsCollection(symbol))
                    {
                        pending.Enqueue(compilation.CreateIEnumerableTypeSymbol(symbol));
                    }
                    if (mode.HasFlag(GenMode.Predicates))
                    {
                        pending.Enqueue(compilation.CreateFuncTypeSymbol(symbol, compilation.@bool));
                    }
                    if (root && mode.HasFlag(GenMode.Nullable) && symbol.IsValueType && !symbol.IsNullableStruct)
                    {
                        pending.Enqueue(compilation.CreateNullableTypeSymbol(symbol), root: true);
                    }
                }
                continue;
            }
            // NOTE: types first reached through properties (including element types of collection properties and
            // underlying types of nullable properties) need predicate/enumerable/array descriptors as well, but
            // (unlike roots) collection-of-collection types are not generated for them.
            if (root || fromProperty)
            {
                // add array type if not already present
                if (mode.HasFlag(GenMode.Array) && (root || !compilation.IsCollection(symbol)))
                {
                    pending.Enqueue(compilation.CreateArrayTypeSymbol(symbol));
                }
                // add enumerable type if not already present
                if (mode.HasFlag(GenMode.Enumerable) && (root || !compilation.IsCollection(symbol)))
                {
                    pending.Enqueue(compilation.CreateIEnumerableTypeSymbol(symbol));
                }
                // add predicate lambda type
                if (mode.HasFlag(GenMode.Predicates))
                {
                    pending.Enqueue(compilation.CreateFuncTypeSymbol(symbol, compilation.@bool));
                }
            }
            var isOpaque = opaqueTypes.Contains(symbol)
                || IsEnumerableOfOpaqueType(symbol)
                || IsArrayOfOpaqueType(symbol);
            TypeDataV2 data;
            if (symbol is INamedTypeSymbol namedSymbol)
            {
                data = TypeDataV2.Create(compilation, symbol, isOpaque, safeNameFactory, out var elementType, out var properties);
                targetTypes.Add(namedSymbol, data);
                if (isOpaque)
                {
                    continue;
                }
                if (data.IsValueType)
                {
                    if (!data.IsNullable && mode.HasFlag(GenMode.Nullable))
                    {
                        pending.Enqueue(compilation.CreateNullableTypeSymbol(namedSymbol), root: true);
                    }
                    if (data.IsNullable && (root || fromProperty))
                    {
                        // add T for T? (e.g. lambdas over nullable property may operate on T)
                        pending.Enqueue(namedSymbol.TypeArguments[0], fromProperty: true);
                    }
                }
                else
                {
                    var baseType = namedSymbol.BaseType;
                    if (baseType is not null
                        && baseType.SpecialType != SpecialType.System_ValueType
                        && baseType.SpecialType != SpecialType.System_Object
                        && baseType.SpecialType != SpecialType.System_Delegate
                        && baseType.SpecialType != SpecialType.System_MulticastDelegate)
                    {
                        pending.Enqueue(baseType, root: true);
                    }
                }
                if (data.IsEnumerable)
                {
                    var enumerableSymbol = compilation.CreateIEnumerableTypeSymbol(elementType!);
                    if (!SymbolEqualityComparer.Default.Equals(namedSymbol, enumerableSymbol))
                    {
                        // add IEnumerable<T> for types implementing it!
                        pending.Enqueue(enumerableSymbol);
                    }
                }
                if (elementType is ITypeSymbol elementSymbol)
                {
                    // add T for IEnumerable<T> its derivatives (T of collection property is used in lambdas)
                    pending.Enqueue(elementSymbol, fromProperty: root || fromProperty);
                }
                foreach (var prop in properties)
                {
                    pending.Enqueue(prop.Type, fromProperty: true);
                    // // NOTE: if property is array/enumerable generate predicate lambdas for subselection
                    // if (mode.HasFlag(GenMode.Predicates)
                    //     && (compilation.TryGetArrayElementType(prop.Type, out var propTypeElement)
                    //         || compilation.TryGetEnumerableElementType(prop.Type, out propTypeElement, out _)))
                    // {
                    //     pending.Enqueue(compilation.CreateFuncTypeSymbol(propTypeElement, compilation.@bool));
                    // }
                }
            }
            else if (symbol is IArrayTypeSymbol arraySymbol)
            {
                data = TypeDataV2.Create(compilation, arraySymbol, isOpaque, safeNameFactory, out var elementType, out _);
                targetTypes.Add(arraySymbol, data);
                pending.Enqueue(elementType!, fromProperty: root || fromProperty);
                // add IEnumerable<T> for array types!
                pending.Enqueue(compilation.CreateIEnumerableTypeSymbol(elementType!));
            }
            else
            {
                throw new Exception($"Not handled type: {symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}");
            }
        }

        bool IsEnumerableOfOpaqueType(ITypeSymbol symbol)
            => compilation.TryGetEnumerableElementType(symbol, out var elementType, out _)
                && opaqueTypes.Contains(elementType);

        bool IsArrayOfOpaqueType(ITypeSymbol symbol)
            => compilation.TryGetArrayElementType(symbol, out var elementType)
                && opaqueTypes.Contains(elementType);
    }

    private static SymbolDisplayFormat MaybeNullableFullyQualifiedFormat { get; } =
            new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                    SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private static void EnsureSafeNamesAreUnique(IEnumerable<ITypeData> types, Location? location)
    {
        var collision = types
            // NOTE: only generated types own generated names
            .OfType<TypeDataV2>()
            .GroupBy(static t => t.SafeName, StringComparer.Ordinal)
            .Where(static g => g.Skip(1).Any())
            // NOTE: derived names (e.g. FuncOfItemBoolean) contain the name they are derived from --> the shortest
            // collision is the root cause, resolving it resolves the derived ones as well.
            .OrderBy(static g => g.Key.Length)
            .ThenBy(static g => g.Key, StringComparer.Ordinal)
            .FirstOrDefault();
        if (collision is not null)
        {
            throw new GenerationException(new DiagnosticData(
                DiagnosticDescriptors.SafeNameCollision,
                location,
                [
                    collision.Key,
                    string.Join(", ", collision.Select(static t => t.FullName).OrderBy(static n => n, StringComparer.Ordinal))
                ]
            ));
        }
    }

    /// <summary>
    /// Argument/result types of the lambda types present in <c>PortableDataUtils</c> by default (all pairs). Must be
    /// kept in sync with <c>DefaultsGenerator.BuiltInTypes</c>.
    /// </summary>
    private static HashSet<ITypeSymbol> GetDefaultLambdaTypeArgs(Compilation compilation, INamedTypeSymbol nullableT)
    {
        ITypeSymbol[] valueTypes =
        [
            compilation.GetSpecialType(SpecialType.System_Boolean),
            compilation.GetSpecialType(SpecialType.System_Int16),
            compilation.GetSpecialType(SpecialType.System_Int32),
            compilation.GetSpecialType(SpecialType.System_Int64),
            compilation.GetSpecialType(SpecialType.System_UInt16),
            compilation.GetSpecialType(SpecialType.System_UInt32),
            compilation.GetSpecialType(SpecialType.System_UInt64),
            compilation.GetSpecialType(SpecialType.System_Single),
            compilation.GetSpecialType(SpecialType.System_Double),
            compilation.GetSpecialType(SpecialType.System_DateTime),
            compilation.GetTypeSymbol("System.DateTimeOffset"),
            compilation.GetTypeSymbol("System.Guid")
        ];
        return new HashSet<ITypeSymbol>(
            valueTypes
                .Concat(valueTypes.Select(t => nullableT.Construct(t)))
                .Append(compilation.GetSpecialType(SpecialType.System_String)),
            SymbolEqualityComparer.Default
        );
    }

    private static bool IsQueryableBaseType([NotNullWhen(true)] INamedTypeSymbol? baseType)
        => baseType is not null
            && baseType.SpecialType != SpecialType.System_Object
            && baseType.SpecialType != SpecialType.System_ValueType
            && baseType.SpecialType != SpecialType.System_Enum
            && baseType.SpecialType != SpecialType.System_Delegate
            && baseType.SpecialType != SpecialType.System_MulticastDelegate;

    /// <summary>
    /// Types of the properties reachable from the root through (non-collection) member paths, e.g. <c>o.a.b</c>.
    /// Only types with generated descriptors are traversed (opaque, explicitly described, nullable and built-in
    /// types are not).
    /// </summary>
    private static HashSet<ITypeSymbol> GetReachablePropertyTypes(
        CompilationContext compilation,
        ITypeSymbol root,
        IReadOnlyDictionary<ITypeSymbol, ITypeData> targetTypes,
        CancellationToken cancellationToken)
    {
        var result = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default) { root };
        var pending = new Queue<ITypeSymbol>();
        pending.Enqueue(root);
        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var type = pending.Dequeue();
            if (!(targetTypes.TryGetValue(type, out var data) && data is TypeDataV2 { IsOpaque: false, Properties.Count: > 0 })
                || type.IsNullableStruct)
            {
                continue;
            }
            foreach (var prop in TypeDataV2.GetPropertiesRecursive(compilation, type))
            {
                var propType = prop.Type;
                // NOTE: sorting/selecting by collection is not supported
                if (compilation.IsCollection(propType))
                {
                    continue;
                }
                result.Add(propType);
                if (visited.Add(propType))
                {
                    pending.Enqueue(propType);
                }
            }
        }
        return result;
    }

    private ProtocolContextTargetV2 ExtractTarget(
        SemanticModel semanticModel,
        ClassDeclarationSyntax cds,
        INamedTypeSymbol symbol,
        CancellationToken cancellationToken)
    {
        var compilation = semanticModel.Compilation;
        var cctx = new CompilationContext(semanticModel, compilation);

        var rootEntityTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var lambdaTypes = new HashSet<LambdaTypeSymbols>();
        var descriptors = new Dictionary<ITypeSymbol, ITypeSymbol>(SymbolEqualityComparer.Default);
        var opaqueTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var opaqueLocations = new Dictionary<ITypeSymbol, Location?>(SymbolEqualityComparer.Default);
        var genMode = GenMode.Predicates | GenMode.Enumerable;
        var safeNames = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default);

        foreach (var attributeData in symbol.GetAttributes())
        {
            try
            {
                ProcessAttribute(attributeData);
            }
            // NOTE: errors raised while processing attribute are reported at the attribute.
            catch (GenerationException exn) when (exn.DiagnosticData.Location is null)
            {
                throw new GenerationException(exn.DiagnosticData.WithLocation(GetAttributeLocation(attributeData)));
            }
            catch (InvalidOperationException exn) when (exn is not GenerationException)
            {
                throw new GenerationException(new DiagnosticData(
                    DiagnosticDescriptors.InvalidAttribute,
                    GetAttributeLocation(attributeData),
                    [attributeData.AttributeClass?.Name, exn.Message]
                ));
            }
        }

        Location? GetAttributeLocation(AttributeData attributeData)
            => attributeData.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation();

        void ProcessAttribute(AttributeData attributeData)
        {
            if (cctx.IsEntity(attributeData, out var entityType))
            {
                rootEntityTypes.Add(entityType);
            }
            else if (cctx.IsLambda(attributeData, out var argType, out var resType))
            {
                lambdaTypes.Add(new(argType, resType));
            }
            else if (cctx.IsDescriptor(attributeData, out var descriptorType, out var describedType))
            {
                if (descriptors.TryGetValue(describedType, out var prev))
                {
                    throw new GenerationException(new DiagnosticData(
                        DiagnosticDescriptors.DuplicateExplicitDescriptor,
                        GetAttributeLocation(attributeData),
                        [
                            describedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            prev.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            descriptorType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                        ]
                    ));
                }
                descriptors.Add(describedType, descriptorType);
            }
            else if (cctx.IsOpaque(attributeData, out var opaqueType))
            {
                opaqueTypes.Add(opaqueType);
                opaqueLocations[opaqueType] = GetAttributeLocation(attributeData);
            }
            else if (cctx.IsOptions(attributeData, out var explicitMode))
            {
                genMode = explicitMode;
            }
            else if (cctx.IsSafeName(attributeData, out var targetType, out var safeName))
            {
                if (safeNames.TryGetValue(targetType, out var prevSafeName))
                {
                    throw new GenerationException(new DiagnosticData(
                        DiagnosticDescriptors.DuplicateSafeName,
                        GetAttributeLocation(attributeData),
                        [targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), prevSafeName, safeName]
                    ));
                }
                safeNames.Add(targetType, safeName);
            }
        }

        // var nullableT = compilation.GetSpecialType(SpecialType.System_Nullable_T)!;
        // var enumerableT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T)!;
        // var readOnlyListT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyList_T)!;
        // var func2T = compilation.GetTypeByMetadataName("System.Func`2") ?? throw new InvalidOperationException("Unable to get type symbol for System.Func<>.");
        var builtInTypes = GetBuiltInTypes(compilation, cctx.nullable);
        var targetTypes = new Dictionary<ITypeSymbol, ITypeData>(SymbolEqualityComparer.Default);
        AddPrimitiveValueTypes(compilation, targetTypes);
        var pending = new Queue<(ITypeSymbol Symbol, bool Root, bool FromProperty)>(rootEntityTypes.Select(ty => (ty, true, false)));
        string? safeNameFactory(ITypeSymbol ty) => safeNames.TryGetValue(ty, out var name) ? name : default;

        // explicitly described types are known types with user supplied descriptors: no descriptor is generated for
        // them but they can be referenced by other types (properties, elements, lambdas).
        foreach (var kv in descriptors)
        {
            var (describedType, descriptorType) = (kv.Key, kv.Value);
            if (opaqueTypes.Contains(describedType))
            {
                throw new GenerationException(new DiagnosticData(
                    DiagnosticDescriptors.OpaqueAndExplicitlyDescribed,
                    opaqueLocations.TryGetValue(describedType, out var opaqueLocation) ? opaqueLocation : default,
                    [
                        describedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        descriptorType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    ]
                ));
            }
            targetTypes[describedType] = new ExplicitTypeData(
                fullName: describedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                safeName: TypeDataV2.GetSafeName(describedType, safeNameFactory),
                isValueType: describedType.IsValueType,
                descriptorFullName: descriptorType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );
            pending.Enqueue(describedType, root: true);
        }

        // types reached through properties (including elements of collection properties), used to distinguish base
        // types that are only present as base types.
        var propertyReachedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        // PASS 1: collect types starting from root
        cancellationToken.ThrowIfCancellationRequested();
        AddTargetTypes(
            pending: pending,
            compilation: cctx,
            mode: genMode,
            targetTypes: targetTypes,
            opaqueTypes: opaqueTypes,
            builtin: builtInTypes,
            safeNameFactory: safeNameFactory,
            propertyReachedTypes: propertyReachedTypes,
            cancellationToken: cancellationToken
        );
        // PASS 2: find indirect types (type args)
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var kv in targetTypes)
        {
            var type = kv.Key;
            if (cctx.TryGetEnumerableElementType(type, out var elementSymbol, out _)
                || cctx.TryGetArrayElementType(type, out elementSymbol)
                || cctx.TryGetNullableElementType(type, out elementSymbol))
            {
                pending.Enqueue(elementSymbol);
            }
            else if (cctx.TryGetLambdaTypes(type, out var argType, out var resType))
            {
                pending.Enqueue(argType);
                pending.Enqueue(resType);
            }
        }
        AddTargetTypes(
            pending: pending,
            compilation: cctx,
            mode: genMode,
            targetTypes: targetTypes,
            opaqueTypes: opaqueTypes,
            builtin: builtInTypes,
            safeNameFactory: safeNameFactory,
            propertyReachedTypes: propertyReachedTypes,
            cancellationToken: cancellationToken
        );
        // all generated names (descriptors, enum factories, parse accessors) are derived from safe names --> these must
        // be unique.
        cancellationToken.ThrowIfCancellationRequested();
        EnsureSafeNamesAreUnique(targetTypes.Values, symbol.Locations.FirstOrDefault());
        // PASS 3: connect types
        cancellationToken.ThrowIfCancellationRequested();

        SomeType GetSomeType(ITypeSymbol requestedType)
        {
            if (targetTypes.TryGetValue(requestedType, out var data))
            {
                return new(data);
            }
            return new(requestedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }

        foreach (var kv in targetTypes)
        {
            if (kv.Value is TypeDataV2 data)
            {
                var type = kv.Key;
                // NOTE: collect base types
                var assignableTos = new HashSet<SomeType>(SomeType.ByNameEqualityComparer);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void AddAssignableTo(ITypeSymbol atype) => assignableTos.Add(GetSomeType(atype));

                var baseType = type;
                while (baseType is not null && baseType.SpecialType != SpecialType.System_Object
                    && baseType.SpecialType != SpecialType.System_ValueType
                    && baseType.SpecialType != SpecialType.System_Enum)
                {
                    AddAssignableTo(baseType);
                    baseType = baseType.BaseType;
                }
                if (type.IsValueType && !type.IsNullableStruct)
                {
                    var ntype = cctx.CreateNullableTypeSymbol(type);
                    if (targetTypes.TryGetValue(ntype, out var btype))
                    {
                        assignableTos.Add(new(btype));
                    }
                    else
                    {
                        assignableTos.Add(new(ntype.ToDisplayString(MaybeNullableFullyQualifiedFormat)));
                    }
                }
                data.AssignableToTypes = assignableTos;
                // ------------------------------------------------------
                if (cctx.TryGetArrayElementType(type, out var elementSymbol))
                {
                    if (!targetTypes.TryGetValue(elementSymbol, out var elementData))
                    {
                        throw new GenerationException(new DiagnosticData(
                            DiagnosticDescriptors.TypeNotFoundArrayElement,
                            default,
                            [elementSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)]
                        ));
                    }
                    AddAssignableTo(cctx.CreateIEnumerableTypeSymbol(elementSymbol));
                    AddAssignableTo(cctx.CreateIReadOnlyListTypeSymbol(elementSymbol));
                    data.ElementType = elementData;
                }
                else if (cctx.TryGetEnumerableElementType(type, out elementSymbol, out var isIEnumerable))
                {
                    if (!targetTypes.TryGetValue(elementSymbol, out var elementData))
                    {
                        throw new GenerationException(new DiagnosticData(
                            DiagnosticDescriptors.TypeNotFoundEnumerableElement,
                            default,
                            [
                                elementSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                                string.Join(", ", targetTypes.Keys.Select(static key => key.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)))
                            ]
                        ));
                    }
                    if (!isIEnumerable)
                    {
                        AddAssignableTo(cctx.CreateIEnumerableTypeSymbol(elementSymbol));
                    }
                    data.ElementType = elementData;
                }
                else if (cctx.TryGetNullableElementType(type, out var nullableSymbol))
                {
                    data.UnderlyingType = targetTypes.TryGetValue(nullableSymbol, out var nullableData)
                        ? new SomeType(nullableData)
                        : new SomeType(nullableSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }
                else if (cctx.TryGetLambdaTypes(type, out var argType, out var resType))
                {
                    if (!targetTypes.TryGetValue(argType, out var argData))
                    {
                        throw new GenerationException(new DiagnosticData(
                            DiagnosticDescriptors.TypeNotFoundLambdaArgument,
                            default,
                            [argType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)]
                        ));
                    }
                    if (!targetTypes.TryGetValue(resType, out var resData))
                    {
                        throw new GenerationException(new DiagnosticData(
                            DiagnosticDescriptors.TypeNotFoundLambdaResult,
                            default,
                            [resType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)]
                        ));
                    }
                    data.LambdaArg = new(argData);
                    data.LambdaRes = new(resData);
                }
            }
        }
        // PASS 4: collect lambda types
        cancellationToken.ThrowIfCancellationRequested();
        var defaultLambdaTypeArgs = GetDefaultLambdaTypeArgs(compilation, cctx.nullable);

        void AddLambda(ITypeSymbol argType, ITypeSymbol resType)
        {
            // NOTE: PortableDataUtils already contains lambda types for these pairs
            if (!(defaultLambdaTypeArgs.Contains(argType) && defaultLambdaTypeArgs.Contains(resType)))
            {
                lambdaTypes.Add(new(argType, resType));
            }
        }

        // types only present as base types of other types (not entities, not reached through properties): lambda
        // types are only generated for them if requested.
        var baseOnlyTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        if (!genMode.HasFlag(GenMode.BaseTypes))
        {
            foreach (var type in targetTypes.Keys)
            {
                for (var baseType = type.BaseType; IsQueryableBaseType(baseType); baseType = baseType.BaseType)
                {
                    if (!rootEntityTypes.Contains(baseType) && !propertyReachedTypes.Contains(baseType))
                    {
                        baseOnlyTypes.Add(baseType);
                    }
                }
            }
        }

        // (1) every lambda type known to the context (predicates of the entities, elements, properties, explicitly
        // listed lambda entities) except for collection, lambda, explicitly described and base-only arguments (these
        // should be registered via ProtocolLambdaAttribute or ProtocolGenerationMode.BaseTypes).
        foreach (var type in targetTypes.Keys)
        {
            if (cctx.TryGetLambdaTypes(type, out var argType, out var resType)
                && !cctx.IsCollection(argType)
                && !cctx.IsLambda(argType)
                && !descriptors.ContainsKey(argType)
                && !baseOnlyTypes.Contains(argType))
            {
                AddLambda(argType, resType);
            }
        }
        // (2) predicates of the built-in types not covered by defaults
        foreach (var biType in builtInTypes)
        {
            AddLambda(biType, cctx.@bool);
        }
        // (3) root lambdas (filtering, sorting, selection): root -> built-in types (expression/function results) and
        // root -> types of the properties reachable from root (sorting by property).
        // NOTE: collection roots (e.g. List<X> -> Y) and explicitly described types are not included, lambda types
        // for them should be registered via ProtocolLambdaAttribute.
        var queryRoots = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var root in rootEntityTypes)
        {
            if (cctx.IsLambda(root) || cctx.IsCollection(root) || descriptors.ContainsKey(root))
            {
                continue;
            }
            queryRoots.Add(root);
            if (genMode.HasFlag(GenMode.BaseTypes))
            {
                for (var baseType = root.BaseType; IsQueryableBaseType(baseType); baseType = baseType.BaseType)
                {
                    if (!descriptors.ContainsKey(baseType))
                    {
                        queryRoots.Add(baseType);
                    }
                }
            }
        }
        // NOTE: excluding rarely used result types (byte, sbyte, short, ushort) was measured to have no significant
        // effect (~0.5 KiB of NativeAOT binary size per lambda type) --> all built-in types are included.
        foreach (var root in queryRoots)
        {
            foreach (var resType in builtInTypes)
            {
                AddLambda(root, resType);
            }
            foreach (var resType in GetReachablePropertyTypes(cctx, root, targetTypes, cancellationToken))
            {
                AddLambda(root, resType);
            }
        }

        // FINALIZE
        cancellationToken.ThrowIfCancellationRequested();
        var name = cds.Identifier.ValueText;
        var @namespace = Helpers.GetSyntaxNamespace(cds) ?? "NCoreUtils.Data.Proto";
        return new(
            @namespace: @namespace,
            name: name,
            types: new HashSet<ITypeData>(targetTypes.Values, TypeDataEqualityComparer.Singleton),
            lambdaTypes: [.. lambdaTypes.Select(tup => (
                GetSomeType(tup.ArgType),
                GetSomeType(tup.ResType)
            ))]
        );
    }


    private TargetOrError GetTarget(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
    {
        var symbol = ctx.TargetSymbol;
        var location = symbol.Locations.FirstOrDefault();
        try
        {
            if (!ctx.SemanticModel.Compilation.HasLanguageVersionAtLeastEqualTo(LanguageVersion.CSharp10, out _))
            {
                return new DiagnosticData(DiagnosticDescriptors.UnsupportedCSharpVersion, location, []);
            }
            if (symbol is not INamedTypeSymbol namedTypeSymbol)
            {
                return new DiagnosticData(DiagnosticDescriptors.InvalidTargetSyntax, location, []);
            }
            if (ctx.TargetNode is not ClassDeclarationSyntax cds)
            {
                return new DiagnosticData(DiagnosticDescriptors.InvalidTargetSyntax, ctx.TargetNode.GetLocation(), []);
            }
            return new(ExtractTarget(ctx.SemanticModel, cds, namedTypeSymbol, cancellationToken));
        }
        catch (GenerationException exn)
        {
            // NOTE: errors without own location (e.g. unresolved types) are reported at the context class.
            return exn.DiagnosticData.Location is null
                ? exn.DiagnosticData.WithLocation(location)
                : exn.DiagnosticData;
        }
        catch (Exception exn) when (exn is not OperationCanceledException)
        {
            return DiagnosticData.UnexpectedError(exn, location);
        }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // context.SyntaxProvider.CreateSyntaxProvider(
        //     predicate: (node, _) => node is ClassDeclarationSyntax,
        //     transform:
        // )

        context.RegisterPostInitializationOutput(context => context.AddSource("ProtocolContextGeneratorAttributes.g.cs", SourceText.From(attributeSource, Utf8)));

        var targets0 = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "NCoreUtils.Data.Protocol.ProtocolEntityAttribute",
            predicate: (node, _) => node is ClassDeclarationSyntax,
            transform: GetTarget
        );

        var errors = targets0.Where(t => t.Error is not null).Select((t, _) => t.Error!);

        var targets = targets0.Where(t => t.Target is not null);

        // NOTE: RegisterSourceOutput (unlike RegisterImplementationSourceOutput) also runs in IDE --> errors are shown
        // while editing.
        context.RegisterSourceOutput(errors, (ctx, err) =>
        {
           ctx.ReportDiagnostic(Diagnostic.Create(err.Descriptor, err.Location, err.MessageArgs));
        });

        context.RegisterSourceOutput(targets, (ctx, wrp) =>
        {
            var target = wrp.Target!;
            try
            {
                var unit = ProtocolContextEmitterV2.EmitContext(
                    target.Namespace,
                    target.Name,
                    target.Types,
                    target.LambdaTypes
                );
                // NOTE: hint name must be unique within generator --> contexts with the same name may exist in
                // different namespaces.
                ctx.AddSource($"{target.Namespace}.{target.Name}.g.cs", unit.GetText(Utf8));
            }
            catch (GenerationException exn)
            {
                var err = exn.DiagnosticData;
                ctx.ReportDiagnostic(Diagnostic.Create(
                    descriptor: err.Descriptor,
                    location: err.Location,
                    messageArgs: err.MessageArgs
                ));
            }
            catch (Exception exn) when (exn is not OperationCanceledException)
            {
                // NOTE: no source is added --> partial output is never produced.
                var err = DiagnosticData.UnexpectedError(exn, default);
                ctx.ReportDiagnostic(Diagnostic.Create(err.Descriptor, err.Location, err.MessageArgs));
            }
        });
    }
}