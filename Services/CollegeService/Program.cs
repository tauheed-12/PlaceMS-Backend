// using CollegeService.Infrastructure.Extensions;
// using CollegeService.Application.DTOs.Responses;
// using CollegeService.Application.Interfaces;
// using SharedKernel.Wrappers;

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.
// builder.Services
//     .AddSettings(builder.Configuration)
//     .AddDatabase(builder.Configuration)
//     .AddApplicationServices()
//     .AddHttpClients(builder.Configuration);

// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

// app.MapGet("/api/v1/colleges/validate/{code}", async (string code, ICollegeRepository collegeRepository, CancellationToken ct) =>
// {
//     if (string.IsNullOrWhiteSpace(code))
//         return Results.BadRequest(ApiResponse.Fail("College code is required."));

//     var college = await collegeRepository.GetByCodeAsync(code, ct);
//     if (college is null)
//         return Results.NotFound(ApiResponse.Fail($"College with code '{code}' not found."));

//     var response = new ValidateCollegeCodeResponseDto
//     {
//         IsValid = true,
//         VerificationStatus = college.VerificationStatus,
//         CollegeId = college.Id,
//         CollegeName = college.Name,
//         CollegeCode = college.Code,
//         IsActive = college.VerificationStatus != SharedKernel.Enums.VerificationStatus.Deactivated
//     };

//     return Results.Ok(ApiResponse<ValidateCollegeCodeResponseDto>.Ok(response));
// });

// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast = Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast");

// app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
