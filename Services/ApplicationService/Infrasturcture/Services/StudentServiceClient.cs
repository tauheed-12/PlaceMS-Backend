using System.Net;
using ApplicationService.Application.Interfaces;
using SharedKernel.Exceptions;
using SharedKernel.Wrappers;

namespace ApplicationService.Infrastructure.Services;

public class StudentServiceClient : IStudentServiceClient
{
    private readonly HttpClient _httpClient;

    public StudentServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<StudentEligibility?> GetEligibilityAsync(Guid studentId, CancellationToken ct = default)
        => GetInternalResponseAsync<StudentEligibility>($"api/v1/internal/students/{studentId}/eligibility", ct);

    public Task<StudentSummary?> GetStudentSummaryAsync(Guid studentId, CancellationToken ct = default)
        => GetInternalResponseAsync<StudentSummary>($"api/v1/internal/students/{studentId}/summary", ct);

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
