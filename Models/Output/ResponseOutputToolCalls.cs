using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHappey.Responses.Output;

public sealed class ResponseOutputFunctionCallResult : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "function_call_output";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("output")]
    public required ResponseMessageContent Output { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("call_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CallId { get; set; }
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }
    [JsonPropertyName("namespace")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Namespace { get; set; }
    [JsonPropertyName("caller")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseCaller? Caller { get; set; }
    [JsonPropertyName("created_by")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; set; }
}

public sealed class ResponseOutputCustomToolCallResult : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "custom_tool_call_output";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("call_id")]
    public required string CallId { get; set; }
    [JsonPropertyName("output")]
    public required ResponseMessageContent Output { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("caller")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseCaller? Caller { get; set; }
    [JsonPropertyName("created_by")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; set; }
}

public sealed class ResponseOutputAdditionalTools : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "additional_tools";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("role")]
    public required string Role { get; set; }
    [JsonPropertyName("tools")]
    public required List<ResponseToolDefinition> Tools { get; set; }
}

public sealed class ResponseOutputFileSearchCall : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "file_search_call";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("queries")]
    public required List<string> Queries { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("results")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ResponseOutputFileSearchResult>? Results { get; set; }
}

public sealed class ResponseOutputFileSearchResult : ResponseOutputObject
{
    [JsonPropertyName("attributes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, ResponseOutputAttributeValue>? Attributes { get; set; }
    [JsonPropertyName("file_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FileId { get; set; }
    [JsonPropertyName("filename")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Filename { get; set; }
    [JsonPropertyName("score")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Score { get; set; }
    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; set; }
}

/// <summary>A file-search metadata scalar: string, number, or boolean.</summary>
[JsonConverter(typeof(ResponseOutputAttributeValueJsonConverter))]
public sealed class ResponseOutputAttributeValue
{
    public string? Text { get; }
    public double? Number { get; }
    public bool? Boolean { get; }

    public ResponseOutputAttributeValue(string value) => Text = value ?? throw new ArgumentNullException(nameof(value));
    public ResponseOutputAttributeValue(double value) => Number = value;
    public ResponseOutputAttributeValue(bool value) => Boolean = value;
}

public sealed class ResponseOutputAttributeValueJsonConverter : JsonConverter<ResponseOutputAttributeValue>
{
    public override bool HandleNull => true;

    public override ResponseOutputAttributeValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => new(reader.GetString()!),
            JsonTokenType.Number => new(reader.GetDouble()),
            JsonTokenType.True => new(true),
            JsonTokenType.False => new(false),
            _ => throw new JsonException("File-search attributes must be strings, numbers, or booleans.")
        };

    public override void Write(Utf8JsonWriter writer, ResponseOutputAttributeValue value, JsonSerializerOptions options)
    {
        if (value is null) throw new JsonException("File-search attributes cannot be null.");
        if (value.Text is not null) writer.WriteStringValue(value.Text);
        else if (value.Number is double number) writer.WriteNumberValue(number);
        else writer.WriteBooleanValue(value.Boolean!.Value);
    }
}

public sealed class ResponseOutputWebSearchCall : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "web_search_call";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("action")]
    public required ResponseOutputWebSearchAction Action { get; set; }
}

[JsonConverter(typeof(ResponseOutputVariantJsonConverter<ResponseOutputWebSearchAction>))]
public abstract class ResponseOutputWebSearchAction : ResponseOutputVariant;

public sealed class ResponseOutputWebSearch : ResponseOutputWebSearchAction
{
    [JsonPropertyName("type")]
    public override string Type => "search";
    [JsonPropertyName("query")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Query { get; set; }
    [JsonPropertyName("queries")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Queries { get; set; }
    [JsonPropertyName("sources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ResponseOutputWebSearchSource>? Sources { get; set; }
}

public sealed class ResponseOutputWebSearchSource : ResponseOutputVariant
{
    [JsonPropertyName("type")]
    public override string Type => "url";
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}

public sealed class ResponseOutputWebOpenPage : ResponseOutputWebSearchAction
{
    [JsonPropertyName("type")]
    public override string Type => "open_page";
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; set; }
}

public sealed class ResponseOutputWebFind : ResponseOutputWebSearchAction
{
    [JsonPropertyName("type")]
    public override string Type => "find_in_page";
    [JsonPropertyName("url")]
    public required string Url { get; set; }
    [JsonPropertyName("pattern")]
    public required string Pattern { get; set; }
}

public sealed class ResponseUnknownOutputWebSearchAction : ResponseOutputWebSearchAction
{
    [JsonIgnore]
    public string Discriminator { get; set; } = default!;
    [JsonPropertyName("type")]
    public override string Type => Discriminator;
}

public sealed class ResponseOutputLocalShellCall : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "local_shell_call";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("call_id")]
    public required string CallId { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("action")]
    public required ResponseOutputLocalShellAction Action { get; set; }
}

public sealed class ResponseOutputLocalShellAction : ResponseOutputVariant
{
    [JsonPropertyName("type")]
    public override string Type => "exec";
    [JsonPropertyName("command")]
    public required List<string> Command { get; set; }
    [JsonPropertyName("env")]
    public required Dictionary<string, string> Env { get; set; }
    [JsonPropertyName("timeout_ms")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TimeoutMs { get; set; }
    [JsonPropertyName("user")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? User { get; set; }
    [JsonPropertyName("working_directory")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? WorkingDirectory { get; set; }
}

public sealed class ResponseOutputCodeInterpreterCall : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "code_interpreter_call";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("container_id")]
    public required string ContainerId { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Code { get; set; }
    [JsonPropertyName("outputs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required List<ResponseOutputCodeInterpreterResult>? Outputs { get; set; }
}

[JsonConverter(typeof(ResponseOutputVariantJsonConverter<ResponseOutputCodeInterpreterResult>))]
public abstract class ResponseOutputCodeInterpreterResult : ResponseOutputVariant;

public sealed class ResponseOutputCodeInterpreterLogs : ResponseOutputCodeInterpreterResult
{
    [JsonPropertyName("type")]
    public override string Type => "logs";
    [JsonPropertyName("logs")]
    public required string Logs { get; set; }
}

public sealed class ResponseOutputCodeInterpreterImage : ResponseOutputCodeInterpreterResult
{
    [JsonPropertyName("type")]
    public override string Type => "image";
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}

public sealed class ResponseUnknownOutputCodeInterpreterResult : ResponseOutputCodeInterpreterResult
{
    [JsonIgnore]
    public string Discriminator { get; set; } = default!;
    [JsonPropertyName("type")]
    public override string Type => Discriminator;
}

public sealed class ResponseOutputImageGenerationCall : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "image_generation_call";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Result { get; set; }
    [JsonPropertyName("action")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Action { get; set; }
    [JsonPropertyName("background")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Background { get; set; }
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseOutputImageGenerationError? Error { get; set; }
    [JsonPropertyName("output_format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OutputFormat { get; set; }
    [JsonPropertyName("quality")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Quality { get; set; }
    [JsonPropertyName("revised_prompt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RevisedPrompt { get; set; }
    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Size { get; set; }
}

public sealed class ResponseOutputImageGenerationError : ResponseOutputObject
{
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Code { get; set; }
    [JsonPropertyName("message")]
    public required string Message { get; set; }
    [JsonPropertyName("param")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Param { get; set; }
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    [JsonPropertyName("moderation_details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseOutputImageModerationDetails? ModerationDetails { get; set; }
}

public sealed class ResponseOutputImageModerationDetails : ResponseOutputObject
{
    [JsonPropertyName("categories")]
    public required List<string> Categories { get; set; }
    [JsonPropertyName("moderation_stage")]
    public required string ModerationStage { get; set; }
}
