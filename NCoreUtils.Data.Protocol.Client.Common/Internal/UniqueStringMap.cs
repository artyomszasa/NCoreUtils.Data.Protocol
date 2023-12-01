using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Internal;

public class UniqueStringMap
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

    private Dictionary<ParameterExpression, UniqueString> Parameters { get; } = [];

    public UniqueString Add(ParameterExpression parameter)
    {
        var uname = new UniqueString(GetName(_supply++));
        Parameters.Add(parameter, uname);
        return uname;
    }

    public UniqueString GetParameterName(ParameterExpression parameter)
    {
        if (Parameters.TryGetValue(parameter, out var uname))
        {
            return uname;
        }
        throw new InvalidOperationException($"Parameter {parameter} is out of scope.");
    }
}