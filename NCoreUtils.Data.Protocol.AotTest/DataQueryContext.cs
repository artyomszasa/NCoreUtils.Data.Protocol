using System;

namespace NCoreUtils.Data.Protocol;

[ProtocolEntity(typeof(DataEntity))]
[ProtocolEntity(typeof(Func<DataEntity, int>))]
public partial class DataQueryContext { }