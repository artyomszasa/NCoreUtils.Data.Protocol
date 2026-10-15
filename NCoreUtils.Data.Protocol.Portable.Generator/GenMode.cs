namespace NCoreUtils.Data.Protocol.Generator;

[Flags]
internal enum GenMode
{
    Predicates = 0x01,
    Array = 0x02,
    Enumerable = 0x04,
    Nullable = 0x08,
    /// <summary>
    /// Generate lambda types for base types of the entities (descriptors of base types are always generated).
    /// </summary>
    BaseTypes = 0x10
}