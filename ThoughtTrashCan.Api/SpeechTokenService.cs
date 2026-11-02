using Microsoft.Extensions.Options;

namespace ThoughtTrashCan;

/// <summary>Exchanges the Azure AI Speech key for a 10-minute token the browser SDK can use.</summary>
public sealed class SpeechTokenService(HttpClient http, IOptions<AzureOptions> options)
{
    private readonly AzureOptions _o = options.Value;

    public async Task<SpeechToken?> IssueAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_o.SpeechKey) || string.IsNullOrWhiteSpace(_o.SpeechRegion)) return null;

        using var req = new HttpRequestMessage(HttpMethod.Post,
            $"https://{_o.SpeechRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken");
        req.Headers.Add("Ocp-Apim-Subscription-Key", _o.SpeechKey);
        req.Content = new StringContent("");

        using var res = await http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode) return null;
        return new SpeechToken(await res.Content.ReadAsStringAsync(ct), _o.SpeechRegion);
    }
}
