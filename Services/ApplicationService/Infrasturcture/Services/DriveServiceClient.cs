using System.Net;
using ApplicationService.Application.Interfaces;
using SharedKernel.Exceptions;
using SharedKernel.Wrappers;

namespace ApplicationService.Infrastructure.Services;

public class DriveServiceClient : IDriveServiceClient
{
    private readonly HttpClient _httpClient;

    public DriveServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<InternalDriveDetail?> GetInternalDriveDetailAsync(Guid driveId, CancellationToken ct = default)
        => GetInternalResponseAsync<InternalDriveDetail>($"api/v1/internal/drives/{driveId}", ct);

    public Task<DriveCollegeStatus?> GetDriveCollegeStatusAsync(Guid driveId, Guid collegeId, CancellationToken ct = default)
        => GetInternalResponseAsync<DriveCollegeStatus>($"api/v1/internal/drives/{driveId}/college-status/{collegeId}", ct);

    private async Task<T?> GetInternalResponseAsync<T>(string path, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);


        using var response = await _httpClient.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        if (!response.IsSuccessStatusCode)
            throw new ServiceUnavailableException("DriveService", $"Internal endpoint returned {(int)response.StatusCode}");

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: ct)
            ?? throw new ServiceUnavailableException("DriveService", "The response body could not be deserialized.");

        if (!envelope.Success)
            throw new ServiceUnavailableException("DriveService", envelope.Message);

        return envelope.Data;
    }
}
