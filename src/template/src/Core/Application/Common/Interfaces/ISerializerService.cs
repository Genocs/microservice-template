namespace Genocs.Microservice.Template.Application.Common.Interfaces;

/// <summary>
/// It is used to serialize and deserialize objects.
/// </summary>
public interface ISerializerService : ITransientService
{
    /// <summary>
    /// The method is used to serialize an object.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">Instance of object to serialize.</param>
    /// <returns></returns>
    string Serialize<T>(T obj);

    string Serialize<T>(T obj, Type type);

    T Deserialize<T>(string text);
}