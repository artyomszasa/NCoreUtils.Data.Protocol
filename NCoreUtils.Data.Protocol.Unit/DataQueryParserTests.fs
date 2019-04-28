module NCoreUtils.Data.Protocol.DataQueryParserTests

open System.Collections.Generic
open NCoreUtils.Data
open NCoreUtils.Data.Protocol.Ast
open Xunit;
open System.Collections.Immutable

[<Literal>]
let private Query00 = "o => o.int = 2"
[<Literal>]
let private Query01 = "o => o.int != 2"
[<Literal>]
let private Query02 = "o => o.int > 2"
[<Literal>]
let private Query03 = "o => o.int < 2"
[<Literal>]
let private Query04 = "o => o.int >= 2"
[<Literal>]
let private Query05 = "o => o.int <= 2"

[<Literal>]
let private Query10 = "o => o.string = \"abc\""

[<Literal>]
let private Query11 = "o => o.string = null"

[<Literal>]
let private Query12 = "o => (o.string = null)"

[<Literal>]
let private Query2 = "o => lower(o.string) = \"abc\""

let private expectedNode =
  let map = Dictionary ()
  let q0x op =
    Lambda (
      Identifier "o",
      Binary (
        Member (Identifier "o", "int"),
        op,
        Constant "2"
      )
    )

  map.Add (Query00, q0x BinaryOperation.Equal)
  map.Add (Query01, q0x BinaryOperation.NotEqual)
  map.Add (Query02, q0x BinaryOperation.GreaterThan)
  map.Add (Query03, q0x BinaryOperation.LessThan)
  map.Add (Query04, q0x BinaryOperation.GreaterThanOrEqual)
  map.Add (Query05, q0x BinaryOperation.LessThanOrEqual)
  map.Add (
    Query10,
    Lambda (
      Identifier "o",
      Binary (
        Member (Identifier "o", "string"),
        BinaryOperation.Equal,
        Constant "abc"
      )
    )
  )
  map.Add (
    Query11,
    Lambda (
      Identifier "o",
      Binary (
        Member (Identifier "o", "string"),
        BinaryOperation.Equal,
        Constant null
      )
    )
  )
  map.Add (
    Query12,
    Lambda (
      Identifier "o",
      Binary (
        Member (Identifier "o", "string"),
        BinaryOperation.Equal,
        Constant null
      )
    )
  )
  map.Add (
    Query2,
    Lambda (
      Identifier "o",
      Binary (
        Call ("lower", ImmutableArray.Create (Member (Identifier "o", "string"))),
        BinaryOperation.Equal,
        Constant "abc"
      )
    )
  )
  map :> IReadOnlyDictionary<_, _>


[<Theory>]
[<InlineData(Query00)>]
[<InlineData(Query01)>]
[<InlineData(Query02)>]
[<InlineData(Query03)>]
[<InlineData(Query04)>]
[<InlineData(Query05)>]
[<InlineData(Query10)>]
[<InlineData(Query11)>]
[<InlineData(Query12)>]
let ``member access`` (raw : string) =
  let parser = DataQueryParser () :> IDataQueryParser
  let actual = parser.ParseQuery raw
  let expected = expectedNode.[raw]
  Assert.Equal(expected.GetHashCode (), actual.GetHashCode ())
  Assert.Equal(expected, actual)
  Assert.Equal(box expected, box actual)

[<Theory>]
[<InlineData(Query2)>]
let ``function call`` (raw : string) =
  let parser = DataQueryParser () :> IDataQueryParser
  let actual = parser.ParseQuery raw
  let expected = expectedNode.[raw]
  Assert.Equal(expected.GetHashCode (), actual.GetHashCode ())
  Assert.Equal(expected, actual)
  Assert.Equal(box expected, box actual)