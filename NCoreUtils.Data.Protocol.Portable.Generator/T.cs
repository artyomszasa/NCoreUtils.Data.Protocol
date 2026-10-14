using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NCoreUtils.Data.Protocol.Generator;

internal static class T
{
    public static class Internal
    {
        public static TypeSyntax IDataTypeVisitor => ParseTypeName("global::NCoreUtils.Data.Protocol.Internal.IDataTypeVisitor");

        public static TypeSyntax IEnumFactory => ParseTypeName("global::NCoreUtils.Data.Protocol.Internal.IEnumFactory");

        public static TypeSyntax ITypeDescriptor => ParseTypeName("global::NCoreUtils.Data.Protocol.Internal.ITypeDescriptor");

        public static TypeSyntax EnumParserHelper => ParseTypeName("global::NCoreUtils.Data.Protocol.Internal.EnumParserHelper");

        public static NameSyntax DescribedTypeAttribute => ParseName("global::NCoreUtils.Data.Protocol.Internal.DescribedTypeAttribute");

        public static NameSyntax IPortableDataContext => ParseName("global::NCoreUtils.Data.Protocol.IPortableDataContext");

        public static TypeSyntax OpaqueTypeDescriptorBase => ParseTypeName("global::NCoreUtils.Data.Protocol.Internal.OpaqueTypeDescriptorBase");
    }

    public static class CodeAnalysis
    {
        public static NameSyntax DynamicallyAccessedMembersAttribute => ParseName("global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute");

        public static NameSyntax DynamicallyAccessedMemberTypes => ParseName("global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes");

        public static NameSyntax UnconditionalSuppressMessageAttribute => ParseName("global::System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessageAttribute");

        public static NameSyntax MaybeNullWhen => ParseName("global::System.Diagnostics.CodeAnalysis.MaybeNullWhen");
    }

    public static TypeSyntax Expression => ParseTypeName("global::System.Linq.Expressions.Expression");

    public static PredefinedTypeSyntax Void => PredefinedType(Token(SyntaxKind.VoidKeyword));

    public static PredefinedTypeSyntax Boolean => PredefinedType(Token(SyntaxKind.BoolKeyword));

    public static PredefinedTypeSyntax Object => PredefinedType(Token(SyntaxKind.ObjectKeyword));

    public static PredefinedTypeSyntax String => PredefinedType(Token(SyntaxKind.StringKeyword));

    public static TypeSyntax Type => ParseTypeName("global::System.Type");

    public static TypeSyntax Enumerable => ParseTypeName("global::System.Linq.Enumerable");

    public static TypeSyntax FieldInfo => ParseTypeName("global::System.Reflection.FieldInfo");

    public static TypeSyntax IReadOnlyListOfPropertyInfo => ParseTypeName("global::System.Collections.Generic.IReadOnlyList<PropertyInfo>");

    public static TypeSyntax IReadOnlySpanOfChar => ParseTypeName("global::System.ReadOnlySpan<char>");

    public static TypeSyntax IFormattable => ParseTypeName("global::System.IFormattable");

    public static TypeSyntax InvalidOperationException => ParseTypeName("global::System.InvalidOperationException");

    public static TypeSyntax NotSupportedException => ParseTypeName("global::System.NotSupportedException");

    public static TypeSyntax MemberExpression => ParseTypeName("global::System.Linq.Expressions.MemberExpression");

    public static TypeSyntax MethodInfo => ParseTypeName("global::System.Reflection.MethodInfo");

    public static TypeSyntax PropertyInfo => ParseTypeName("global::System.Reflection.PropertyInfo");

    public static ArrayTypeSyntax ArrayOfPropertyInfo => ArrayType(PropertyInfo, SingletonList(ArrayRankSpecifier()));

    #region generics

    private static class Id
    {
        public static IdentifierNameSyntax @global => IdentifierName("global");

        public static IdentifierNameSyntax System => IdentifierName("System");

        public static IdentifierNameSyntax Collections => IdentifierName("Collections");

        public static IdentifierNameSyntax Generic => IdentifierName("Generic");

        public static IdentifierNameSyntax Linq => IdentifierName("Linq");

        public static IdentifierNameSyntax Expressions => IdentifierName("Expressions");

        public static SyntaxToken Expression => Identifier("Expression");

        public static SyntaxToken Func => Identifier("Func");

        public static SyntaxToken IEnumerable => Identifier("IEnumerable");
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
