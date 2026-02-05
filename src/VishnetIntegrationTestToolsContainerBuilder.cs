using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools;

public static class VishnetIntegrationTestToolsContainerBuilder {
    public static ContainerBuilder RegisterForVishizhukelNetIntegrationTest(this ContainerBuilder builder, string applicationName) {
        builder.UseDvinAndPegh(applicationName);
        return builder;
    }
}