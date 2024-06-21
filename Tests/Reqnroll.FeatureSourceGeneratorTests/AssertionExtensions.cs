using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.Contracts;

namespace Reqnroll.FeatureSourceGenerator;
public static class AssertionExtensions
{
    /// <summary>
    /// Returns an <see cref="CSharpSyntaxAssertions{TNode}"/> object that can be used to assert the
    /// current <see cref="CSharpSyntaxNode"/>.
    /// </summary>
    [Pure]
    public static CSharpSyntaxAssertions<TNode> Should<TNode>(this TNode? actualValue) where TNode : CSharpSyntaxNode => 
        new(actualValue);

    /// <summary>
    /// Returns an <see cref="CSharpClassDeclarationAssertions"/> object that can be used to assert the
    /// current <see cref="ClassDeclarationSyntax"/>.
    /// </summary>
    [Pure]
    public static CSharpClassDeclarationAssertions Should(this ClassDeclarationSyntax? actualValue) =>
        new(actualValue);

    /// <summary>
    /// Returns an <see cref="CSharpNamespaceDeclarationAssertions"/> object that can be used to assert the
    /// current <see cref="ClassDeclarationSyntax"/>.
    /// </summary>
    [Pure]
    public static CSharpNamespaceDeclarationAssertions Should(this NamespaceDeclarationSyntax? actualValue) =>
        new(actualValue);

    /// <summary>
    /// Returns an <see cref="CSharpMethodDeclarationAssertions"/> object that can be used to assert the
    /// current <see cref="MethodDeclarationSyntax"/>.
    /// </summary>
    [Pure]
    public static CSharpMethodDeclarationAssertions Should(this MethodDeclarationSyntax? actualValue) =>
        new(actualValue);

    /// <summary>
    /// Returns an <see cref="TestMethodAssertions"/> object that can be used to assert the
    /// current <see cref="TestMethod"/>.
    /// </summary>
    [Pure]
    public static TestMethodAssertions Should(this TestMethod? actualValue) =>
        new(actualValue);
}
