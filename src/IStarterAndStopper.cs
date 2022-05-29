// ReSharper disable UnusedMember.Global
namespace Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools;

public interface IStarterAndStopper {
    void Start();
    void Start(string windowUnderTestClassName);
    void Stop();
}