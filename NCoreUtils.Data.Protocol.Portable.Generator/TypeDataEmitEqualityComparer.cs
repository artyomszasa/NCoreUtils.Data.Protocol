using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class TypeDataEmitEqualityComparer : IEqualityComparer<TypeData>
{
    private static class B
    {
        public const uint AnyObject = 0x00000000u;

        public const uint ValueType = 0x00000001u;

        public const uint Array = 0x00000002u;

        public const uint Enum = 0x00000003u;

        public const uint Lambda = 0x00000004u;
    }

    private sealed class PropertyNameAndTypeEqualityComparer : IEqualityComparer<IPropertySymbol>
    {
        public bool Equals(IPropertySymbol x, IPropertySymbol y)
        {
            if (x is null)
            {
                return y is null;
            }
            if (y is null)
            {
                return false;
            }
            return Eq(x.Name, y.Name)
                && Eq(x.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), y.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }

        public int GetHashCode(IPropertySymbol obj)
            => obj.Name.GetHashCode() ^ obj.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).GetHashCode();
    }

    private static PropertyNameAndTypeEqualityComparer PropertyEqualityComparer { get; } = new();

    public static TypeDataEmitEqualityComparer Singleton { get; } = new();

    private static uint NameHash(string name)
        => name is null ? 0u : unchecked((uint)name.GetHashCode());

    private static uint PropertiesHash(IEnumerable<IPropertySymbol> properties)
    {
        var hash = 0u;
        foreach (var property in properties)
        {
            hash ^= NameHash(property.Name);
        }
        return hash;
    }

    private static int I(uint hash)
        => unchecked((int)hash);

    private static bool Eq(string? a, string? b)
        => StringComparer.InvariantCulture.Equals(a, b);

    private TypeDataEmitEqualityComparer() { /* noop */ }

    public bool Equals(TypeData? x, TypeData? y)
    {
        if (x is null)
        {
            return y is null;
        }
        if (y is null)
        {
            return false;
        }
        if (x.IsEnum)
        {
            return y.IsEnum && Eq(x.FullName, y.FullName);
        }
        if (x.IsArray)
        {
            return y.IsArray && Eq(y.ElementTypeFullName, y.ElementTypeFullName);
        }
        if (x.IsLambda)
        {
            return y.IsLambda
                && Eq(x.LambdaArgTypeFullName, y.LambdaArgTypeFullName)
                && Eq(x.LambdaResTypeFullName, y.LambdaResTypeFullName);
        }
        // generic class/struct
        if (x.IsValueType != y.IsValueType || y.Properties.Count != y.Properties.Count || !Eq(x.FullName, y.FullName))
        {
            return false;
        }
        // from usage point of view only names and types are relevant
#pragma warning disable RS1024
        return x.Properties.SequenceEqual(y.Properties, PropertyEqualityComparer);
#pragma warning restore RS1024

    }

    public int GetHashCode(TypeData obj)
    {
        if (obj is null)
        {
            return 0;
        }
        if (obj.IsEnum)
        {
            var nameHash = NameHash(obj.FullName);
            return I((nameHash << 4) | B.Enum);
        }
        if (obj.IsArray)
        {
            var nameHash = NameHash(obj.ElementTypeFullName);
            return I((nameHash << 4) | B.Array);
        }
        if (obj.IsLambda)
        {
            var argHash = NameHash(obj.LambdaArgTypeFullName);
            var resHash = NameHash(obj.LambdaResTypeFullName);
            return I(((argHash ^ resHash) << 4) | B.Lambda);
        }
        {
            // generic class/struct
            var nameHash = NameHash(obj.FullName);
            var propertiesHash = PropertiesHash(obj.Properties);
            return I(((nameHash ^ propertiesHash) << 4) | (obj.IsValueType ? B.ValueType : B.AnyObject));
        }
    }
}