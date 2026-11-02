using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using AIHappey.Responses;
using AIHappey.Responses.Output;

// Fixtures follow the official ResponseOutputItem union and its nested schemas:
// https://github.com/openai/openai-node/blob/aab204589e6a512883aa4d1e63dcfafbb4490f62/src/resources/responses/responses.ts
var tests = new (string Name, Action Run)[]
{
    ("all 28 official output item discriminators", OutputTests.AllOutputItems),
    ("assistant messages, annotations, refusals and logprobs", OutputTests.Messages),
    ("reasoning summaries and reasoning text", OutputTests.Reasoning),
    ("function and custom call arguments and multimodal results", OutputTests.FunctionAndCustomTools),
    ("web and file search results", OutputTests.Search),
    ("all computer actions and safety checks", OutputTests.Computer),
    ("MCP tools, errors and approvals", OutputTests.Mcp),
    ("code interpreter and image generation results", OutputTests.GeneratedMedia),
    ("shell and patch contracts are reused without envelopes", OutputTests.ReusedTools),
    ("unknown discriminators and extension data", OutputTests.UnknownVariants),
    ("known variants preserve future fields at every discriminated level", OutputTests.KnownExtensions),
    ("optional, nullable and required properties", OutputTests.OptionalAndRequired),
    ("programmatic construction and concrete serialization", OutputTests.Construction),
    ("existing serializer settings and contracts remain isolated", OutputTests.Isolation),
    ("canonical outputs cross result, replay and streaming boundaries", OutputTests.TransportBoundaries),
    ("constructed files and tool payloads remain lossless", OutputTests.PayloadConversions)
};

foreach (var (name, run) in tests)
{
    run();
    Console.WriteLine($"PASS {name}");
}
Console.WriteLine($"PASS {tests.Length} output test groups");

internal static class OutputTests
{
    private static readonly JsonSerializerOptions[] Settings =
    [
        new(),
        new(JsonSerializerDefaults.Web),
        ResponseJson.Default,
        new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }
    ];

    private static readonly (string Json, Type Expected)[] Fixtures =
    [
        ("""{"type":"message","id":"msg_1","role":"assistant","status":"completed","content":[{"type":"output_text","text":"Hello!","annotations":[]}],"phase":"final_answer"}""", typeof(ResponseOutputMessage)),
        ("""{"type":"file_search_call","id":"fs_1","queries":["annual report"],"status":"completed","results":[{"file_id":"file_1","filename":"report.pdf","score":0.97,"text":"Revenue grew."}]}""", typeof(ResponseOutputFileSearchCall)),
        ("""{"type":"function_call","id":"fc_1","call_id":"call_1","name":"get_weather","arguments":"{\"city\":\"Paris\"}","status":"completed"}""", typeof(ResponseOutputItem<ResponseFunctionCallItem>)),
        ("""{"type":"function_call_output","id":"fco_1","call_id":"call_1","output":"{\"temperature\":18}","status":"completed"}""", typeof(ResponseOutputFunctionCallResult)),
        ("""{"type":"web_search_call","id":"ws_1","status":"completed","action":{"type":"search","queries":["OpenAI Responses API"]}}""", typeof(ResponseOutputWebSearchCall)),
        ("""{"type":"computer_call","id":"cu_1","call_id":"call_2","status":"completed","action":{"type":"click","button":"left","x":100,"y":200},"pending_safety_checks":[]}""", typeof(ResponseOutputComputerCall)),
        ("""{"type":"computer_call_output","id":"cuo_1","call_id":"call_2","status":"completed","output":{"type":"computer_screenshot","image_url":"data:image/png;base64,abc"}}""", typeof(ResponseOutputComputerCallResult)),
        ("""{"type":"reasoning","id":"rs_1","summary":[{"type":"summary_text","text":"I will compare the sources."}],"content":[{"type":"reasoning_text","text":"The sources agree."}],"encrypted_content":"opaque"}""", typeof(ResponseOutputReasoningItem)),
        ("""{"type":"program","id":"prg_1","call_id":"call_3","code":"return 1;","fingerprint":"opaque"}""", typeof(ResponseOutputItem<ResponseProgramItem>)),
        ("""{"type":"program_output","id":"prgo_1","call_id":"call_3","result":"1","status":"completed"}""", typeof(ResponseOutputItem<ResponseProgramOutputItem>)),
        ("""{"type":"tool_search_call","id":"ts_1","call_id":null,"execution":"server","status":"completed","arguments":{"query":"weather tools"},"created_by":"agent_1"}""", typeof(ResponseOutputToolSearchCall)),
        ("""{"type":"tool_search_output","id":"tso_1","call_id":null,"execution":"server","status":"completed","tools":[{"type":"function","name":"get_weather","parameters":{"type":"object"}}],"created_by":"agent_1"}""", typeof(ResponseOutputToolSearchResult)),
        ("""{"type":"additional_tools","id":"at_1","role":"developer","tools":[{"type":"function","name":"get_weather","parameters":{"type":"object"}}]}""", typeof(ResponseOutputAdditionalTools)),
        ("""{"type":"compaction","id":"cmp_1","encrypted_content":"opaque","created_by":"agent_1"}""", typeof(ResponseOutputItem<ResponseCompactionItem>)),
        ("""{"type":"image_generation_call","id":"ig_1","result":null,"status":"generating","quality":"auto"}""", typeof(ResponseOutputImageGenerationCall)),
        ("""{"type":"code_interpreter_call","id":"ci_1","container_id":"cntr_1","code":"print(1)","outputs":[{"type":"logs","logs":"1\n"},{"type":"image","url":"https://example.com/chart.png"}],"status":"completed"}""", typeof(ResponseOutputCodeInterpreterCall)),
        ("""{"type":"local_shell_call","id":"ls_1","call_id":"call_4","status":"completed","action":{"type":"exec","command":["echo","hello"],"env":{"LANG":"C"},"timeout_ms":1000,"user":"runner","working_directory":"/workspace"}}""", typeof(ResponseOutputLocalShellCall)),
        ("""{"type":"local_shell_call_output","id":"lso_1","output":"{\"stdout\":\"hello\"}","status":"completed"}""", typeof(ResponseOutputItem<ResponseLocalShellCallOutputItem>)),
        ("""{"type":"shell_call","id":"sh_1","call_id":"call_5","status":"completed","action":{"commands":["echo hello"],"max_output_length":100,"timeout_ms":1000},"environment":{"type":"container_reference","container_id":"cntr_1"},"caller":{"type":"direct"}}""", typeof(ResponseOutputShellCall)),
        ("""{"type":"shell_call_output","id":"sho_1","call_id":"call_5","status":"completed","max_output_length":100,"output":[{"stdout":"hello","stderr":"","outcome":{"type":"exit","exit_code":0}}],"caller":{"type":"direct"}}""", typeof(ResponseOutputShellCallResult)),
        ("""{"type":"apply_patch_call","id":"ap_1","call_id":"call_6","status":"completed","operation":{"type":"create_file","path":"hello.txt","diff":"+hello"},"caller":{"type":"program","caller_id":"prg_1"}}""", typeof(ResponseOutputItem<ResponseApplyPatchCallItem>)),
        ("""{"type":"apply_patch_call_output","id":"apo_1","call_id":"call_6","status":"completed","output":"Created hello.txt"}""", typeof(ResponseOutputItem<ResponseApplyPatchCallOutputItem>)),
        ("""{"type":"mcp_call","id":"mcp_1","name":"search","server_label":"docs","arguments":"{\"query\":\"Responses\"}","output":"Found documentation","status":"completed"}""", typeof(ResponseOutputMcpCall)),
        ("""{"type":"mcp_list_tools","id":"mcpl_1","server_label":"docs","tools":[{"name":"search","description":"Search docs","input_schema":{"type":"object","properties":{"query":{"type":"string"}}}}]}""", typeof(ResponseOutputMcpListTools)),
        ("""{"type":"mcp_approval_request","id":"mcpa_1","name":"search","server_label":"docs","arguments":"{\"query\":\"Responses\"}"}""", typeof(ResponseOutputItem<ResponseMcpApprovalRequestItem>)),
        ("""{"type":"mcp_approval_response","id":"mcpar_1","approval_request_id":"mcpa_1","approve":true,"reason":"Approved"}""", typeof(ResponseOutputItem<ResponseMcpApprovalResponseItem>)),
        ("""{"type":"custom_tool_call","id":"ct_1","call_id":"call_7","name":"grammar","input":"SELECT 1","namespace":"sql","async":true,"caller":{"type":"program","caller_id":"prg_1"}}""", typeof(ResponseOutputItem<ResponseCustomToolCallItem>)),
        ("""{"type":"custom_tool_call_output","id":"cto_1","call_id":"call_7","status":"completed","output":[{"type":"input_text","text":"1"},{"type":"input_file","file_id":"file_1"}]}""", typeof(ResponseOutputCustomToolCallResult))
    ];

    public static void AllOutputItems()
    {
        Check(Fixtures.Length == 28, "official union coverage count");
        foreach (var options in Settings)
        {
            foreach (var (json, expected) in Fixtures)
            {
                var item = JsonSerializer.Deserialize<ResponseOutputItem>(json, options)!;
                Check(item.GetType() == expected, expected.Name);
                var serialized = JsonSerializer.Serialize<ResponseOutputItem>(item, options);
                AssertSubset(JsonNode.Parse(json), JsonNode.Parse(serialized), expected.Name);
                var copy = JsonSerializer.Deserialize<ResponseOutputItem>(serialized, options)!;
                Check(copy.GetType() == expected, "round-trip runtime type");
            }
            var arrayJson = "[" + string.Join(",", Fixtures.Select(f => f.Json)) + "]";
            var array = JsonSerializer.Deserialize<List<ResponseOutputItem>>(arrayJson, options)!;
            Check(array.Count == 28 && array.Select(i => i.Type).Distinct().Count() == 28, "heterogeneous output array");
            AssertSubset(JsonNode.Parse(arrayJson), JsonSerializer.SerializeToNode(array, options), "output array");
        }
    }

    public static void Messages()
    {
        const string json = """
            {"type":"message","id":"msg_1","role":"assistant","status":"completed","phase":"commentary","content":[
              {"type":"output_text","text":"Sources and files.","annotations":[
                {"type":"file_citation","file_id":"file_1","filename":"report.pdf","index":0},
                {"type":"url_citation","start_index":0,"end_index":7,"url":"https://example.com","title":"Source"},
                {"type":"container_file_citation","container_id":"cntr_1","file_id":"file_2","filename":"chart.png","start_index":8,"end_index":17},
                {"type":"file_path","file_id":"file_3","index":1}
              ],"logprobs":[{"token":"Sources","bytes":[83,111],"logprob":-0.1,"top_logprobs":[{"token":"Source","bytes":[83],"logprob":-1.2}]}]},
              {"type":"refusal","refusal":"I cannot help with that request."}
            ]}
            """;
        var message = Read<ResponseOutputMessage>(json);
        Check(message.Role == ResponseRole.Assistant && message.Phase == "commentary", "output message metadata");
        var text = (ResponseOutputText)message.Content[0];
        Check(text.Annotations[0] is ResponseOutputFileCitation { Filename: "report.pdf", Index: 0 }, "file citation");
        Check(text.Annotations[1] is ResponseOutputUrlCitation { StartIndex: 0, EndIndex: 7 }, "URL citation");
        Check(text.Annotations[2] is ResponseOutputContainerFileCitation { ContainerId: "cntr_1" }, "container citation");
        Check(text.Annotations[3] is ResponseOutputFilePath { Index: 1 }, "file path");
        Check(text.Logprobs![0].TopLogprobs[0].Logprob == -1.2, "nested logprobs");
        Check(message.Content[1] is ResponseOutputRefusal { Part.Refusal: "I cannot help with that request." }, "reused refusal");
        var refusal = (ResponseOutputRefusal)message.Content[1];
        var refusalJson = JsonSerializer.Serialize(refusal);
        Check(JsonSerializer.Deserialize<ResponseOutputRefusal>(refusalJson)!.Part.Refusal == refusal.Part.Refusal, "concrete refusal round trip");
        RoundTrip(json);
    }

    public static void Reasoning()
    {
        var reasoning = Read<ResponseOutputReasoningItem>(Fixtures[7].Json);
        Check(reasoning.Summary[0] is ResponseReasoningSummaryTextPart { Text: "I will compare the sources." }, "reused summary");
        Check(reasoning.Content![0].Text == "The sources agree.", "typed reasoning text");
        Check(reasoning.EncryptedContent == "opaque", "encrypted content");
        var minimal = Read<ResponseOutputReasoningItem>("""{"type":"reasoning","id":"rs_2","summary":[]}""");
        Check(minimal.Content is null && minimal.EncryptedContent is null && minimal.Status is null, "optional reasoning properties");
    }

    public static void FunctionAndCustomTools()
    {
        var function = Read<ResponseOutputItem<ResponseFunctionCallItem>>(Fixtures[2].Json);
        Check(function.Item.Arguments == """{"city":"Paris"}""", "arguments remain a JSON string");
        var result = Read<ResponseOutputFunctionCallResult>(Fixtures[3].Json);
        Check(result.Output.Text == """{"temperature":18}""", "string result");
        var custom = Read<ResponseOutputItem<ResponseCustomToolCallItem>>(Fixtures[26].Json);
        Check(custom.Item.Input == "SELECT 1" && custom.Item.Async == true && custom.Item.Caller?.CallerId == "prg_1", "custom call");
        const string multimodal = """
            {"type":"function_call_output","id":"fco_2","status":"completed","call_id":"call_1",
             "caller":{"type":"program","caller_id":"prg_1"},"created_by":"agent_1","name":"get_weather","namespace":"weather",
             "output":[{"type":"input_text","text":"Here is the report."},
                       {"type":"input_image","image_url":"https://example.com/chart.png","detail":"high"},
                       {"type":"input_file","file_data":"data:application/pdf;base64,abc","filename":"report.pdf"}]}
            """;
        var multimodalResult = Read<ResponseOutputFunctionCallResult>(multimodal);
        Check(multimodalResult.Output.Parts![0] is InputTextPart { Text: "Here is the report." }, "reused input text");
        Check(multimodalResult.Output.Parts[1] is InputImagePart { Detail: "high" }, "reused input image");
        Check(multimodalResult.Output.Parts[2] is InputFilePart { Filename: "report.pdf" }, "reused input file");
        Check(multimodalResult.Caller?.CallerId == "prg_1" && multimodalResult.CreatedBy == "agent_1", "result metadata");
        RoundTrip(multimodal);
        Check(Read<ResponseOutputCustomToolCallResult>(Fixtures[27].Json).Output.Parts![1] is InputFilePart, "custom multimodal result");
        RoundTrip("""{"type":"custom_tool_call_output","id":"cto_2","call_id":"call_7","status":"incomplete","output":""}""");
    }

    public static void Search()
    {
        const string fileJson = """
            {"type":"file_search_call","id":"fs_1","queries":["quarterly revenue"],"status":"completed",
             "results":[{"file_id":"file_1","filename":"report.pdf","score":0.97,"text":"Revenue grew.",
                         "attributes":{"department":"finance","year":2026,"published":true}},{}]}
            """;
        var file = Read<ResponseOutputFileSearchCall>(fileJson);
        Check(file.Queries[0] == "quarterly revenue" && file.Results![0].Score == 0.97, "typed search results");
        var attributes = file.Results![0].Attributes!;
        Check(attributes["department"].Text == "finance" && attributes["year"].Number == 2026 && attributes["published"].Boolean == true, "typed metadata scalar union");
        Check(file.Results[1].FileId is null && file.Results[1].Score is null, "optional result fields");
        RoundTrip(fileJson);
        foreach (var action in new[]
        {
            """{"type":"search","query":"Responses","queries":["Responses API"],"sources":[{"type":"url","url":"https://openai.com"}]}""",
            """{"type":"open_page","url":"https://openai.com"}""",
            """{"type":"find_in_page","url":"https://openai.com","pattern":"Responses"}"""
        })
            RoundTrip("""{"type":"web_search_call","id":"ws_1","status":"completed","action":""" + action + "}");
        var search = JsonSerializer.Deserialize<ResponseOutputWebSearchAction>("""{"type":"search","queries":["Responses"]}""")!;
        Check(search is ResponseOutputWebSearch { Queries.Count: 1 }, "typed search action");
        Throws<JsonException>(() => JsonSerializer.Deserialize<ResponseOutputAttributeValue>("{}"), "invalid metadata object");
        Throws<JsonException>(() => JsonSerializer.Deserialize<ResponseOutputAttributeValue>("null"), "invalid metadata null");
    }

    public static void Computer()
    {
        var actions = new (string Json, Type Expected)[]
        {
            ("""{"type":"click","button":"left","x":1,"y":2,"keys":["SHIFT"]}""", typeof(ResponseOutputComputerClick)),
            ("""{"type":"double_click","x":1,"y":2,"keys":null}""", typeof(ResponseOutputComputerDoubleClick)),
            ("""{"type":"drag","path":[{"x":1,"y":2},{"x":3,"y":4}],"keys":["CTRL"]}""", typeof(ResponseOutputComputerDrag)),
            ("""{"type":"keypress","keys":["ENTER"]}""", typeof(ResponseOutputComputerKeypress)),
            ("""{"type":"move","x":1.5,"y":2,"keys":[]}""", typeof(ResponseOutputComputerMove)),
            ("""{"type":"screenshot"}""", typeof(ResponseOutputComputerScreenshotAction)),
            ("""{"type":"scroll","scroll_x":0,"scroll_y":100,"x":1,"y":2,"keys":[]}""", typeof(ResponseOutputComputerScroll)),
            ("""{"type":"type","text":"hello"}""", typeof(ResponseOutputComputerType)),
            ("""{"type":"wait"}""", typeof(ResponseOutputComputerWait))
        };
        foreach (var options in Settings)
            foreach (var (json, expected) in actions)
            {
                var action = JsonSerializer.Deserialize<ResponseOutputComputerAction>(json, options)!;
                Check(action.GetType() == expected, "computer " + expected.Name);
                AssertSubset(JsonNode.Parse(json), JsonSerializer.SerializeToNode(action, options), "computer action");
            }
        var batched = """{"type":"computer_call","id":"cu_1","call_id":"call_2","status":"in_progress","actions":["""
            + string.Join(",", actions.Select(a => a.Json))
            + """],"pending_safety_checks":[{"id":"sc_1","code":"sensitive","message":"Confirm action"}]}""";
        Check(Read<ResponseOutputComputerCall>(batched).Actions!.Count == 9, "batched actions");
        RoundTrip(batched);
        const string result = """
            {"type":"computer_call_output","id":"cuo_1","call_id":"call_2","status":"completed",
             "output":{"type":"computer_screenshot","file_id":"file_screenshot"},
             "acknowledged_safety_checks":[{"id":"sc_1","code":"sensitive","message":"Confirm action"}],"created_by":"agent_1"}
            """;
        var screenshot = Read<ResponseOutputComputerCallResult>(result);
        Check(screenshot.Output.FileId == "file_screenshot" && screenshot.AcknowledgedSafetyChecks![0].Id == "sc_1", "screenshot and safety checks");
        RoundTrip(result);
    }

    public static void Mcp()
    {
        const string listing = """
            {"type":"mcp_list_tools","id":"mcpl_1","server_label":"docs","tools":[
              {"name":"search","description":"Search docs","input_schema":{"type":"object","required":["query"],"properties":{"query":{"type":"string"}}},
               "annotations":{"readOnlyHint":true},"x_tool":5}],"error":null}
            """;
        var tools = Read<ResponseOutputMcpListTools>(listing);
        Check(tools.Tools[0].InputSchema.GetProperty("required")[0].GetString() == "query", "arbitrary JSON schema");
        Check(tools.Tools[0].Annotations!.Value.GetProperty("readOnlyHint").GetBoolean(), "arbitrary MCP annotations");
        RoundTrip(listing, allowOmittedNull: true);
        var errors = new (string Json, Type Expected)[]
        {
            ("""{"type":"mcp_protocol_error","code":-32602,"message":"Invalid arguments"}""", typeof(ResponseOutputMcpProtocolError)),
            ("""{"type":"http_error","code":503,"message":"Unavailable"}""", typeof(ResponseOutputMcpHttpError)),
            ("""{"type":"mcp_tool_execution_error","content":[{"type":"text","text":"Tool failed"}]}""", typeof(ResponseOutputMcpToolExecutionError)),
            ("""{"type":"mcp_tool_execution_error","content":null}""", typeof(ResponseOutputMcpToolExecutionError))
        };
        foreach (var (json, expected) in errors)
        {
            var callJson = """{"type":"mcp_call","id":"mcp_1","name":"search","server_label":"docs","arguments":"{}","status":"failed","approval_request_id":"mcpa_1","error":""" + json + "}";
            Check(Read<ResponseOutputMcpCall>(callJson).Error!.GetType() == expected, "typed MCP error");
            RoundTrip(callJson);
        }
        var approval = Read<ResponseOutputItem<ResponseMcpApprovalResponseItem>>(Fixtures[25].Json);
        Check(approval.Item.Approve == true && approval.Item.ApprovalRequestId == "mcpa_1", "approval reuse");
    }

    public static void GeneratedMedia()
    {
        var code = Read<ResponseOutputCodeInterpreterCall>(Fixtures[15].Json);
        Check(code.Outputs![0] is ResponseOutputCodeInterpreterLogs { Logs: "1\n" }, "code logs");
        Check(code.Outputs[1] is ResponseOutputCodeInterpreterImage { Url: "https://example.com/chart.png" }, "code image");
        RoundTrip("""{"type":"code_interpreter_call","id":"ci_2","container_id":"cntr_1","status":"in_progress","code":null,"outputs":null}""");
        const string image = """
            {"type":"image_generation_call","id":"ig_2","result":null,"status":"failed","action":"generate",
             "background":"transparent","output_format":"png","quality":"high","revised_prompt":"A blue sky","size":"1536x1024",
             "error":{"code":"moderation_blocked","message":"Blocked","param":null,"type":"invalid_request_error",
                      "moderation_details":{"categories":["violence"],"moderation_stage":"output","x_moderation":true}}}
            """;
        var generated = Read<ResponseOutputImageGenerationCall>(image);
        Check(generated.Error!.ModerationDetails!.Categories[0] == "violence", "typed moderation details");
        RoundTrip(image);
        RoundTrip("""{"type":"image_generation_call","id":"ig_3","result":"aGVsbG8=","status":"completed"}""");
    }

    public static void ReusedTools()
    {
        var shell = Read<ResponseOutputShellCall>(Fixtures[18].Json);
        Check(shell.Action.Commands[0] == "echo hello", "typed shell action");
        Check(shell.Environment is ResponseShellContainerReferenceEnvironment { ContainerId: "cntr_1" }, "reused shell environment");
        var result = Read<ResponseOutputShellCallResult>(Fixtures[19].Json);
        Check(result.Output[0].Outcome is ResponseShellExitOutcome { ExitCode: 0 }, "reused shell outcome");
        var patch = Read<ResponseOutputItem<ResponseApplyPatchCallItem>>(Fixtures[20].Json);
        Check(patch.Item.Operation is ResponseApplyPatchCreateFile { Path: "hello.txt", Diff: "+hello" }, "reused patch operation");
        foreach (var operation in new[]
        {
            """{"type":"create_file","path":"new.txt","diff":"+new"}""",
            """{"type":"update_file","path":"old.txt","diff":"@@\n-old\n+new"}""",
            """{"type":"delete_file","path":"old.txt"}"""
        })
            RoundTrip("""{"type":"apply_patch_call","id":"ap_2","call_id":"call_6","status":"completed","operation":""" + operation + "}");
        RoundTrip("""{"type":"shell_call","id":"sh_2","call_id":"call_5","status":"completed","action":{"commands":["sleep 10"],"timeout_ms":null,"max_output_length":null},"environment":null}""");
        RoundTrip("""{"type":"shell_call_output","id":"sho_2","call_id":"call_5","status":"incomplete","max_output_length":null,"output":[{"stdout":"","stderr":"timed out","outcome":{"type":"timeout"}}]}""");
        var directJson = JsonSerializer.Serialize(patch, ResponseJson.Default);
        Check(!JsonNode.Parse(directJson)!.AsObject().ContainsKey("Item"), "no wrapper envelope");
        var directCopy = JsonSerializer.Deserialize<ResponseOutputItem<ResponseApplyPatchCallItem>>(directJson, ResponseJson.Default)!;
        Check(directCopy.Item.CallId == "call_6", "concrete wrapper deserialization");
        var local = Read<ResponseOutputLocalShellCall>(Fixtures[16].Json);
        Check(local.Action.Command.SequenceEqual(["echo", "hello"]) && local.Action.Env["LANG"] == "C", "typed local shell action");
    }

    public static void UnknownVariants()
    {
        const string unknown = """{"type":"future_output","id":"future_1","payload":{"items":[1,null,{"enabled":true}]}}""";
        Check(Read<ResponseUnknownOutputItem>(unknown).AdditionalProperties!["payload"].GetProperty("items").GetArrayLength() == 3, "unknown root payload");
        RoundTrip(unknown);
        const string message = """
            {"type":"message","id":"msg_1","role":"assistant","status":"completed","x_message":{"v":1},"content":[
              {"type":"output_text","text":"Hi","annotations":[{"type":"future_citation","source":{"id":"s_1"}},{"type":"file_path","file_id":"file_1","index":0,"x_path":true}],"x_text":42},
              {"type":"future_content","payload":["a","b"]}]}
            """;
        var typed = Read<ResponseOutputMessage>(message);
        Check(typed.Content[1] is ResponseUnknownOutputContent { Type: "future_content" }, "unknown content");
        Check(((ResponseOutputText)typed.Content[0]).Annotations[0] is ResponseUnknownOutputAnnotation { Type: "future_citation" }, "unknown annotation");
        RoundTrip(message);
        RoundTrip("""{"type":"web_search_call","id":"ws_1","status":"completed","action":{"type":"future_search","nested":{"v":1}}}""");
        RoundTrip("""{"type":"computer_call","id":"cu_1","call_id":"call_2","status":"completed","pending_safety_checks":[],"actions":[{"type":"future_action","nested":{"v":1}}]}""");
        RoundTrip("""{"type":"code_interpreter_call","id":"ci_1","container_id":"cntr_1","status":"completed","code":"","outputs":[{"type":"future_result","nested":{"v":1}}]}""");
        RoundTrip("""{"type":"mcp_call","id":"mcp_1","name":"search","server_label":"docs","arguments":"{}","error":{"type":"future_error","nested":{"v":1}}}""");
        RoundTrip("""{"type":"function_call","id":"fc_1","call_id":"call_1","name":"weather","arguments":"{}","future_field":{"x":1}}""");
    }

    public static void OptionalAndRequired()
    {
        foreach (var options in Settings)
        {
            var minimal = JsonSerializer.Deserialize<ResponseOutputItem>("""{"type":"message","id":"msg_1","role":"assistant","status":"in_progress","content":[]}""", options)!;
            var json = JsonSerializer.SerializeToNode(minimal, options)!.AsObject();
            Check(!json.ContainsKey("phase"), "optional property omitted independently of caller settings");
            RoundTrip("""{"type":"image_generation_call","id":"ig_1","status":"in_progress","result":null}""");
            RoundTrip("""{"type":"code_interpreter_call","id":"ci_1","container_id":"cntr_1","status":"in_progress","code":null,"outputs":null}""");
            Check(Read<ResponseOutputFileSearchCall>("""{"type":"file_search_call","id":"fs_1","status":"searching","queries":[],"results":null}""").Results is null, "nullable results");
            Check(Read<ResponseOutputMcpCall>("""{"type":"mcp_call","id":"mcp_1","name":"search","server_label":"docs","arguments":"{}","output":null,"error":null,"approval_request_id":null}""").Output is null, "nullable MCP fields");
        }
        foreach (var invalid in new[]
        {
            "{}",
            """{"type":null}""",
            """{"type":7}""",
            "[]",
            """{"type":"message","status":"completed","content":[]}""",
            """{"type":"message","id":"msg_1","status":"completed"}""",
            """{"type":"code_interpreter_call","id":"ci_1","container_id":"cntr_1","status":"completed","outputs":null}""",
            """{"type":"image_generation_call","id":"ig_1","status":"completed"}"""
        })
            Throws<JsonException>(() => JsonSerializer.Deserialize<ResponseOutputItem>(invalid), "required property or discriminator");
        Throws<JsonException>(() => JsonSerializer.Deserialize<ResponseOutputContent>("{}"), "required nested discriminator");
        Throws<JsonException>(() => JsonSerializer.Deserialize<ResponseOutputItem<ResponseShellCallItem>>(Fixtures[0].Json), "mismatched reuse type");

        foreach (var (fixture, modelType) in Fixtures.Where(f => !f.Expected.IsGenericType))
            foreach (var property in modelType.GetProperties().Where(p => Attribute.IsDefined(p, typeof(RequiredMemberAttribute))))
            {
                var name = ((JsonPropertyNameAttribute)Attribute.GetCustomAttribute(property, typeof(JsonPropertyNameAttribute))!).Name;
                var missing = JsonNode.Parse(fixture)!.AsObject();
                Check(missing.Remove(name), "fixture includes required " + name);
                Throws<JsonException>(() => JsonSerializer.Deserialize<ResponseOutputItem>(missing.ToJsonString()), modelType.Name + "." + name);
            }
    }

    public static void KnownExtensions()
    {
        foreach (var (fixture, _) in Fixtures)
        {
            var extended = JsonNode.Parse(fixture)!;
            Extend(extended);
            foreach (var options in Settings)
            {
                var item = JsonSerializer.Deserialize<ResponseOutputItem>(extended.ToJsonString(), options)!;
                AssertSubset(extended, JsonSerializer.SerializeToNode(item, options), item.Type);
            }
        }

        var reused = Read<ResponseOutputItem<ResponseFunctionCallItem>>(Fixtures[2].Json);
        reused.AdditionalProperties = new() { ["x_new"] = JsonSerializer.SerializeToElement(new[] { 1, 2 }) };
        Check(reused.Item.AdditionalProperties!.ContainsKey("x_new"), "wrapper extensions delegate to existing item");
        Check(JsonSerializer.SerializeToNode(reused)!["x_new"]!.AsArray().Count == 2, "wrapper extension serialization");
        var refusal = new ResponseOutputRefusal(new() { Refusal = "Declined." })
        {
            AdditionalProperties = new() { ["x_new"] = JsonSerializer.SerializeToElement(true) }
        };
        Check(JsonSerializer.SerializeToNode(refusal)!["x_new"]!.GetValue<bool>(), "reused refusal extensions");

        static void Extend(JsonNode? node)
        {
            if (node is JsonArray array)
                foreach (var child in array) Extend(child);
            else if (node is JsonObject obj)
            {
                foreach (var (_, child) in obj.ToList()) Extend(child);
                if (obj.ContainsKey("type"))
                    obj["x_future"] = new JsonObject { ["values"] = new JsonArray(1, null, true) };
            }
        }
    }

    public static void Construction()
    {
        ResponseOutputItem message = new ResponseOutputMessage
        {
            Id = "msg_1", Role = ResponseRole.Assistant, Status = "completed",
            Content = [new ResponseOutputText { Text = "Hello", Annotations = [] }]
        };
        var json = JsonSerializer.Serialize(message);
        Check(Read<ResponseOutputMessage>(json).Content[0] is ResponseOutputText { Text: "Hello" }, "constructed message");
        var constructed = new ResponseOutputItem<ResponseFunctionCallItem>(new()
        {
            Id = "fc_1", CallId = "call_1", Name = "weather", Arguments = """{"city":"Paris"}"""
        });
        var serialized = JsonSerializer.Serialize(constructed);
        Check(JsonNode.Parse(serialized)!["arguments"]!.GetValue<string>() == """{"city":"Paris"}""", "constructed reused function");
        Check(Read<ResponseOutputItem<ResponseFunctionCallItem>>(serialized).Item.Name == "weather", "constructed function round trip");
        var concrete = new ResponseOutputText { Text = "Hello", Annotations = [] };
        Check(JsonSerializer.Deserialize<ResponseOutputText>(JsonSerializer.Serialize(concrete))!.Text == "Hello", "concrete new payload");
    }

    public static void Isolation()
    {
        Check(ResponseJson.Default.Converters.Count == 5, "no global converter registrations");
        Check(ResponseJson.Default.Converters.All(c => c.GetType().Namespace != "AIHappey.Responses.Output"), "output converters remain opt-in");
        var oldInput = JsonSerializer.Deserialize<ResponseInputItem>(Fixtures[0].Json, ResponseJson.Default);
        Check(oldInput is ResponseInputMessage, "old input discrimination is unchanged");
        var oldText = JsonSerializer.Deserialize<ResponseContentPart>("""{"type":"output_text","text":"Hi","annotations":[]}""", ResponseJson.Default);
        Check(oldText is OutputTextPart, "old content discrimination is unchanged");
        var response = JsonSerializer.Deserialize<ResponseResult>("""{"id":"resp_1","created_at":1,"model":"gpt-5.1","output":[""" + Fixtures[0].Json + "]}", ResponseJson.Default)!;
        var outputProperty = typeof(ResponseResult).GetProperty("Output")!;
        Check(outputProperty.PropertyType == typeof(IEnumerable<object>), "existing generic output property unchanged");
        Check(JsonSerializer.SerializeToNode(response, ResponseJson.Default)!["output"]![0]!["type"]!.GetValue<string>() == "message", "old response serialization");
    }

    public static void TransportBoundaries()
    {
        var result = new ResponseResult
        {
            Id = "resp_1", Model = "test",
            Output = [new ResponseOutputMessage
            {
                Id = "msg_1", Role = ResponseRole.Assistant, Status = "completed",
                Content = [new ResponseOutputText { Text = "Answer", Annotations = [] }]
            }, new ResponseOutputItem<ResponseFunctionCallItem>(new()
            {
                Id = "fc_1", CallId = "call_1", Name = "lookup", Status = "completed", Arguments = "{}"
            })]
        };
        foreach (var options in Settings)
        {
            var json = JsonSerializer.SerializeToElement(result, options);
            foreach (var value in json.GetProperty("output").EnumerateArray())
                Check(value.Deserialize<ResponseOutputItem>(options) is not null, "object-typed result output remains canonical");
        }
        Check(result.Output.GetAssistantOutputText() == "Answer", "typed text extraction");
        Check(result.OutputItems.First() is ResponseInputMessage, "canonical message can be replayed");
        Check(result.OutputItems.Last() is ResponseFunctionCallItem, "reused call can be replayed");
        var item = (ResponseOutputItem)result.Output.First();
        var stream = item.ToStreamItem();
        Check(stream.Id == "msg_1" && stream.Content!.Value[0].GetProperty("annotations").GetArrayLength() == 0, "stream projection includes required fields");
    }

    public static void PayloadConversions()
    {
        var file = new InputFilePart
        {
            FileUrl = "https://files.test/report?sig=a%2Fb", Filename = "report.pdf",
            AdditionalProperties = new() { ["x_file"] = JsonSerializer.SerializeToElement(new[] { 1, 2 }) }
        }.ToOutputContent();
        Check(file is ResponseUnknownOutputContent { Type: "input_file" }, "provider file content uses existing unknown union member");
        var json = JsonSerializer.SerializeToElement<ResponseOutputContent>(file, ResponseJson.Default);
        Check(json.GetProperty("file_url").GetString() == "https://files.test/report?sig=a%2Fb", "file URL unchanged");
        Check(json.GetProperty("x_file").GetArrayLength() == 2, "file extensions preserved");
        var content = ResponseOutputConversions.ToToolOutput(JsonSerializer.Deserialize<JsonElement>("""[{"type":"input_file","file_id":"file_1"},{"type":"input_text","text":"Report"}]"""));
        Check(content.Parts![0] is InputFilePart { FileId: "file_1" }, "multimodal result remains structured");
        Check(ResponseOutputConversions.ToToolOutput("plain result").Text == "plain result", "plain text is not double encoded");
        var arbitrary = JsonSerializer.Deserialize<JsonElement>("""[{"type":"business_record","id":7}]""");
        Check(ResponseOutputConversions.ToToolOutput(arbitrary).Text == arbitrary.GetRawText(), "ordinary JSON arrays stay tool data");
        var future = JsonSerializer.Deserialize<ResponseOutputAnnotation>("""{"type":"future_citation","x":true}""")!;
        var text = (ResponseOutputText)new OutputTextPart("text") { Annotations = [future] }.ToOutputContent();
        Check(ReferenceEquals(text.Annotations[0], future), "already typed annotations are not reserialized");
    }

    private static T Read<T>(string json) where T : ResponseOutputItem
        => (T)JsonSerializer.Deserialize<ResponseOutputItem>(json, ResponseJson.Default)!;

    private static void RoundTrip(string json, bool allowOmittedNull = false)
    {
        foreach (var options in Settings)
        {
            var item = JsonSerializer.Deserialize<ResponseOutputItem>(json, options)!;
            var serialized = JsonSerializer.Serialize<ResponseOutputItem>(item, options);
            AssertSubset(JsonNode.Parse(json), JsonNode.Parse(serialized), item.Type, allowOmittedNull);
            var copy = JsonSerializer.Deserialize<ResponseOutputItem>(serialized, options)!;
            Check(copy.GetType() == item.GetType(), "round-trip type " + item.Type);
            Check(JsonNode.DeepEquals(JsonNode.Parse(serialized), JsonSerializer.SerializeToNode<ResponseOutputItem>(copy, options)), "stable round trip " + item.Type);
        }
    }

    private static void AssertSubset(JsonNode? expected, JsonNode? actual, string path, bool allowOmittedNull = false)
    {
        if (expected is JsonObject expectedObject && actual is JsonObject actualObject)
        {
            foreach (var (key, value) in expectedObject)
            {
                if (allowOmittedNull && value is null && !actualObject.ContainsKey(key)) continue;
                Check(actualObject.ContainsKey(key), path + "." + key + " exists");
                AssertSubset(value, actualObject[key], path + "." + key, allowOmittedNull);
            }
        }
        else if (expected is JsonArray expectedArray && actual is JsonArray actualArray)
        {
            Check(expectedArray.Count == actualArray.Count, path + " array length");
            for (var i = 0; i < expectedArray.Count; i++)
                AssertSubset(expectedArray[i], actualArray[i], path + "[" + i + "]", allowOmittedNull);
        }
        else Check(JsonNode.DeepEquals(expected, actual), path + " value");
    }

    private static void Throws<T>(Action action, string description) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name + ": " + description);
    }

    private static void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException("Output contracts: " + description);
    }
}
