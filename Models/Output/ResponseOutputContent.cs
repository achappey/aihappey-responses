using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHappey.Responses.Output;

public sealed class ResponseOutputMessage : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "message";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("role")]
    public required ResponseRole Role { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("content")]
    public required List<ResponseOutputContent> Content { get; set; }
    [JsonPropertyName("phase")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Phase { get; set; }
}

[JsonConverter(typeof(ResponseOutputVariantJsonConverter<ResponseOutputContent>))]
public abstract class ResponseOutputContent : ResponseOutputVariant;

/// <summary>Reuses RefusalContentPart without a wire envelope.</summary>
[JsonConverter(typeof(ResponseOutputVariantJsonConverter<ResponseOutputContent>))]
public sealed class ResponseOutputRefusal(RefusalContentPart part) : ResponseOutputContent
{
    [JsonIgnore]
    public RefusalContentPart Part { get; } = part ?? throw new ArgumentNullException(nameof(part));

    [JsonPropertyName("type")]
    public override string Type => Part.Type;

    [JsonExtensionData]
    public override Dictionary<string, JsonElement>? AdditionalProperties
    {
        get => Part.AdditionalProperties;
        set => Part.AdditionalProperties = value;
    }

    internal override void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, Part, options);
}

public sealed class ResponseOutputText : ResponseOutputContent
{
    [JsonPropertyName("type")]
    public override string Type => "output_text";
    [JsonPropertyName("text")]
    public required string Text { get; set; }
    [JsonPropertyName("annotations")]
    public required List<ResponseOutputAnnotation> Annotations { get; set; }
    [JsonPropertyName("logprobs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ResponseOutputLogprob>? Logprobs { get; set; }
}

[JsonConverter(typeof(ResponseOutputVariantJsonConverter<ResponseOutputAnnotation>))]
public abstract class ResponseOutputAnnotation : ResponseOutputVariant;

public sealed class ResponseOutputFileCitation : ResponseOutputAnnotation
{
    [JsonPropertyName("type")]
    public override string Type => "file_citation";
    [JsonPropertyName("file_id")]
    public required string FileId { get; set; }
    [JsonPropertyName("filename")]
    public required string Filename { get; set; }
    [JsonPropertyName("index")]
    public required int Index { get; set; }
}

public sealed class ResponseOutputUrlCitation : ResponseOutputAnnotation
{
    [JsonPropertyName("type")]
    public override string Type => "url_citation";
    [JsonPropertyName("start_index")]
    public required int StartIndex { get; set; }
    [JsonPropertyName("end_index")]
    public required int EndIndex { get; set; }
    [JsonPropertyName("url")]
    public required string Url { get; set; }
    [JsonPropertyName("title")]
    public required string Title { get; set; }
}

public sealed class ResponseOutputContainerFileCitation : ResponseOutputAnnotation
{
    [JsonPropertyName("type")]
    public override string Type => "container_file_citation";
    [JsonPropertyName("container_id")]
    public required string ContainerId { get; set; }
    [JsonPropertyName("file_id")]
    public required string FileId { get; set; }
    [JsonPropertyName("filename")]
    public required string Filename { get; set; }
    [JsonPropertyName("start_index")]
    public required int StartIndex { get; set; }
    [JsonPropertyName("end_index")]
    public required int EndIndex { get; set; }
}

public sealed class ResponseOutputFilePath : ResponseOutputAnnotation
{
    [JsonPropertyName("type")]
    public override string Type => "file_path";
    [JsonPropertyName("file_id")]
    public required string FileId { get; set; }
    [JsonPropertyName("index")]
    public required int Index { get; set; }
}

public sealed class ResponseOutputLogprob : ResponseOutputObject
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }
    [JsonPropertyName("bytes")]
    public required List<int> Bytes { get; set; }
    [JsonPropertyName("logprob")]
    public required double Logprob { get; set; }
    [JsonPropertyName("top_logprobs")]
    public required List<ResponseOutputTopLogprob> TopLogprobs { get; set; }
}

public sealed class ResponseOutputTopLogprob : ResponseOutputObject
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }
    [JsonPropertyName("bytes")]
    public required List<int> Bytes { get; set; }
    [JsonPropertyName("logprob")]
    public required double Logprob { get; set; }
}

public sealed class ResponseOutputReasoningItem : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "reasoning";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("summary")]
    public required List<ResponseReasoningSummaryTextPart> Summary { get; set; }
    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ResponseOutputReasoningText>? Content { get; set; }
    [JsonPropertyName("encrypted_content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EncryptedContent { get; set; }
    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Status { get; set; }
}

public sealed class ResponseOutputReasoningText : ResponseOutputVariant
{
    [JsonPropertyName("type")]
    public override string Type => "reasoning_text";
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public sealed class ResponseUnknownOutputContent : ResponseOutputContent
{
    [JsonIgnore]
    public string Discriminator { get; set; } = default!;
    [JsonPropertyName("type")]
    public override string Type => Discriminator;
}

public sealed class ResponseUnknownOutputAnnotation : ResponseOutputAnnotation
{
    [JsonIgnore]
    public string Discriminator { get; set; } = default!;
    [JsonPropertyName("type")]
    public override string Type => Discriminator;
}
