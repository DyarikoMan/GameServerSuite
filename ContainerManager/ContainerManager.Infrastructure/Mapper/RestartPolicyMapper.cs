using ContainerManager.Domain;
using Docker.DotNet.Models;

namespace ContainerManager.Infrastructure.Docker.Mappers;

public static class RestartPolicyMapper
{
    public static RestartPolicyKind ToDockerKind(this RestartPolicyValue policy)
    {
        return policy.Value switch
        {
            RestartPolicyEnum.Always => RestartPolicyKind.Always,
            RestartPolicyEnum.UnlessStopped => RestartPolicyKind.UnlessStopped,
            _ => RestartPolicyKind.No
        };
    }
}
