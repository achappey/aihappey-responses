using System.Text.Json;
using AIHappey.Responses;
using AIHappey.Responses.Streaming;

internal static class ApplyPatchCompatibilityTests
{
    public static void Run()
    {
        const string call = """
            {"type":"apply_patch_call","id":"apc_item","call_id":"call_patch","status":"completed",
             "operation":{"type":"update_file","path":"lib/fib.py","diff":"@@\n-old\n+new","future":true},
             "caller":{"type":"program","caller_id":"program_1"},"created_by":"agent_1","future_item":42}
            """;
        var typed = JsonSerializer.Deserialize<ResponseInputItem>(call, ResponseJson.Default) as ResponseApplyPatchCallItem;
        Check(typed?.Id == "apc_item" && typed.CallId == "call_patch", "distinct ids");
        Check(typed!.Operation is ResponseApplyPatchUpdateFile { Path: "lib/fib.py", Diff: "@@\n-old\n+new" }, "typed operation");
        Check(typed.Caller?.CallerId == "program_1" && typed.CreatedBy == "agent_1", "caller metadata");
        var replay = JsonSerializer.SerializeToElement<ResponseInputItem>(typed, ResponseJson.Default);
        Check(replay.GetProperty("operation").GetProperty("future").GetBoolean(), "operation extension");
        Check(replay.GetProperty("future_item").GetInt32() == 42, "item extension");

        foreach (var operation in new ResponseApplyPatchOperation[] {
            new ResponseApplyPatchCreateFile { Path = "new.txt", Diff = "+hello\n" },
            new ResponseApplyPatchUpdateFile { Path = "old.txt", Diff = "@@\n-a\n+b" },
            new ResponseApplyPatchDeleteFile { Path = "old.txt" } })
        {
            var json = JsonSerializer.SerializeToElement(operation, ResponseJson.Default);
            var copy = json.Deserialize<ResponseApplyPatchOperation>(ResponseJson.Default)!;
            Check(copy.Type == operation.Type && copy.Path == operation.Path, "operation round trip");
            Check(json.TryGetProperty("diff", out _) == (operation is not ResponseApplyPatchDeleteFile), "delete has no diff");
        }

        foreach (var status in new[] { "completed", "failed" })
        {
            ResponseInputItem output = new ResponseApplyPatchCallOutputItem { CallId = "call_patch", Status = status, Output = "Result" };
            var copy = JsonSerializer.Deserialize<ResponseInputItem>(JsonSerializer.Serialize(output, ResponseJson.Default), ResponseJson.Default);
            Check(copy is ResponseApplyPatchCallOutputItem { CallId: "call_patch", Output: "Result" } outputCopy && outputCopy.Status == status, "native output replay");
        }

        foreach (var eventType in new[] { "response.output_item.added", "response.output_item.done" })
        {
            var json = "{\"type\":\"" + eventType + "\",\"sequence_number\":7,\"output_index\":0,\"item\":" + call + "}";
            var stream = JsonSerializer.Deserialize<ResponseStreamPart>(json, ResponseJson.Default)!;
            var item = stream is ResponseOutputItemAdded added ? added.Item : ((ResponseOutputItemDone)stream).Item;
            Check(item.Operation is ResponseApplyPatchUpdateFile && item.CallId == "call_patch", "stream operation");
            Check(JsonSerializer.SerializeToElement(stream, ResponseJson.Default).GetProperty("item").GetProperty("operation").GetProperty("diff").GetString() == "@@\n-old\n+new", "stream round trip");
        }

        var request = JsonSerializer.Deserialize<ResponseRequest>("""
            {"model":"gpt-5.1","tools":[{"type":"apply_patch"}],"tool_choice":{"type":"apply_patch"},"input":[]}
            """, ResponseJson.Default)!;
        var requestJson = JsonSerializer.SerializeToElement(request, ResponseJson.Default);
        Check(requestJson.GetProperty("tools")[0].EnumerateObject().Count() == 1, "minimal native tool");
        Check(requestJson.GetProperty("tool_choice").GetProperty("type").GetString() == "apply_patch", "native tool choice");
        var resultJson = "{\"id\":\"resp_1\",\"created_at\":1,\"model\":\"gpt-5.1\",\"output\":[" + call + "]}";
        var result = JsonSerializer.Deserialize<ResponseResult>(resultJson, ResponseJson.Default)!;
        Check(JsonSerializer.SerializeToElement(result, ResponseJson.Default).GetProperty("output")[0].GetProperty("operation").GetProperty("path").GetString() == "lib/fib.py", "response preservation");
    }

    private static void Check(bool value, string description)
    {
        if (!value) throw new InvalidOperationException("Apply Patch: " + description);
    }
}
