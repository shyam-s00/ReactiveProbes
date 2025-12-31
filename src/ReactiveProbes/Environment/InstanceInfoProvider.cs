using System.Reflection;
using System.Net;
using ReactiveProbes.Models;

namespace ReactiveProbes.Environment;

public static class InstanceInfoProvider
{
    private static readonly Lazy<InstanceInfo> _instance = new(Build);
    
    public static InstanceInfo Instance => _instance.Value;
    
    private static InstanceInfo Build()
    {
        return new InstanceInfo
        {
            InstanceId = GetInstanceId(),
            MachineName = System.Environment.MachineName,
            ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name,
            Version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                          ?? System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"),
            IpAddress = GetLocalIpAddress(),
            Kubernetes = GetKubernetesInfo(),
            Cloud = GetCloudInfo()
        };
    }

    private static string GetInstanceId()
    {
        return System.Environment.GetEnvironmentVariable("POD_UID")
               ?? $"{System.Environment.MachineName}-{System.Environment.ProcessId}";
    }

    private static KubernetesInfo? GetKubernetesInfo()
    {
        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST")))
            return null;

        return new KubernetesInfo
        {
            PodName = System.Environment.GetEnvironmentVariable("POD_NAME") ?? string.Empty,
            PodUid = System.Environment.GetEnvironmentVariable("POD_UID") ?? string.Empty,
            Namespace = System.Environment.GetEnvironmentVariable("POD_NAMESPACE") ?? string.Empty,
            NodeName = System.Environment.GetEnvironmentVariable("NODE_NAME") ?? string.Empty,
            ClusterName = System.Environment.GetEnvironmentVariable("CLUSTER_NAME") ?? string.Empty,
            ServiceAccount = System.Environment.GetEnvironmentVariable("SERVICEACCOUNT_NAME") ?? string.Empty
        };
    }

    private static CloudInfo? GetCloudInfo()
    {
        var provider = DetectCloudProvider();
        if (provider == null)
            return null;
        
        return new CloudInfo
        {
            Provider = provider,
            Region = System.Environment.GetEnvironmentVariable("REGION") ?? string.Empty // This should be revisited
        };
    }

    private static string? DetectCloudProvider()
    {
        if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")))
            return "Azure";
        if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT")))
            return "GCP";
        if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("AWS_EXECUTION_ENV")))
            return "AWS";
        
        return null;
    }
    
    private static string? GetLocalIpAddress()
    {
        try
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                .ToString();
        }
        catch
        {
            return null;
        }
    }
}