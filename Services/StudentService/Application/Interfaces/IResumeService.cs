using StudentService.Application.DTOs.Responses;


namespace StudentService.Application.Interfaces;

public interface IResumeService
{
    Task<ResumeResponse> UploadResumeAsync(Guid userId, IFormFile file, CancellationToken ct = default);
    Task<ResumeResponse> GetResumeUrlAsync(Guid userId, CancellationToken ct = default);
    Task DeleteResumeAsync(Guid userId, CancellationToken ct = default);
}