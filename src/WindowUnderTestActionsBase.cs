using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Enums;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools;

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

    public ControllableProcessTask CreateControllableProcessTask(ControllableProcess process, string type, string controlName, string text) {
        return new ControllableProcessTask {
            Id = Guid.NewGuid(),
            ProcessId = process.ProcessId,
            Type = type,
            ControlName = controlName,
            Status = ControllableProcessTaskStatus.Requested,
            Text = text
        };
    }

    public ControllableProcessTask CreateResetTask(ControllableProcess process) {
        return CreateControllableProcessTask(process, ControllableProcessTaskType.Reset, "", "");
    }

    public async Task RemotelyProcessTaskListAsync(ControllableProcess process, List<ControllableProcessTask> tasks, bool oneByOne, Func<int, ControllableProcessTask, Task> onTaskCompleted) {
        if (oneByOne) {
            for (var i = 0; i < tasks.Count; i ++) {
                await SubmitNewTaskAndAwaitCompletionAsync(tasks[i]);
                await onTaskCompleted(i, tasks[i]);
            }
        } else {
            var task = CreateControllableProcessTask(process, ControllableProcessTaskType.ProcessTaskList, "",
                JsonSerializer.Serialize(tasks));
            await SubmitNewTaskAndAwaitCompletionAsync(task);
        }
    }

    public async Task<string> SubmitNewTaskAndAwaitCompletionAsync(ControllableProcessTask task) {
        return await SubmitNewTaskAndAwaitCompletionAsync(task, true);
    }

    private const int MilliSecondsToAttemptWhileRequestedOrProcessing = 120000;

    public async Task<string> SubmitNewTaskAndAwaitCompletionAsync(ControllableProcessTask task, bool successIsExpected) {
        var status = await TashAccessor.PutControllableProcessTaskAsync(task);
        Assert.AreEqual(HttpStatusCode.Created, status);

        var startTime = DateTime.Now;
        var result
            = await TashAccessor.AwaitCompletionAsync(task.Id, MilliSecondsToAttemptWhileRequestedOrProcessing)
              ?? await TashAccessor.GetControllableProcessTaskAsync(task.Id);
        if (result.Status == ControllableProcessTaskStatus.Processing) {
            var endTime = DateTime.Now;
            var elapsed = (endTime - startTime).TotalMilliseconds;
            Assert.IsTrue(elapsed > MilliSecondsToAttemptWhileRequestedOrProcessing,
                $"Task started at {startTime.ToLongTimeString()} and given up at {endTime.ToLongTimeString()}");
        }
        if (successIsExpected) {
            var errorMessage = $"Task status is {Enum.GetName(typeof(ControllableProcessTaskStatus), result.Status)}, error message: {result.ErrorMessage}";
            Assert.AreEqual(ControllableProcessTaskStatus.Completed, result.Status, errorMessage);
            return result.Text;
        } else {
            var errorMessage = $"Unexpected task status {Enum.GetName(typeof(ControllableProcessTaskStatus), result.Status)}";
            Assert.AreEqual(ControllableProcessTaskStatus.Failed, result.Status, errorMessage);
            return result.ErrorMessage;
        }
    }

    public ControllableProcessTask CreateMaximizeTask(ControllableProcess process) {
        return CreateControllableProcessTask(process, ControllableProcessTaskType.Maximize, "", "");
    }

    public async Task<string> RemotelyGetValueAsync(ControllableProcess process, string controlName) {
        var task = CreateControllableProcessTask(process, ControllableProcessTaskType.GetValue, controlName, "");
        return await SubmitNewTaskAndAwaitCompletionAsync(task);
    }

    public ControllableProcessTask CreateSetValueTask(ControllableProcess process, string controlName, string value) {
        return CreateControllableProcessTask(process, ControllableProcessTaskType.SetValue, controlName, value);
    }

    public async Task RemotelySetValueAsync(ControllableProcess process, string controlName, string value) {
        var task = CreateSetValueTask(process, controlName, value);
        await SubmitNewTaskAndAwaitCompletionAsync(task);
    }

    public ControllableProcessTask CreatePressButtonTask(ControllableProcess process, string controlName) {
        return CreateControllableProcessTask(process, ControllableProcessTaskType.PressButton, controlName, "");
    }

    public async Task RemotelyPressButtonAsync(ControllableProcess process, string controlName, bool successIsExpected) {
        var task = CreatePressButtonTask(process, controlName);
        await SubmitNewTaskAndAwaitCompletionAsync(task, successIsExpected);
    }

    public ControllableProcessTask CreateVerifyIntegrationTestEnvironmentTask(ControllableProcess process) {
        return CreateControllableProcessTask(process, ControllableProcessTaskType.VerifyIntegrationTestEnvironment, "", "");
    }

    public ControllableProcessTask CreateVerifyNumberOfItemsTask(ControllableProcess process, string controlName, int expectedNumberOfItems) {
        return CreateControllableProcessTask(process, ControllableProcessTaskType.VerifyNumberOfItems, controlName, expectedNumberOfItems.ToString());
    }

    public ControllableProcessTask CreateVerifyItemsTask(ControllableProcess process, string controlName, IList<string> names) {
        return CreateControllableProcessTask(process, ControllableProcessTaskType.VerifyItems, controlName, string.Join("^", names));
    }

    public ControllableProcessTask CreateVerifyWhetherEnabledTask(ControllableProcess process, string controlName, bool enabled) {
        return CreateControllableProcessTask(process, ControllableProcessTaskType.VerifyWhetherEnabled, controlName, enabled ? "true" : "false");
    }

    public ControllableProcessTask CreateVerifyValueTask(ControllableProcess process, string controlName, string value) {
        return CreateControllableProcessTask(process, ControllableProcessTaskType.VerifyValue, controlName, value);
    }

    public ControllableProcessTask CreateVerifyLabelTask(ControllableProcess process, string controlName, string label) {
        return CreateControllableProcessTask(process, ControllableProcessTaskType.VerifyLabel, controlName, label);
    }

    public async Task RemotelyVerifyLabelAsync(ControllableProcess process, string controlName, string label) {
        var task = CreateVerifyLabelTask(process, controlName, label);
        await SubmitNewTaskAndAwaitCompletionAsync(task);
    }
}