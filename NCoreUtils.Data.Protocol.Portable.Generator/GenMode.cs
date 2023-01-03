using System;

namespace NCoreUtils.Data.Protocol.Generator;

[Flags]
internal enum GenMode
{
    Predicates = 0x01,
    Array = 0x02,
    Enumerable = 0x04,
    Nullable = 0x08
}