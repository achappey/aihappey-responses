using System.Text.Json;
using AIHappey.Responses.Streaming;

namespace AIHappey.Responses.Output;

/// <summary>
/// Explicit conversions for locally constructed output. Native provider replay JSON
/// must not pass through these conversions: it is already the authoritative payload.
/// </summary>
public static class ResponseOutputConversions
{
    public static ResponseOutputContent ToOutputContent(this ResponseContentPart part)
    {
        ArgumentNullException.ThrowIfNull(part);
        return part switch
        {
            OutputTextPart text => new ResponseOutputText
            {
                Text = text.Text,
                Annotations = text.Annotations.Select(annotation => annotation is ResponseOutputAnnotation typed
                    ? typed
                    : JsonSerializer.SerializeToElement(annotation, ResponseJson.Default)
                        .Deserialize<ResponseOutputAnnotation>(ResponseJson.Default)
                        ?? throw new JsonException("Output annotation cannot be null.")).ToList(),
                Logprobs = text.Logprobs?.Select(value => value.Deserialize<ResponseOutputLogprob>(ResponseJson.Default)
                    ?? throw new JsonException("Output logprob cannot be null.")).ToList(),
                AdditionalProperties = CloneExtensions(text.AdditionalProperties)
            },
            RefusalContentPart refusal => new ResponseOutputRefusal(refusal),
            // File/image parts and future provider-specific content keep their exact
            // discriminator and fields via the output union's existing unknown case.
            _ => JsonSerializer.SerializeToElement(part, ResponseJson.Default)
                .Deserialize<ResponseOutputContent>(ResponseJson.Default)
                ?? throw new JsonException("Output content cannot be null.")
        };
    }

    public static ResponseMessageContent ToToolOutput(object? value)
    {
        if (value is ResponseMessageContent content)
            return content;
        if (value is IEnumerable<ResponseContentPart> parts)
            return new ResponseMessageContent(parts);
        if (value is string text)
            return new ResponseMessageContent(text);
        if (value is null)
            return new ResponseMessageContent("{}");

        var json = value is JsonElement element
            ? element
            : JsonSerializer.SerializeToElement(value, JsonSerializerOptions.Web);
        if (json.ValueKind == JsonValueKind.String)
            return new ResponseMessageContent(json.GetString() ?? string.Empty);
        if (json.ValueKind == JsonValueKind.Array && json.EnumerateArray().All(IsToolContentPart))
            return json.Deserialize<ResponseMessageContent>(ResponseJson.Default)
                ?? throw new JsonException("Tool output content cannot be null.");

        // Arbitrary tool JSON is a JSON string in the Responses contract, not an
        // untyped output object. Do not reinterpret ordinary object/array results.
        return new ResponseMessageContent(json.GetRawText());
    }

    public static ResponseStreamItem ToStreamItem(this ResponseOutputItem item)
        => JsonSerializer.SerializeToElement<ResponseOutputItem>(item, ResponseJson.Default)
            .Deserialize<ResponseStreamItem>(ResponseJson.Default)
            ?? throw new JsonException("Could not project output to a stream item.");

    private static bool IsToolContentPart(JsonElement value)
        => value.ValueKind == JsonValueKind.Object
            && value.TryGetProperty("type", out var type)
            && type.ValueKind == JsonValueKind.String
            && type.GetString() is "input_text" or "input_image" or "input_file";

    private static Dictionary<string, JsonElement>? CloneExtensions(Dictionary<string, JsonElement>? extensions)
        => extensions?.ToDictionary(pair => pair.Key, pair => pair.Value.Clone(), StringComparer.Ordinal);
}
