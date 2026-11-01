
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHappey.Responses;

public sealed class ResponseInputItemJsonConverter : JsonConverter<ResponseInputItem>
{
    public override ResponseInputItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        // "type" is optional in some docs/implementations, but usually present.
        var type = root.TryGetProperty("type", out var typeEl) && typeEl.ValueKind == JsonValueKind.String
            ? typeEl.GetString()
            : null;

        // Fallback heuristic: if it has "role" -> message
        if (string.Equals(type, "message", StringComparison.OrdinalIgnoreCase) ||
            (type is null && root.TryGetProperty("role", out _)))
        {
            return root.Deserialize<ResponseInputMessage>(options)
                   ?? throw new JsonException("Could not deserialize message input item.");
        }

        if (string.Equals(type, "item_reference", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseItemReference>(options)
                   ?? throw new JsonException("Could not deserialize item_reference.");
        }

        if (string.Equals(type, "function_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseFunctionCallItem>(options)
                   ?? throw new JsonException("Could not deserialize function_call.");
        }

        if (string.Equals(type, "image_generation_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseImageGenerationCallItem>(options)
                   ?? throw new JsonException("Could not deserialize image_generation_call.");
        }

        if (string.Equals(type, "function_call_output", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseFunctionCallOutputItem>(options)
                   ?? throw new JsonException("Could not deserialize function_call_output.");
        }

        if (string.Equals(type, "web_search_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseWebSearchCallItem>(options)
                   ?? throw new JsonException("Could not deserialize web_search_call.");
        }

        if (string.Equals(type, "file_search_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseFileSearchCallItem>(options)
                   ?? throw new JsonException("Could not deserialize file_search_call.");
        }

        if (string.Equals(type, "computer_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseComputerCallItem>(options)
                   ?? throw new JsonException("Could not deserialize computer_call.");
        }

        if (string.Equals(type, "computer_call_output", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseComputerCallOutputItem>(options)
                   ?? throw new JsonException("Could not deserialize computer_call_output.");
        }

        if (string.Equals(type, "configuration_update", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseConfigurationUpdateItem>(options)
                   ?? throw new JsonException("Could not deserialize configuration_update.");
        }

        if (string.Equals(type, "mcp_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseMcpCallItem>(options)
                   ?? throw new JsonException("Could not deserialize mcp_call.");
        }

        if (string.Equals(type, "mcp_list_tools", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseMcpListToolsItem>(options)
                   ?? throw new JsonException("Could not deserialize mcp_list_tools.");
        }

        if (string.Equals(type, "mcp_approval_request", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseMcpApprovalRequestItem>(options)
                   ?? throw new JsonException("Could not deserialize mcp_approval_request.");
        }

        if (string.Equals(type, "mcp_approval_response", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseMcpApprovalResponseItem>(options)
                   ?? throw new JsonException("Could not deserialize mcp_approval_response.");
        }

        if (string.Equals(type, "custom_tool_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseCustomToolCallItem>(options)
                   ?? throw new JsonException("Could not deserialize custom_tool_call.");
        }

        if (string.Equals(type, "custom_tool_call_output", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseCustomToolCallOutputItem>(options)
                   ?? throw new JsonException("Could not deserialize custom_tool_call_output.");
        }

        if (string.Equals(type, "local_shell_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseLocalShellCallItem>(options)
                   ?? throw new JsonException("Could not deserialize local_shell_call.");
        }

        if (string.Equals(type, "local_shell_call_output", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseLocalShellCallOutputItem>(options)
                   ?? throw new JsonException("Could not deserialize local_shell_call_output.");
        }

        if (string.Equals(type, "compaction_trigger", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseCompactionTriggerItem>(options)
                   ?? throw new JsonException("Could not deserialize compaction_trigger.");
        }

        if (string.Equals(type, "tool_search_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseToolSearchCallItem>(options)
                   ?? throw new JsonException("Could not deserialize tool_search_call.");
        }

        if (string.Equals(type, "tool_search_output", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseToolSearchOutputItem>(options)
                   ?? throw new JsonException("Could not deserialize tool_search_output.");
        }

        if (string.Equals(type, "additional_tools", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseAdditionalToolsItem>(options)
                   ?? throw new JsonException("Could not deserialize additional_tools.");
        }

        if (string.Equals(type, "program", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseProgramItem>(options)
                   ?? throw new JsonException("Could not deserialize program.");
        }

        if (string.Equals(type, "program_output", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseProgramOutputItem>(options)
                   ?? throw new JsonException("Could not deserialize program_output.");
        }

        if (string.Equals(type, "code_interpreter_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseCodeInterpreterCallItem>(options)
                   ?? throw new JsonException("Could not deserialize code_interpreter_call.");
        }

        if (string.Equals(type, "apply_patch_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseApplyPatchCallItem>(options)
                   ?? throw new JsonException("Could not deserialize apply_patch_call.");
        }

        if (string.Equals(type, "apply_patch_call_output", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseApplyPatchCallOutputItem>(options)
                   ?? throw new JsonException("Could not deserialize apply_patch_call_output.");
        }

        if (string.Equals(type, "shell_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseShellCallItem>(options)
                   ?? throw new JsonException("Could not deserialize shell_call.");
        }

        if (string.Equals(type, "shell_call_output", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseShellCallOutputItem>(options)
                   ?? throw new JsonException("Could not deserialize shell_call_output.");
        }

        if (string.Equals(type, "reasoning", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseReasoningItem>(options)
                   ?? throw new JsonException("Could not deserialize reasoning.");
        }

        if (string.Equals(type, "multi_agent_call", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseMultiAgentCallItem>(options)
                   ?? throw new JsonException("Could not deserialize multi_agent_call.");
        }

        if (string.Equals(type, "multi_agent_call_output", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseMultiAgentCallOutputItem>(options)
                   ?? throw new JsonException("Could not deserialize multi_agent_call_output.");
        }

        if (string.Equals(type, "agent_message", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseAgentMessageItem>(options)
                   ?? throw new JsonException("Could not deserialize agent_message.");
        }

        if (string.Equals(type, "compaction", StringComparison.OrdinalIgnoreCase))
        {
            return root.Deserialize<ResponseCompactionItem>(options)
                   ?? throw new JsonException("Could not deserialize compaction.");
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            return root.Deserialize<ResponseUnknownInputItem>(options)
                   ?? throw new JsonException($"Could not preserve unknown input item type '{type}'.");
        }

        throw new JsonException("Input item is missing required string 'type'.");
    }

    public override void Write(Utf8JsonWriter writer, ResponseInputItem value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case ResponseInputMessage msg:
                JsonSerializer.Serialize(writer, msg, options);
                return;

            case ResponseItemReference itemRef:
                JsonSerializer.Serialize(writer, itemRef, options);
                return;

            case ResponseFunctionCallItem functionCall:
                JsonSerializer.Serialize(writer, functionCall, options);
                return;

            case ResponseImageGenerationCallItem imageGenerationCall:
                JsonSerializer.Serialize(writer, imageGenerationCall, options);
                return;

            case ResponseFunctionCallOutputItem functionCallOutput:
                JsonSerializer.Serialize(writer, functionCallOutput, options);
                return;

            case ResponseWebSearchCallItem webSearchCall:
                JsonSerializer.Serialize(writer, webSearchCall, options);
                return;

            case ResponseFileSearchCallItem fileSearchCall:
                JsonSerializer.Serialize(writer, fileSearchCall, options);
                return;

            case ResponseComputerCallItem computerCall:
                JsonSerializer.Serialize(writer, computerCall, options);
                return;

            case ResponseComputerCallOutputItem computerCallOutput:
                JsonSerializer.Serialize(writer, computerCallOutput, options);
                return;

            case ResponseConfigurationUpdateItem configurationUpdate:
                JsonSerializer.Serialize(writer, configurationUpdate, options);
                return;

            case ResponseMcpCallItem mcpCall:
                JsonSerializer.Serialize(writer, mcpCall, options);
                return;

            case ResponseMcpListToolsItem mcpListTools:
                JsonSerializer.Serialize(writer, mcpListTools, options);
                return;

            case ResponseMcpApprovalRequestItem mcpApprovalRequest:
                JsonSerializer.Serialize(writer, mcpApprovalRequest, options);
                return;

            case ResponseMcpApprovalResponseItem mcpApprovalResponse:
                JsonSerializer.Serialize(writer, mcpApprovalResponse, options);
                return;

            case ResponseCustomToolCallItem customToolCall:
                JsonSerializer.Serialize(writer, customToolCall, options);
                return;

            case ResponseCustomToolCallOutputItem customToolCallOutput:
                JsonSerializer.Serialize(writer, customToolCallOutput, options);
                return;

            case ResponseLocalShellCallItem localShellCall:
                JsonSerializer.Serialize(writer, localShellCall, options);
                return;

            case ResponseLocalShellCallOutputItem localShellCallOutput:
                JsonSerializer.Serialize(writer, localShellCallOutput, options);
                return;

            case ResponseCompactionTriggerItem compactionTrigger:
                JsonSerializer.Serialize(writer, compactionTrigger, options);
                return;

            case ResponseToolSearchCallItem toolSearchCall:
                JsonSerializer.Serialize(writer, toolSearchCall, options);
                return;

            case ResponseToolSearchOutputItem toolSearchOutput:
                JsonSerializer.Serialize(writer, toolSearchOutput, options);
                return;

            case ResponseAdditionalToolsItem additionalTools:
                JsonSerializer.Serialize(writer, additionalTools, options);
                return;

            case ResponseProgramItem program:
                JsonSerializer.Serialize(writer, program, options);
                return;

            case ResponseProgramOutputItem programOutput:
                JsonSerializer.Serialize(writer, programOutput, options);
                return;

            case ResponseCodeInterpreterCallItem codeInterpreterCall:
                JsonSerializer.Serialize(writer, codeInterpreterCall, options);
                return;

            case ResponseApplyPatchCallItem patchCall:
                JsonSerializer.Serialize(writer, patchCall, options);
                return;

            case ResponseApplyPatchCallOutputItem patchOutput:
                JsonSerializer.Serialize(writer, patchOutput, options);
                return;

            case ResponseShellCallItem shellCall:
                JsonSerializer.Serialize(writer, shellCall, options);
                return;

            case ResponseShellCallOutputItem shellCallOutput:
                JsonSerializer.Serialize(writer, shellCallOutput, options);
                return;

            case ResponseReasoningItem reasoning:
                JsonSerializer.Serialize(writer, reasoning, options);
                return;

            case ResponseMultiAgentCallItem multiAgentCall:
                JsonSerializer.Serialize(writer, multiAgentCall, options);
                return;

            case ResponseMultiAgentCallOutputItem multiAgentCallOutput:
                JsonSerializer.Serialize(writer, multiAgentCallOutput, options);
                return;

            case ResponseAgentMessageItem agentMessage:
                JsonSerializer.Serialize(writer, agentMessage, options);
                return;

            case ResponseCompactionItem compaction:
                JsonSerializer.Serialize(writer, compaction, options);
                return;

            case ResponseUnknownInputItem unknown:
                JsonSerializer.Serialize(writer, unknown, options);
                return;

            default:
                throw new JsonException($"Unsupported input item: {value.GetType().Name}");
        }
    }
}
