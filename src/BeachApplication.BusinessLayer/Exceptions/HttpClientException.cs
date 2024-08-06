using System.Text.Json;
using TinyHelpers.Extensions;

namespace BeachApplication.BusinessLayer.Exceptions;

public class HttpClientException(int code, string message) : Exception(message)
{
    public int Code
    {
        get
        {
            return code;
        }
    }

    public static async Task<HttpClientException> ReadFromResponseAsync(HttpResponseMessage response)
    {
        using var responseStream = await response.Content.ReadAsStreamAsync();

        try
        {
            using var jsonDocument = await JsonDocument.ParseAsync(responseStream);
            var error = jsonDocument.RootElement.GetProperty("error");

            var statusCode = Convert.ToInt32(error.GetProperty("code").GetString());
            var message = error.GetProperty("message").GetString() ?? string.Empty;

            return new HttpClientException(statusCode, message);
        }
        catch
        {
            responseStream.Position = 0;
            using var reader = new StreamReader(responseStream);

            var message = (await reader.ReadToEndAsync()).GetValueOrDefault("Unknown error");
            return new HttpClientException(500, message);
        }
    }
}