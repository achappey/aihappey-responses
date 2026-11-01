
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Runtime.Serialization;

namespace AIHappey.Responses;

public sealed class ResponseRequest
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    [JsonPropertyName("input")]
    public ResponseInput? Input { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; } // ✅ number

    [JsonPropertyName("truncation")]
    public TruncationStrategy? Truncation { get; set; } // ✅ "auto" | "disabled"

    [JsonPropertyName("max_output_tokens")]
    public int? MaxOutputTokens { get; set; }

    [JsonPropertyName("top_logprobs")]
    public int? TopLogprobs { get; set; }

    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("store")]
    public bool? Store { get; set; }

    [JsonPropertyName("previous_response_id")]
    public string? PreviousResponseId { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("safety_identifier")]
    public string? SafetyIdentifier { get; set; }

    [JsonPropertyName("prompt_cache_retention")]
    public string? PromptCacheRetention { get; set; }

    [JsonPropertyName("max_tool_calls")]
    public int? MaxToolCalls { get; set; }

    [JsonPropertyName("prompt")]
    public object? Prompt { get; set; }

    [JsonPropertyName("service_tier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ServiceTier { get; set; }

    [JsonPropertyName("text")]
    public object? Text { get; set; }

    [JsonPropertyName("include")]
    public List<string>? Include { get; set; }

    /// <summary>
    /// Docs say: "map of up to 16 keys; values are string/boolean/number".
    /// Keep as object? for max flexibility.
    /// </summary>
    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? Metadata { get; set; }

    [JsonIgnore]
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Tools are a big tree. Keep as flexible objects for now.
    /// If you want, I can type all tool variants too.
    /// </summary>
    [JsonPropertyName("tools")]
    public List<ResponseToolDefinition>? Tools { get; set; }

    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; } // string or object (keep flexible in headstart)

    [JsonPropertyName("reasoning")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Reasoning? Reasoning { get; set; }

    [JsonPropertyName("context_management")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement[]? ContextManagement { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    [JsonPropertyName("background")]
    public bool? Background { get; set; }

    [JsonPropertyName("prompt_cache_key")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PromptCacheKey { get; set; }

    [JsonPropertyName("prompt_cache_options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? PromptCacheOptions { get; set; }

    [JsonPropertyName("stream_options")]
    public object? StreamOptions { get; set; }

    [JsonPropertyName("moderation")]
    public object? Moderation { get; set; }

    [JsonPropertyName("conversation")]
    public object? Conversation { get; set; }

    [JsonPropertyName("access_programs")]
    public object? AccessPrograms { get; set; }
}

public sealed class Reasoning
{
    [JsonPropertyName("effort")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Effort { get; set; } = null;

    [JsonPropertyName("summary")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Summary { get; set; } = null;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}


[JsonConverter(typeof(TruncationStrategyJsonConverter))]
public enum TruncationStrategy
{
    Auto,

    Disabled
}

#region ResponseInput (string OR array)



#endregion

#region Input Items (message, item_reference)

/// <summary>
/// ✅ Item in input array. Discriminator: "type"
/// Commonly: { type:"message", role:"user", content:[...] }
/// </summary>
[JsonConverter(typeof(ResponseInputItemJsonConverter))]
public abstract class ResponseInputItem
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>
/// Preserves input item variants introduced by the API without losing their JSON.
/// </summary>
public sealed class ResponseUnknownInputItem : ResponseInputItem
{
}


[JsonConverter(typeof(ResponseRoleJsonConverter))]
public enum ResponseRole
{
    [EnumMember(Value = "user")]
    User,
    [EnumMember(Value = "assistant")]
    Assistant,
    [EnumMember(Value = "system")]
    System,
    [EnumMember(Value = "developer")]
    Developer
}

#endregion

#region Content Parts (input_text, output_text, input_image, input_file)

[JsonConverter(typeof(ResponseContentPartJsonConverter))]
public abstract class ResponseContentPart
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>
/// Preserves content part variants introduced by the API without losing their JSON.
/// </summary>
public sealed class ResponseUnknownContentPart : ResponseContentPart
{
}

public sealed class InputTextPart : ResponseContentPart
{
    public InputTextPart()
    {
        Type = "input_text";
    }

    public InputTextPart(string text) : this()
    {
        Text = text;
    }

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("prompt_cache_breakpoint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? PromptCacheBreakpoint { get; set; }
}

public sealed class OutputTextPart : ResponseContentPart
{
    public OutputTextPart()
    {
        Type = "output_text";
    }

    public OutputTextPart(string text) : this()
    {
        Text = text;
    }

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("annotations")]
    public object[] Annotations { get; set; } = [];

    [JsonPropertyName("logprobs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement[]? Logprobs { get; set; }
}

public sealed class RefusalContentPart : ResponseContentPart
{
    public RefusalContentPart()
    {
        Type = "refusal";
    }

    [JsonPropertyName("refusal")]
    public string Refusal { get; set; } = "";
}

public sealed class InputImagePart : ResponseContentPart
{
    public InputImagePart()
    {
        Type = "input_image";
    }

    /// <summary>
    /// "high" | "low" | "auto"
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("file_id")]
    public string? FileId { get; set; }

    /// <summary>
    /// URL or data URL (base64)
    /// </summary>
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("prompt_cache_breakpoint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? PromptCacheBreakpoint { get; set; }
}

#endregion

public static class ResponseJson
{
    /// <summary>
    /// Plug this into your HttpClient JSON calls.
    /// </summary>
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new ResponseInputJsonConverter(),
            new ResponseInputItemJsonConverter(),
            new ResponseMessageContentJsonConverter(),
            new ResponseContentPartJsonConverter(),
        }
    };
}
