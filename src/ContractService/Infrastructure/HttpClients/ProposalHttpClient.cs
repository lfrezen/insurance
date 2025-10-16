using System.Net;
using System.Text.Json;
using ContractService.Domain.Ports;

namespace ContractService.Infrastructure.HttpClients;

public class ProposalHttpClient : IProposalClient
{
    private readonly HttpClient _httpClient;

    public ProposalHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProposalDto?> GetProposalByIdAsync(Guid proposalId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/proposals/{proposalId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var proposal = JsonSerializer.Deserialize<ProposalDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return proposal;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
