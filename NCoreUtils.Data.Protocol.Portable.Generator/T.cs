using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NCoreUtils.Data.Protocol.Generator;

internal static class T
{
    public static class Internal
    {
        public static TypeSyntax IDataTypeVisitor { get; } = ParseTypeName("global::NCoreUtils.Data.Protocol.Internal.IDataTypeVisitor");

        public static TypeSyntax IEnumFactory { get; } = ParseTypeName("global::NCoreUtils.Data.Protocol.Internal.IEnumFactory");

        public static TypeSyntax ITypeDescriptor { get; } = ParseTypeName("global::NCoreUtils.Data.Protocol.Internal.ITypeDescriptor");

        public static TypeSyntax EnumParserHelper { get; } = ParseTypeName("global::NCoreUtils.Data.Protocol.Internal.EnumParserHelper");

        public static NameSyntax DescribedTypeAttribute { get; } = ParseName("global::NCoreUtils.Data.Protocol.Internal.DescribedTypeAttribute");

        public static NameSyntax IPortableDataContext { get; } = ParseName("global::NCoreUtils.Data.Protocol.IPortableDataContext");
    }

    public static class CodeAnalysis
    {
        public static NameSyntax DynamicallyAccessedMembersAttribute { get; } = ParseName("global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute");

        public static NameSyntax DynamicallyAccessedMemberTypes { get; } = ParseName("global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes");

        public static NameSyntax UnconditionalSuppressMessageAttribute { get; } = ParseName("global::System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessageAttribute");

        public static NameSyntax MaybeNullWhen { get; } = ParseName("global::System.Diagnostics.CodeAnalysis.MaybeNullWhen");
    }

    public static TypeSyntax Expression { get; } = ParseTypeName("global::System.Linq.Expressions.Expression");

    public static PredefinedTypeSyntax Void { get; } = PredefinedType(Token(SyntaxKind.VoidKeyword));

    public static PredefinedTypeSyntax Boolean { get; } = PredefinedType(Token(SyntaxKind.BoolKeyword));

    public static PredefinedTypeSyntax Object { get; } = PredefinedType(Token(SyntaxKind.ObjectKeyword));

    public static PredefinedTypeSyntax String { get; } = PredefinedType(Token(SyntaxKind.StringKeyword));

    public static TypeSyntax Type { get; } = ParseTypeName("global::System.Type");

    public static TypeSyntax Enumerable { get; } = ParseTypeName("global::System.Linq.Enumerable");

    public static TypeSyntax FieldInfo { get; } = ParseTypeName("global::System.Reflection.FieldInfo");

    public static TypeSyntax IReadOnlyListOfPropertyInfo { get; } = ParseTypeName("global::System.Collections.Generic.IReadOnlyList<PropertyInfo>");

    public static TypeSyntax IReadOnlySpanOfChar { get; } = ParseTypeName("global::System.ReadOnlySpan<char>");

    public static TypeSyntax InvalidOperationException { get; } = ParseTypeName("global::System.InvalidOperationException");

    public static TypeSyntax NotSupportedException { get; } = ParseTypeName("global::System.NotSupportedException");

    public static TypeSyntax MemberExpression { get; } = ParseTypeName("global::System.Linq.Expressions.MemberExpression");

    public static TypeSyntax MethodInfo { get; } = ParseTypeName("global::System.Reflection.MethodInfo");

    public static TypeSyntax PropertyInfo { get; } = ParseTypeName("global::System.Reflection.PropertyInfo");

    public static ArrayTypeSyntax ArrayOfPropertyInfo { get; } = ArrayType(PropertyInfo);

    #region generics

    private static class Id
    {
        public static IdentifierNameSyntax @global { get; } = IdentifierName("global");

        public static IdentifierNameSyntax System { get; } = IdentifierName("System");

        public static IdentifierNameSyntax Collections { get; } = IdentifierName("Collections");

        public static IdentifierNameSyntax Generic { get; } = IdentifierName("Generic");

        public static IdentifierNameSyntax Linq { get; } = IdentifierName("Linq");

        public static IdentifierNameSyntax Expressions { get; } = IdentifierName("Expressions");

        public static SyntaxToken Expression { get; } = Identifier("Expression");

        public static SyntaxToken Func { get; } = Identifier("Func");

        public static SyntaxToken IEnumerable { get; } = Identifier("IEnumerable");
    }

    public static TypeSyntax FuncOf(TypeSyntax source, TypeSyntax target)
        => QualifiedName(
            AliasQualifiedName(
                Id.global,
                Id.System
            ),
            GenericName(
                Id.Func,
                TypeArgumentList(SeparatedList(new TypeSyntax[]
                {
                    source,
                    target
                }))
            )
        );

    public static TypeSyntax Func2Of(TypeSyntax source1, TypeSyntax source2, TypeSyntax target)
        => QualifiedName(
            AliasQualifiedName(
                Id.global,
                Id.System
            ),
            GenericName(
                Id.Func,
                TypeArgumentList(SeparatedList(new TypeSyntax[]
                {
                    source1,
                    source2,
                    target
                }))
            )
        );

    public static TypeSyntax ExpressionFuncOf(TypeSyntax source, TypeSyntax target)
        => QualifiedName(
            QualifiedName(
                QualifiedName(
                    AliasQualifiedName(
                        Id.global,
                        Id.System
                    ),
                    Id.Linq
                ),
                Id.Expressions
            ),
            GenericName(
                Id.Expression,
                TypeArgumentList(SeparatedList(new TypeSyntax[]
                {
                    FuncOf(source, target)
                }))
            )
        );

    public static TypeSyntax IEnumerableOf(TypeSyntax elementType)
        => QualifiedName(
            QualifiedName(
                QualifiedName(
                    AliasQualifiedName(
                        Id.global,
                        Id.System
                    ),
                    Id.Collections
                ),
                Id.Generic
            ),
            GenericName(
                Id.IEnumerable,
                TypeArgumentList(SeparatedList(new TypeSyntax[]
                {
                    elementType
                }))
            )
        );

    #endregion
}
