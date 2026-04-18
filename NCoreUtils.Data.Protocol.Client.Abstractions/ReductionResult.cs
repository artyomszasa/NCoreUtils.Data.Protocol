using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol;

internal readonly struct Buffer64(double value) : IEquatable<Buffer64>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator==(Buffer64 a, Buffer64 b) => a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator!=(Buffer64 a, Buffer64 b) => !a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Buffer64 PackBoolean(bool value)
    {
        double tmp = 0.0;
        Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref tmp), value);
        return new(tmp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Buffer64 PackInt16(short value)
    {
        double tmp = 0.0;
        Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref tmp), value);
        return new(tmp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Buffer64 PackInt32(int value)
    {
        double tmp = 0.0;
        Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref tmp), value);
        return new(tmp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Buffer64 PackInt64(long value)
    {
        double tmp = 0.0;
        Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref tmp), value);
        return new(tmp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Buffer64 PackFloat32(float value)
    {
        double tmp = 0.0;
        Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref tmp), value);
        return new(tmp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Buffer64 PackFloat64(double value)
        => new(value);

    /// <summary>
    /// Used to store any numeric/boolean data (not just double), may be considered as fixed 8 byte buffer;
    /// </summary>
    // FIXME: once relevant should be rewritted to generated fix-size array!
    internal readonly double _value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Buffer64 other) => _value == other._value;

    public override bool Equals(object? obj)
        => obj is Buffer64 other && Equals(other);

    public override int GetHashCode()
        => _value.GetHashCode();
}

internal static class Buffer64Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool UnpackBoolean(this in Buffer64 buffer)
        => Unsafe.ReadUnaligned<bool>(ref Unsafe.As<double, byte>(ref Unsafe.AsRef(in buffer._value)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short UnpackInt16(this in Buffer64 buffer)
        => Unsafe.ReadUnaligned<short>(ref Unsafe.As<double, byte>(ref Unsafe.AsRef(in buffer._value)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int UnpackInt32(this in Buffer64 buffer)
        => Unsafe.ReadUnaligned<int>(ref Unsafe.As<double, byte>(ref Unsafe.AsRef(in buffer._value)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long UnpackInt64(this in Buffer64 buffer)
        => Unsafe.ReadUnaligned<long>(ref Unsafe.As<double, byte>(ref Unsafe.AsRef(in buffer._value)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float UnpackFloat32(this in Buffer64 buffer)
        => Unsafe.ReadUnaligned<float>(ref Unsafe.As<double, byte>(ref Unsafe.AsRef(in buffer._value)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double UnpackFloat64(this in Buffer64 buffer)
        => buffer._value;
}

public readonly struct ReductionResult<T> : IEquatable<ReductionResult<T>>
{
    private enum Tag
    {
        Null = 0,
        Boolean,
        Int16,
        Int32,
        Int64,
        Float32,
        Float64,
        Item
    }

    public static bool operator==(ReductionResult<T> a, ReductionResult<T> b) => a.Equals(b);

    public static bool operator!=(ReductionResult<T> a, ReductionResult<T> b) => !a.Equals(b);

    private const MethodImplOptions Opt =
#if NET6_0_OR_GREATER
        MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining
#else
        MethodImplOptions.AggressiveInlining
#endif
        ;

    public static readonly ReductionResult<T> Null = default;

    [MethodImpl(Opt)]
    public static ReductionResult<T> Boolean(bool value) => new(Tag.Boolean, Buffer64.PackBoolean(value), default);

    [MethodImpl(Opt)]
    public static ReductionResult<T> Int16(short value) => new(Tag.Int16, Buffer64.PackInt16(value), default);

    [MethodImpl(Opt)]
    public static ReductionResult<T> Int32(int value) => new(Tag.Int32, Buffer64.PackInt32(value), default);

    [MethodImpl(Opt)]
    public static ReductionResult<T> Int64(long value) => new(Tag.Int64, Buffer64.PackInt64(value), default);

    [MethodImpl(Opt)]
    public static ReductionResult<T> Single(float value) => new(Tag.Float32, Buffer64.PackFloat32(value), default);

    [MethodImpl(Opt)]
    public static ReductionResult<T> Double(double value) => new(Tag.Float64, Buffer64.PackFloat64(value), default);

    [MethodImpl(Opt)]
    public static ReductionResult<T> Item(T? value) => new(Tag.Item, default, value);

    private readonly Buffer64 _numValue;

    private readonly T? _itemValue;

    private readonly Tag _tag;

    public bool BooleanValue
    {
        [MethodImpl(Opt)]
        get => _tag == Tag.Boolean ? _numValue.UnpackBoolean() : throw new InvalidCastException($"Result value with type {_tag} cannot be cast to Boolean.");
    }

    public short Int16Value
    {
        [MethodImpl(Opt)]
        get => _tag == Tag.Int16 ? _numValue.UnpackInt16() : throw new InvalidCastException($"Result value with type {_tag} cannot be cast to Int16.");
    }

    public int Int32Value
    {
        [MethodImpl(Opt)]
        get => _tag == Tag.Int32 ? _numValue.UnpackInt32() : throw new InvalidCastException($"Result value with type {_tag} cannot be cast to Int32.");
    }

    public long Int64Value
    {
        [MethodImpl(Opt)]
        get => _tag == Tag.Int64 ? _numValue.UnpackInt64() : throw new InvalidCastException($"Result value with type {_tag} cannot be cast to Int64.");
    }

    public float SingleValue
    {
        [MethodImpl(Opt)]
        get => _tag == Tag.Float32 ? _numValue.UnpackFloat32() : throw new InvalidCastException($"Result value with type {_tag} cannot be cast to Single.");
    }

    public double DoubleValue
    {
        [MethodImpl(Opt)]
        get => _tag == Tag.Float64 ? _numValue.UnpackFloat64() : throw new InvalidCastException($"Result value with type {_tag} cannot be cast to Double.");
    }

    public T? ItemValue
    {
        [MethodImpl(Opt)]
        get => _tag == Tag.Item ? _itemValue : throw new InvalidCastException($"Result value with type {_tag} cannot be cast to Item.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReductionResult(Tag tag, in Buffer64 numValue, T? itemValue)
    {
        _numValue = numValue;
        _itemValue = itemValue;
        _tag = tag;
    }

    public bool Equals(ReductionResult<T> other) => _tag switch
    {
        Tag.Null => other._tag == Tag.Null,
        Tag.Item => other._tag == Tag.Item && true == _itemValue?.Equals(other), // FIXME: find a way to avoid boxing when T : struct
        var tag => other._tag == tag && _numValue == other._numValue
    };

    public override bool Equals(object? obj)
        => obj is ReductionResult<T> other && Equals(other);

    public override int GetHashCode() => _tag switch
    {
        Tag.Null => 0,
        Tag.Item => ((int)Tag.Item << 24) | ((_itemValue?.GetHashCode() ?? 0) & 0xFFFFFF),
        _ => ((int)_tag << 24) | (_numValue.GetHashCode() & 0xFFFFFF)
    };

    public bool TryGetValue<TResult>(out TResult? value)
    {
        if (typeof(TResult) == typeof(int))
        {
            var tmp = Int32Value;
            value = Unsafe.As<int, TResult>(ref tmp);
            return true;
        }
        if (typeof(TResult) == typeof(bool))
        {
            var tmp = BooleanValue;
            value = Unsafe.As<bool, TResult>(ref tmp);
            return true;
        }
        if (typeof(TResult) == typeof(T))
        {
            if (typeof(TResult).IsValueType)
            {
                var tmp = ItemValue;
                value = Unsafe.As<T, TResult>(ref tmp!);
                return true;
            }
            var boxed = (object?)ItemValue;
            value = boxed is null ? default : (TResult?)boxed;
            return true;
        }
        if (typeof(TResult) == typeof(long))
        {
            var tmp = Int64Value;
            value = Unsafe.As<long, TResult>(ref tmp);
            return true;
        }
        if (typeof(TResult) == typeof(short))
        {
            var tmp = Int16Value;
            value = Unsafe.As<short, TResult>(ref tmp);
            return true;
        }
        if (typeof(TResult) == typeof(float))
        {
            var tmp = SingleValue;
            value = Unsafe.As<float, TResult>(ref tmp);
            return true;
        }
        if (typeof(TResult) == typeof(double))
        {
            var tmp = DoubleValue;
            value = Unsafe.As<double, TResult>(ref tmp);
            return true;
        }
        value = default;
        return false;
    }
}