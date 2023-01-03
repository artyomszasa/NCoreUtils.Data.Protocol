using System;

namespace NCoreUtils.Data.Protocol;

[ProtocolGenerationOptions(ProtocolGenerationMode.Optimal | ProtocolGenerationMode.Array)]
[ProtocolEntity(typeof(DataEntity))]
[ProtocolEntity(typeof(Func<DataEntity, int>))]
public partial class DataQueryContext { }