using FluentAssertions;
using System;
using Reqnroll.TestProjectGenerator.Dotnet;
using Reqnroll.TestProjectGenerator.Tests.Stubs;
using Xunit;
using Xunit.Abstractions;

namespace Reqnroll.TestProjectGenerator.Tests
{

    public class CommandBuilderTests
    {
        private readonly ITestOutputHelper output;

        public CommandBuilderTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void AbleToRetryOnce()
        {
            //arrange
            var commandBuilder = new CommandBuilder(new OutputWriterStub(output), string.Empty, string.Empty, string.Empty);

            //act
            var ex = Assert.Throws<AggregateException>(() => commandBuilder.ExecuteWithRetry(1, TimeSpan.Zero, ex => ex));

            //assert
            ex.InnerExceptions.Count.Should().Be(2);
        }

        [Fact]
        public void AbleToRetryMultipleTimes()
        {
            //arrange
            var commandBuilder = new CommandBuilder(new OutputWriterStub(output), string.Empty, string.Empty, string.Empty);

            //act
            var ex = Assert.Throws<AggregateException>(() => commandBuilder.ExecuteWithRetry(2, TimeSpan.Zero, ex => ex));

            //assert
            ex.InnerExceptions.Count.Should().Be(3);
        }
    }
}
