using System.Security.Cryptography;
using Anubis.Logging;
using Microsoft.Extensions.Logging;

namespace Anubis.Scanner;

public static class Scales
{
    private static HttpClient _httpClient;

    private static Dictionary<string, bool> _forbiddenHashes;
    private static ILogger _logger;
    
    static Scales()
    {
        _httpClient = new HttpClient();
        _forbiddenHashes = new Dictionary<string, bool>();
        _logger = LoggingProvider.NewLogger("Anubis.Scales");
    }

    public static async Task<ScalesResponse> AddImage(string url)
    {
        var responseMessage = await _httpClient.GetAsync(url);
        if (!responseMessage.IsSuccessStatusCode)
        {
            return new ScalesResponse()
            {
               IsSuccess = false,
               Reason = $"Provided URL returned error code: {responseMessage.StatusCode}, {responseMessage.ReasonPhrase}"
            };
        }
        
        var data = await responseMessage.Content.ReadAsByteArrayAsync();
        var hashStr = Convert.ToBase64String(SHA256.HashData(data));

        var wasAdded = _forbiddenHashes.TryAdd(hashStr, true);
        
        _logger.LogDebug($"Tried adding url '{url}',hash={hashStr},success?={wasAdded}");
        
        return new ScalesResponse()
        {
            IsSuccess = wasAdded,
            Reason = wasAdded? null : "Linked content is already marked as forbidden."
        };
    }

    public static async Task<bool> CheckForbidden(string url)
    {
        var responseMessage = await _httpClient.GetAsync(url);
        if (!responseMessage.IsSuccessStatusCode) return false;
        
        var data = await responseMessage.Content.ReadAsByteArrayAsync();
        var hashStr = Convert.ToBase64String(SHA256.HashData(data));
        
        return _forbiddenHashes.ContainsKey(hashStr);
    }
}