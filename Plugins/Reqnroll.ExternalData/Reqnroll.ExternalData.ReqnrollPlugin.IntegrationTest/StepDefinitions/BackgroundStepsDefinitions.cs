using NUnit.Framework;
using Reqnroll;

namespace XReqnroll.ExternalData.ReqnrollPlugin.IntegrationTest.StepDefinitions
{
    [Binding]
    internal class BackgroundStepsDefinitions
    {
        [Given("my favorite color is {word}")]
        public void GivenMyFavoriteColorIs(string color)
        {
            _color = color;
        }

        private string _color;

        [Then("the color given as my favorite was {word}")]
        public void ThenTheColorGivenAsMyFavoriteWas(string color)
        {
            Assert.That(_color, Is.EqualTo(color));
        }
    }
}
