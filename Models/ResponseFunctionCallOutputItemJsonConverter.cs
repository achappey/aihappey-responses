using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHappey.Responses;

public sealed class ResponseFunctionCallOutputItemJsonConverter
    : JsonConverter<ResponseFunctionCallOutputItem>
{
    private static readonly HashSet<string> KnownProperties = new(StringComparer.Ordinal)
    {
        "type",
        "id",
        "call_id",
        "output",
        "name",
        "namespace",
        "status",
        "caller"
    };

    public override ResponseFunctionCallOutputItem Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("Function call output must be a JSON object.");

        var item = new ResponseFunctionCallOutputItem
        {
            Type = ReadString(root, "type") ?? "function_call_output",
            Id = ReadString(root, "id"),
            CallId = ReadString(root, "call_id"),
            Name = ReadString(root, "name"),
            Namespace = ReadString(root, "namespace"),
            Status = ReadString(root, "status"),
            Caller = root.TryGetProperty("caller", out var caller)
                && caller.ValueKind == JsonValueKind.Object
                    ? caller.Deserialize<ResponseCaller>(options)
                    : null,
            AdditionalProperties = root.EnumerateObject()
                .Where(property => !KnownProperties.Contains(property.Name))
                .ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.Ordinal)
        };

        if (root.TryGetProperty("output", out var output))
        {
            if (output.ValueKind == JsonValueKind.String)
            {
                item.Output = output.GetString() ?? string.Empty;
            }
            else
            {
                item.StructuredOutput = output.Clone();
                item.Output = output.GetRawText();
            }
        }

        return item;
    }

    public override void Write(
        Utf8JsonWriter writer,
        ResponseFunctionCallOutputItem value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);
        WriteString(writer, "id", value.Id);
        WriteString(writer, "call_id", value.CallId);
        writer.WritePropertyName("output");
        if (value.StructuredOutput is { } structuredOutput)
            structuredOutput.WriteTo(writer);
        else
            writer.WriteStringValue(value.Output);
        WriteString(writer, "name", value.Name);
        WriteString(writer, "namespace", value.Namespace);
        WriteString(writer, "status", value.Status);
        if (value.Caller is not null)
        {
            writer.WritePropertyName("caller");
            JsonSerializer.Serialize(writer, value.Caller, options);
        }

        if (value.AdditionalProperties is not null)
        {
            foreach (var property in value.AdditionalProperties)
            {
                if (KnownProperties.Contains(property.Key))
                    continue;

                writer.WritePropertyName(property.Key);
                property.Value.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }

    private static string? ReadString(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;

    private static void WriteString(Utf8JsonWriter writer, string propertyName, string? value)
    {
        if (value is not null)
            writer.WriteString(propertyName, value);
    }
}
