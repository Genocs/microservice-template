namespace Genocs.Microservice.Template.Infrastructure.Notifications;

public class SignalRSettings
{
    /// <summary>
    /// The backplane settings.
    /// </summary>
    public class Backplane
    {
        /// <summary>
        /// The backplane provider.
        /// At the moment, only "redis" is supported.
        /// </summary>
        public string? Provider { get; set; }

        /// <summary>
        /// The connection string to the backplane.
        /// At the moment, this is the Redis connection string.
        /// </summary>
        public string? StringConnection { get; set; }
    }

    /// <summary>
    /// It is used to enable or disable the backplane.
    /// In case of enabling the backplane, the backplane settings must be provided.
    /// </summary>
    public bool UseBackplane { get; set; }
}