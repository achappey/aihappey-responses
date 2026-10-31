
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHappey.Responses;

public class ResponseResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("object")]
    public string Object { get; set; } = "response";

    [JsonPropertyName("created_at")]
    [JsonConverter(typeof(FlexibleUnixTimestampConverter))]
    public long CreatedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public long? CompletedAt { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("previous_response_id")]
    public string? PreviousResponseId { get; set; }

    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; set; }

    [JsonPropertyName("background")]
    public bool? Background { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("safety_identifier")]
    public string? SafetyIdentifier { get; set; }

    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("top_logprobs")]
    public int? TopLogprobs { get; set; }

    [JsonPropertyName("output")]
    public IEnumerable<object> Output { get; set; } = [];

    [JsonPropertyName("usage")]
    /// <summary>
    /// Usage remains object-typed for source compatibility. Responses mappers
    /// normalize it through <see cref="ResponseUsage"/>.
    /// </summary>
    [JsonConverter(typeof(ResponseUsageObjectJsonConverter))]
    public object? Usage { get; set; }

    [JsonIgnore]
    public ResponseUsage? NormalizedUsage => Usage as ResponseUsage;

    [JsonPropertyName("text")]
    public object? Text { get; set; }

    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; }

    [JsonPropertyName("tools")]
    public IEnumerable<object> Tools { get; set; } = [];

    [JsonPropertyName("reasoning")]
    public object? Reasoning { get; set; }

    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    [JsonPropertyName("prompt")]
    public object? Prompt { get; set; }

    [JsonPropertyName("prompt_cache_key")]
    public string? PromptCacheKey { get; set; }

    [JsonPropertyName("prompt_cache_retention")]
    public string? PromptCacheRetention { get; set; }

    [JsonPropertyName("store")]
    public bool? Store { get; set; }

    [JsonPropertyName("max_tool_calls")]
    public int? MaxToolCalls { get; set; }

    [JsonPropertyName("truncation")]
    public string? Truncation { get; set; }

    [JsonPropertyName("access_programs")]
    public JsonElement? AccessPrograms { get; set; }

    [JsonPropertyName("max_output_tokens")]
    public int? MaxOutputTokens { get; set; }

    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    [JsonPropertyName("output_text")]
    public string? OutputText { get; set; }

    [JsonPropertyName("incomplete_details")]
    public JsonElement? IncompleteDetails { get; set; }

    [JsonPropertyName("tool_usage")]
    public JsonElement? ToolUsage { get; set; }

    [JsonPropertyName("prompt_cache_options")]
    public JsonElement? PromptCacheOptions { get; set; }

    [JsonPropertyName("prompt_cache_diagnostics")]
    public JsonElement? PromptCacheDiagnostics { get; set; }

    [JsonPropertyName("moderation")]
    public JsonElement? Moderation { get; set; }

    [JsonPropertyName("conversation")]
    public JsonElement? Conversation { get; set; }

    [JsonPropertyName("error")]
    public ResponseResultError? Error { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object?>? Metadata { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

}

public class ResponseResultError
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}


public sealed class FlexibleUnixTimestampConverter : JsonConverter<long>
{
    public override long Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out var integer))
                return integer;

            if (reader.TryGetDouble(out var number))
                return checked((long)number);
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();

            if (double.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var number))
            {
                return checked((long)number);
            }
        }

        throw new JsonException(
            $"Invalid Unix timestamp token: {reader.TokenType}.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        long value,
        JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
