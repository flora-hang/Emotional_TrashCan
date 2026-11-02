namespace ThoughtTrashCan;

public sealed class AzureOptions
{
    /// <summary>e.g. https://my-resource.openai.azure.com/</summary>
    public string? OpenAIEndpoint { get; set; }
    /// <summary>Optional. Leave empty to sign in with Managed Identity / az login instead.</summary>
    public string? OpenAIKey { get; set; }
    /// <summary>Your model deployment name, e.g. gpt-4o-mini.</summary>
    public string? OpenAIDeployment { get; set; }

    public string? SpeechKey { get; set; }
    /// <summary>e.g. canadacentral</summary>
    public string? SpeechRegion { get; set; }
}

public sealed record ReleaseRequest(string? Text);

/// <param name="Kind">worry | regret | to-do | self-criticism | feeling | other</param>
/// <param name="Reply">One or two warm sentences shown in the room.</param>
/// <param name="Urgent">True when the text suggests risk; the page then keeps the crisis line on screen.</param>
/// <param name="BreatheCycles">0 = no breathing guide; 1-4 = the agent chose to start one.</param>
public sealed record ReleaseResult(string Kind, string Reply, bool Urgent, int BreatheCycles);

public sealed record SpeechToken(string Token, string Region);
