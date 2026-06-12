using DriveService.Application.Interfaces;
using SharedKernel.Exceptions;
using SharedKernel.Wrappers;
using System.Net;

namespace DriveService.Infrastructure.Services;

public class CollegeServiceClient : ICollegeServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CollegeServiceClient> _logger;

    public CollegeServiceClient(HttpClient httpClient, ILogger<CollegeServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CollegeInfoResult?> GetCollegeInfoAsync(Guid collegeId, CancellationToken ct = default)
    {
        var result = await GetInternalResponseAsync<CollegeInfoResult>($"api/v1/internal/colleges/{collegeId}", ct);
        return result;
    }

    public async Task<List<CollegeInfoResult>> GetCollegesInfoAsync(List<Guid> collegeIds, CancellationToken ct = default)
    {
        var result = await GetInternalResponseAsync<List<CollegeInfoResult>>($"api/v1/internal/colleges?ids={string.Join(",", collegeIds)}", ct);
        return result ?? new List<CollegeInfoResult>();
    }

    private async Task<T?> GetInternalResponseAsync<T>(string path, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);

        using var response = await _httpClient.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        if (!response.IsSuccessStatusCode)
            throw new ServiceUnavailableException("StudentService", $"Internal endpoint returned {(int)response.StatusCode}");

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: ct);
        if (envelope is null)
            throw new ServiceUnavailableException("StudentService", "The response body could not be deserialized.");

        if (!envelope.Success)
            throw new ServiceUnavailableException("StudentService", envelope.Message);

        return envelope.Data;
    }
}
