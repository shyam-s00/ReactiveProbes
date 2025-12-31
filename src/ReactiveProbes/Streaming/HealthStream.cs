using Microsoft.Extensions.Diagnostics.HealthChecks;
using ReactiveProbes.Environment;
using ReactiveProbes.Models;

namespace ReactiveProbes.Streaming;

public class HealthStream
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double TotalDuration { get; set; }
    public List<HealthEntry> Checks { get; set; } = [];
    public InstanceInfo Instance { get; set; }

    public class HealthEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Duration { get; set; }
        public IReadOnlyDictionary<string, object>? Data { get; set; }
    }
    
    public static HealthStream From(HealthReport report)
    {
        return new HealthStream
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            TotalDuration = report.TotalDuration.TotalMilliseconds,
            Instance = InstanceInfoProvider.Instance,
            Checks = report.Entries.Select(entry => new HealthEntry
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Duration = entry.Value.Duration.TotalMilliseconds,
                Data = entry.Value.Data
            }).ToList()
        };
    }
}