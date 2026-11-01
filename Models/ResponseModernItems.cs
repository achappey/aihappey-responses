using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHappey.Responses;

public sealed class ResponseFileSearchCallItem : ResponseInputItem
{
    public ResponseFileSearchCallItem() => Type = "file_search_call";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("queries")]
    public JsonElement? Queries { get; set; }

    [JsonPropertyName("results")]
    public JsonElement? Results { get; set; }
}

public sealed class ResponseComputerCallItem : ResponseInputItem
{
    public ResponseComputerCallItem() => Type = "computer_call";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("call_id")]
    public string? CallId { get; set; }

    [JsonPropertyName("action")]
    public JsonElement? Action { get; set; }

    [JsonPropertyName("actions")]
    public JsonElement? Actions { get; set; }

    [JsonPropertyName("pending_safety_checks")]
    public JsonElement? PendingSafetyChecks { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public sealed class ResponseComputerCallOutputItem : ResponseInputItem
{
    public ResponseComputerCallOutputItem() => Type = "computer_call_output";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("call_id")]
    public string? CallId { get; set; }

    [JsonPropertyName("output")]
    public JsonElement? Output { get; set; }

    [JsonPropertyName("acknowledged_safety_checks")]
    public JsonElement? AcknowledgedSafetyChecks { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public sealed class ResponseConfigurationUpdateItem : ResponseInputItem
{
    public ResponseConfigurationUpdateItem() => Type = "configuration_update";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("reasoning")]
    public JsonElement? Reasoning { get; set; }

    [JsonPropertyName("text")]
    public JsonElement? Text { get; set; }
}

public sealed class ResponseMcpCallItem : ResponseInputItem
{
    public ResponseMcpCallItem() => Type = "mcp_call";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("server_label")]
    public string? ServerLabel { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }

    [JsonPropertyName("output")]
    public JsonElement? Output { get; set; }

    [JsonPropertyName("error")]
    public JsonElement? Error { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("approval_request_id")]
    public string? ApprovalRequestId { get; set; }
}

public sealed class ResponseMcpListToolsItem : ResponseInputItem
{
    public ResponseMcpListToolsItem() => Type = "mcp_list_tools";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("server_label")]
    public string? ServerLabel { get; set; }

    [JsonPropertyName("tools")]
    public JsonElement? Tools { get; set; }

    [JsonPropertyName("error")]
    public JsonElement? Error { get; set; }
}

public sealed class ResponseMcpApprovalRequestItem : ResponseInputItem
{
    public ResponseMcpApprovalRequestItem() => Type = "mcp_approval_request";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("server_label")]
    public string? ServerLabel { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }
}

public sealed class ResponseMcpApprovalResponseItem : ResponseInputItem
{
    public ResponseMcpApprovalResponseItem() => Type = "mcp_approval_response";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("approval_request_id")]
    public string? ApprovalRequestId { get; set; }

    [JsonPropertyName("approve")]
    public bool? Approve { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

public sealed class ResponseCustomToolCallItem : ResponseInputItem
{
    public ResponseCustomToolCallItem() => Type = "custom_tool_call";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("call_id")]
    public string? CallId { get; set; }

    [JsonPropertyName("caller")]
    public ResponseCaller? Caller { get; set; }

    [JsonPropertyName("namespace")]
    public string? Namespace { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("input")]
    public string? Input { get; set; }

    [JsonPropertyName("async")]
    public bool? Async { get; set; }
}

public sealed class ResponseCustomToolCallOutputItem : ResponseInputItem
{
    public ResponseCustomToolCallOutputItem() => Type = "custom_tool_call_output";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("call_id")]
    public string? CallId { get; set; }

    [JsonPropertyName("caller")]
    public ResponseCaller? Caller { get; set; }

    [JsonPropertyName("output")]
    public JsonElement? Output { get; set; }
}

public sealed class ResponseLocalShellCallItem : ResponseInputItem
{
    public ResponseLocalShellCallItem() => Type = "local_shell_call";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("call_id")]
    public string? CallId { get; set; }

    [JsonPropertyName("action")]
    public JsonElement? Action { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public sealed class ResponseLocalShellCallOutputItem : ResponseInputItem
{
    public ResponseLocalShellCallOutputItem() => Type = "local_shell_call_output";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("output")]
    public string? Output { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public sealed class ResponseCompactionTriggerItem : ResponseInputItem
{
    public ResponseCompactionTriggerItem() => Type = "compaction_trigger";

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
