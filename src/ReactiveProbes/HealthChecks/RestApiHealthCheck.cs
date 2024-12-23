using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ReactiveProbes.HealthChecks;

public class RestApiHealthCheck(string url, HttpClient httpClient) : IHealthCheck
{
    private readonly string _url = url ?? throw new ArgumentNullException(nameof(url));
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(_url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy($"URL '{_url}' is accessible. Status code: {response.StatusCode}");
            }
            else
            {
                return new HealthCheckResult(
                    HealthStatus.Unhealthy,
                    $"URL '{_url}' returned an unhealthy status code: {response.StatusCode}",
                    data: new Dictionary<string, object>
                    {
                        {"StatusCode", (int)response.StatusCode},
                        {"ReasonPhrase", response.ReasonPhrase ?? "Unknown"}
                    });
            }
        }
        catch (HttpRequestException ex)
        {
            return new HealthCheckResult(HealthStatus.Unhealthy, $"Error checking URL '{_url}': {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            return new HealthCheckResult(HealthStatus.Degraded, $"Timeout checking URL '{_url}': {ex.Message}");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(HealthStatus.Unhealthy, $"Unexpected error checking URL '{_url}': {ex.Message}");
        }
    }
}

