using FluentAssertions.Equivalency;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.Tests
{
    public class FluentAssertionsCustomStringComparisons : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            
            x = x.Replace("\r\n", "\n");
            y = y.Replace("\r\n", "\n");
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            return obj.GetHashCode();
        }

    }
}
