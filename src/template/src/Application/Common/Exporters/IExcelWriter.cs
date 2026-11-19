using Genocs.Common.Dependency;

namespace Genocs.Microservice.Template.Application.Common.Exporters;

public interface IExcelWriter : ITransientDependency
{
    Stream WriteToStream<T>(IList<T> data);
}