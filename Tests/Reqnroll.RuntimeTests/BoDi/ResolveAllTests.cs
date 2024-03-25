using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public interface IFancy { }
    public class ImFancy : IFancy { }
    public class ImFancier : IFancy { }
    public class ImFanciest : IFancy { }

    public class ResolveAllTests
    {
        [Fact]
        public void ShouldResolveTheRightNumberOfRegisteredTypes()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<ImFancy, IFancy>("fancy");
            container.RegisterTypeAs<ImFancier, IFancy>("fancier");
            container.RegisterTypeAs<ImFanciest, IFancy>("fanciest");

            // when
            var results = container.ResolveAll<IFancy>();

            // then
            results.Should().HaveCount(3);
        }

        [Fact]
        public void ShouldResolveTheRightTypes()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<ImFancy, IFancy>("fancy");
            container.RegisterTypeAs<ImFancier, IFancy>("fancier");

            // when
            var results = container.ResolveAll<IFancy>();

            // then
            results.Should().Contain(container.Resolve<IFancy>("fancy"));
            results.Should().Contain(container.Resolve<IFancy>("fancier"));
        }
    }
}
