using Genocs.Microservice.Template.Application.Common.Interfaces;

namespace Genocs.Microservice.Template.Application.Common.Mailing;

public interface IMailService : ITransientService
{
    Task SendAsync(MailRequest request, CancellationToken ct);
}