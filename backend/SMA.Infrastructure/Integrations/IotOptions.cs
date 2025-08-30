using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMA.Application.Interfaces;
using SMA.Infrastructure.Integrations.ObjectMock;

namespace SMA.Infrastructure.Integrations;

public class IotOptions
{
    public string BaseUrl { get; set; } = ""; // ex.: http://localhost:5080
}

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
        // MOCK enquanto não houver simulador
        if (string.IsNullOrWhiteSpace(_opts.BaseUrl))
        {
            _logger.LogWarning("Iot.BaseUrl não configurado. Usando MOCK para integrationId.");
            return $"mock-{Guid.NewGuid()}";
        }

        var payload = new { name, location };
        var res = await _http.PostAsJsonAsync("/register", payload, ct);

        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError("Falha ao registrar no IoT Mock. HTTP {Status}", res.StatusCode);
            // fallback simples: mock
            return $"mock-{Guid.NewGuid()}";
        }

        var data = await res.Content.ReadFromJsonAsync<RegisterResponse>(cancellationToken: ct)
                   ?? throw new InvalidOperationException("Resposta IoT inválida");
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
        {
            _logger.LogError("Falha no /unregister IoT. HTTP {Status}", res.StatusCode);
        }
    }

}
