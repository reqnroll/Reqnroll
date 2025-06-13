using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

internal class BuiltSourceText(StringBuilder builder, Encoding? encoding, SourceHashAlgorithm checksumAlgorithm) : 
    SourceText(checksumAlgorithm: checksumAlgorithm)
{
    public override char this[int position] => builder[position];

    public override Encoding? Encoding => encoding;

    public override int Length => builder.Length;

    public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
    {
        builder.CopyTo(sourceIndex, destination, destinationIndex, count);
    }
}
