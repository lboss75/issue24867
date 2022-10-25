using Microsoft.AspNetCore.Mvc.Testing;

namespace Test
{
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
    }
}
