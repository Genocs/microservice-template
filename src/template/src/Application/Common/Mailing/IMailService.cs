using Genocs.Common.Dependency;

namespace Genocs.Microservice.Template.Application.Common.Mailing;

public interface IMailService : ITransientDependency
{
    Task SendAsync(MailRequest request, CancellationToken cancellationToken = default);
}