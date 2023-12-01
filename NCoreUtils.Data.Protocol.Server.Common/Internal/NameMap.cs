using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Internal;

public class NameMap
{
    private static readonly string[] _letters =
    [
        "a",
        "b",
        "c",
        "d",
        "e",
        "f",
        "g",
        "h",
        "i",
        "j",
        "k",
        "l",
        "m",
        "n",
        "o",
        "p",
        "q",
        "r",
        "s",
        "t",
        "u",
        "v",
        "w",
        "x",
        "y",
        "z"
    ];

    private static string GetName(int ix)
    {
        if (ix < _letters.Length)
        {
            return _letters[ix];
        }
        var i = ix % _letters.Length;
        var j = ix / _letters.Length;
        return _letters[i] + j;
    }

    private int _supply = 0;

    private Dictionary<UniqueString, ParameterExpression> Parameters { get; } = new();

    public ParameterExpression Add(UniqueString uname, Type type)
    {
        var parameter = Expression.Parameter(type, GetName(_supply++));
        Parameters.Add(uname, parameter);
        return parameter;
    }

    public ParameterExpression GetParameter(UniqueString uname)
    {
        if (Parameters.TryGetValue(uname, out var parameter))
        {
            return parameter;
        }
        throw new InvalidOperationException($"Unique name {uname} is out of scope.");
    }
}