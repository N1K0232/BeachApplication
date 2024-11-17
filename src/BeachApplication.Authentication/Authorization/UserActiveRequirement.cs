using Microsoft.AspNetCore.Authorization;

namespace BeachApplication.Authorization;

public class UserActiveRequirement : IAuthorizationRequirement
{
    public UserActiveRequirement(string applicationId, string clientId)
    {
        ApplicationId = applicationId;
        ClientId = clientId;
    }

    public string ApplicationId { get; }

    public string ClientId { get; }
}