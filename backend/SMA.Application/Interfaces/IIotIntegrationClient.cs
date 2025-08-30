namespace SMA.Application.Interfaces;

public interface IIotIntegrationClient
{
    Task<string> RegisterAsync(string name, string location, CancellationToken ct);
    Task UnregisterAsync(string integrationId, CancellationToken ct);
}
