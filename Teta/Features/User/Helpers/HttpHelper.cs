namespace TetaBackend.Features.User.Helpers;

public static class HttpHelper
{
    public static async Task<string> SendPostRequestAsync(string url, Dictionary<string, string> values)
    {
        var body = new FormUrlEncodedContent(values);
        using var client = new HttpClient();
        
        var response = await client.PostAsync(url, body);
        return await response.Content.ReadAsStringAsync();
    }
}