using System.Collections.Generic;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace Reqnroll.TestProjectGenerator.Tests
{
    public class TRXParserTests
    {
        private static readonly XNamespace TrxXmlns = XNamespace.Get("http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

        public static readonly IEnumerable<object[]> GetXUnitPendingCount_PendingTests_ShouldReturnNumberOfPendingTests_Data = new[]
        {
            new object[] { new XElement(TrxXmlns + "Results"), 0},
            new object[] { new XElement(TrxXmlns + "Results", new XElement(TrxXmlns + "UnitTestResult")), 0},
            new object[]
            {
                new XElement(
                    TrxXmlns + "Results",
                    new XElement(
                        TrxXmlns + "UnitTestResult",
                        new XElement(
                            TrxXmlns + "Output",
                            new XElement(TrxXmlns + "StdOut")
                            {
                                Value = "XUnitPendingStepException"
                            }))),
                    1
            },
            new object[]
            {
                new XElement(
                    TrxXmlns + "Results",
                    new XElement(
                        TrxXmlns + "UnitTestResult",
                        new XElement(
                            TrxXmlns + "Output",
                            new XElement(TrxXmlns + "StdOut")
                            {
                                Value = "XUnitPendingStepException XUnitPendingStepException"
                            }))),
                    1
            },
            new object[]
            {
                new XElement(
                    TrxXmlns + "Results",
                    new XElement(
                        TrxXmlns + "UnitTestResult",
                        new XElement(
                            TrxXmlns + "Output",
                            new XElement(TrxXmlns + "StdOut")
                            {
                                Value = "XUnitInconclusiveException"
                            }))),
                    1
            },
            new object[]
            {
                new XElement(
                    TrxXmlns + "Results",
                    new XElement(
                        TrxXmlns + "UnitTestResult",
                        new XElement(
                            TrxXmlns + "Output",
                            new XElement(TrxXmlns + "StdOut")
                            {
                                Value = "XUnitPendingStepException"
                            })),
                    new XElement(
                        TrxXmlns + "UnitTestResult",
                        new XElement(
                            TrxXmlns + "Output",
                            new XElement(TrxXmlns + "StdOut")
                            {
                                Value = "XUnitInconclusiveException"
                            }))),
                    2
            }
        };

        [Theory]
        [MemberData(nameof(GetXUnitPendingCount_PendingTests_ShouldReturnNumberOfPendingTests_Data))]
        public void GetXUnitPendingCount_PendingTests_ShouldReturnNumberOfPendingTests(XElement resultsElement, int expectedPendingTests)
        {
            // ARRANGE
            var parser = new TRXParser(new TestRunConfiguration());

            // ACT
            int actualPendingTests = parser.GetXUnitPendingCount(resultsElement);

            // ASSERT
            actualPendingTests.Should().Be(expectedPendingTests);
        }
    }
}
