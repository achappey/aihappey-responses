using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHappey.Responses.Output;

public sealed class ResponseOutputMcpCall : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "mcp_call";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("arguments")]
    public required string Arguments { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("server_label")]
    public required string ServerLabel { get; set; }
    [JsonPropertyName("approval_request_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ApprovalRequestId { get; set; }
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseOutputMcpError? Error { get; set; }
    [JsonPropertyName("output")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Output { get; set; }
    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Status { get; set; }
}

public sealed class ResponseOutputMcpListTools : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "mcp_list_tools";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("server_label")]
    public required string ServerLabel { get; set; }
    [JsonPropertyName("tools")]
    public required List<ResponseOutputMcpTool> Tools { get; set; }
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }
}

public sealed class ResponseOutputMcpTool : ResponseOutputObject
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    // The specification deliberately defines schemas and annotations as arbitrary JSON.
    [JsonPropertyName("input_schema")]
    public required JsonElement InputSchema { get; set; }
    [JsonPropertyName("annotations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Annotations { get; set; }
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }
}

[JsonConverter(typeof(ResponseOutputVariantJsonConverter<ResponseOutputMcpError>))]
public abstract class ResponseOutputMcpError : ResponseOutputVariant;

public sealed class ResponseOutputMcpProtocolError : ResponseOutputMcpError
{
    [JsonPropertyName("type")]
    public override string Type => "mcp_protocol_error";
    [JsonPropertyName("code")]
    public required int Code { get; set; }
    [JsonPropertyName("message")]
    public required string Message { get; set; }
}

public sealed class ResponseOutputMcpHttpError : ResponseOutputMcpError
{
    [JsonPropertyName("type")]
    public override string Type => "http_error";
    [JsonPropertyName("code")]
    public required int Code { get; set; }
    [JsonPropertyName("message")]
    public required string Message { get; set; }
}

public sealed class ResponseOutputMcpToolExecutionError : ResponseOutputMcpError
{
    [JsonPropertyName("type")]
    public override string Type => "mcp_tool_execution_error";
    [JsonPropertyName("content")]
    public required JsonElement Content { get; set; }
}

public sealed class ResponseUnknownOutputMcpError : ResponseOutputMcpError
{
    [JsonIgnore]
    public string Discriminator { get; set; } = default!;
    [JsonPropertyName("type")]
    public override string Type => Discriminator;
}
