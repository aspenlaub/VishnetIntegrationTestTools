using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools.Test;

[TestClass]
public class NotAClassyTest {
    [TestMethod]
    public void ContainerBuilderTest() {
        var sut = new ContainerBuilder().RegisterForVishizhukelNetIntegrationTest("VishnetIntegrationTestTools").Build();
        Assert.IsNotNull(sut);
    }

    [TestMethod]
    public void WindowUnderTestActionsBaseTest() {
        var tashAccessor = new Mock<ITashAccessor>();
        var sut = new WindowUnderTestActionsBase(tashAccessor.Object, "");
        Assert.IsNotNull(sut);
    }
}