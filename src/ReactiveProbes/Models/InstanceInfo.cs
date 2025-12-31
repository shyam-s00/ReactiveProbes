namespace ReactiveProbes.Models;

public record InstanceInfo
{
    public required string InstanceId { get; init; }
    public required string MachineName { get; init; }
    public string? ApplicationName { get; init; }
    public string? Environment { get; init; }
    public string? Version { get; init; }
    
    public string? IpAddress { get; init; }
    public int? Port { get; init; }
    
    public KubernetesInfo? Kubernetes { get; init; }
    public CloudInfo? Cloud { get; init; }
}

public record KubernetesInfo
{
    public string? Namespace { get; init; }
    public string? PodName { get; init; }
    public string? PodUid { get; init; }
    public string? NodeName { get; init; }
    public string? ClusterName { get; init; }
    public string? ServiceAccount { get; init; }
    public IReadOnlyDictionary<string, string>? Labels { get; init; }
}

public record CloudInfo
{
    public string? Provider { get; init; }              // AWS, Azure, GCP, etc.
    public string? Region { get; init; }
    public string? AvailabilityZone { get; init; }
    public string? InstanceId { get; init; }
    public string? InstanceType { get; init; }          // t3.micro, m5.xlarge, etc.
}

