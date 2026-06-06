using DriveService.Application.Interfaces;
using DriveService.Infrastructure.Settings;
using SharedKernel.Exceptions;
using SharedKernel.Wrappers;
using System.Net.Http;
using System.Net.Http.Json;

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
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/colleges/{collegeId}", ct);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<CollegeInfoResult>>(cancellationToken: ct);
            return envelope?.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch college info for {CollegeId}", collegeId);
            throw new ServiceUnavailableException("CollegeService", "Unable to reach CollegeService.");
        }
    }

    public async Task<List<CollegeInfoResult>> GetCollegesInfoAsync(List<Guid> collegeIds, CancellationToken ct = default)
    {
        if (collegeIds is null || collegeIds.Count == 0)
            return new List<CollegeInfoResult>();

        var tasks = collegeIds.Select(id => GetCollegeInfoAsync(id, ct)).ToList();
        var results = await Task.WhenAll(tasks);
        return results.Where(c => c is not null).Cast<CollegeInfoResult>().ToList();
    }
}
