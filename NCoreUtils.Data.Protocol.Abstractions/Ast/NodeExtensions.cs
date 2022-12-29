using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.Ast;

public static class NodeExtensions
{
    internal sealed class EmplaceVisitor
    {
        public static EmplaceVisitor Singleton { get; } = new EmplaceVisitor();

        private EmplaceVisitor() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VisitBinary(Binary binary, bool complex, ref SpanBuilder builder)
        {
            if (complex)
            {
                builder.Append('(');
            }
            binary.Left.Accept(this, true, ref builder);
            builder.Append(' ');
            builder.Append(binary.Operation.GetString());
            builder.Append(' ');
            binary.Right.Accept(this, true, ref builder);
            if (complex)
            {
                builder.Append(')');
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Style", "IDE0060", MessageId = "complex")]
        public void VisitCall(Call call, bool complex, ref SpanBuilder builder)
        {
            builder.Append(call.Name);
            builder.Append('(');
            for (var i = 0; i < call.Arguments.Count; ++i)
            {
                if (i != 0)
                {
                    builder.Append(", ");
                }
                call.Arguments[i].Accept(this, false, ref builder);
            }
            builder.Append(')');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Style", "IDE0060", MessageId = "complex")]
        [SuppressMessage("Performance", "CA1822")]
        public void VisitConstant(Constant constant, bool complex, ref SpanBuilder builder)
        {
            var v = constant.RawValue;
            switch (v)
            {
                case null:
                    builder.Append(NullLiteral.Length);
                    break;
                case "":
                    builder.Append("\"\"");
                    break;
                default:
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
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Style", "IDE0060", MessageId = "complex")]
        [SuppressMessage("Performance", "CA1822")]
        public void VisitIdentifier(Identifier identifier, bool complex, ref SpanBuilder builder)
        {
            builder.Append(identifier.Value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Style", "IDE0060", MessageId = "complex")]
        public void VisitLambda(Lambda lambda, bool complex, ref SpanBuilder builder)
        {
            VisitIdentifier(lambda.Arg, false, ref builder);
            builder.Append(" => ");
            lambda.Body.Accept(this, false, ref builder);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Style", "IDE0060", MessageId = "complex")]
        public void VisitMember(Member member, bool complex, ref SpanBuilder builder)
        {
            member.Instance.Accept(this, true, ref builder);
            builder.Append('.');
            builder.Append(member.MemberName);
        }
    }

    internal sealed class GetStringifiedSizeVisitor
    {
        public static GetStringifiedSizeVisitor Singleton { get; } = new GetStringifiedSizeVisitor();

        private GetStringifiedSizeVisitor() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int VisitBinary(Binary binary, bool complex)
            => binary.Left.Accept(this, true) + 1 + binary.Operation.GetString().Length + 1
                + binary.Right.Accept(this, true) + (complex ? 2 : 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Style", "IDE0060", MessageId = "complex")]
        public int VisitCall(Call call, bool complex)
        {
            var size = call.Name.Length + 2;
            for (var i = 0; i < call.Arguments.Count; ++i)
            {
                if (i != 0)
                {
                    size += 2;
                }
                size += call.Arguments[i].Accept(this, false);
            }
            return size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Style", "IDE0060", MessageId = "complex")]
        [SuppressMessage("Performance", "CA1822")]
        public int VisitConstant(Constant constant, bool complex)
            => constant.RawValue switch
            {
                null => NullLiteral.Length,
                "" => 2,
                var v => IsNumeric(v) ? v.Length : CalculateEscapedStringSize(v) + 2
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Style", "IDE0060", MessageId = "complex")]
        [SuppressMessage("Performance", "CA1822")]
        public int VisitIdentifier(Identifier identifier, bool complex)
            => identifier.Value.Value.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Style", "IDE0060", MessageId = "complex")]
        public int VisitLambda(Lambda lambda, bool complex)
            => VisitIdentifier(lambda.Arg, false) + 4 + lambda.Body.Accept(this, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Style", "IDE0060", MessageId = "complex")]
        public int VisitMember(Member member, bool complex)
            => member.Instance.Accept(this, true) + 1 + member.MemberName.Length;
    };

    internal sealed class SubstituteParameterVisitor : INodeVisitor<UniqueString, Node, Node>
    {
        public static SubstituteParameterVisitor Singleton { get; } = new SubstituteParameterVisitor();

        private SubstituteParameterVisitor() { }

        public Node VisitBinary(Binary binary, UniqueString source, Node target)
            => Node.Binary(
                binary.Left.Accept(this, source, target),
                binary.Operation,
                binary.Right.Accept(this, source, target)
            );

        public Node VisitCall(Call call, UniqueString source, Node target)
        {
            var newArguments = new Node[call.Arguments.Count];
            for (var i = 0; i < newArguments.Length; ++i)
            {
                newArguments[i] = call.Arguments[i].Accept(this, source, target);
            }
            return Node.Call(call.Name, newArguments);
        }


        public Node VisitConstant(Constant constant, UniqueString source, Node target)
            => constant;

        public Node VisitIdentifier(Identifier identifier, UniqueString source, Node target)
            => identifier.Value == source ? target : identifier;

        public Node VisitLambda(Lambda lambda, UniqueString source, Node target)
            => Node.Lambda(lambda.Arg, lambda.Body.Accept(this, source, target));

        public Node VisitMember(Member member, UniqueString source, Node target)
            => Node.Member(member.Instance.Accept(this, source, target), member.MemberName);
    }

    internal const string NullLiteral = "null";

    internal static string GetString(this BinaryOperation operation) => operation switch
    {
        BinaryOperation.Equal => "=",
        BinaryOperation.NotEqual => "!=",
        BinaryOperation.LessThan => "<",
        BinaryOperation.LessThanOrEqual => "<=",
        BinaryOperation.GreaterThan => ">",
        BinaryOperation.GreaterThanOrEqual => ">=",
        BinaryOperation.OrElse => "||",
        BinaryOperation.AndAlso => "&&",
        BinaryOperation.Add => "+",
        BinaryOperation.Subtract => "-",
        BinaryOperation.Multiply => "*",
        BinaryOperation.Divide => "/",
        BinaryOperation.Modulo => "%",
        _ => throw new InvalidOperationException("Invalid binary operation")
    };

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

    public static Node SubstituteParameter(this Node node, UniqueString source, Node target)
        => node.Accept(SubstituteParameterVisitor.Singleton, source, target);

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

    internal static int CalculateStringifiedSize(this Node node)
        => node.Accept(GetStringifiedSizeVisitor.Singleton, false);

    public static void EmplaceTo(this Node node, ref SpanBuilder builder)
        => node.Accept(EmplaceVisitor.Singleton, false, ref builder);

    public static int EmplaceTo(this Node node, Span<char> buffer)
    {
        var builder = new SpanBuilder(buffer);
        node.EmplaceTo(ref builder);
        return builder.Length;
    }
}