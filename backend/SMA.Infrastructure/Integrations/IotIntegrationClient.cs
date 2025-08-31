using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMA.Application.Interfaces;
using SMA.Infrastructure.Integrations.ObjectMock;
using System.Net.Http.Json;

namespace SMA.Infrastructure.Integrations;

public class IotIntegrationClient : IIotIntegrationClient
{
    private readonly HttpClient _http;
    private readonly ILogger<IotIntegrationClient> _logger;
    private readonly IotOptions _opts;

    public IotIntegrationClient(HttpClient http, IOptions<IotOptions> opts, ILogger<IotIntegrationClient> logger)
    {
        _http = http;
        _opts = opts.Value;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_opts.BaseUrl))
            _http.BaseAddress = new Uri(_opts.BaseUrl);
    }

    public async Task<string> RegisterAsync(string name, string location, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_opts.BaseUrl))
        {
            _logger.LogWarning("Iot.BaseUrl não configurado. Usando MOCK para integrationId.");
            return $"mock-{Guid.NewGuid()}";
        }

        if (string.IsNullOrWhiteSpace(_opts.CallbackUrl))
            throw new InvalidOperationException("Iot.CallbackUrl não está configurado.");

        var payload = new
        {
            deviceName = name,
            location = location,
            callbackUrl = _opts.CallbackUrl
        };

        var res = await _http.PostAsJsonAsync("/register", payload, ct);
        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError("Falha no /register IoT. HTTP {Status}", res.StatusCode);
            return $"mock-{Guid.NewGuid()}";
        }

        var data = await res.Content.ReadFromJsonAsync<RegisterResponse>(cancellationToken: ct)
                   ?? throw new InvalidOperationException("Resposta do IoT inválida");
        return data.IntegrationId;
    }

    public async Task UnregisterAsync(string integrationId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_opts.BaseUrl))
        {
            _logger.LogWarning("Iot.BaseUrl não configurado. Ignorando /unregister (MOCK).");
            return;
        }

        var res = await _http.DeleteAsync($"/unregister/{integrationId}", ct);
        if (!res.IsSuccessStatusCode)
            _logger.LogError("Falha no /unregister IoT. HTTP {Status}", res.StatusCode);
    }
}
