using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHappey.Responses.Output;

/// <summary>Discrimination is confined to the new output hierarchy.</summary>
public sealed class ResponseOutputVariantJsonConverter<T> : JsonConverter<T> where T : ResponseOutputVariant
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(T)
            || (typeof(T) == typeof(ResponseOutputContent)
                && typeToConvert == typeof(ResponseOutputRefusal));

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var discriminator = ResponseOutputDiscriminators.Read(root);
        var result = typeof(T) == typeof(ResponseOutputContent) && discriminator == "refusal"
            ? (T)(ResponseOutputVariant)new ResponseOutputRefusal(root.Deserialize<RefusalContentPart>(options)!)
            : (T)root.Deserialize(ResponseOutputDiscriminators.Resolve(typeof(T), discriminator), options)!;
        switch (result)
        {
            case ResponseUnknownOutputContent unknown: unknown.Discriminator = discriminator; break;
            case ResponseUnknownOutputAnnotation unknown: unknown.Discriminator = discriminator; break;
            case ResponseUnknownOutputWebSearchAction unknown: unknown.Discriminator = discriminator; break;
            case ResponseUnknownOutputComputerAction unknown: unknown.Discriminator = discriminator; break;
            case ResponseUnknownOutputCodeInterpreterResult unknown: unknown.Discriminator = discriminator; break;
            case ResponseUnknownOutputMcpError unknown: unknown.Discriminator = discriminator; break;
        }
        if (!typeToConvert.IsInstanceOfType(result))
            throw new JsonException($"Output content '{discriminator}' is not compatible with {typeToConvert.Name}.");
        return result;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => value.Write(writer, options);
}

internal static class ResponseOutputDiscriminators
{
    internal static string Read(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("type", out var type)
            || type.ValueKind != JsonValueKind.String)
            throw new JsonException("Output variant is missing a string 'type' discriminator.");
        return type.GetString()!;
    }

    internal static Type Resolve(Type union, string discriminator)
    {
        if (union == typeof(ResponseOutputContent))
            return discriminator switch
            {
                "output_text" => typeof(ResponseOutputText),
                _ => typeof(ResponseUnknownOutputContent)
            };
        if (union == typeof(ResponseOutputAnnotation))
            return discriminator switch
            {
                "file_citation" => typeof(ResponseOutputFileCitation),
                "url_citation" => typeof(ResponseOutputUrlCitation),
                "container_file_citation" => typeof(ResponseOutputContainerFileCitation),
                "file_path" => typeof(ResponseOutputFilePath),
                _ => typeof(ResponseUnknownOutputAnnotation)
            };
        if (union == typeof(ResponseOutputWebSearchAction))
            return discriminator switch
            {
                "search" => typeof(ResponseOutputWebSearch),
                "open_page" => typeof(ResponseOutputWebOpenPage),
                "find_in_page" => typeof(ResponseOutputWebFind),
                _ => typeof(ResponseUnknownOutputWebSearchAction)
            };
        if (union == typeof(ResponseOutputComputerAction))
            return discriminator switch
            {
                "click" => typeof(ResponseOutputComputerClick),
                "double_click" => typeof(ResponseOutputComputerDoubleClick),
                "drag" => typeof(ResponseOutputComputerDrag),
                "keypress" => typeof(ResponseOutputComputerKeypress),
                "move" => typeof(ResponseOutputComputerMove),
                "screenshot" => typeof(ResponseOutputComputerScreenshotAction),
                "scroll" => typeof(ResponseOutputComputerScroll),
                "type" => typeof(ResponseOutputComputerType),
                "wait" => typeof(ResponseOutputComputerWait),
                _ => typeof(ResponseUnknownOutputComputerAction)
            };
        if (union == typeof(ResponseOutputCodeInterpreterResult))
            return discriminator switch
            {
                "logs" => typeof(ResponseOutputCodeInterpreterLogs),
                "image" => typeof(ResponseOutputCodeInterpreterImage),
                _ => typeof(ResponseUnknownOutputCodeInterpreterResult)
            };
        if (union == typeof(ResponseOutputMcpError))
            return discriminator switch
            {
                "mcp_protocol_error" => typeof(ResponseOutputMcpProtocolError),
                "http_error" => typeof(ResponseOutputMcpHttpError),
                "mcp_tool_execution_error" => typeof(ResponseOutputMcpToolExecutionError),
                _ => typeof(ResponseUnknownOutputMcpError)
            };
        throw new NotSupportedException($"Unsupported output union: {union.Name}.");
    }
}
