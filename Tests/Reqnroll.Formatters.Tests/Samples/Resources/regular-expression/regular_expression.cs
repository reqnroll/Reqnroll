using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reqnroll;

namespace Reqnroll.Formatters.Tests.Samples.Resources.regular_expression;

[Binding]
public class regular_expression
{
    [Given("^a (.*?)(?: and a (.*?))?(?: and a (.*?))?$")]
    public void GivenVegetables(string vegetable1, string vegetable2 = null, string vegetable3 = null)
    {
        // no-op
    }
}
