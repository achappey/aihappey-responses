using System.Text.Json;
using AIHappey.Responses;

internal static class NativeMetadataCompatibilityTests
{
    internal static void Run()
    {
        foreach (var value in new[] { "null", "{}", "[]", "false", "9007199254740993", "{\"nested\":[1,true,null]}" })
        {
            var source = JsonSerializer.Deserialize<JsonElement>("{\"model\":\"test\",\"input\":\"hello\",\"metadata\":{\"openai\":\"application\"},\"future\":" + value + "}");
            var request = source.Deserialize<ResponseRequest>(ResponseJson.Default)!;
            var replay = JsonSerializer.SerializeToElement(request, ResponseJson.Default);
            if (!JsonElement.DeepEquals(source.GetProperty("future"), request.AdditionalProperties!["future"])
                || !JsonElement.DeepEquals(source.GetProperty("future"), replay.GetProperty("future"))
                || !JsonElement.DeepEquals(source.GetProperty("metadata"), replay.GetProperty("metadata"))
                || replay.TryGetProperty("openai", out _))
                throw new InvalidOperationException("Native metadata and unknown root properties must round-trip independently: " + value);
        }
    }
}
