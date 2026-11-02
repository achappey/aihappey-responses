using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHappey.Responses.Output;

/// <summary>Output requires nullable limits and environment even when their values are null.</summary>
public sealed class ResponseOutputShellCall : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "shell_call";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("call_id")]
    public required string CallId { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("action")]
    public required ResponseOutputShellAction Action { get; set; }
    [JsonPropertyName("environment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required ResponseShellCallEnvironment? Environment { get; set; }
    [JsonPropertyName("caller")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseCaller? Caller { get; set; }
    [JsonPropertyName("created_by")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; set; }
}

public sealed class ResponseOutputShellAction : ResponseOutputObject
{
    [JsonPropertyName("commands")]
    public required List<string> Commands { get; set; }
    [JsonPropertyName("timeout_ms")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required int? TimeoutMs { get; set; }
    [JsonPropertyName("max_output_length")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required int? MaxOutputLength { get; set; }
}

public sealed class ResponseOutputShellCallResult : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "shell_call_output";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("call_id")]
    public required string CallId { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("max_output_length")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required int? MaxOutputLength { get; set; }
    [JsonPropertyName("output")]
    public required List<ResponseShellCallOutputChunk> Output { get; set; }
    [JsonPropertyName("caller")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseCaller? Caller { get; set; }
    [JsonPropertyName("created_by")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; set; }
}

public sealed class ResponseOutputToolSearchCall : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "tool_search_call";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("execution")]
    public required string Execution { get; set; }
    [JsonPropertyName("call_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? CallId { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    // Tool-search arguments are explicitly arbitrary JSON in the specification.
    [JsonPropertyName("arguments")]
    public required JsonElement Arguments { get; set; }
    [JsonPropertyName("created_by")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; set; }
}

public sealed class ResponseOutputToolSearchResult : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "tool_search_output";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("execution")]
    public required string Execution { get; set; }
    [JsonPropertyName("call_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? CallId { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("tools")]
    public required List<ResponseToolDefinition> Tools { get; set; }
    [JsonPropertyName("created_by")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; set; }
}
