using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools.Test {
    [TestClass]
    public class NotAClassyTest {
        [TestMethod]
        public void NotATestCase() {
            var logConfigurationMock = new Mock<ILogConfiguration>();
            var sut = new ContainerBuilder().RegisterForVishizhukelNetIntegrationTest(logConfigurationMock.Object).Build();
            Assert.IsNotNull(sut);
        }
    }
}
