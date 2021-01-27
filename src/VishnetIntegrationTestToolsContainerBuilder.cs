﻿using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools {
    public static class VishnetIntegrationTestToolsContainerBuilder {
        public static ContainerBuilder RegisterForVishizhukelNetIntegrationTest(this ContainerBuilder builder) {
            builder.UseDvinAndPegh(new DummyCsArgumentPrompter());
            return builder;
        }
    }
}
