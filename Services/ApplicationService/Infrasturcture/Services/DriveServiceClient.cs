using System.Net;
using System.Net.Http.Json;
using ApplicationService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using SharedKernel.Exceptions;
using SharedKernel.Wrappers;

namespace ApplicationService.Infrastructure.Services;

public class DriveServiceClient : IDriveServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _internalSecret;

    public DriveServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _internalSecret = configuration["InternalServiceSecret"] ?? string.Empty;
    }

    public Task<InternalDriveDetail?> GetInternalDriveDetailAsync(Guid driveId, CancellationToken ct = default)
        => GetInternalResponseAsync<InternalDriveDetail>($"api/v1/internal/drives/{driveId}", ct);

    public Task<DriveCollegeStatus?> GetDriveCollegeStatusAsync(Guid driveId, Guid collegeId, CancellationToken ct = default)
        => GetInternalResponseAsync<DriveCollegeStatus>($"api/v1/internal/drives/{driveId}/college-status/{collegeId}", ct);

    private async Task<T?> GetInternalResponseAsync<T>(string path, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (!string.IsNullOrWhiteSpace(_internalSecret))
            request.Headers.Add("X-Internal-Secret", _internalSecret);

        using var response = await _httpClient.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        if (!response.IsSuccessStatusCode)
            throw new ServiceUnavailableException("DriveService", $"Internal endpoint returned {(int)response.StatusCode}");

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: ct);
        if (envelope is null)
            throw new ServiceUnavailableException("DriveService", "The response body could not be deserialized.");

        if (!envelope.Success)
            throw new ServiceUnavailableException("DriveService", envelope.Message);

        return envelope.Data;
    }
}
