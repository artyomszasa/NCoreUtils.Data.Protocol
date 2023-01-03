using System;
using System.Linq;
using BinaryOperation = NCoreUtils.Data.Protocol.Ast.BinaryOperation;

namespace NCoreUtils.Data.Protocol.TypeInference.Ast;

public static class NodeExtensions
{
    private const string NullLiteral = Protocol.Ast.NodeExtensions.NullLiteral;

    private static string GetString(this BinaryOperation operation)
        => Protocol.Ast.NodeExtensions.GetString(operation);

    private static bool IsNumeric(string input)
    {
        foreach (var ch in input.AsSpan())
        {
            if (!char.IsDigit(ch))
            {
                return false;
            }
        }
        return true;
    }

    private static int CalculateEscapedStringSize(string input)
    {
        var counter = 0;
        foreach (var ch in input.AsSpan())
        {
            if (ch is '"' or '\\')
            {
                counter += 1;
            }
        }
        return input.Length + counter;
    }

    public static int CalculateStringifiedSize<T>(this Node<T> node)
    {
        return Calc(node, false);

        static int Calc(Node<T> node, bool complex) => node switch
        {
            Lambda<T> { Arg: var arg, Body: var body } => Calc(arg, false) + 4 + Calc(body, false),
            Binary<T> { Left: var left, Operation: var op, Right: var right } =>
                Calc(left, true) + 1 + op.GetString().Length + 1 + Calc(right, true) + (complex ? 2 : 0),
            Call<T> { Descriptor: var descriptor, Arguments: var args } =>
                descriptor.Name.Length + 1 + args.Aggregate(0, (sum, node) => sum + Calc(node, false)) + Math.Max(0, (args.Count - 1) * 2),
            Member<T> { Instance: var inst, MemberName: var name } =>
                Calc(inst, true) + 1 + name.Length,
            Identifier<T> { Value: var uname } => uname.Value.Length,
            Constant<T> { RawValue: null } => NullLiteral.Length,
            Constant<T> { RawValue: "" } => 2,
            Constant<T> { RawValue: var v } => IsNumeric(v) ? v.Length : CalculateEscapedStringSize(v) + 2,
            _ => throw new InvalidOperationException("Should never happen.")
        };
    }

    public static void EmplaceTo<T>(this Node<T> node, ref SpanBuilder builder)
    {
        DoEmplaceTo(node, false, ref builder);

        static void DoEmplaceTo(Node<T> node, bool complex, ref SpanBuilder builder)
        {
            switch (node)
            {
                case Lambda<T> { Arg: var arg, Body: var body }:
                    DoEmplaceTo(arg, false, ref builder);
                    builder.Append(" => ");
                    DoEmplaceTo(body, false, ref builder);
                    break;
                case Binary<T> { Left: var left, Operation: var op, Right: var right }:
                    DoEmplaceTo(left, true, ref builder);
                    builder.Append(' ');
                    builder.Append(op.GetString());
                    builder.Append(' ');
                    DoEmplaceTo(right, true, ref builder);
                    break;
                case Call<T> { Descriptor: var descriptor, Arguments: var args }:
                    builder.Append(descriptor.Name);
                    builder.Append('(');
                    for (var i = 0; i < args.Count; ++i)
                    {
                        if (i != 0)
                        {
                            builder.Append(", ");
                        }
                        DoEmplaceTo(args[i], false, ref builder);
                    }
                    builder.Append(')');
                    break;
                case Member<T> { Instance: var inst, MemberName: var name }:
                    DoEmplaceTo(inst, true, ref builder);
                    builder.Append('.');
                    builder.Append(name);
                    break;
                case Identifier<T> { Value: var uname }:
                    builder.Append(uname.Value);
                    break;
                case Constant<T> { RawValue: null }:
                    builder.Append(NullLiteral.Length);
                    break;
                case Constant<T> { RawValue: "" }:
                    builder.Append("\"\"");
                    break;
                case Constant<T> { RawValue: var v }:
                    if (IsNumeric(v))
                    {
                        builder.Append(v);
                    }
                    else
                    {
                        builder.Append('"');
                        foreach (var ch in v.AsSpan())
                        {
                            if (ch is '"' or '\\')
                            {
                                builder.Append('\\');
                            }
                            builder.Append(ch);
                        }
                        builder.Append('"');
                    }
                    break;
                default:
                    throw new InvalidOperationException("Should never happen.");
            }
        }
    }

    public static int EmplaceTo<T>(this Node<T> node, Span<char> buffer)
    {
        var builder = new SpanBuilder(buffer);
        node.EmplaceTo(ref builder);
        return builder.Length;
    }
}