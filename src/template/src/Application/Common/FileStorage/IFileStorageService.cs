using Genocs.Common.Dependency;

namespace Genocs.Microservice.Template.Application.Common.FileStorage;

public interface IFileStorageService : ITransientDependency
{
    public Task<string> UploadAsync<T>(FileUploadRequest? request, FileType supportedFileType, CancellationToken cancellationToken = default)
    where T : class;

    public void Remove(string? path);
}