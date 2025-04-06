using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// A wrapper for either a <see cref="SyntaxNode"/> or <see cref="SyntaxToken"/>.
/// </summary>
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public readonly struct SyntaxNodeOrToken : IEquatable<SyntaxNodeOrToken>
{
    private readonly SyntaxNode? _node;
    private readonly InternalNode? _token;
    private readonly int _position;

    private SyntaxNodeOrToken(SyntaxNode node)
    {
        _node = node;
    }

    private SyntaxNodeOrToken(SyntaxNode? node, InternalNode? token, int position)
    {
        _node = node;
        _token = token;
        _position = position;
    }

    public bool IsToken => _token != null;

    public bool IsNode => _node != null && _token == null;

    public SyntaxKind Kind
    {
        get
        {
            if (_token != null)
            {
                return _token.Kind;
            }

            if (_node != null)
            {
                return _node.Kind;
            }

            return SyntaxKind.None;
        }
    }

    public SyntaxToken AsToken()
    {
        if (_token != null)
        {
            return new SyntaxToken(_node, _token, _position);
        }

        return default;
    }

    public SyntaxNode? AsNode()
    {
        if (_token == null)
        {
            return _node;
        }

        return null;
    }

    public override string ToString()
    {
        if (_token != null)
        {
            return _token.ToString();
        }

        if (_node != null)
        {
            return _node.ToString();
        }

        return string.Empty;
    }

    public string ToFullString()
    {
        if (_token != null)
        {
            return _token.ToFullString();
        }

        if (_node != null)
        {
            return _node.ToFullString();
        }

        return string.Empty;
    }

    public bool Equals(SyntaxNodeOrToken other)
    {
        if (_token != null)
        {
            return _token.Equals(other._token);
        }

        if (_node != null)
        {
            return _node.Equals(other._node);
        }

        return other._token == null && other._node == null;
    }

    public override bool Equals(object? obj)
    {
        if (obj is SyntaxNodeOrToken other)
        {
            return Equals(other);
        }

        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 15040323;

            hash *= 75456306 + _node?.GetHashCode() ?? 0;
            hash *= 75456306 + _token?.GetHashCode() ?? 0;
            hash *= 75456306 + _position.GetHashCode();

            return hash;
        }
    }

    public static implicit operator SyntaxNodeOrToken(SyntaxNode? node)
    {
        if (node == null)
        {
            return default;
        }

        return new SyntaxNodeOrToken(node);
    }

    public static implicit operator SyntaxNodeOrToken(SyntaxToken token)
    {
        return new SyntaxNodeOrToken(token.Parent, token.InternalNode, token.Position);
    }

    internal string GetDebuggerDisplay() => GetType().Name + " " + Kind + " " + ToString();

    public static bool operator ==(SyntaxNodeOrToken left, SyntaxNodeOrToken right) => left.Equals(right);

    public static bool operator !=(SyntaxNodeOrToken left, SyntaxNodeOrToken right) => !(left == right);
}
