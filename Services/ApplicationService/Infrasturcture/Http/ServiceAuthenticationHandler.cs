using System.Net.Http.Headers;
using ApplicationService.Infrastructure.Services;

namespace ApplicationService.Infrastructure.Http;

public sealed class ServiceAuthenticationHandler : DelegatingHandler
{
    private readonly IServiceTokenProvider _tokenProvider;

    public ServiceAuthenticationHandler(IServiceTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetServiceTokenAsync(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}