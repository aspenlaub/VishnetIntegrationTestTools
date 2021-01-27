using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools {
    public static class VishnetIntegrationTestToolsContainerBuilder {
        public static ContainerBuilder RegisterForVishizhukelNetIntegrationTest(this ContainerBuilder builder, ILogConfiguration logConfiguration) {
            builder.UseVishizhukelNetDvinAndPegh(new DummyCsArgumentPrompter(), logConfiguration);
            return builder;
        }
    }
}
