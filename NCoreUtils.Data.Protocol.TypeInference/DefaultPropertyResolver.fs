namespace NCoreUtils.Data.Protocol.TypeInference

open System
open System.Diagnostics.CodeAnalysis
open System.Collections.Concurrent
open System.Linq.Expressions
open System.Reflection

[<ExcludeFromCodeCoverage>]
type internal DefaultProperty =
  { Property : PropertyInfo }
  with
    interface IProperty with
      member this.CreateExpression instance =
        Expression.Property (instance, this.Property) :> _
      member this.PropertyType =
        this.Property.PropertyType


type DefaultPropertyResolver () =

  let cache = ConcurrentDictionary<struct (Type * string), DefaultProperty>()

  static member val Instance = DefaultPropertyResolver ()

  abstract TryResolve : ``type``:Type * name:string -> IProperty voption

  default __.TryResolve (``type``, name) =
    let key = struct (``type``, name)
    let mutable prop = Unchecked.defaultof<_>
    match cache.TryGetValue (key, &prop) with
    | true -> ValueSome (prop :> IProperty)
    | _    ->
      match ``type``.GetProperty (name, BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.IgnoreCase) with
      | null -> ValueNone
      | propertyInfo ->
        let prop = cache.GetOrAdd (key, { Property = propertyInfo })
        ValueSome (prop :> IProperty)

  interface IPropertyResolver with
    member this.TryResolve (``type``, name) = this.TryResolve (``type``, name)


