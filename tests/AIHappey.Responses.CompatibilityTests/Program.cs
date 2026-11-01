using System.Text.Json;
using AIHappey.Responses;
using AIHappey.Responses.Streaming;

var tests = new (string Name, Action Run)[]
{
    ("native Apply Patch requests, operations, outputs, streaming and replay", ApplyPatchCompatibilityTests.Run),
    ("request fields serialize and retain future properties", RequestRoundTrip),
    ("multimodal input and unknown item variants round-trip", InputRoundTrip),
    ("response, output, usage, and unknown fields round-trip", ResponseRoundTrip),
    ("response and stream errors retain standard and future fields", ErrorRoundTrip),
    ("tool definitions preserve variant-specific fields", ToolRoundTrip),
    ("official stream events deserialize and preserve event-specific fields", StreamEvents),
    ("all current official stream discriminators are accepted", AllOfficialStreamDiscriminators),
    ("all current official input item discriminators are accepted", AllOfficialInputDiscriminators),
    ("known content and input items preserve additional fields", KnownItemExtensions),
    ("enum values use official wire strings", EnumWireValues),
    ("unknown shell discriminators remain forward-compatible", UnknownShellVariants)
};

foreach (var (name, run) in tests)
{
    run();
    Console.WriteLine($"PASS {name}");
}

static void RequestRoundTrip()
{
    const string json = """
        {
          "model": "gpt-6-astra",
          "input": "Tell me a three sentence bedtime story about a unicorn.",
          "truncation": "disabled",
          "temperature": 1.0,
          "top_p": 0.95,
          "top_logprobs": 3,
          "max_output_tokens": 256,
          "parallel_tool_calls": true,
          "background": false,
          "store": false,
          "stream": true,
          "service_tier": "default",
          "previous_response_id": "resp_previous",
          "max_tool_calls": 4,
          "include": ["reasoning.encrypted_content"],
          "reasoning": { "effort": "medium", "summary": "auto" },
          "text": { "format": { "type": "text" } },
          "tools": [{ "type": "web_search" }],
          "tool_choice": { "type": "web_search" },
          "prompt": { "id": "pmpt_123", "version": "2" },
          "stream_options": { "include_obfuscation": false },
          "moderation": { "enabled": true },
          "conversation": { "id": "conv_123" },
          "access_programs": [],
          "context_management": [{ "type": "compaction", "compact_threshold": 1000 }],
          "user": "user-123",
          "safety_identifier": "safe-user-123",
          "prompt_cache_key": "cache-key",
          "prompt_cache_retention": "24h",
          "x_future_request": { "enabled": true }
        }
        """;

    var request = JsonSerializer.Deserialize<ResponseRequest>(json, ResponseJson.Default)!;
    Check(request.PreviousResponseId == "resp_previous", "previous_response_id");
    Check(request.MaxToolCalls == 4, "max_tool_calls");
    Check(request.Truncation == TruncationStrategy.Disabled, "truncation strategy");
    Check(request.User == "user-123", "user");
    Check(request.SafetyIdentifier == "safe-user-123", "safety_identifier");
    Check(request.Input?.Text?.StartsWith("Tell me", StringComparison.Ordinal) == true, "string input");

    var roundTrip = JsonSerializer.SerializeToElement(request, ResponseJson.Default);
    Check(roundTrip.GetProperty("prompt").GetProperty("id").GetString() == "pmpt_123", "prompt object");
    Check(roundTrip.GetProperty("tool_choice").GetProperty("type").GetString() == "web_search",
        "object tool_choice");
    Check(roundTrip.GetProperty("stream_options").GetProperty("include_obfuscation").GetBoolean() == false,
        "stream_options");
    Check(roundTrip.GetProperty("x_future_request").GetProperty("enabled").GetBoolean(), "future request field");
}

static void InputRoundTrip()
{
    const string json = """
        [
          {
            "type": "message",
            "role": "user",
            "content": [
              { "type": "input_text", "text": "Summarize this media.", "prompt_cache_breakpoint": { "type": "default" } },
              { "type": "input_image", "image_url": "https://example.com/image.png", "detail": "high" },
              { "type": "input_file", "file_id": "file_123", "filename": "report.pdf", "detail": "auto" },
              { "type": "future_media_part", "payload": { "media_id": "media_123" } }
            ],
            "future_message_field": "preserved"
          },
          {
            "type": "mcp_call",
            "id": "mcp_123",
            "server_label": "docs",
            "name": "search",
            "arguments": "{\"query\":\"responses\"}",
            "status": "completed",
            "future_item_field": [1, 2]
          }
        ]
        """;

    var items = JsonSerializer.Deserialize<List<ResponseInputItem>>(json, ResponseJson.Default)!;
    Check(items[0] is ResponseInputMessage, "message discriminator");
    Check(items[1] is ResponseUnknownInputItem, "unknown input discriminator");
    var roundTrip = JsonSerializer.SerializeToElement(items, ResponseJson.Default);
    Check(roundTrip[0].GetProperty("content")[0].GetProperty("prompt_cache_breakpoint")
        .GetProperty("type").GetString() == "default", "prompt cache breakpoint");
    Check(roundTrip[0].GetProperty("content")[2].GetProperty("detail").GetString() == "auto",
        "file detail");
    Check(roundTrip[0].GetProperty("content")[3].GetProperty("payload")
        .GetProperty("media_id").GetString() == "media_123", "unknown multimodal part");
    Check(roundTrip[0].GetProperty("future_message_field").GetString() == "preserved",
        "unknown message field");
    Check(roundTrip[1].GetProperty("future_item_field").GetArrayLength() == 2, "unknown item field");

    var responseInput = JsonSerializer.Deserialize<ResponseInput>(
        "\"plain text input\"", ResponseJson.Default)!;
    Check(JsonSerializer.Serialize(responseInput, ResponseJson.Default) == "\"plain text input\"",
        "string input round trip");
}

static void ResponseRoundTrip()
{
    const string json = """
        {
          "id": "resp_123",
          "object": "response",
          "access_programs": null,
          "created_at": 1740855869,
          "status": "completed",
          "completed_at": 1740855870,
          "model": "gpt-6-astra",
          "output": [
            {
              "id": "msg_123",
              "type": "message",
              "role": "assistant",
              "status": "completed",
              "content": [
                { "type": "output_text", "text": "Hello.", "annotations": [] },
                { "type": "refusal", "refusal": "Not applicable." }
              ]
            },
            { "type": "mcp_call", "server_label": "docs", "status": "completed" }
          ],
          "usage": {
            "input_tokens": 12,
            "input_tokens_details": { "cached_tokens": 2, "cache_write_tokens": 1 },
            "output_tokens": 8,
            "output_tokens_details": { "reasoning_tokens": 3 },
            "total_tokens": 20,
            "future_usage_counter": 5
          },
          "incomplete_details": null,
          "prompt_cache_diagnostics": { "hit": true },
          "safety_identifier": "safe-user-123",
          "truncation": "disabled",
          "future_response_field": { "revision": 3 }
        }
        """;

    var response = JsonSerializer.Deserialize<ResponseResult>(json, ResponseJson.Default)!;
    Check(response.Id == "resp_123", "response id");
    Check(response.NormalizedUsage?.InputTokensDetails?.CacheWriteTokens == 1, "cache write usage");
    Check(response.NormalizedUsage?.OutputTokensDetails?.ReasoningTokens == 3, "reasoning usage");
    Check(response.Output.GetAssistantOutputText() == "Hello.", "assistant output text");

    var roundTrip = JsonSerializer.SerializeToElement(response, ResponseJson.Default);
    Check(roundTrip.GetProperty("future_response_field").GetProperty("revision").GetInt32() == 3,
        "future response field");
    Check(roundTrip.GetProperty("usage").GetProperty("future_usage_counter").GetInt32() == 5,
        "future usage field");
    Check(roundTrip.GetProperty("output")[1].GetProperty("server_label").GetString() == "docs",
        "unknown output item");
}

static void ToolRoundTrip()
{
    const string json = """
        {
          "type": "function",
          "name": "lookup",
          "description": "Look up a record.",
          "parameters": { "type": "object", "properties": { "key": { "type": "string" } } },
          "strict": true,
          "future_tool_option": "preserved"
        }
        """;

    var tool = JsonSerializer.Deserialize<ResponseToolDefinition>(json, ResponseJson.Default)!;
    var roundTrip = JsonSerializer.SerializeToElement(tool, ResponseJson.Default);
    Check(roundTrip.GetProperty("type").GetString() == "function", "tool type");
    Check(roundTrip.GetProperty("strict").GetBoolean(), "strict function tool");
    Check(roundTrip.GetProperty("future_tool_option").GetString() == "preserved", "unknown tool option");

    string[] types =
    [
        "function",
        "file_search",
        "computer",
        "computer_use_preview",
        "web_search",
        "mcp",
        "code_interpreter",
        "programmatic_tool_calling",
        "image_generation",
        "local_shell",
        "shell",
        "custom",
        "namespace",
        "tool_search",
        "web_search_preview",
        "apply_patch"
    ];
    foreach (var type in types)
    {
        var toolVariant = new ResponseToolDefinition { Type = type };
        var serialized = JsonSerializer.SerializeToElement(toolVariant, ResponseJson.Default);
        var deserialized = serialized.Deserialize<ResponseToolDefinition>(ResponseJson.Default)!;
        Check(deserialized.Type == type, $"tool discriminator {type}");
        Check(JsonSerializer.SerializeToElement(deserialized, ResponseJson.Default)
            .GetProperty("type").GetString() == type, $"tool round trip {type}");
    }

    Check(types.Length == 16, "official tool definition fixture count");
}

static void ErrorRoundTrip()
{
    const string responseJson = """
        {
          "id": "resp_failed",
          "object": "response",
          "created_at": 1740855869,
          "status": "failed",
          "model": "gpt-6-astra",
          "output": [],
          "error": {
            "code": "server_error",
            "message": "The response failed.",
            "type": "server_error",
            "param": "input",
            "future_error_field": { "retryable": true }
          }
        }
        """;
    var response = JsonSerializer.Deserialize<ResponseResult>(responseJson, ResponseJson.Default)!;
    Check(response.Error?.Type == "server_error" && response.Error.Param == "input",
        "standard response error fields");
    var responseRoundTrip = JsonSerializer.SerializeToElement(response, ResponseJson.Default);
    Check(responseRoundTrip.GetProperty("error").GetProperty("future_error_field")
        .GetProperty("retryable").GetBoolean(), "future response error field");

    const string streamJson = """
        {
          "type": "error",
          "sequence_number": 5,
          "code": null,
          "message": "The request could not be completed.",
          "param": null,
          "future_error_field": "preserved"
        }
        """;
    var streamError = (ResponseError)JsonSerializer.Deserialize<ResponseStreamPart>(
        streamJson, ResponseJson.Default)!;
    Check(streamError.Code is null && streamError.Param is null, "nullable stream error fields");
    Check(JsonSerializer.SerializeToElement(streamError, ResponseJson.Default)
        .GetProperty("future_error_field").GetString() == "preserved", "future stream error field");
}

static void StreamEvents()
{
    const string audio = """
        {
          "type": "response.audio.transcript.delta",
          "response_id": "resp_123",
          "delta": "Hello",
          "sequence_number": 2,
          "future_event_field": true
        }
        """;
    var transcript = JsonSerializer.Deserialize<ResponseStreamPart>(audio, ResponseJson.Default)!;
    Check(transcript is ResponseAudioTranscriptDeltaEvent, "audio transcript discriminator");
    Check(transcript.ResponseId == "resp_123", "response_id");
    var audioRoundTrip = JsonSerializer.SerializeToElement(transcript, ResponseJson.Default);
    Check(audioRoundTrip.GetProperty("future_event_field").GetBoolean(), "audio event extension");

    const string incomplete = """
        {
          "type": "response.incomplete",
          "response_id": "resp_456",
          "sequence_number": 4,
          "response": {
            "id": "resp_456",
            "object": "response",
            "created_at": 1740855869,
            "status": "incomplete",
            "model": "gpt-6-astra",
            "output": [],
            "incomplete_details": { "reason": "max_output_tokens" }
          }
        }
        """;
    var incompleteEvent = JsonSerializer.Deserialize<ResponseStreamPart>(incomplete, ResponseJson.Default)!;
    Check(incompleteEvent is ResponseIncompleteEvent, "incomplete event discriminator");

    const string futureEvent = """
        {
          "type": "response.mcp_call.completed",
          "response_id": "resp_789",
          "sequence_number": 9,
          "server_label": "docs",
          "output": "done"
        }
        """;
    var unknown = (ResponseUnknownEvent)JsonSerializer.Deserialize<ResponseStreamPart>(futureEvent, ResponseJson.Default)!;
    var unknownRoundTrip = JsonSerializer.SerializeToElement(unknown, ResponseJson.Default);
    Check(unknownRoundTrip.GetProperty("server_label").GetString() == "docs", "unknown stream event field");
    Check(unknownRoundTrip.GetProperty("response_id").GetString() == "resp_789", "unknown event response_id");
}

static void AllOfficialStreamDiscriminators()
{
    string[] types =
    [
        "response.audio.delta",
        "response.audio.done",
        "response.audio.transcript.delta",
        "response.audio.transcript.done",
        "response.code_interpreter_call_code.delta",
        "response.code_interpreter_call_code.done",
        "response.code_interpreter_call.completed",
        "response.code_interpreter_call.in_progress",
        "response.code_interpreter_call.interpreting",
        "response.compaction.compacting",
        "response.completed",
        "response.content_part.added",
        "response.content_part.done",
        "response.created",
        "error",
        "response.file_search_call.completed",
        "response.file_search_call.in_progress",
        "response.file_search_call.searching",
        "response.function_call_arguments.delta",
        "response.function_call_arguments.done",
        "response.shell_call_command.added",
        "response.shell_call_command.delta",
        "response.shell_call_command.done",
        "response.shell_call_output_content.delta",
        "response.shell_call_output_content.done",
        "response.in_progress",
        "response.failed",
        "response.incomplete",
        "response.output_item.added",
        "response.output_item.done",
        "response.reasoning_summary_part.added",
        "response.reasoning_summary_part.done",
        "response.reasoning_summary_text.delta",
        "response.reasoning_summary_text.done",
        "response.reasoning_text.delta",
        "response.reasoning_text.done",
        "response.refusal.delta",
        "response.refusal.done",
        "response.output_text.delta",
        "response.output_text.done",
        "response.web_search_call.completed",
        "response.web_search_call.in_progress",
        "response.web_search_call.searching",
        "response.image_generation_call.completed",
        "response.image_generation_call.generating",
        "response.image_generation_call.in_progress",
        "response.image_generation_call.partial_image",
        "response.mcp_call_arguments.delta",
        "response.mcp_call_arguments.done",
        "response.mcp_call.completed",
        "response.mcp_call.failed",
        "response.mcp_call.in_progress",
        "response.mcp_list_tools.completed",
        "response.mcp_list_tools.failed",
        "response.mcp_list_tools.in_progress",
        "response.output_text.annotation.added",
        "response.queued",
        "response.custom_tool_call_input.delta",
        "response.custom_tool_call_input.done"
    ];

    foreach (var type in types)
    {
        var json = JsonSerializer.Serialize(new { type, sequence_number = 1 }, ResponseJson.Default);
        var part = JsonSerializer.Deserialize<ResponseStreamPart>(json, ResponseJson.Default);
        Check(part?.Type == type, $"stream discriminator {type}");
        Check(JsonSerializer.SerializeToElement(part, ResponseJson.Default).GetProperty("type").GetString() == type,
            $"stream discriminator round trip {type}");
    }

    Check(types.Length == 59, "official stream event fixture count");
}

static void AllOfficialInputDiscriminators()
{
    string[] types =
    [
        "message",
        "file_search_call",
        "computer_call",
        "computer_call_output",
        "web_search_call",
        "function_call",
        "function_call_output",
        "tool_search_call",
        "tool_search_output",
        "additional_tools",
        "configuration_update",
        "reasoning",
        "compaction",
        "image_generation_call",
        "code_interpreter_call",
        "local_shell_call",
        "local_shell_call_output",
        "shell_call",
        "shell_call_output",
        "apply_patch_call",
        "apply_patch_call_output",
        "mcp_list_tools",
        "mcp_approval_request",
        "mcp_approval_response",
        "mcp_call",
        "custom_tool_call_output",
        "custom_tool_call",
        "compaction_trigger",
        "item_reference",
        "program",
        "program_output"
    ];

    foreach (var type in types)
    {
        var fixture = new Dictionary<string, object?> { ["type"] = type };
        if (type == "web_search_call")
            fixture["action"] = new { };
        if (type == "code_interpreter_call")
            fixture["outputs"] = Array.Empty<object>();

        var json = JsonSerializer.Serialize(fixture, ResponseJson.Default);
        var item = JsonSerializer.Deserialize<ResponseInputItem>(json, ResponseJson.Default);
        Check(item?.Type == type, $"input item discriminator {type}");
        Check(JsonSerializer.SerializeToElement(item, ResponseJson.Default).GetProperty("type").GetString() == type,
            $"input item discriminator round trip {type}");
    }

    Check(types.Length == 31, "official input item fixture count");
}

static void KnownItemExtensions()
{
    const string json = """
        {
          "type": "function_call",
          "call_id": "call_123",
          "name": "lookup",
          "arguments": "{\"key\":\"value\"}",
          "new_api_field": { "nested": true }
        }
        """;

    var item = JsonSerializer.Deserialize<ResponseInputItem>(json, ResponseJson.Default)!;
    var roundTrip = JsonSerializer.SerializeToElement(item, ResponseJson.Default);
    Check(roundTrip.GetProperty("new_api_field").GetProperty("nested").GetBoolean(),
        "known input item extension");

    const string functionOutput = """
        {
          "type": "function_call_output",
          "call_id": "call_123",
          "output": [
            { "type": "input_text", "text": "Result" },
            { "type": "input_image", "image_url": "https://example.com/result.png", "detail": "auto" }
          ]
        }
        """;
    var outputItem = JsonSerializer.Deserialize<ResponseInputItem>(functionOutput, ResponseJson.Default)!;
    var outputRoundTrip = JsonSerializer.SerializeToElement(outputItem, ResponseJson.Default);
    Check(outputRoundTrip.GetProperty("output").ValueKind == JsonValueKind.Array,
        "array function_call_output");
    Check(outputRoundTrip.GetProperty("output")[1].GetProperty("type").GetString() == "input_image",
        "function_call_output content");
    const string textOutput = """
        { "type": "function_call_output", "call_id": "call_456", "output": "plain output" }
        """;
    var textOutputItem = JsonSerializer.Deserialize<ResponseInputItem>(textOutput, ResponseJson.Default)!;
    Check(JsonSerializer.SerializeToElement(textOutputItem, ResponseJson.Default)
        .GetProperty("output").GetString() == "plain output", "string function_call_output");

    const string unknownContent = """
        { "type": "future_content", "payload": { "id": 7 } }
        """;
    var part = JsonSerializer.Deserialize<ResponseContentPart>(unknownContent, ResponseJson.Default)!;
    var contentRoundTrip = JsonSerializer.SerializeToElement(part, ResponseJson.Default);
    Check(contentRoundTrip.GetProperty("payload").GetProperty("id").GetInt32() == 7,
        "unknown content part round trip");

    var refusal = JsonSerializer.Deserialize<ResponseContentPart>(
        """{ "type": "refusal", "refusal": "I cannot help with that request." }""",
        ResponseJson.Default)!;
    Check(refusal is RefusalContentPart, "refusal output content");
}

static void EnumWireValues()
{
    Check(JsonSerializer.Serialize(TruncationStrategy.Auto) == "\"auto\"", "truncation enum serialization");
    Check(JsonSerializer.Serialize(TruncationStrategy.Disabled) == "\"disabled\"", "disabled enum serialization");
    Check(JsonSerializer.Deserialize<TruncationStrategy>("\"auto\"") == TruncationStrategy.Auto,
        "truncation enum deserialization");
    Check(JsonSerializer.Serialize(ResponseRole.Developer) == "\"developer\"", "role enum serialization");
    Check(JsonSerializer.Deserialize<ResponseRole>("\"assistant\"") == ResponseRole.Assistant,
        "role enum deserialization");
}

static void UnknownShellVariants()
{
    const string json = """
        {
          "type": "shell_call",
          "call_id": "call_123",
          "action": { "commands": ["echo ok"] },
          "environment": { "type": "remote_future", "region": "test" }
        }
        """;
    var item = JsonSerializer.Deserialize<ResponseInputItem>(json, ResponseJson.Default)!;
    var roundTrip = JsonSerializer.SerializeToElement(item, ResponseJson.Default);
    Check(roundTrip.GetProperty("environment").GetProperty("type").GetString() == "remote_future",
        "unknown shell environment type");
    Check(roundTrip.GetProperty("environment").GetProperty("region").GetString() == "test",
        "unknown shell environment field");

    const string unknownOutcomeJson = """
        {
          "type": "shell_call_output",
          "call_id": "call_456",
          "output": [
            {
              "stdout": "ok",
              "stderr": "",
              "outcome": { "type": "signal", "signal": "SIGTERM" }
            }
          ]
        }
        """;
    var output = JsonSerializer.Deserialize<ResponseInputItem>(unknownOutcomeJson, ResponseJson.Default)!;
    var outputJson = JsonSerializer.SerializeToElement(output, ResponseJson.Default);
    Check(outputJson.GetProperty("output")[0].GetProperty("outcome").GetProperty("type").GetString() ==
        "signal", "unknown shell outcome type");
    Check(outputJson.GetProperty("output")[0].GetProperty("outcome").GetProperty("signal").GetString() ==
        "SIGTERM", "unknown shell outcome field");
}

static void Check(bool condition, string name)
{
    if (!condition)
        throw new InvalidOperationException($"Compatibility check failed: {name}");
}
