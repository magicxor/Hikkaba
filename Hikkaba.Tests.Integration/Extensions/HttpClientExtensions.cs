using System.Net.Http.Headers;
using System.Text;

namespace Hikkaba.Tests.Integration.Extensions;

public static class HttpClientExtensions
{
    public static void Authenticate(this HttpClient client, string username, string password)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{username}:{password}")));
    }
}
