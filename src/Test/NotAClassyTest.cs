using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools.Test {
    [TestClass]
    public class NotAClassyTest {
        [TestMethod]
        public void NotATestCase() {
            var sut = new ContainerBuilder().RegisterForVishizhukelNetIntegrationTest().Build();
            Assert.IsNotNull(sut);
        }
    }
}
