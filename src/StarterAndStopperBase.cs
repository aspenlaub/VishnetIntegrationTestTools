using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools {
    public abstract class StarterAndStopperBase : IStarterAndStopper {
        protected abstract string ProcessName { get; }
        protected abstract List<string> AdditionalProcessNamesToStop { get; }
        protected abstract string ExecutableFile();

        public void Start() {
            Start("");
        }

        public void Start(string windowUnderTestClassName) {
            var executableFile = ExecutableFile();
            if (!File.Exists(executableFile)) {
                throw new Exception("File '" + executableFile + "' does not exist");
            }

            Stop();

            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = executableFile,
                    Arguments = "/UnitTest" + (string.IsNullOrEmpty(windowUnderTestClassName) ? "" : " /Window=" + windowUnderTestClassName),
                    WindowStyle = ProcessWindowStyle.Normal,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(executableFile) ?? ""
                }
            };
            process.Start();
            Thread.Sleep(TimeSpan.FromSeconds(5));
            if (Process.GetProcessesByName(ProcessName).Length != 1) {
                throw new Exception($"{ProcessName} could not be started");
            }
        }

        public void Stop() {
            bool again;
            var attempts = 10;
            do {
                again = false;
                try {
                    foreach (var additionalProcessName in AdditionalProcessNamesToStop) {
                        foreach (var process in Process.GetProcessesByName(additionalProcessName)) {
                            process.Kill();
                        }
                    }

                    foreach (var process in Process.GetProcessesByName(ProcessName)) {
                        process.Kill();
                    }
                } catch {
                    again = --attempts >= 0;
                }
            } while (again);
        }
    }
}
