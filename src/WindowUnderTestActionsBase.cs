using System;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools {
    public class WindowUnderTestActionsBase {
        protected readonly ITashAccessor TashAccessor;
        protected bool Initialized;
        protected string ProcessTitle;

        public WindowUnderTestActionsBase(ITashAccessor tashAccessor, string processTitle) {
            TashAccessor = tashAccessor;
            Initialized = false;
            ProcessTitle = processTitle;
        }

        public virtual async Task InitializeAsync() {
            var errorsAndInfos = await TashAccessor.EnsureTashAppIsRunningAsync();
            Assert.IsFalse(errorsAndInfos.AnyErrors(), string.Join("\r\n", errorsAndInfos.Errors));

            await TashAccessor.AssumeDeath(p => p.Title == ProcessTitle);

            var processes = await TashAccessor.GetControllableProcessesAsync();
            Assert.IsFalse(processes.Any(p => p.Title == ProcessTitle && p.Status != ControllableProcessStatus.Dead), "Non-dead processes exist before test execution");
            Initialized = true;
        }

        public async Task<ControllableProcess> FindIdleProcessAsync() {
            if (!Initialized) { throw new Exception("InitializeAsync has not been called"); }

            ControllableProcess process = null;
            await Wait.UntilAsync(async () => (process = await TryFindIdleProcess()) != null, TimeSpan.FromSeconds(60));
            Assert.IsNotNull(process);
            return process;
        }

        protected async Task<ControllableProcess> TryFindIdleProcess() {
            var findIdleProcessResult = await TashAccessor.FindIdleProcess(p => p.Title == ProcessTitle);
            return findIdleProcessResult.ControllableProcess;
        }
    }
}
