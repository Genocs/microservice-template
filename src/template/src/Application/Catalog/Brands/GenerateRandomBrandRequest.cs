namespace Genocs.Microservice.Template.Application.Catalog.Brands;

public class GenerateRandomBrandRequest : IRequest<string>
{
    public int NSeed { get; set; }
}

public class GenerateRandomBrandRequestHandler(IJobService jobService) : IRequestHandler<GenerateRandomBrandRequest, string>
{
    private readonly IJobService _jobService = jobService;

    public Task<string> Handle(GenerateRandomBrandRequest request, CancellationToken cancellationToken)
    {
        string jobId = _jobService.Enqueue<IBrandGeneratorJob>(x => x.GenerateAsync(request.NSeed, default));
        return Task.FromResult(jobId);
    }
}