using System.Net;
using System.Net.Http.Json;
using ApplicationService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using SharedKernel.Exceptions;
using SharedKernel.Wrappers;

namespace ApplicationService.Infrastructure.Services;

public class StudentServiceClient : IStudentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _internalSecret;

    public StudentServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _internalSecret = configuration["InternalServiceSecret"] ?? string.Empty;
    }

    public Task<StudentEligibility?> GetEligibilityAsync(Guid studentId, CancellationToken ct = default)
        => GetInternalResponseAsync<StudentEligibility>($"api/v1/internal/students/{studentId}/eligibility", ct);

    public Task<StudentSummary?> GetStudentSummaryAsync(Guid studentId, CancellationToken ct = default)
        => GetInternalResponseAsync<StudentSummary>($"api/v1/internal/students/{studentId}/summary", ct);

    private async Task<T?> GetInternalResponseAsync<T>(string path, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (!string.IsNullOrWhiteSpace(_internalSecret))
            request.Headers.Add("X-Internal-Secret", _internalSecret);

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
