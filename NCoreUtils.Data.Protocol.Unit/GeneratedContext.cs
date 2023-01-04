using System;
using System.Collections.Generic;

namespace NCoreUtils.Data.Protocol.Unit;

[ProtocolEntity(typeof(Func<int, bool>))]
[ProtocolEntity(typeof(Func<string, bool>))]
[ProtocolEntity(typeof(Func<DateTimeOffset, bool>))]
[ProtocolEntity(typeof(Func<AOrB, bool>))]
[ProtocolEntity(typeof(Func<SubItem, bool>))]
[ProtocolEntity(typeof(Func<Item, bool>))]
[ProtocolEntity(typeof(ItemWithEnum))]
[ProtocolEntity(typeof(ItemWithNullableInt32))]
[ProtocolEntity(typeof(ItemWithNullableDateTimeOffset))]
[ProtocolEntity(typeof(Item))]
[ProtocolLambda(typeof(Item), typeof(Func<string, bool>))]
[ProtocolLambda(typeof(Item), typeof(Func<SubItem, bool>))]
[ProtocolEntity(typeof(DerivedEntity))]
[ProtocolEntity(typeof(List<int>))]
[ProtocolEntity(typeof(HashSet<int>))]
public partial class GeneratedContext { }