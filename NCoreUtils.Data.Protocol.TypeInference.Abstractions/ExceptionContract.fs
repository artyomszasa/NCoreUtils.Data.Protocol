namespace NCoreUtils.Data

open System
open System.Runtime.Serialization

[<Serializable>]
type ProtocolTypeInferenceException =
  inherit ProtocolException
  new (message) = { inherit ProtocolException (message) }
  new (message : string, innerException) = { inherit ProtocolException (message, innerException) }
  new (info : SerializationInfo, context) = { inherit ProtocolException (info, context) }
