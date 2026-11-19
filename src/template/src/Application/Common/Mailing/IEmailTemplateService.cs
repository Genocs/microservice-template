using Genocs.Common.Dependency;

namespace Genocs.Microservice.Template.Application.Common.Mailing;

public interface IEmailTemplateService : ITransientDependency
{
    string GenerateEmailTemplate<T>(string templateName, T mailTemplateModel);
}