using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Common;

public sealed class RequiredClaimsAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] claimNames;

    public RequiredClaimsAttribute(params string[] claimNames)
    {
        this.claimNames = claimNames;
    }
    
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!claimNames.All(claim => context.HttpContext.User.Claims.Any(clm => clm.Value == claim)))
        {
            context.Result = new UnauthorizedObjectResult(string.Empty);
        }
    }
}