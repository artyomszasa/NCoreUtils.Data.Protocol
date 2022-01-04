using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.TypeInference.Ast;
using BinaryOperation = NCoreUtils.Data.Protocol.Ast.BinaryOperation;

namespace NCoreUtils.Data.Protocol.TypeInference;

public static class Helpers
{
    internal sealed class UnresolvedFunction : IFunctionDescriptor
    {
        public Type ResultType
        {
            [UnconditionalSuppressMessage("Trimming", "IL2063", Justification = "Method always throws.")]
            [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            get => throw new InvalidOperationException();
        }

        public ReadOnlyConstraintedTypeList ArgumentTypes => throw new InvalidOperationException();

        public string Name { get; }

        public UnresolvedFunction(string name)
            => Name = name ?? throw new ArgumentNullException(nameof(name));

        public Expression CreateExpression(IReadOnlyList<Expression> arguments)
            => throw new InvalidOperationException();
    }

    internal sealed class TypingVisitor : INodeRefVisitor<int, ImmutableDictionary<UniqueString, TypeUid>, Node<TypeUid>>
    {
        public static TypingVisitor Singleton { get; } = new TypingVisitor();

        private TypingVisitor() { }

        private IReadOnlyList<Node<TypeUid>> VisitCollection(IReadOnlyList<Node> nodes, ref int arg, ImmutableDictionary<UniqueString, TypeUid> args)
        {
            var result = new Node<TypeUid>[nodes.Count];
            for (var i = 0; i < result.Length; ++i)
            {
                result[i] = nodes[i].Accept(this, ref arg, args);
            }
            return result;
        }

        public Node<TypeUid> VisitBinary(Binary binary, ref int arg, ImmutableDictionary<UniqueString, TypeUid> args) => Node<TypeUid>.Binary(
            new TypeUid(arg++),
            binary.Left.Accept(this, ref arg, args),
            binary.Operation,
            binary.Right.Accept(this, ref arg, args)
        );

        public Node<TypeUid> VisitCall(Call call, ref int arg, ImmutableDictionary<UniqueString, TypeUid> args) => Node<TypeUid>.Call(
            new TypeUid(arg++),
            new UnresolvedFunction(call.Name),
            VisitCollection(call.Arguments, ref arg, args)
        );

        public Node<TypeUid> VisitConstant(Constant constant, ref int arg, ImmutableDictionary<UniqueString, TypeUid> args)
            => Node<TypeUid>.Constant(new TypeUid(arg++), constant.RawValue);

        public Node<TypeUid> VisitIdentifier(Identifier identifier, ref int arg, ImmutableDictionary<UniqueString, TypeUid> args)
            => Node<TypeUid>.Identifier(
                args.TryGetValue(identifier.Value, out var uid) ? uid : new TypeUid(arg++),
                identifier.Value
            );

        public Node<TypeUid> VisitLambda(Lambda lambda, ref int arg, ImmutableDictionary<UniqueString, TypeUid> args)
        {
            var argUid = new TypeUid(arg++);
            return Node<TypeUid>.Lambda(
                new TypeUid(arg++),
                Node<TypeUid>.Identifier(argUid, lambda.Arg.Value),
                lambda.Body.Accept(this, ref arg, args.Add(lambda.Arg.Value, argUid))
            );
        }

        public Node<TypeUid> VisitMember(Member member, ref int arg, ImmutableDictionary<UniqueString, TypeUid> args) => Node<TypeUid>.Member(
            new TypeUid(arg++),
            member.Instance.Accept(this, ref arg, args),
            member.MemberName
        );
    }

    private static HashSet<BinaryOperation> BooleanResultOperations { get; } = new()
    {
        BinaryOperation.OrElse,
        BinaryOperation.AndAlso,
        BinaryOperation.GreaterThan,
        BinaryOperation.GreaterThanOrEqual,
        BinaryOperation.LessThan,
        BinaryOperation.LessThanOrEqual,
        BinaryOperation.Equal,
        BinaryOperation.NotEqual
    };

    private static HashSet<BinaryOperation> NumericArgOperation { get; } = new()
    {
        BinaryOperation.GreaterThan,
        BinaryOperation.GreaterThanOrEqual,
        BinaryOperation.LessThan,
        BinaryOperation.LessThanOrEqual,
        BinaryOperation.Add,
        BinaryOperation.Substract,
        BinaryOperation.Multiply,
        BinaryOperation.Divide,
        BinaryOperation.Modulo
    };

    private static HashSet<BinaryOperation> NumericResultOperation { get; } = new()
    {
        BinaryOperation.Add,
        BinaryOperation.Substract,
        BinaryOperation.Multiply,
        BinaryOperation.Divide,
        BinaryOperation.Modulo
    };

    public static Node<TypeUid> Idfy(Node node)
    {
        var supply = 0;
        return node.Accept(TypingVisitor.Singleton, ref supply, ImmutableDictionary<UniqueString, TypeUid>.Empty);
    }

    private static ImmutableDictionary<TypeUid, TypeVariable> CollectIds(ImmutableDictionary<TypeUid, TypeVariable> acc, Node<TypeUid> node)
        => node.GetChildren()
            .Aggregate(
                acc.SetItem(node.Type, TypeVariable.Empty),
                CollectIds
            );

    public static TypeInferenceContext CollectIds(Node<TypeUid> node)
        => new(
            CollectIds(ImmutableDictionary<TypeUid, TypeVariable>.Empty, node),
            ImmutableDictionary<TypeUid, ImmutableHashSet<Substitution>>.Empty
        );

    private static TypeInferenceContext ApplyIf(this TypeInferenceContext ctx, bool condition, Func<TypeInferenceContext, TypeInferenceContext> action)
        => condition switch
        {
            true => action(ctx),
            _ => ctx
        };

    private sealed class PullDownConstraintsVisitor : ITypedNodeVisitor<TypeUid, TypeInferenceContext, IPropertyResolver, TypeInferenceContext>
    {
        public static PullDownConstraintsVisitor Singleton { get; } = new();

        private PullDownConstraintsVisitor() { }

        public TypeInferenceContext VisitBinary(Binary<TypeUid> binary, TypeInferenceContext ctx, IPropertyResolver propertyResolver)
        {
            var (uid, left, op, right) = (binary.Type, binary.Left, binary.Operation, binary.Right);
            return ctx
                .ApplyIf(BooleanResultOperations.Contains(op), ctx => ctx.ApplyConstraint(uid, TypeVariable.Boolean))
                .ApplyIf(NumericResultOperation.Contains(op), ctx => ctx.ApplyConstraint(uid, TypeVariable.Numeric))
                .ApplyIf(NumericArgOperation.Contains(op), ctx => ctx
                    .ApplyConstraint(left.Type, TypeVariable.Numeric)
                    .ApplyConstraint(right.Type, TypeVariable.Numeric)
                )
                .Substitute(left.Type, TypeRelation.SameAs, right.Type)
                .Substitute(right.Type, TypeRelation.SameAs, left.Type)
                .PullDownConstraints(this, propertyResolver, left)
                .PullDownConstraints(this, propertyResolver, right);
        }

        public TypeInferenceContext VisitCall(Call<TypeUid> call, TypeInferenceContext ctx, IPropertyResolver propertyResolver)
            => call.Arguments.Aggregate(ctx, (context, node) => node.Accept(this, context, propertyResolver));

        public TypeInferenceContext VisitConstant(Constant<TypeUid> constant, TypeInferenceContext ctx, IPropertyResolver propertyResolver)
            => constant.RawValue is null
                ? ctx.ApplyConstraint(constant.Type, TypeVariable.Nullable)
                : ctx;

        public TypeInferenceContext VisitIdentifier(Identifier<TypeUid> identifier, TypeInferenceContext ctx, IPropertyResolver propertyResolver)
            => ctx;

        public TypeInferenceContext VisitLambda(Lambda<TypeUid> lambda, TypeInferenceContext ctx, IPropertyResolver propertyResolver)
            => lambda.Body
                .Accept(this, ctx, propertyResolver)
                .ApplyConstraint(lambda.Type, TypeVariable.Lambda)
                .Substitute(lambda.Type, TypeRelation.ArgOf, lambda.Arg.Type)
                .Substitute(lambda.Type, TypeRelation.ResultOf, lambda.Body.Type);


        public TypeInferenceContext VisitMember(Member<TypeUid> member, TypeInferenceContext ctx, IPropertyResolver propertyResolver)
        {
            var (uid, instance, name) = (member.Type, member.Instance, member.MemberName);
            var ctx1 = instance.Accept(this, ctx, propertyResolver);
            if (ctx1.GetAllConstraints(instance.Type).TryGetExactType(out var instanceType))
            {
                var property = propertyResolver.ResolveProperty(instanceType, name);
                return ctx1.ApplyConstraint(uid, new(property.PropertyType));
            }
            return ctx1
                .ApplyConstraint(instance.Type, TypeVariable.HasMember(name))
                .ApplyConstraint(uid, TypeVariable.IsMemberOf(instance.Type, name));
        }
    }

    private sealed class CollectConstraintsVisitor : ITypedNodeVisitor1Out<TypeUid, TypeInferenceContext, IPropertyResolver, IFunctionDescriptorResolver, Node<TypeUid>, TypeInferenceContext>
    {
        public static CollectConstraintsVisitor Singleton { get; } = new();

        private CollectConstraintsVisitor() { }

        public TypeInferenceContext VisitBinary(
            Binary<TypeUid> binary,
            TypeInferenceContext ctx,
            IPropertyResolver propertyResolver,
            IFunctionDescriptorResolver functionResolver,
            out Node<TypeUid> result)
        {
            var (uid, left, op, right) = (binary.Type, binary.Left, binary.Operation, binary.Right);
            var ctx1 = ctx
                .CollectConstraints(this, propertyResolver, functionResolver, left, out var newLeft)
                .CollectConstraints(this, propertyResolver, functionResolver, right, out var newRight);
            result = Node<TypeUid>.Binary(uid, newLeft, op, newRight);
            return ctx1;
        }

        private TypeInferenceContext TryVisitCallUsingFunctionDescriptor(
            string functionName,
            TypeVariable resultTypeConstraints,
            IReadOnlyList<TypeVariable> argumentTypeConstraints,
            IReadOnlyList<IFunctionDescriptor> descCandidates,
            int index,
            TypeUid uid,
            IReadOnlyList<Node<TypeUid>> arguments,
            TypeInferenceContext ctx,
            IPropertyResolver propertyResolver,
            IFunctionDescriptorResolver functionResolver,
            out Node<TypeUid> result)
        {
            if (index >= descCandidates.Count)
            {
                // If none of candidates could been successfully applyied resolved with both inherited and gained attributes --> fail
                Exn.UnableToResolveCall(functionName, resultTypeConstraints, argumentTypeConstraints);
            }
            var desc = descCandidates[index];
            try
            {
                var ctx1 = ApplyDescriptor(ctx, desc, uid, arguments);
                var newArguments = new Node<TypeUid>[arguments.Count];
                for (var i = 0; i < newArguments.Length; ++i)
                {
                    ctx1 = ctx1.CollectConstraints(this, propertyResolver, functionResolver, arguments[i], out newArguments[i]);
                }
                if (desc is null)
                {
                    var newResultTypeConstraints = ctx1.GetAllConstraints(uid);
                    // FIXME: pool
                    var newArgumentTypeConstraints = newArguments.MapToArray(arg => ctx1.GetAllConstraints(arg.Type));
                    // If function have not been resolved using inherited attributes retry with gained attributes
                    desc = functionResolver.ResolveFunction(functionName, newResultTypeConstraints, newArgumentTypeConstraints);
                    if (desc is null)
                    {
                        // If function still could not be resolved with both inherited and gained attributes --> try next
                        // NOTE: orignal constraints and type variables are passed!
                        return TryVisitCallUsingFunctionDescriptor(
                            functionName,
                            resultTypeConstraints,
                            argumentTypeConstraints,
                            descCandidates,
                            index + 1,
                            uid,
                            arguments,
                            ctx,
                            propertyResolver,
                            functionResolver,
                            out result
                        );
                    }
                    ctx1 = ApplyDescriptor(ctx1, desc, uid, newArguments);
                }
                result = Node<TypeUid>.Call(uid, desc, newArguments);
                return ctx1;
            }
            catch (ProtocolTypeInferenceException)
            {
                // candidate could not be processed --> try next
                // NOTE: orignal constraints and type variables are passed!
                return TryVisitCallUsingFunctionDescriptor(
                    functionName,
                    resultTypeConstraints,
                    argumentTypeConstraints,
                    descCandidates,
                    index + 1,
                    uid,
                    arguments,
                    ctx,
                    propertyResolver,
                    functionResolver,
                    out result
                );
            }

            static TypeInferenceContext ApplyDescriptor(TypeInferenceContext ctx, IFunctionDescriptor desc, TypeUid resultUid, IReadOnlyList<Node<TypeUid>> arguments)
            {
                var ctx1 = ctx;
                ctx1 = ctx1.ApplyConstraint(resultUid, new(desc.ResultType));
                for (var i = 0; i < arguments.Count; ++i)
                {
                    var arg = arguments[i];
                    var argType = desc.ArgumentTypes[i];
                    ctx1 = ctx1.ApplyConstraint(arg.Type, new(argType));
                    // handle lambda arguments
                    if (argType.IsConstructedGenericType && argType.GetGenericTypeDefinition() == typeof(Func<,>))
                    {
                        var genericTypes = argType.GetGenericArguments();
                        var genericArgType = genericTypes[0];
                        var genericResType = genericTypes[1];
                        if (arg is not Lambda<TypeUid> lambda)
                        {
                            // method above will always throw but there is no way to tell compiler so
                            // see: https://github.com/dotnet/csharplang/issues/739
                            return Exn.LambdaArgumentExpected(i, desc);
                        }
                        ctx1 = ctx1
                            .ApplyConstraint(lambda.Arg.Type, TypeVariable.UncheckedType(genericArgType))
                            .ApplyConstraint(lambda.Body.Type, TypeVariable.UncheckedType(genericResType));
                    }
                }
                return ctx1;
            }
        }

        public TypeInferenceContext VisitCall(
            Call<TypeUid> call,
            TypeInferenceContext ctx,
            IPropertyResolver propertyResolver,
            IFunctionDescriptorResolver functionResolver,
            out Node<TypeUid> result)
        {
            var (uid, udesc, arguments) = (call.Type, call.Descriptor, call.Arguments);
            var ctx1 = ctx;
            // try resolve arguments with
            // try resolve function call early --> if inherited attributes allow this then type substitution can be
            // performed prior infering types of arguments. This can be usefull if arguments resolvation depends on the
            // inherited attributes.
            var resultTypeConstraints = ctx.GetAllConstraints(uid);
            var argumentTypeConstraints = arguments.MapToArray(arg => ctx.GetAllConstraints(arg.Type));
            // FIXME: pool
            var descCandidates = new List<IFunctionDescriptor>();
            if (functionResolver is IAmbigousFunctionDescriptorResolver ambigousFunctionResolver)
            {
                ambigousFunctionResolver.TryResolveAllMatchingFunctions(udesc.Name, resultTypeConstraints, argumentTypeConstraints, descCandidates);
            }
            else if (functionResolver.TryResolveFunction(udesc.Name, resultTypeConstraints, argumentTypeConstraints, out var desc))
            {
                descCandidates.Add(desc);
            }

            return TryVisitCallUsingFunctionDescriptor(
                udesc.Name,
                resultTypeConstraints,
                argumentTypeConstraints,
                descCandidates,
                0,
                uid,
                arguments,
                ctx,
                propertyResolver,
                functionResolver,
                out result
            );
        }

        public TypeInferenceContext VisitConstant(
            Constant<TypeUid> constant,
            TypeInferenceContext ctx,
            IPropertyResolver propertyResolver,
            IFunctionDescriptorResolver functionResolver,
            out Node<TypeUid> result)
        {
            result = constant;
            return ctx;
        }

        public TypeInferenceContext VisitIdentifier(
            Identifier<TypeUid> identifier,
            TypeInferenceContext ctx,
            IPropertyResolver propertyResolver,
            IFunctionDescriptorResolver functionResolver,
            out Node<TypeUid> result)
        {
            result = identifier;
            return ctx;
        }

        public TypeInferenceContext VisitLambda(
            Lambda<TypeUid> lambda,
            TypeInferenceContext ctx,
            IPropertyResolver propertyResolver,
            IFunctionDescriptorResolver functionResolver,
            out Node<TypeUid> result)
        {
            var ctx1 = lambda.Body.Accept(this, ctx, propertyResolver, functionResolver, out var newBody);
            result = Node<TypeUid>.Lambda(lambda.Type, lambda.Arg, newBody);
            return ctx1;
        }

        public TypeInferenceContext VisitMember(
            Member<TypeUid> member,
            TypeInferenceContext ctx,
            IPropertyResolver propertyResolver,
            IFunctionDescriptorResolver functionResolver,
            out Node<TypeUid> result)
        {
            var (uid, instance, name) = (member.Type, member.Instance, member.MemberName);
            var ctx1 = ctx.CollectConstraints(this, propertyResolver, functionResolver, instance, out var newInstance);
            result = Node<TypeUid>.Member(uid, newInstance, name);
            if (ctx1.GetAllConstraints(newInstance.Type).TryGetExactType(out var instanceType))
            {
                var property = propertyResolver.ResolveProperty(instanceType, name);
                return ctx1.ApplyConstraint(uid, new(property.PropertyType));
            }
            return ctx1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TypeInferenceContext CollectConstraints(
        this TypeInferenceContext ctx,
        CollectConstraintsVisitor visitor,
        IPropertyResolver propertyResolver,
        IFunctionDescriptorResolver functionResolver,
        Node<TypeUid> node,
        out Node<TypeUid> result)
        => node.Accept(visitor, ctx, propertyResolver, functionResolver, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TypeInferenceContext PullDownConstraints(
        this TypeInferenceContext ctx,
        PullDownConstraintsVisitor visitor,
        IPropertyResolver propertyResolver,
        Node<TypeUid> node)
        => node.Accept(visitor, ctx, propertyResolver);

    public static TypeInferenceContext CollectConstraintsRoot(
        this TypeInferenceContext ctx,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type rootType,
        IPropertyResolver propertyResolver,
        IFunctionDescriptorResolver functionResolver,
        Lambda<TypeUid> node,
        out Node<TypeUid> result)
        => ctx
            .ApplyConstraint(node.Arg.Type, new(rootType))
            .PullDownConstraints(PullDownConstraintsVisitor.Singleton, propertyResolver, node)
            .CollectConstraints(CollectConstraintsVisitor.Singleton, propertyResolver, functionResolver, node, out result);
}