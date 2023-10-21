using Genocs.Microservice.Template.Application.Common.Interfaces;

namespace Genocs.Microservice.Template.Application.Common.Mailing;

public interface IEmailTemplateService : ITransientService
{
    string GenerateEmailTemplate<T>(string templateName, T mailTemplateModel);
}