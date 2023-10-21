using Genocs.Microservice.Template.Application.Common.Interfaces;

namespace Genocs.Microservice.Template.Application.Common.Exporters;

public interface IExcelWriter : ITransientService
{
    Stream WriteToStream<T>(IList<T> data);
}