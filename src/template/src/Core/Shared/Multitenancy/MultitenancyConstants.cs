namespace Genocs.Microservice.Template.Shared.Multitenancy;

public class MultitenancyConstants
{
    public static class Root
    {
        public const string Id = "root";
        public const string Name = "Root";
        public const string EmailAddress = "admin@root.com"; // TODO: Change the email address.
    }

    // TODO: Default password is used only for developing purpose.
    public const string DefaultPassword = "123Pa$$word!";

    public const string TenantIdName = "tenant";
}