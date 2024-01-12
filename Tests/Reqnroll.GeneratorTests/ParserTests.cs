using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Reqnroll.Parser;

namespace Reqnroll.GeneratorTests
{
    
    public class ParserTests
    {
        [Fact]
        public void Parser_handles_empty_feature_file_without_error()
        {
            var parser = new ReqnrollGherkinParser(CultureInfo.GetCultureInfo("en"));

            Action act = () => parser.Parse(new StringReader(""), null);

            act.Should().NotThrow();
        }
    }
}
