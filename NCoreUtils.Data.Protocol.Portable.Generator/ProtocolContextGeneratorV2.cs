using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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

        public TimeSpan CollectionDuration { get; }

        public ProtocolContextTargetV2? Target { get; }

        public DiagnosticData? Error { get; }

        private TargetOrError(ProtocolContextTargetV2? target, DiagnosticData? error, TimeSpan collectionDuration)
        {
            if (target is null && error is null)
            {
                throw new InvalidOperationException("Either target or error must be not null.");
            }
            Target = target;
            Error = error;
            CollectionDuration = collectionDuration;
        }

        public TargetOrError(ProtocolContextTargetV2 target, TimeSpan collectionDuration) : this(target, default, collectionDuration) { }

        public TargetOrError(DiagnosticData error) : this(default, error, default) { }

        public bool Equals(TargetOrError other)
        {
            if (Target is ProtocolContextTargetV2 target)
            {
                return other.Target is ProtocolContextTargetV2 otherTarget && target == otherTarget;
            }
            return false;
        }
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

    private static void AddPrimitiveValueType(Compilation compilation, Dictionary<ITypeSymbol, ITypeData> types, SpecialType specialType)
    {
        if (CommonPrimitiveValueTypes.TryGetValue(specialType, out var type))
        {
            types.Add(compilation.GetSpecialType(specialType), type);
        }
    }

    private static void AddPrimitiveValueType(Compilation compilation, Dictionary<ITypeSymbol, ITypeData> types, string qualifiedName, string name)
    {
        if (compilation.GetTypeByMetadataName(qualifiedName) is ITypeSymbol symbol)
        {
            // FIXME: cache
            types.Add(symbol, new PrimitiveValueTypeData(qualifiedName, name));
        }
    }

    private static (SpecialType? TypeData, Action<Compilation, Dictionary<ITypeSymbol, ITypeData>>? Factory) GetPrimitiveType(ProtocolPrimitiveType key) => key switch
    {
        ProtocolPrimitiveType.Boolean => (SpecialType.System_Boolean, null),
        ProtocolPrimitiveType.Guid => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "global::System.Guid", "Guid")),
        ProtocolPrimitiveType.DateTime => (SpecialType.System_DateTime, null),
        ProtocolPrimitiveType.DateTimeOffset => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "System.DateTimeOffset", "DateTimeOffset")),
        ProtocolPrimitiveType.SByte => (SpecialType.System_SByte, null),
        ProtocolPrimitiveType.Int16 => (SpecialType.System_Int16, null),
        ProtocolPrimitiveType.Int32 => (SpecialType.System_Int32, null),
        ProtocolPrimitiveType.Int64 => (SpecialType.System_Int64, null),
        ProtocolPrimitiveType.Int128 => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "global::System.Int128", "Int128")),
        ProtocolPrimitiveType.Byte => (SpecialType.System_Byte, null),
        ProtocolPrimitiveType.UInt16 => (SpecialType.System_UInt16, null),
        ProtocolPrimitiveType.UInt32 => (SpecialType.System_UInt32, null),
        ProtocolPrimitiveType.UInt64 => (SpecialType.System_UInt64, null),
        ProtocolPrimitiveType.UInt128 => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "global::System.UInt128", "UInt128")),
        ProtocolPrimitiveType.Half => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "global::System.Half", "Half")),
        ProtocolPrimitiveType.Single => (SpecialType.System_Single, null),
        ProtocolPrimitiveType.Double => (SpecialType.System_Double, null),
        ProtocolPrimitiveType.Decimal => (SpecialType.System_Decimal, null),
        ProtocolPrimitiveType.DateOnly => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "global::System.DateOnly", "DateOnly")),
        ProtocolPrimitiveType.TimeOnly => (null, static (compilation, types) => AddPrimitiveValueType(compilation, types, "global::System.TimeOnly", "TimeOnly")),
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
                types.Add(compilation.GetSpecialType(stype), CommonPrimitiveValueTypes[stype]);
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
        Queue<(ITypeSymbol Symbol, bool Root)> pending,
        CompilationContext compilation,
        GenMode mode,
        IDictionary<ITypeSymbol, ITypeData> targetTypes,
        HashSet<ITypeSymbol> opaqueTypes,
        HashSet<ITypeSymbol> builtin,
        Func<ITypeSymbol, bool> isExplicitlyDescribed,
        Func<ITypeSymbol, string?> safeNameFactory,
        CancellationToken cancellationToken)
    {
        while (pending.TryDequeue(out var symbol, out var root))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (builtin.Contains(symbol) || targetTypes.ContainsKey(symbol) || isExplicitlyDescribed(symbol))
            {
                continue;
            }
            if (root)
            {
                // add array type if not already present
                if (mode.HasFlag(GenMode.Array))
                {
                    pending.Enqueue(compilation.CreateArrayTypeSymbol(symbol));
                }
                // add enumerable type if not already present
                if (mode.HasFlag(GenMode.Enumerable))
                {
                    pending.Enqueue(compilation.CreateIEnumerableTypeSymbol(symbol));
                }
                // add predicate lambda type
                if (mode.HasFlag(GenMode.Predicates))
                {
                    pending.Enqueue(compilation.CreateFuncTypeSymbol(symbol, compilation.@bool));
                }
            }
            var isOpaque = opaqueTypes.Contains(symbol);
            TypeDataV2 data;
            if (symbol is INamedTypeSymbol namedSymbol)
            {
                data = TypeDataV2.Create(compilation, symbol, isOpaque, safeNameFactory, out var elementType, out var properties);
                // throw new InvalidOperationException(string.Join(", ", properties.Select(p => p.Name)));
                targetTypes.Add(namedSymbol, data);
                if (isOpaque)
                {
                    return;
                }
                if (data.IsValueType)
                {
                    if (!data.IsNullable && mode.HasFlag(GenMode.Nullable))
                    {
                        pending.Enqueue(compilation.CreateNullableTypeSymbol(namedSymbol), root: true);
                        // AddTargetType(compilation, nullableT.Construct(namedSymbol), mode, true, targetTypes, opaqueTypes, nullableT, enumerableT, func2T, builtin, isExplicitlyDescribed);
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
                        // AddTargetType(compilation, baseType, mode, true, targetTypes, opaqueTypes, nullableT, enumerableT, func2T, builtin, isExplicitlyDescribed);
                    }
                }
                if (data.IsEnumerable)
                {
                    var enumerableSymbol = compilation.CreateIEnumerableTypeSymbol(elementType!);
                    if (!SymbolEqualityComparer.Default.Equals(namedSymbol, enumerableSymbol))
                    {
                        // add IEnumerable<T> for types implementing it!
                        pending.Enqueue(enumerableSymbol);
                        // AddTargetType(compilation, enumerableSymbol, mode, false, targetTypes, opaqueTypes, nullableT, enumerableT, func2T, builtin, isExplicitlyDescribed);
                    }
                }
                foreach (var prop in properties)
                {
                    pending.Enqueue(prop.Type);
                    // AddTargetType(compilation, prop.Type, mode, true, targetTypes, opaqueTypes, nullableT, enumerableT, func2T, builtin, isExplicitlyDescribed);
                }
            }
            else if (symbol is IArrayTypeSymbol arraySymbol)
            {
                data = TypeDataV2.Create(compilation, arraySymbol, isOpaque, safeNameFactory, out var elementType, out _);
                targetTypes.Add(arraySymbol, data);
                pending.Enqueue(elementType!);
                // AddTargetType(compilation, arraySymbol.ElementType, mode, true, targetTypes, opaqueTypes, nullableT, enumerableT, func2T, builtin, isExplicitlyDescribed);
                // add IEnumerable<T> for array types!
                pending.Enqueue(compilation.CreateIEnumerableTypeSymbol(elementType!));
                // AddTargetType(compilation, enumerableT.Construct(arraySymbol.ElementType), mode, false, targetTypes, opaqueTypes, nullableT, enumerableT, func2T, builtin, isExplicitlyDescribed);
            }
            else
            {
                throw new Exception($"Not handled type: {symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}");
            }
        }
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
        var genMode = GenMode.Predicates | GenMode.Enumerable;
        var safeNames = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default);

        foreach (var attributeData in symbol.GetAttributes())
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
                    // FIXME: CompilationExeption
                    throw new InvalidOperationException($"{describedType.Name} already has explicit descriptor {prev.Name}.");
                }
                descriptors.Add(describedType, descriptorType);
            }
            else if (cctx.IsOpaque(attributeData, out var opaqueType))
            {
                rootEntityTypes.Add(opaqueType);
            }
            else if (cctx.IsOptions(attributeData, out var explicitMode))
            {
                genMode = explicitMode;
            }
            else if (cctx.IsSafeName(attributeData, out var targetType, out var safeName))
            {
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
        var pending = new Queue<(ITypeSymbol Symbol, bool Root)>(rootEntityTypes.Select(ty => (ty, true)));
        string? safeNameFactory(ITypeSymbol ty) => safeNames.TryGetValue(ty, out var name) ? name : default;

        // PASS 1: collect types starting from root
        cancellationToken.ThrowIfCancellationRequested();
        AddTargetTypes(
            pending: pending,
            compilation: cctx,
            mode: genMode,
            targetTypes: targetTypes,
            opaqueTypes: opaqueTypes,
            builtin: builtInTypes,
            isExplicitlyDescribed: descriptors.ContainsKey,
            safeNameFactory: safeNameFactory,
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
            isExplicitlyDescribed: descriptors.ContainsKey,
            safeNameFactory: safeNameFactory,
            cancellationToken: cancellationToken
        );
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
                var assignableTos = new HashSet<SomeType>();

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
                if (type.IsValueType)
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
                            DiagnosticDescriptors.TypeNotFound,
                            default,
                            [elementSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)]
                        ));
                    }
                    AddAssignableTo(cctx.CreateIEnumerableTypeSymbol(type));
                    AddAssignableTo(cctx.CreateIReadOnlyListTypeSymbol(type));
                    data.ElementType = elementData;
                }
                else if (cctx.TryGetEnumerableElementType(type, out elementSymbol, out var isIEnumerable))
                {
                    if (!targetTypes.TryGetValue(elementSymbol, out var elementData))
                    {
                        throw new GenerationException(new DiagnosticData(
                            DiagnosticDescriptors.TypeNotFound,
                            default,
                            [elementSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)]
                        ));
                    }
                    if (!isIEnumerable)
                    {
                        AddAssignableTo(cctx.CreateIEnumerableTypeSymbol(type));
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
                            DiagnosticDescriptors.TypeNotFound,
                            default,
                            [argType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)]
                        ));
                    }
                    if (!targetTypes.TryGetValue(resType, out var resData))
                    {
                        throw new GenerationException(new DiagnosticData(
                            DiagnosticDescriptors.TypeNotFound,
                            default,
                            [resType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)]
                        ));
                    }
                    data.LambdaArg = argData;
                    data.LambdaRes = resData;
                }
            }
        }
        // PASS 4: collect lambda types
        foreach (var type in targetTypes.Keys.Where(ty => !builtInTypes.Contains(ty) && !cctx.IsLambda(ty) && ty is not IArrayTypeSymbol))
        {
            foreach (var biType in builtInTypes)
            {
                lambdaTypes.Add(new (
                    argType: type,
                    resType: biType
                ));
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
            explicitDescriptors: descriptors.ToDictionary(
                kv => kv.Key.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                kv => new SomeType(kv.Value.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))),
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
            var stopwatch = Stopwatch.StartNew();
            var res = ExtractTarget(ctx.SemanticModel, cds, namedTypeSymbol, cancellationToken);
            stopwatch.Stop();
            return new(res, stopwatch.Elapsed);
        }
        catch (GenerationException exn)
        {
            return exn.DiagnosticData;
        }
        catch (Exception exn)
        {
            return new DiagnosticData(DiagnosticDescriptors.GenericError, location,
            [
                exn.GetType().Name,
                exn.Message,
                exn.StackTrace.Replace('\n', ' ').Replace('\r', ' ')
            ]);
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

        context.RegisterImplementationSourceOutput(errors, (ctx, err) =>
        {
           ctx.ReportDiagnostic(Diagnostic.Create(err.Descriptor, err.Location, err.MessageArgs));
        });

        context.RegisterSourceOutput(targets, (ctx, wrp) =>
        {
            var target = wrp.Target!;
            ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.DoneCollectingTypes, default, [wrp.CollectionDuration.TotalMilliseconds, target.Name]));
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var unit = ProtocolContextEmitterV2.EmitContext(
                    target.Namespace,
                    target.Name,
                    target.Types,
                    target.ExplicitDescriptors,
                    target.LambdaTypes,
                    ctx.ReportDiagnostic
                );
                ctx.AddSource($"{target.Name}.g.cs", unit.GetText(Utf8));
                stopwatch.Stop();
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
            catch (Exception exn)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    descriptor: DiagnosticDescriptors.GenericError,
                    location: default,
                    messageArgs: [exn.GetType().Name, exn.Message, exn.StackTrace.Replace('\n', ' ').Replace('\r', ' ')]
                ));
            }
        });
    }
}