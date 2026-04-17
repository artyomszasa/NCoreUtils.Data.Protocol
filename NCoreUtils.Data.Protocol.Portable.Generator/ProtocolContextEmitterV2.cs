using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NCoreUtils.Data.Protocol.Generator;

internal static class Identifiers
{
    public static SyntaxToken All { get; } = Identifier("All");

    public static SyntaxToken Any { get; } = Identifier("Any");

    public static SyntaxToken Accept { get; } = Identifier("Accept");

    public static SyntaxToken ArgType { get; } = Identifier("ArgType");

    public static SyntaxToken ArrayOfType { get; } = Identifier("ArrayOfType");

    public static SyntaxToken Body { get; } = Identifier("Body");

    public static SyntaxToken Box { get; } = Identifier("Box");

    public static SyntaxToken BoxNullable { get; } = Identifier("BoxNullable");

    public static SyntaxToken BoxValueField { get; } = Identifier("BoxValueField");

    public static SyntaxToken CreateBoxedConstant { get; } = Identifier("CreateBoxedConstant");

    public static SyntaxToken CreateAdd { get; } = Identifier("CreateAdd");

    public static SyntaxToken CreateAndAlso { get; } = Identifier("CreateAndAlso");

    public static SyntaxToken CreateDivide { get; } = Identifier("CreateDivide");

    public static SyntaxToken CreateEqual { get; } = Identifier("CreateEqual");

    public static SyntaxToken CreateGreaterThan { get; } = Identifier("CreateGreaterThan");

    public static SyntaxToken CreateGreaterThanOrEqual { get; } = Identifier("CreateGreaterThanOrEqual");

    public static SyntaxToken CreateLessThan { get; } = Identifier("CreateLessThan");

    public static SyntaxToken CreateLessThanOrEqual { get; } = Identifier("CreateLessThanOrEqual");

    public static SyntaxToken CreateModulo { get; } = Identifier("CreateModulo");

    public static SyntaxToken CreateMultiply { get; } = Identifier("CreateMultiply");

    public static SyntaxToken CreateNotEqual { get; } = Identifier("CreateNotEqual");

    public static SyntaxToken CreateOrElse { get; } = Identifier("CreateOrElse");

    public static SyntaxToken CreateSubtract { get; } = Identifier("CreateSubtract");

    public static SyntaxToken Contains { get; } = Identifier("Contains");

    public static SyntaxToken Convert { get; } = Identifier("Convert");

    public static SyntaxToken Constant { get; } = Identifier("Constant");

    public static SyntaxToken EnumerableAnyMethod { get; } = Identifier("EnumerableAnyMethod");

    public static SyntaxToken EnumerableAllMethod { get; } = Identifier("EnumerableAllMethod");

    public static SyntaxToken EnumerableContainsMethod { get; } = Identifier("EnumerableContainsMethod");

    public static SyntaxToken EnumerableOfType { get; } = Identifier("EnumerableOfType");

    public static SyntaxToken Equal { get; } = Identifier("Equal");

    public static new SyntaxToken Equals { get; } = Identifier("Equals");

    public static SyntaxToken Field { get; } = Identifier("Field");

    public static SyntaxToken FromRawValue { get; } = Identifier("FromRawValue");

    public static SyntaxToken GetMethod { get; } = Identifier("GetMethod");

    public static SyntaxToken GetLambdaTypes { get; } = Identifier("GetLambdaTypes");

    public static SyntaxToken GetTypeDescriptors { get; } = Identifier("GetTypeDescriptors");

    public static SyntaxToken Singleton { get; } = Identifier("Singleton");

    public static SyntaxToken SingleFromRawValue { get; } = Identifier("SingleFromRawValue");

    public static SyntaxToken Stringify { get; } = Identifier("Stringify");

    public static SyntaxToken EnumerateFlags { get; } = Identifier("EnumerateFlags");

    public static SyntaxToken LambdaType { get; } = Identifier("LambdaType");

    public static SyntaxToken Member { get; } = Identifier("Member");

    public static SyntaxToken Method { get; } = Identifier("Method");

    public static SyntaxToken NotEqual { get; } = Identifier("NotEqual");

    public static SyntaxToken IsArithmetic { get; } = Identifier("IsArithmetic");

    public static SyntaxToken IsArray { get; } = Identifier("IsArray");

    public static SyntaxToken IsAssignableTo { get; } = Identifier("IsAssignableTo");

    public static SyntaxToken IsEnum { get; } = Identifier("IsEnum");

    public static SyntaxToken IsEnumerable { get; } = Identifier("IsEnumerable");

    public static SyntaxToken IsLambda { get; } = Identifier("IsLambda");

    public static SyntaxToken IsMaybe { get; } = Identifier("IsMaybe");

    public static SyntaxToken IsNullable { get; } = Identifier("IsNullable");

    public static SyntaxToken IsSame { get; } = Identifier("IsSame");

    public static SyntaxToken IsValue { get; } = Identifier("IsValue");

    public static SyntaxToken Parse { get; } = Identifier("Parse");

    public static SyntaxToken Properties { get; } = Identifier("Properties");

    public static SyntaxToken ResType { get; } = Identifier("ResType");

    public static new SyntaxToken ToString { get; } = Identifier("ToString");

    public static SyntaxToken TryGetEnumFactory { get; } = Identifier("TryGetEnumFactory");

    public static SyntaxToken TryParse { get; } = Identifier("TryParse");

    public static SyntaxToken Type { get; } = Identifier("Type");

    public static SyntaxToken Value { get; } = Identifier("Value");

    public static SyntaxToken Visit { get; } = Identifier("Visit");

    public static SyntaxToken argType { get; } = Identifier("argType");

    public static SyntaxToken e { get; } = Identifier("e");

    public static SyntaxToken elementType { get; } = Identifier("elementType");

    public static SyntaxToken enumFactory { get; } = Identifier("enumFactory");

    public static SyntaxToken func { get; } = Identifier("func");

    public static SyntaxToken i { get; } = Identifier("i");

    public static SyntaxToken item { get; } = Identifier("item");

    public static SyntaxToken rawValue { get; } = Identifier("rawValue");

    public static SyntaxToken res { get; } = Identifier("res");

    public static SyntaxToken resType { get; } = Identifier("resType");

    public static SyntaxToken right { get; } = Identifier("right");

    public static SyntaxToken self { get; } = Identifier("self");

    public static SyntaxToken v { get; } = Identifier("v");

    public static SyntaxToken value { get; } = Identifier("value");

    public static SyntaxToken visitor { get; } = Identifier("visitor");

    public static SyntaxToken _descriptors { get; } = Identifier("_descriptors");

    public static SyntaxToken _lambdaTypes { get; } = Identifier("_lambdaTypes");

    public static SyntaxToken _properties { get; } = Identifier("_properties");
}

internal static class IdentifierNames
{
    public static IdentifierNameSyntax All { get; } = IdentifierName(Identifiers.All);

    public static IdentifierNameSyntax Any { get; } = IdentifierName(Identifiers.Any);

    public static IdentifierNameSyntax ArrayOfType { get; } = IdentifierName(Identifiers.ArrayOfType);

    public static IdentifierNameSyntax ArgType { get; } = IdentifierName(Identifiers.ArgType);

    public static IdentifierNameSyntax Body { get; } = IdentifierName(Identifiers.Body);

    public static IdentifierNameSyntax Box { get; } = IdentifierName(Identifiers.Box);

    public static IdentifierNameSyntax BoxValueField { get; } = IdentifierName(Identifiers.BoxValueField);

    public static IdentifierNameSyntax Contains { get; } = IdentifierName(Identifiers.Contains);

    public static IdentifierNameSyntax Convert { get; } = IdentifierName(Identifiers.Convert);

    public static IdentifierNameSyntax Constant { get; } = IdentifierName(Identifiers.Constant);

    public static IdentifierNameSyntax EnumerableOfType { get; } = IdentifierName(Identifiers.EnumerableOfType);

    public static IdentifierNameSyntax Equal { get; } = IdentifierName(Identifiers.Equal);

    public static new IdentifierNameSyntax Equals { get; } = IdentifierName(Identifiers.Equals);

    public static IdentifierNameSyntax Field { get; } = IdentifierName(Identifiers.Field);

    public static IdentifierNameSyntax SingleFromRawValue { get; } = IdentifierName(Identifiers.SingleFromRawValue);

    public static IdentifierNameSyntax Singleton { get; } = IdentifierName(Identifiers.Singleton);

    public static IdentifierNameSyntax EnumerateFlags { get; } = IdentifierName(Identifiers.EnumerateFlags);

    public static IdentifierNameSyntax LambdaType { get; } = IdentifierName(Identifiers.LambdaType);

    public static IdentifierNameSyntax Member { get; } = IdentifierName(Identifiers.Member);

    public static IdentifierNameSyntax Method { get; } = IdentifierName(Identifiers.Method);

    public static IdentifierNameSyntax NotEqual { get; } = IdentifierName(Identifiers.NotEqual);

    public static IdentifierNameSyntax IsArithmetic { get; } = IdentifierName(Identifiers.IsArithmetic);

    public static IdentifierNameSyntax IsEnum { get; } = IdentifierName(Identifiers.IsEnum);

    public static IdentifierNameSyntax IsSame { get; } = IdentifierName(Identifiers.IsSame);

    public static IdentifierNameSyntax IsValue { get; } = IdentifierName(Identifiers.IsValue);

    public static IdentifierNameSyntax Parse { get; } = IdentifierName(Identifiers.Parse);

    public static IdentifierNameSyntax Properties { get; } = IdentifierName(Identifiers.Properties);

    public static IdentifierNameSyntax ResType { get; } = IdentifierName(Identifiers.ResType);

    public static new IdentifierNameSyntax ToString { get; } = IdentifierName(Identifiers.ToString);

    public static IdentifierNameSyntax TryParse { get; } = IdentifierName(Identifiers.TryParse);

    public static IdentifierNameSyntax Type { get; } = IdentifierName(Identifiers.Type);

    public static IdentifierNameSyntax Value { get; } = IdentifierName(Identifiers.Value);

    public static IdentifierNameSyntax Visit { get; } = IdentifierName(Identifiers.Visit);

    public static IdentifierNameSyntax @var { get; } = IdentifierName("var");

    public static IdentifierNameSyntax argType { get; } = IdentifierName(Identifiers.argType);

    public static IdentifierNameSyntax e { get; } = IdentifierName(Identifiers.e);

    public static IdentifierNameSyntax elementType { get; } = IdentifierName(Identifiers.elementType);

    public static IdentifierNameSyntax enumFactory { get; } = IdentifierName(Identifiers.enumFactory);

    public static IdentifierNameSyntax func { get; } = IdentifierName(Identifiers.func);

    public static IdentifierNameSyntax i { get; } = IdentifierName(Identifiers.i);

    public static IdentifierNameSyntax item { get; } = IdentifierName(Identifiers.item);

    public static IdentifierNameSyntax rawValue { get; } = IdentifierName(Identifiers.rawValue);

    public static IdentifierNameSyntax res { get; } = IdentifierName(Identifiers.res);

    public static IdentifierNameSyntax resType { get; } = IdentifierName(Identifiers.resType);

    public static IdentifierNameSyntax right { get; } = IdentifierName(Identifiers.right);

    public static IdentifierNameSyntax self { get; } = IdentifierName(Identifiers.self);

    public static IdentifierNameSyntax v { get; } = IdentifierName(Identifiers.v);

    public static IdentifierNameSyntax value { get; } = IdentifierName(Identifiers.value);

    public static IdentifierNameSyntax visitor { get; } = IdentifierName(Identifiers.visitor);

    public static IdentifierNameSyntax _descriptors { get; } = IdentifierName(Identifiers._descriptors);

    public static IdentifierNameSyntax _lambdaTypes { get; } = IdentifierName(Identifiers._lambdaTypes);

    public static IdentifierNameSyntax _properties { get; } = IdentifierName(Identifiers._properties);
}

internal static class Tokens
{
    public static SyntaxToken Semicolon { get; } = Token(SyntaxKind.SemicolonToken);

    public static SyntaxToken Out { get; } = Token(SyntaxKind.OutKeyword);

    public static SyntaxToken Public { get; } = Token(SyntaxKind.PublicKeyword);

    public static SyntaxToken Private { get; } = Token(SyntaxKind.PrivateKeyword);

    public static SyntaxToken Static { get; } = Token(SyntaxKind.StaticKeyword);

    public static SyntaxToken ReadOnly { get; } = Token(SyntaxKind.ReadOnlyKeyword);
}

internal class ProtocolContextEmitterV2
{
    private static readonly string SelfVersion = typeof(ProtocolContextEmitterV2).Assembly.GetName()?.Version.ToString() ?? string.Empty;

    private static AttributeSyntax GeneratedCodeAttribute => field ??= Attribute(
        ParseName("System.CodeDom.Compiler.GeneratedCodeAttribute"),
        AttributeArgumentList(SeparatedList(new AttributeArgumentSyntax[]
        {
            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("NCoreUtils.Data.Protocol.Portable.Geerator"))),
            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(SelfVersion)))
        }))
    );

    private static AccessorListSyntax AccessorAutoGet => field ??= AccessorList(
        openBraceToken: Token(SyntaxKind.OpenBraceToken),
        accessors: List(new AccessorDeclarationSyntax[]
        {
            AccessorDeclaration(SyntaxKind.GetKeyword)
        }),
        closeBraceToken: Token(SyntaxKind.CloseBraceToken)
    );

    private static MemberAccessExpressionSyntax NumberStylesInteger => field ??= SimpleMemberAccessExpression(
        type: ParseTypeName("global::System.Globalization.NumberStyles"),
        name: IdentifierName("Integer")
    );

    private static MemberAccessExpressionSyntax InvariantCulture => field ??= SimpleMemberAccessExpression(
        type: ParseTypeName("global::System.Globalization.CultureInfo"),
        name: IdentifierName("InvariantCulture")
    );

    private static AttributeListSyntax DynamicallyAccesedMembersAll => field ??= AttributeList(SingletonSeparatedList(
        Attribute(T.CodeAnalysis.DynamicallyAccessedMembersAttribute, AttributeArgumentList(
            openParenToken: Token(SyntaxKind.OpenParenToken),
            arguments: SingletonSeparatedList(
                AttributeArgument(SimpleMemberAccessExpression(T.CodeAnalysis.DynamicallyAccessedMemberTypes, IdentifierNames.All))
            ),
            closeParenToken: Token(SyntaxKind.CloseParenToken)
        ))
    ));

    private static AttributeListSyntax SuppressArrayTypeWarning => field ??= AttributeList(SingletonSeparatedList(
        Attribute(T.CodeAnalysis.UnconditionalSuppressMessageAttribute, AttributeArgumentList(
            openParenToken: Token(SyntaxKind.OpenParenToken),
            arguments: SeparatedList(
            [
                AttributeArgument(StringLiteralExpression("Trimming")),
                AttributeArgument(StringLiteralExpression("IL3050")),
            ]),
            closeParenToken: Token(SyntaxKind.CloseParenToken)
        ))
    ));

    private static ThrowExpressionSyntax ThrowNotSupportedExpression => field ??= ThrowExpression(
        ObjectCreationExpression(
            type: T.NotSupportedException,
            argumentList: Args(),
            initializer: default
        )
    );

    private static LiteralExpressionSyntax NullLiteralExpression => field ??= LiteralExpression(SyntaxKind.NullLiteralExpression);

    private static InterpolatedStringTextSyntax InterpolatedStringText(string raw)
        => SyntaxFactory.InterpolatedStringText(Token(
            leading: default,
            kind: SyntaxKind.InterpolatedStringTextToken,
            text: raw,
            valueText: raw,
            trailing: default
        ));

    private static ArgumentListSyntax Args(params ArgumentSyntax[] arguments)
        => ArgumentList(SeparatedList(arguments));

    private static BlockSyntax BracedBlock(params StatementSyntax[] body)
        => Block(
            openBraceToken: Token(SyntaxKind.OpenBraceToken),
            statements: List(body),
            closeBraceToken: Token(SyntaxKind.CloseBraceToken)
        );

    private static MemberAccessExpressionSyntax SimpleMemberAccessExpression(ExpressionSyntax type, SimpleNameSyntax name)
        => MemberAccessExpression(
            kind: SyntaxKind.SimpleMemberAccessExpression,
            expression: type,
            name: name
        );

    private static MemberAccessExpressionSyntax SimpleMemberAccessExpression(
        ExpressionSyntax root,
        ReadOnlySpan<SimpleNameSyntax> nested,
        SimpleNameSyntax name)
    {
        ExpressionSyntax source = root;
        foreach (var next in nested)
        {
            source = SimpleMemberAccessExpression(
                source,
                next
            );
        }
        return SimpleMemberAccessExpression(source, name);
    }

    private static MemberAccessExpressionSyntax SimpleMemberAccessExpression(
        ExpressionSyntax root,
        SimpleNameSyntax nested,
        SimpleNameSyntax name)
        => SimpleMemberAccessExpression(root, [nested], name);

    private static MemberAccessExpressionSyntax SimpleMemberAccessExpression(
        ExpressionSyntax root,
        SimpleNameSyntax nested1,
        SimpleNameSyntax nested2,
        SimpleNameSyntax name)
        => SimpleMemberAccessExpression(root, [nested1, nested2], name);

    private static InvocationExpressionSyntax SimpleInvocationExpression(
        TypeSyntax type,
        SimpleNameSyntax name,
        params ArgumentSyntax[] args)
        => InvocationExpression(
            SimpleMemberAccessExpression(type, name),
            Args(args)
        );

    private static InvocationExpressionSyntax SimpleInvocationExpression(
        SimpleNameSyntax name,
        params ArgumentSyntax[] args)
        => InvocationExpression(
            name,
            Args(args)
        );

    private static LiteralExpressionSyntax StringLiteralExpression(string raw)
        => LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            Token(
                leading: default,
                kind: SyntaxKind.StringLiteralToken,
                text: $"\"{raw.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"", // FIXME: optimize
                valueText: raw,
                trailing: default
            )
        );

    private static AssignmentExpressionSyntax SimpleAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        => AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);

    private static PostfixUnaryExpressionSyntax SuppressNullableWarningExpression(ExpressionSyntax body)
        => PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, body, Token(SyntaxKind.ExclamationToken));

    private static LocalDeclarationStatementSyntax SingleVar(
        TypeSyntax? type,
        SyntaxToken name,
        ExpressionSyntax? initializer = default)
        => LocalDeclarationStatement(
            modifiers: default,
            declaration: VariableDeclaration(
                type: type ?? IdentifierNames.var,
                variables: SeparatedList(new VariableDeclaratorSyntax[]
                {
                    VariableDeclarator(
                        identifier: name,
                        argumentList: default,
                        initializer: initializer is null
                            ? null
                            : EqualsValueClause(initializer)
                    )
                })

            ),
            semicolonToken: Token(SyntaxKind.SemicolonToken)
        );

    private static LocalDeclarationStatementSyntax SingleVar(
        SyntaxToken name,
        ExpressionSyntax initializer)
        => SingleVar(default, name, initializer);

    private static ClassDeclarationSyntax EmitEnumFactory(ITypeData data)
    {
        if (data.IsEnum)
        {
            var fields = data.EnumFields;
            var intType = data.EnumUndelyingTypeFullName;

            var enumFactoryName = $"{data.SafeName}EnumFactory";
            var enumFactoryTypeName = ParseTypeName(enumFactoryName);

            var singleton = PropertyDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.StaticKeyword)),
                type: enumFactoryTypeName,
                explicitInterfaceSpecifier: default,
                identifier: Identifiers.Singleton,
                accessorList: AccessorAutoGet,
                expressionBody: default,
                initializer: EqualsValueClause(ObjectCreationExpression(
                    type: enumFactoryTypeName,
                    argumentList: ArgumentList(
                        openParenToken: Token(SyntaxKind.OpenParenToken),
                        arguments: SeparatedList<ArgumentSyntax>(),
                        closeParenToken: Token(SyntaxKind.CloseParenToken)
                    ),
                    initializer: default
                )),
                semicolonToken: Tokens.Semicolon
            );

            MemberDeclarationSyntax[] methods;
            BlockSyntax singleFromRawValueBody;
            {
                var singleFromRawValueFallback = BracedBlock(
                    ThrowStatement(
                        throwKeyword: Token(SyntaxKind.ThrowKeyword),
                        expression: ObjectCreationExpression(
                            type: T.InvalidOperationException,
                            argumentList: Args(
                                Argument(InterpolatedStringExpression(
                                    stringStartToken: Token(SyntaxKind.InterpolatedStringStartToken),
                                    contents: List(new InterpolatedStringContentSyntax[]
                                    {
                                        InterpolatedStringText("Unable to parse "),
                                        Interpolation(IdentifierNames.rawValue),
                                        InterpolatedStringText($" as {data.FullName}."),
                                    }),
                                    stringEndToken: Token(SyntaxKind.InterpolatedStringEndToken)
                                ))
                            ),
                            initializer: default
                        ),
                        semicolonToken: Token(SyntaxKind.SemicolonToken)
                    )
                );

                singleFromRawValueBody = BracedBlock(
                    EmitFlagNameCheck(singleFromRawValueFallback, fields.Count - 1, data, fields)
                );

                static StatementSyntax EmitFlagNameCheck(
                StatementSyntax elseStatement,
                int index,
                ITypeData data,
                IReadOnlyList<EnumFieldData> fields)
                {
                    if (index < 0)
                    {
                        return elseStatement;
                    }
                    var field = fields[index];
                    return IfStatement(
                        condition: SimpleInvocationExpression(
                            T.Internal.EnumParserHelper,
                            IdentifierNames.IsSame,
                            Argument(StringLiteralExpression(field.Name)),
                            Argument(IdentifierNames.rawValue)
                        ),
                        statement: ReturnStatement(
                            returnKeyword: Token(SyntaxKind.ReturnKeyword),
                            expression: SimpleMemberAccessExpression(data.TypeName, IdentifierName(field.Name)),
                            semicolonToken: Token(SyntaxKind.SemicolonToken)
                        ),
                        @else: ElseClause(elseStatement)
                    );
                }
            }
            // MethodDeclarationSyntax singleFromRawValue;
            if (data.IsEnumFlags)
            {


                var singleFromRawValue = MethodDeclaration(
                    attributeLists: default,
                    modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                    returnType: data.TypeName,
                    explicitInterfaceSpecifier: default!,
                    identifier: Identifiers.SingleFromRawValue,
                    typeParameterList: default!,
                    parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
                    {
                        Parameter(
                            attributeLists: default,
                            modifiers: default,
                            type: T.IReadOnlySpanOfChar,
                            identifier: Identifiers.rawValue,
                            @default: default
                        )
                    })),
                    constraintClauses: default,
                    body: singleFromRawValueBody,
                    semicolonToken: default
                );

                MethodDeclarationSyntax fromRawValue;
                {
                    var viaEnumeration = BracedBlock(
                        SingleVar(Identifiers.res, DefaultExpression(data.TypeName)),
                        ForEachStatement(
                            type: IdentifierNames.var,
                            identifier: Identifiers.item,
                            expression: SimpleInvocationExpression(
                                type: T.Internal.EnumParserHelper,
                                name: IdentifierNames.EnumerateFlags,
                                Argument(IdentifierNames.rawValue)
                            ),
                            statement: BracedBlock(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        kind: SyntaxKind.OrAssignmentExpression,
                                        left: IdentifierNames.res,
                                        right: SimpleInvocationExpression(
                                            IdentifierNames.SingleFromRawValue,
                                            Argument(IdentifierNames.item)
                                        )
                                    ),
                                    semicolonToken: Tokens.Semicolon
                                )
                            )
                        ),
                        ReturnStatement(IdentifierNames.res)
                    );

                    var tryParseExpression = SimpleInvocationExpression(
                        type: ParseTypeName(intType),
                        name: IdentifierNames.TryParse,
                        args:
                        [
                            Argument(IdentifierNames.TryParse),
                            Argument(NumberStylesInteger),
                            Argument(InvariantCulture),
                            Argument(
                                nameColon: default,
                                refKindKeyword: Token(SyntaxKind.OutKeyword),
                                expression: DeclarationExpression(
                                    type: IdentifierNames.var,
                                    designation: SingleVariableDesignation(Identifiers.i)
                                )
                            )
                        ]
                    );

                    fromRawValue = MethodDeclaration(
                        attributeLists: default,
                        modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                        returnType: T.Object,
                        explicitInterfaceSpecifier: default!,
                        identifier: Identifiers.FromRawValue,
                        typeParameterList: default!,
                        parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
                        {
                            Parameter(
                                attributeLists: default,
                                modifiers: default,
                                type: T.String,
                                identifier: Identifiers.rawValue,
                                @default: default
                            )
                        })),
                        constraintClauses: default,
                        body: BracedBlock(
                            IfStatement(
                                condition: tryParseExpression,
                                statement: BracedBlock(
                                    ReturnStatement(CastExpression(data.TypeName, IdentifierNames.i))
                                ),
                                @else: ElseClause(viaEnumeration)
                            )
                        ),
                        semicolonToken: default
                    );
                }

                methods = [singleFromRawValue, fromRawValue];
            }
            else
            {
                var fromRawValue = MethodDeclaration(
                    attributeLists: default,
                    modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                    returnType: data.TypeName,
                    explicitInterfaceSpecifier: default!,
                    identifier: Identifiers.FromRawValue,
                    typeParameterList: default!,
                    parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
                    {
                        Parameter(
                            attributeLists: default,
                            modifiers: default,
                            type: T.String,
                            identifier: Identifiers.rawValue,
                            @default: default
                        )
                    })),
                    constraintClauses: default,
                    body: singleFromRawValueBody,
                    semicolonToken: default
                );
                methods = [fromRawValue];
            }

            var cls = ClassDeclaration(enumFactoryName)
                .AddBaseListTypes(
                    SimpleBaseType(T.Internal.IEnumFactory)
                )
                .AddAttributeLists(
                    AttributeList(SeparatedList(new AttributeSyntax[] { GeneratedCodeAttribute }))
                )
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.SealedKeyword)
                )
                .AddMembers([
                    singleton,
                    ..methods
                ]);

            return cls;
        }
        throw new InvalidOperationException($"Emitting EnumFactory for non-enum type {data.FullName}.");
    }

    private static ThrowExpressionSyntax EmitThrowCannotCreateException(
        ITypeData data,
        string expressionType,
        ExpressionSyntax arg)
        => ThrowExpression(
            ObjectCreationExpression(
                type: T.InvalidOperationException,
                argumentList: Args(
                    Argument(InterpolatedStringExpression(
                        stringStartToken: Token(SyntaxKind.InterpolatedStringStartToken),
                        contents: List(new InterpolatedStringContentSyntax[]
                        {
                            InterpolatedStringText($"Cannot create {expressionType} expression from {data.FullName} and "),
                            Interpolation(arg),
                            InterpolatedStringText("."),
                        }),
                        stringEndToken: Token(SyntaxKind.InterpolatedStringEndToken)
                    ))
                ),
                initializer: default
            )
        );

    private static ExpressionSyntax EmitCreateLinqMethodBodyForValueType(
        ITypeData data,
        string methodName,
        IdentifierNameSyntax methodId)
    {
        var throwExpression = EmitThrowCannotCreateException(data, methodName, SimpleMemberAccessExpression(
            IdentifierNames.right,
            IdentifierNames.Type
        ));

        var body = ConditionalExpression(
            condition: InvocationExpression(
                SimpleMemberAccessExpression(IdentifierNames.right, IdentifierNames.Type, IdentifierNames.Equals),
                Args(Argument(TypeOfExpression(data.TypeName)))
            ),
            whenTrue: SimpleInvocationExpression(
                T.Expression, methodId,
                Argument(IdentifierNames.self),
                Argument(IdentifierNames.right)
            ),
            whenFalse: ConditionalExpression(
                condition: InvocationExpression(
                    SimpleMemberAccessExpression(IdentifierNames.right, IdentifierNames.Type, IdentifierNames.Equals),
                    Args(Argument(TypeOfExpression(NullableType(data.TypeName, questionToken: Token(SyntaxKind.QuestionToken)))))
                ),
                whenTrue: SimpleInvocationExpression(
                    T.Expression, methodId,
                    Argument(SimpleInvocationExpression(
                        T.Expression, IdentifierNames.Convert,
                        Argument(IdentifierNames.self),
                        Argument(TypeOfExpression(NullableType(data.TypeName, questionToken: Token(SyntaxKind.QuestionToken))))
                    )),
                    Argument(IdentifierNames.right)
                ),
                whenFalse: throwExpression
            )
        );

        return body;
    }

    private static ExpressionSyntax EmitCreateLinqMethodBodyForNullableType(
        ITypeData data,
        SomeType underlying,
        string methodName,
        IdentifierNameSyntax methodId)
    {
        var throwExpression = EmitThrowCannotCreateException(data, methodName, SimpleMemberAccessExpression(
            IdentifierNames.rawValue,
            IdentifierNames.Type
        ));

        // precondition: [TYPE] is nullable
        // right.Type.Equals([Type])
        //  ? Expression.[OP](self, right)
        //  : right.Type.Equals([Underlying])
        //      ? Expression.[OP](self, Expression.Convert(right, [TYPE]))

        var body = ConditionalExpression(
            condition: InvocationExpression(
                SimpleMemberAccessExpression(IdentifierNames.right, IdentifierNames.Type, IdentifierNames.Equals),
                Args(Argument(TypeOfExpression(data.TypeName)))
            ),
            whenTrue: SimpleInvocationExpression(
                T.Expression, methodId,
                Argument(IdentifierNames.self),
                Argument(IdentifierNames.right)
            ),
            whenFalse: ConditionalExpression(
                condition: InvocationExpression(
                    SimpleMemberAccessExpression(IdentifierNames.right, IdentifierNames.Type, IdentifierNames.Equals),
                    Args(Argument(TypeOfExpression(underlying.TypeName)))
                ),
                whenTrue: SimpleInvocationExpression(
                    T.Expression, methodId,
                    Argument(IdentifierNames.self),
                    Argument(SimpleInvocationExpression(
                        T.Expression, IdentifierNames.Convert,
                        Argument(IdentifierNames.right),
                        // Argument(TypeOfExpression(NullableType(data.TypeName, questionToken: Token(SyntaxKind.QuestionToken))))
                        Argument(TypeOfExpression(data.TypeName))
                    ))
                ),
                whenFalse: throwExpression
            )
        );

        return body;
    }

    private static ExpressionSyntax EmitCreateEqualMethodBodyForValueType(ITypeData data)
        => EmitCreateLinqMethodBodyForValueType(data, "Equal", IdentifierNames.Equal);

    private static ExpressionSyntax EmitCreateEqualMethodBodyForNullableType(ITypeData data, SomeType nested)
        => EmitCreateLinqMethodBodyForNullableType(data, nested, "Equal", IdentifierNames.Equal);

    private static ArrowExpressionClauseSyntax EmitCreateEqualMethodBody(ITypeData data) => ArrowExpressionClause(data switch
    {
        { IsValueType: true, IsNullable: true } => EmitCreateEqualMethodBodyForNullableType(data, data.UnderlyingType.Value),
        { IsValueType: true } => EmitCreateEqualMethodBodyForValueType(data),
        _ => SimpleInvocationExpression(
            T.Expression, IdentifierNames.Equal,
            Argument(IdentifierNames.self),
            Argument(IdentifierNames.right)
        )
    });

    private static ExpressionSyntax EmitCreateNotEqualMethodBodyForValueType(ITypeData data)
        => EmitCreateLinqMethodBodyForValueType(data, "NotEqual", IdentifierNames.NotEqual);

    private static ExpressionSyntax EmitCreateNotEqualMethodBodyForNullableType(ITypeData data, SomeType nested)
        => EmitCreateLinqMethodBodyForNullableType(data, nested, "NotEqual", IdentifierNames.NotEqual);

    private static ArrowExpressionClauseSyntax EmitCreateNotEqualMethodBody(ITypeData data) => ArrowExpressionClause(data switch
    {
        { IsValueType: true, IsNullable: true } => EmitCreateNotEqualMethodBodyForNullableType(data, data.UnderlyingType.Value),
        { IsValueType: true } => EmitCreateNotEqualMethodBodyForValueType(data),
        _ => SimpleInvocationExpression(
            T.Expression, IdentifierNames.NotEqual,
            Argument(IdentifierNames.self),
            Argument(IdentifierNames.right)
        )
    });

    private static ClassDeclarationSyntax EmitBox(ITypeData data)
    {
        var valueType = data.IsValueType
            ? data.TypeName
            : NullableType(data.TypeName, Token(SyntaxKind.QuestionToken));
        var field = FieldDeclaration(
            attributeLists: default,
            modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword)),
            declaration: VariableDeclaration(
                type: valueType,
                variables: SeparatedList(new VariableDeclaratorSyntax[]
                {
                    VariableDeclarator(Identifiers.Value)
                })
            ),
            semicolonToken: Tokens.Semicolon
        );
        var ctor = ConstructorDeclaration(
            identifier: Identifiers.Box,
            attributeLists: default,
            modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: valueType,
                    identifier: Identifiers.value,
                    @default: default
                )
            })),
            initializer: default!,
            expressionBody: ArrowExpressionClause(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierNames.Value,
                    IdentifierNames.value
                )
            ),
            semicolonToken: Token(SyntaxKind.SemicolonToken)
        );

        return ClassDeclaration("Box")
            .AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.SealedKeyword)
            )
            .AddMembers(
                field,
                ctor
            );
    }

    private static ExpressionSyntax[] EmitPropertyValues(ITypeData data)
    {
        return [.. data.Properties.Select(p => EmitPropertyExtraction(data, p))];

        static ExpressionSyntax EmitPropertyExtraction(ITypeData data, PropertyDataV2 p)
        {
            // (PropertyInfo)((MemberExpression)((Expression<global::System.Func<{data.FullName}, {p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>>)(e => e!.{p.Name}!)).Body).Member

            var exprType = T.ExpressionFuncOf(data.TypeName, p.TypeName);
            return CastExpression(
                T.PropertyInfo,
                SimpleMemberAccessExpression(
                    ParenthesizedExpression(
                        CastExpression(
                            T.MemberExpression,
                            SimpleMemberAccessExpression(
                                ParenthesizedExpression(
                                    CastExpression(
                                        exprType,
                                        ParenthesizedExpression(
                                            SimpleLambdaExpression(
                                                modifiers: default,
                                                parameter: Parameter(Identifiers.e),
                                                block: default,
                                                expressionBody: SuppressNullableWarningExpression(SimpleMemberAccessExpression(
                                                    SuppressNullableWarningExpression(IdentifierNames.e),
                                                    IdentifierName(p.Name)
                                                ))
                                            )
                                        )
                                    )
                                ) ,
                                IdentifierNames.Body
                            )
                        )
                    ),
                    IdentifierNames.Member
                )
            );
        }
    }

    private static FieldDeclarationSyntax EmitProperties(ITypeData data)
        => FieldDeclaration(
            attributeLists: default,
            modifiers: TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)),
            declaration: VariableDeclaration(
                type: T.ArrayOfPropertyInfo,
                variables: SeparatedList(new VariableDeclaratorSyntax[]
                {
                    VariableDeclarator(
                        identifier: Identifiers._properties,
                        argumentList: default,
                        initializer: EqualsValueClause(
                            ArrayCreationExpression(
                                T.ArrayOfPropertyInfo,
                                initializer: InitializerExpression(
                                    kind: SyntaxKind.ArrayInitializerExpression,
                                    expressions: SeparatedList(EmitPropertyValues(data))
                                )
                            )
                        )
                    )
                })
            ),
            semicolonToken: Tokens.Semicolon
        );

    private static FieldDeclarationSyntax EmitBoxValueField(ITypeData data)
    {
        var valueType = data.IsValueType
            ? data.TypeName
            : NullableType(data.TypeName, Token(SyntaxKind.QuestionToken));
        // (FieldInfo)((MemberExpression)((Expression<global::System.Func<Box, {data.NullName}>>)(e => e.Value)).Body).Member
        return FieldDeclaration(
            attributeLists: default,
            modifiers: TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.ReadOnlyKeyword)),
            declaration: VariableDeclaration(
                type: T.FieldInfo,
                variables: SeparatedList(new VariableDeclaratorSyntax[]
                {
                    VariableDeclarator(
                        identifier: Identifiers.BoxValueField,
                        argumentList: default,
                        initializer: EqualsValueClause(
                            CastExpression(
                                T.FieldInfo,
                                SimpleMemberAccessExpression(
                                    ParenthesizedExpression(
                                        CastExpression(
                                            T.MemberExpression,
                                            SimpleMemberAccessExpression(
                                                ParenthesizedExpression(
                                                    CastExpression(
                                                        T.ExpressionFuncOf(IdentifierNames.Box, valueType),
                                                        ParenthesizedExpression(
                                                            SimpleLambdaExpression(
                                                                modifiers: default,
                                                                parameter: Parameter(Identifiers.e),
                                                                block: default,
                                                                expressionBody: SimpleMemberAccessExpression(
                                                                    IdentifierNames.e,
                                                                    IdentifierNames.Value
                                                                )
                                                            )
                                                        )
                                                    )
                                                ),
                                                IdentifierNames.Body
                                            )
                                        )
                                    ),
                                    IdentifierNames.Member
                                )
                            )
                        )
                    )
                })
            ),
            semicolonToken: Tokens.Semicolon
        );
    }

    private static PropertyDeclarationSyntax EmitTypeProperty(ITypeData data)
    {
        List<AttributeListSyntax> methodAttributes = new(3);
        if (data.IsLambda)
        {
            methodAttributes.Add(AttributeList(SeparatedList(new AttributeSyntax[]
            {
                Attribute(T.CodeAnalysis.UnconditionalSuppressMessageAttribute, AttributeArgumentList(
                    openParenToken: Token(SyntaxKind.OpenParenToken),
                    arguments: SeparatedList(new AttributeArgumentSyntax[]
                    {
                        AttributeArgument(StringLiteralExpression("Trimming")),
                        AttributeArgument(StringLiteralExpression("IL2026")),
                    }),
                    closeParenToken: Token(SyntaxKind.CloseParenToken)
                ))
            })));
            methodAttributes.Add(AttributeList(SeparatedList(new AttributeSyntax[]
            {
                Attribute(T.CodeAnalysis.UnconditionalSuppressMessageAttribute, AttributeArgumentList(
                    openParenToken: Token(SyntaxKind.OpenParenToken),
                    arguments: SeparatedList(new AttributeArgumentSyntax[]
                    {
                        AttributeArgument(StringLiteralExpression("Trimming")),
                        AttributeArgument(StringLiteralExpression("IL2111")),
                    }),
                    closeParenToken: Token(SyntaxKind.CloseParenToken)
                ))
            })));
        }
        if (data.IsEnum || data.IsLambda)
        {
            methodAttributes.Add(AttributeList(SeparatedList(new AttributeSyntax[]
            {
                Attribute(T.CodeAnalysis.UnconditionalSuppressMessageAttribute, AttributeArgumentList(
                    openParenToken: Token(SyntaxKind.OpenParenToken),
                    arguments: SeparatedList(new AttributeArgumentSyntax[]
                    {
                        AttributeArgument(StringLiteralExpression("Trimming")),
                        AttributeArgument(StringLiteralExpression("IL3050")),
                    }),
                    closeParenToken: Token(SyntaxKind.CloseParenToken)
                ))
            })));
        }

        return PropertyDeclaration(
            attributeLists: List([DynamicallyAccesedMembersAll]),
            modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
            type: T.Type,
            explicitInterfaceSpecifier: default,
            identifier: Identifiers.Type,
            accessorList: AccessorList(List(new AccessorDeclarationSyntax[]
            {
                AccessorDeclaration(
                    SyntaxKind.GetAccessorDeclaration,
                    attributeLists: [ ..methodAttributes ],
                    modifiers: TokenList(),
                    keyword: Token(SyntaxKind.GetKeyword),
                    expressionBody: ArrowExpressionClause(TypeOfExpression(data.TypeName)),
                    semicolonToken: Tokens.Semicolon
                )
            }))
        );
    }

    private static SyntaxList<AttributeListSyntax> AttrMaybeNullWhenFalse { get; } = List(new AttributeListSyntax[]
    {
        AttributeList(SeparatedList(new AttributeSyntax[]
        {
            Attribute(T.CodeAnalysis.MaybeNullWhen, AttributeArgumentList(SeparatedList(new AttributeArgumentSyntax[]
            {
                AttributeArgument(LiteralExpression(SyntaxKind.FalseLiteralExpression))
            })))
        }))
    });

    private static LiteralExpressionSyntax BooleanLiteralExpression(bool value)
        => LiteralExpression(value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);

    private static PropertyDeclarationSyntax ArrowPropertyDeclaration(
        SyntaxList<AttributeListSyntax> attributeLists,
        SyntaxTokenList modifiers,
        TypeSyntax type,
        SyntaxToken identifier,
        ExpressionSyntax body)
        => PropertyDeclaration(
            attributeLists: attributeLists,
            modifiers: modifiers,
            type: type,
            explicitInterfaceSpecifier: default,
            identifier: identifier,
            accessorList: default,
            expressionBody: ArrowExpressionClause(body),
            initializer: default,
            semicolonToken: Tokens.Semicolon
        );

    private static PropertyDeclarationSyntax ArrowPropertyDeclaration(
        TypeSyntax type,
        SyntaxToken identifier,
        ExpressionSyntax body)
        => PropertyDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            type: type,
            explicitInterfaceSpecifier: default,
            identifier: identifier,
            accessorList: default,
            expressionBody: ArrowExpressionClause(body),
            initializer: default,
            semicolonToken: Tokens.Semicolon
        );

    private static MethodDeclarationSyntax EmitParse(ITypeData data)
    {
        ExpressionSyntax body;
        // // FIXME: parse via enumfactory
        // if (data.IsEnum)
        // {
        // }
        if (data.IsParseable && data.IsFormattable)
        {
            body = SimpleInvocationExpression(
                data.TypeName,
                IdentifierNames.Parse,
                Argument(IdentifierNames.value)
            );
        }
        else
        {
            body = ThrowExpression(
                ObjectCreationExpression(
                    type: T.InvalidOperationException,
                    argumentList: ArgumentList(SeparatedList(new ArgumentSyntax[]
                    {
                        Argument(StringLiteralExpression($"{data.FullName} cannot be coverted from literal."))
                    })),
                    initializer: default
                )
            );
        }
        return MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Object,
            explicitInterfaceSpecifier: default,
            identifier: Identifiers.Parse,
            typeParameterList: default,
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: T.String,
                    identifier: Identifiers.value,
                    @default: default
                )
            })),
            constraintClauses: default,
            body: default,
            expressionBody: ArrowExpressionClause(body),
            semicolonToken: Tokens.Semicolon
        );
    }

    private static MethodDeclarationSyntax EmitStringify(ITypeData data)
    {
        ExpressionSyntax body;
        if (data.IsEnum)
        {
            SwitchExpressionArmSyntax[] arms =
            [
                SwitchExpressionArm(
                    pattern: ConstantPattern(NullLiteralExpression),
                    expression: DefaultExpression(T.String)
                ),
                ..data.EnumFields.Select(field => EmitEnumArm(data, field)),
                SwitchExpressionArm(
                    pattern: DeclarationPattern(data.TypeName, SingleVariableDesignation(Identifiers.v)),
                    expression: SimpleInvocationExpression(IdentifierNames.v, IdentifierNames.ToString)
                ),
                SwitchExpressionArm(
                    pattern: DiscardPattern(),
                    expression: DefaultExpression(T.String)
                )
            ];
            body = SwitchExpression(
                governingExpression: IdentifierNames.value,
                SeparatedList(arms)
            );
        }
        else if (data.IsParseable && data.IsFormattable)
        {
            body = SimpleInvocationExpression(
                IdentifierNames.value,
                IdentifierNames.ToString,
                Argument(DefaultExpression(T.String)),
                Argument(InvariantCulture)
            );
        }
        else
        {
            body = ThrowExpression(ObjectCreationExpression(
                type: T.NotSupportedException,
                argumentList: ArgumentList(SeparatedList(new ArgumentSyntax[]
                {
                    Argument(StringLiteralExpression($"{data.FullName} cannot be coverted to literal."))
                })),
                initializer: default
            ));
        }

        return MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: NullableType(T.String),
            explicitInterfaceSpecifier: default,
            identifier: Identifiers.Stringify,
            typeParameterList: default,
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: NullableType(T.Object),
                    identifier: Identifiers.value,
                    @default: default
                )
            })),
            constraintClauses: default,
            body: default,
            expressionBody: ArrowExpressionClause(body),
            semicolonToken: Tokens.Semicolon
        );

        static SwitchExpressionArmSyntax EmitEnumArm(ITypeData data, EnumFieldData field)
            => SwitchExpressionArm(
                pattern: ConstantPattern(SimpleMemberAccessExpression(data.TypeName, IdentifierName(field.Name))),
                expression: StringLiteralExpression(field.Name)
            );
    }

    private static MethodDeclarationSyntax EmitCreateBoxedConstant(ITypeData data)
    {
        var valueType = data.IsValueType
            ? data.TypeName
            : NullableType(data.TypeName, Token(SyntaxKind.QuestionToken));
        return MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Expression,
            explicitInterfaceSpecifier: default,
            identifier: Identifiers.CreateBoxedConstant,
            typeParameterList: default,
            parameterList: ParameterList(SingletonSeparatedList(
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: NullableType(T.Object),
                    identifier: Identifiers.value,
                    @default: default
                )
            )),
            constraintClauses: default,
            body: null,
            expressionBody: ArrowExpressionClause(
                SimpleInvocationExpression(
                    T.Expression, IdentifierNames.Field,
                    Argument(SimpleInvocationExpression(
                        T.Expression, IdentifierNames.Constant,
                        Argument(ObjectCreationExpression(
                            type: IdentifierNames.Box,
                            argumentList: Args(
                                Argument(SuppressNullableWarningExpression(
                                    ParenthesizedExpression(CastExpression(valueType, IdentifierNames.value))
                                ))
                            ),
                            initializer: default
                        ))
                    )),
                    Argument(IdentifierNames.BoxValueField)
                )
            ),
            semicolonToken: Tokens.Semicolon
        );
    }

    private static MethodDeclarationSyntax EmitCreateExpressionNotSupportedMethod(SyntaxToken identifier)
        => MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Expression,
            explicitInterfaceSpecifier: default,
            identifier: identifier,
            typeParameterList: default,
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: T.Expression,
                    identifier: Identifiers.self,
                    @default: default
                ),
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: T.Expression,
                    identifier: Identifiers.right,
                    @default: default
                )
            })),
            constraintClauses: default,
            body: null,
            expressionBody: ArrowExpressionClause(ThrowNotSupportedExpression),
            semicolonToken: Tokens.Semicolon
        );

    private static MethodDeclarationSyntax EmitCreateAdd(ITypeData data)
        => EmitCreateExpressionNotSupportedMethod(Identifiers.CreateAdd);

    private static MethodDeclarationSyntax EmitCreateAndAlso(ITypeData data)
        => EmitCreateExpressionNotSupportedMethod(Identifiers.CreateAndAlso);

    private static MethodDeclarationSyntax EmitCreateDivide(ITypeData data)
        => EmitCreateExpressionNotSupportedMethod(Identifiers.CreateDivide);

    private static MethodDeclarationSyntax EmitCreateEqual(ITypeData data)
        => MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Expression,
            explicitInterfaceSpecifier: default,
            identifier: Identifiers.CreateEqual,
            typeParameterList: default,
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: T.Expression,
                    identifier: Identifiers.self,
                    @default: default
                ),
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: T.Expression,
                    identifier: Identifiers.right,
                    @default: default
                )
            })),
            constraintClauses: default,
            body: null,
            expressionBody: EmitCreateEqualMethodBody(data),
            semicolonToken: Tokens.Semicolon
        );

    private static MethodDeclarationSyntax EmitCreateGreaterThan(ITypeData data)
        => EmitCreateExpressionNotSupportedMethod(Identifiers.CreateGreaterThan);

    private static MethodDeclarationSyntax EmitCreateGreaterThanOrEqual(ITypeData data)
        => EmitCreateExpressionNotSupportedMethod(Identifiers.CreateGreaterThanOrEqual);

    private static MethodDeclarationSyntax EmitCreateLessThan(ITypeData data)
        => EmitCreateExpressionNotSupportedMethod(Identifiers.CreateLessThan);

    private static MethodDeclarationSyntax EmitCreateLessThanOrEqual(ITypeData data)
        => EmitCreateExpressionNotSupportedMethod(Identifiers.CreateLessThanOrEqual);

    private static MethodDeclarationSyntax EmitCreateModulo(ITypeData data)
        => EmitCreateExpressionNotSupportedMethod(Identifiers.CreateModulo);

    private static MethodDeclarationSyntax EmitCreateMultiply(ITypeData data)
        => EmitCreateExpressionNotSupportedMethod(Identifiers.CreateMultiply);

    private static MethodDeclarationSyntax EmitCreateNotEqual(ITypeData data)
        => MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Expression,
            explicitInterfaceSpecifier: default,
            identifier: Identifiers.CreateNotEqual,
            typeParameterList: default,
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: T.Expression,
                    identifier: Identifiers.self,
                    @default: default
                ),
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: T.Expression,
                    identifier: Identifiers.right,
                    @default: default
                )
            })),
            constraintClauses: default,
            body: null,
            expressionBody: EmitCreateNotEqualMethodBody(data),
            semicolonToken: Tokens.Semicolon
        );

    private static MethodDeclarationSyntax EmitCreateOrElse(ITypeData data)
        => EmitCreateExpressionNotSupportedMethod(Identifiers.CreateOrElse);

    private static MethodDeclarationSyntax EmitCreateSubtract(ITypeData data)
        => EmitCreateExpressionNotSupportedMethod(Identifiers.CreateSubtract);

    private static AccessorDeclarationSyntax EmptyGetAccessor => field ??= AccessorDeclaration(
        kind: SyntaxKind.GetAccessorDeclaration,
        attributeLists: default,
        modifiers: default,
        keyword: Token(SyntaxKind.GetKeyword),
        expressionBody: default!,
        semicolonToken: Tokens.Semicolon
    );

    private static TupleTypeSyntax LambdaDataType => field ??= TupleType(SeparatedList(
    [
        TupleElement(type: T.Type, identifier: Identifiers.ArgType),
        TupleElement(type: T.Type, identifier: Identifiers.ResType),
        TupleElement(type: T.Type, identifier: Identifiers.LambdaType)
    ]));

    private static MethodDeclarationSyntax EmitLambdaTypesInitializer(IEnumerable<(SomeType ArgType, SomeType ResType)> items)
    {
        int supply = 0;
        var access = new Dictionary<SomeType, ExpressionSyntax>();
        var declarators = new List<VariableDeclaratorSyntax>();
        foreach (var (argType, resType) in items)
        {
            AddType(ref supply, access, declarators, argType);
            AddType(ref supply, access, declarators, resType);
        }
        var fields = LocalDeclarationStatement(
            modifiers: default,
            declaration: VariableDeclaration(
                type: T.Type,
                variables: SeparatedList(declarators)
            ),
            semicolonToken: Tokens.Semicolon
        );
        var ret = ReturnStatement(
            returnKeyword: Token(SyntaxKind.ReturnKeyword),
            expression: ArrayCreationExpression(
                type: ArrayType(LambdaDataType, SingletonList(ArrayRankSpecifier())),
                initializer: InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                    expressions: SeparatedList(items.Select(tup =>
                    {
                        var (arg, res) = tup;
                        var argSyntax = access[arg];
                        var resSyntax = access[res];
                        var lambdaType = T.FuncOf(arg.TypeName, res.TypeName);

                        return (ExpressionSyntax)TupleExpression(SeparatedList(
                        [
                            Argument(argSyntax),
                            Argument(resSyntax),
                            Argument(TypeOfExpression(lambdaType))
                        ]));
                    }))
                )
            ),
            semicolonToken: Tokens.Semicolon
        );
        return MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Private, Tokens.Static),
            returnType: ArrayType(LambdaDataType, SingletonList(ArrayRankSpecifier())),
            explicitInterfaceSpecifier: default!,
            identifier: Identifier("InitializeLambdaTypes"),
            typeParameterList: default!,
            parameterList: ParameterList(SeparatedList<ParameterSyntax>()),
            constraintClauses: default,
            body: Block(List<StatementSyntax>([fields, ret])),
            semicolonToken: default
        );

        static void AddType(ref int supply, Dictionary<SomeType, ExpressionSyntax> access, List<VariableDeclaratorSyntax> declarators, SomeType type)
        {
            if (!access.ContainsKey(type))
            {
                var id0 = supply;
                ++supply;
                var id = Identifier($"type{id0}");
                declarators.Add(VariableDeclarator(
                    identifier: id,
                    argumentList: default,
                    initializer: EqualsValueClause(TypeOfExpression(type.TypeName))
                ));
                access.Add(type, IdentifierName(id));
            }
        }
    }

    private static ClassDeclarationSyntax EmitDescriptorImpl(ITypeData data)
    {
        List<MemberDeclarationSyntax> members = new(32);
        // class:ENUMFACTORY
        if (data.IsEnum)
        {
            members.Add(EmitEnumFactory(data));
        }
        // class:BOX
        members.Add(EmitBox(data));
        // field:_properties
        members.Add(EmitProperties(data));
        // field:BoxValueField
        members.Add(EmitBoxValueField(data));
        // prop:Type
        members.Add(EmitTypeProperty(data));
        // prop:ArrayOfType
        members.Add(ArrowPropertyDeclaration(
            attributeLists: List([DynamicallyAccesedMembersAll, SuppressArrayTypeWarning]),
            modifiers: TokenList(Tokens.Public),
            type: T.Type,
            identifier: Identifiers.ArrayOfType,
            body: TypeOfExpression(ArrayType(data.TypeName, SingletonList(ArrayRankSpecifier())))
        ));
        // prop:EnumerableOfType
        members.Add(ArrowPropertyDeclaration(
            attributeLists: List([DynamicallyAccesedMembersAll]),
            modifiers: TokenList(Tokens.Public),
            type: T.Type,
            identifier: Identifiers.EnumerableOfType,
            body: TypeOfExpression(T.IEnumerableOf(data.TypeName))
        ));
        // prop: Properties
        members.Add(ArrowPropertyDeclaration(
            type: T.IReadOnlyListOfPropertyInfo,
            identifier: Identifiers.Properties,
            body: IdentifierNames._properties
        ));
        // prop: IsArithmetic
        members.Add(ArrowPropertyDeclaration(
            type: T.Boolean,
            identifier: Identifiers.IsArithmetic,
            body: BooleanLiteralExpression(false)
        ));
        // prop: IsEnum
        members.Add(ArrowPropertyDeclaration(
            type: T.Boolean,
            identifier: Identifiers.IsEnum,
            body: BooleanLiteralExpression(data.IsEnum)
        ));
        // prop: IsValue
        members.Add(ArrowPropertyDeclaration(
            type: T.Boolean,
            identifier: Identifiers.IsValue,
            body: BooleanLiteralExpression(data.IsValueType)
        ));
        // method:BoxNullable // FIXME: should be non-throwing for nullable types?
        members.Add(MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: NullableType(T.Object),
            explicitInterfaceSpecifier: default,
            identifier: Identifiers.BoxNullable,
            typeParameterList: default,
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: T.Object,
                    identifier: Identifiers.value,
                    @default: default
                )
            })),
            constraintClauses: default,
            body: default,
            expressionBody: ArrowExpressionClause(ThrowNotSupportedExpression),
            semicolonToken: Tokens.Semicolon
        ));
        // method:IsAssignableTo
        members.Add(MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Boolean,
            explicitInterfaceSpecifier: default!,
            identifier: Identifiers.IsAssignableTo,
            typeParameterList: default!,
            parameterList: ParameterList(SingletonSeparatedList(
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: T.Type,
                    identifier: Identifier("baseType"),
                    @default: default
                )
            )),
            constraintClauses: default,
            body: default,
            expressionBody: ArrowExpressionClause(
                data.AssignableToTypes
                    .Select(ty => BinaryExpression(
                        kind: SyntaxKind.EqualsExpression,
                        left: IdentifierName("baseType"),
                        right: TypeOfExpression(ty.TypeName)
                    ))
                    .Prepend(BinaryExpression(
                        kind: SyntaxKind.EqualsExpression,
                        left: IdentifierName("baseType"),
                        right: TypeOfExpression(data.TypeName)
                    ))
                    .Aggregate((a, b) => BinaryExpression(
                        kind: SyntaxKind.LogicalOrExpression,
                        left: a,
                        right: b
                    ))
            ),
            semicolonToken: Tokens.Semicolon
        ));

        // method:IsEnumerable
        members.Add(MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Boolean,
            explicitInterfaceSpecifier: default!,
            identifier: Identifiers.IsEnumerable,
            typeParameterList: default!,
            parameterList: ParameterList(SingletonSeparatedList(
                Parameter(
                    attributeLists: AttrMaybeNullWhenFalse,
                    modifiers: TokenList(Tokens.Out),
                    type: T.Type,
                    identifier: Identifiers.elementType,
                    @default: default
                )
            )),
            constraintClauses: default,
            body: Block(
                ExpressionStatement(SimpleAssignmentExpression(IdentifierNames.elementType, data.IsEnumerable
                    ? TypeOfExpression(data.ElementType.TypeName)
                    : DefaultExpression(T.Type))),
                ReturnStatement(BooleanLiteralExpression(data.IsEnumerable))
            ),
            semicolonToken: default
        ));
        // method:IsArray
        members.Add(MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Boolean,
            explicitInterfaceSpecifier: default!,
            identifier: Identifiers.IsArray,
            typeParameterList: default!,
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: AttrMaybeNullWhenFalse,
                    modifiers: TokenList(Tokens.Out),
                    type: T.Type,
                    identifier: Identifiers.elementType,
                    @default: default
                )
            })),
            constraintClauses: default,
            body: Block(
                ExpressionStatement(SimpleAssignmentExpression(IdentifierNames.elementType, data.IsArray
                    ? TypeOfExpression(data.ElementType.TypeName)
                    : DefaultExpression(T.Type))),
                ReturnStatement(BooleanLiteralExpression(data.IsArray))
            ),
            semicolonToken: default
        ));
        // method:IsLambda
        members.Add(MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Boolean,
            explicitInterfaceSpecifier: default!,
            identifier: Identifiers.IsLambda,
            typeParameterList: default!,
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: AttrMaybeNullWhenFalse,
                    modifiers: TokenList(Tokens.Out),
                    type: T.Type,
                    identifier: Identifiers.argType,
                    @default: default
                ),
                Parameter(
                    attributeLists: AttrMaybeNullWhenFalse,
                    modifiers: TokenList(Tokens.Out),
                    type: T.Type,
                    identifier: Identifiers.resType,
                    @default: default
                )
            })),
            constraintClauses: default,
            body: Block(
                ExpressionStatement(SimpleAssignmentExpression(IdentifierNames.argType, data.IsLambda
                    ? TypeOfExpression(data.LambdaArg.TypeName)
                    : DefaultExpression(T.Type))),
                ExpressionStatement(SimpleAssignmentExpression(IdentifierNames.resType, data.IsLambda
                    ? TypeOfExpression(data.LambdaRes.TypeName)
                    : DefaultExpression(T.Type))),
                ReturnStatement(BooleanLiteralExpression(data.IsLambda))
            ),
            semicolonToken: default
        ));
        // method:IsMaybe
        members.Add(MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Boolean,
            explicitInterfaceSpecifier: default!,
            identifier: Identifiers.IsMaybe,
            typeParameterList: default!,
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: AttrMaybeNullWhenFalse,
                    modifiers: TokenList(Tokens.Out),
                    type: T.Type,
                    identifier: Identifiers.elementType,
                    @default: default
                )
            })),
            constraintClauses: default,
            body: Block(
                ExpressionStatement(SimpleAssignmentExpression(IdentifierNames.elementType, DefaultExpression(T.Type))),
                ReturnStatement(BooleanLiteralExpression(false))
            ),
            semicolonToken: default
        ));
        // method:IsNullable
        members.Add(MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Boolean,
            explicitInterfaceSpecifier: default!,
            identifier: Identifiers.IsNullable,
            typeParameterList: default!,
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: AttrMaybeNullWhenFalse,
                    modifiers: TokenList(Tokens.Out),
                    type: T.Type,
                    identifier: Identifiers.elementType,
                    @default: default
                )
            })),
            constraintClauses: default,
            body: Block(
                ExpressionStatement(SimpleAssignmentExpression(IdentifierNames.elementType, data.IsNullable
                    ? TypeOfExpression(data.UnderlyingType.Value.TypeName)
                    : DefaultExpression(T.Type))),
                ReturnStatement(BooleanLiteralExpression(data.IsNullable))
            ),
            semicolonToken: default
        ));
        // method:Parse
        members.Add(EmitParse(data));
        // method:Stringify
        members.Add(EmitStringify(data));
        // method:TryGetEnumFactory
        members.Add(MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Boolean,
            explicitInterfaceSpecifier: default!,
            identifier: Identifiers.TryGetEnumFactory,
            typeParameterList: default!,
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: AttrMaybeNullWhenFalse,
                    modifiers: TokenList(Tokens.Out),
                    type: T.Internal.IEnumFactory,
                    identifier: Identifiers.enumFactory,
                    @default: default
                )
            })),
            constraintClauses: default,
            body: Block(
                ExpressionStatement(SimpleAssignmentExpression(IdentifierNames.enumFactory, data.IsEnum
                    ? SimpleMemberAccessExpression(
                        ParseTypeName($"{data.SafeName}EnumFactory"),
                        IdentifierNames.Singleton
                    )
                    : DefaultExpression(T.Internal.IEnumFactory))),
                ReturnStatement(BooleanLiteralExpression(data.IsEnum))
            ),
            semicolonToken: default
        ));
        // method:CreateBoxedConstant
        members.Add(EmitCreateBoxedConstant(data));
        // method:CreateAdd
        members.Add(EmitCreateAdd(data));
        // method:CreateAndAlso
        members.Add(EmitCreateAndAlso(data));
        // method:CreateDivide
        members.Add(EmitCreateDivide(data));
        // method:CreateEqual
        members.Add(EmitCreateEqual(data));
        // method:CreateGreaterThan
        members.Add(EmitCreateGreaterThan(data));
        // method:CreateGreaterThanOrEqual
        members.Add(EmitCreateGreaterThanOrEqual(data));
        // method:CreateLessThan
        members.Add(EmitCreateLessThan(data));
        // method:CreateLessThanOrEqual
        members.Add(EmitCreateLessThanOrEqual(data));
        // method:CreateModulo
        members.Add(EmitCreateModulo(data));
        // method:CreateMultiply
        members.Add(EmitCreateMultiply(data));
        // method:CreateNotEqual
        members.Add(EmitCreateNotEqual(data));
        // method:CreateOrElse
        members.Add(EmitCreateOrElse(data));
        // method:CreateSubtract
        members.Add(EmitCreateSubtract(data));
        // prop:EnumerableAnyMethod
        members.Add(PropertyDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            type: T.MethodInfo,
            explicitInterfaceSpecifier: default,
            identifier: Identifiers.EnumerableAnyMethod,
            accessorList: AccessorList(SingletonList(EmptyGetAccessor)),
            expressionBody: default,
            initializer: EqualsValueClause(
                SimpleInvocationExpression(
                    GenericName(
                        Identifiers.GetMethod,
                        TypeArgumentList(SeparatedList(new TypeSyntax[]
                        {
                            T.IEnumerableOf(data.TypeName),
                            T.FuncOf(data.TypeName, T.Boolean),
                            T.Boolean
                        }))
                    ),
                    Argument(SimpleMemberAccessExpression(T.Enumerable, IdentifierNames.Any))
                )
            ),
            semicolonToken: Tokens.Semicolon
        ));
        // prop:EnumerableAllMethod
        members.Add(PropertyDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            type: T.MethodInfo,
            explicitInterfaceSpecifier: default,
            identifier: Identifiers.EnumerableAllMethod,
            accessorList: AccessorList(SingletonList(EmptyGetAccessor)),
            expressionBody: default,
            initializer: EqualsValueClause(
                SimpleInvocationExpression(
                    GenericName(
                        Identifiers.GetMethod,
                        TypeArgumentList(SeparatedList(new TypeSyntax[]
                        {
                            T.IEnumerableOf(data.TypeName),
                            T.FuncOf(data.TypeName, T.Boolean),
                            T.Boolean
                        }))
                    ),
                    Argument(SimpleMemberAccessExpression(T.Enumerable, IdentifierNames.All))
                )
            ),
            semicolonToken: Tokens.Semicolon
        ));
        // prop:EnumerableContainsMethod
        members.Add(PropertyDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            type: T.MethodInfo,
            explicitInterfaceSpecifier: default,
            identifier: Identifiers.EnumerableContainsMethod,
            accessorList: AccessorList(SingletonList(EmptyGetAccessor)),
            expressionBody: default,
            initializer: EqualsValueClause(
                SimpleInvocationExpression(
                    GenericName(
                        Identifiers.GetMethod,
                        TypeArgumentList(SeparatedList(new TypeSyntax[]
                        {
                            T.IEnumerableOf(data.TypeName),
                            data.TypeName,
                            T.Boolean
                        }))
                    ),
                    Argument(SimpleMemberAccessExpression(T.Enumerable, IdentifierNames.Contains))
                )
            ),
            semicolonToken: Tokens.Semicolon
        ));
        // method:Accept
        members.Add(MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Tokens.Public),
            returnType: T.Void,
            explicitInterfaceSpecifier: default,
            identifier: Identifiers.Accept,
            typeParameterList: default,
            parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
            {
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: T.Internal.IDataTypeVisitor,
                    identifier: Identifiers.visitor,
                    @default: default
                )
            })),
            constraintClauses: default,
            body: default,
            expressionBody: ArrowExpressionClause(
                SimpleInvocationExpression(
                    IdentifierNames.visitor,
                    GenericName(
                        Identifiers.Visit,
                        TypeArgumentList(SeparatedList(new TypeSyntax[]
                        {
                            data.TypeName
                        }))
                    )
                )
            ),
            semicolonToken: Tokens.Semicolon
        ));

        return ClassDeclaration($"{data.SafeName}Descriptor")
            .AddAttributeLists(
                AttributeList(SeparatedList(new AttributeSyntax[]
                {
                    Attribute(T.Internal.DescribedTypeAttribute, AttributeArgumentList(SeparatedList(new AttributeArgumentSyntax[]
                    {
                        AttributeArgument(TypeOfExpression(data.TypeName))
                    })))
                }))
            )
            .AddBaseListTypes(
                SimpleBaseType(T.Internal.ITypeDescriptor)
            )
            .AddModifiers(
                Tokens.Public,
                Token(SyntaxKind.SealedKeyword)
            )
            .AddMembers([.. members]);
    }

    // private static int BuildVersion { get; set; }

    public static CompilationUnitSyntax EmitContext(
        string @namespace,
        string name,
        IReadOnlyCollection<ITypeData> types,
        IReadOnlyDictionary<string, SomeType> explicitDescriptors,
        IReadOnlyCollection<(SomeType Arg, SomeType Res)> lambdaTypes,
        Action<Diagnostic> reportDiagnostics)
    {
        try
        {
            List<MemberDeclarationSyntax> members = new(types.Count + 12);

            // class*:type descriptors
            members.AddRange(types.Where(ty => ty is not PrimitiveValueTypeData).Select(EmitDescriptorImpl));
            // field:_descriptors
            members.Add(FieldDeclaration(
                attributeLists: default,
                modifiers: TokenList(Tokens.Private, Tokens.Static, Tokens.ReadOnly),
                declaration: VariableDeclaration(
                    type: ArrayType(T.Internal.ITypeDescriptor, SingletonList(ArrayRankSpecifier())),
                    variables: SeparatedList(new VariableDeclaratorSyntax[]
                    {
                        VariableDeclarator(
                            identifier: Identifiers._descriptors,
                            argumentList: default,
                            initializer: EqualsValueClause(ArrayCreationExpression(
                                type: ArrayType(T.Internal.ITypeDescriptor, SingletonList(ArrayRankSpecifier())),
                                InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                                    expressions: SeparatedList(types.Where(ty => ty is not PrimitiveValueTypeData).Select(data =>
                                    {
                                        var typeName = explicitDescriptors.TryGetValue(data.FullName, out var ty)
                                            ? ty.TypeName
                                            : ParseTypeName($"{data.SafeName}Descriptor");
                                        return (ExpressionSyntax)ObjectCreationExpression(
                                            type: typeName,
                                            argumentList: Args(),
                                            initializer: default
                                        );
                                    }).ToArray()))
                            ))
                        )
                    })
                ),
                semicolonToken: Tokens.Semicolon
            ));
            // field:_lambdaTypes
            members.Add(EmitLambdaTypesInitializer(lambdaTypes));
            members.Add(FieldDeclaration(
                attributeLists: default,
                modifiers: TokenList(Tokens.Private, Tokens.Static, Tokens.ReadOnly),
                declaration: VariableDeclaration(
                    type: ArrayType(LambdaDataType, SingletonList(ArrayRankSpecifier())),
                    variables: SingletonSeparatedList(VariableDeclarator(
                        identifier: Identifiers._lambdaTypes,
                        argumentList: default,
                        initializer: EqualsValueClause(SimpleInvocationExpression(IdentifierName("InitializeLambdaTypes")))
                    ))
                ),
                semicolonToken: Tokens.Semicolon
            ));
            // prop:Singleton
            members.Add(PropertyDeclaration(
                attributeLists: default,
                modifiers: TokenList(Tokens.Public, Tokens.Static),
                type: T.Internal.IPortableDataContext,
                explicitInterfaceSpecifier: default,
                identifier: Identifiers.Singleton,
                accessorList: AccessorList(SingletonList(EmptyGetAccessor)),
                expressionBody: default,
                initializer: EqualsValueClause(ObjectCreationExpression(IdentifierName(name), Args(), initializer: default)),
                semicolonToken: Tokens.Semicolon
            ));
            // method:GetMethod
            members.Add(MethodDeclaration(
                attributeLists: default,
                modifiers:  TokenList(Tokens.Private, Tokens.Static),
                returnType: T.MethodInfo,
                explicitInterfaceSpecifier: default,
                identifier: Identifiers.GetMethod,
                typeParameterList: TypeParameterList(SeparatedList(new TypeParameterSyntax[]
                {
                    TypeParameter("TArg1"),
                    TypeParameter("TArg2"),
                    TypeParameter("TResult")
                })),
                parameterList: ParameterList(SeparatedList(new ParameterSyntax[]
                {
                    Parameter(
                        attributeLists: default,
                        modifiers: default,
                        type: T.Func2Of(IdentifierName("TArg1"), IdentifierName("TArg2"), IdentifierName("TResult")),
                        identifier: Identifiers.func,
                        @default: default
                    )
                })),
                constraintClauses: default,
                body: default,
                expressionBody: ArrowExpressionClause(SimpleMemberAccessExpression(IdentifierNames.func, IdentifierNames.Method)),
                semicolonToken: Tokens.Semicolon
            ));
            // method:GetTypeDescriptors
            members.Add(MethodDeclaration(
                attributeLists: default,
                modifiers:  TokenList(Tokens.Public),
                returnType: T.IEnumerableOf(T.Internal.ITypeDescriptor),
                explicitInterfaceSpecifier: default,
                identifier: Identifiers.GetTypeDescriptors,
                typeParameterList: default,
                parameterList: ParameterList(SeparatedList(Array.Empty<ParameterSyntax>())),
                constraintClauses: default,
                body: default,
                expressionBody: ArrowExpressionClause(IdentifierNames._descriptors),
                semicolonToken: Tokens.Semicolon
            ));
            // method:GetLambdaTypes
            members.Add(MethodDeclaration(
                attributeLists: default,
                modifiers:  TokenList(Tokens.Public),
                returnType: T.IEnumerableOf(LambdaDataType),
                explicitInterfaceSpecifier: default,
                identifier: Identifiers.GetLambdaTypes,
                typeParameterList: default,
                parameterList: ParameterList(SeparatedList(Array.Empty<ParameterSyntax>())),
                constraintClauses: default,
                body: default,
                expressionBody: ArrowExpressionClause(IdentifierNames._lambdaTypes),
                semicolonToken: Tokens.Semicolon
            ));


            var @class = ClassDeclaration(name)
                .AddAttributeLists(
                    AttributeList(SeparatedList(new AttributeSyntax[] { GeneratedCodeAttribute }))
                )
                .AddBaseListTypes(
                    SimpleBaseType(T.Internal.IPortableDataContext)
                )
                .AddModifiers(
                    Token(SyntaxKind.PartialKeyword)
                )
                .AddMembers([.. members]);

            SyntaxTriviaList syntaxTriviaList = TriviaList(
                Comment("// <auto-generated/>"),
                Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true))
            );

            return CompilationUnit()
                .AddUsings(
                    UsingDirective(
                        usingKeyword: Token(SyntaxKind.UsingKeyword),
                        staticKeyword: default,
                        alias: NameEquals(IdentifierName("PropertyInfo")),
                        name: ParseName("System.Reflection.PropertyInfo"),
                        semicolonToken: Tokens.Semicolon
                    )
                )
                .AddMembers(
                    NamespaceDeclaration(IdentifierName(@namespace))
                        .WithLeadingTrivia(syntaxTriviaList)
                        .AddMembers(@class)
                )
                .NormalizeWhitespace();
        }
        catch (Exception exn)
        {
            reportDiagnostics(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "NCU0000",
                    "An exception was thrown by the ProtocolContextEmitter generator",
                    "An exception was thrown by the ProtocolContextEmitter generator: '{0}'",
                    "ProtocolContextEmitter",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                Location.None,
                exn.ToString()
            ));
            return CompilationUnit();
        }
    }
}