using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHappey.Responses;

/// <summary>Native Responses apply_patch_call, usable both as output and replay input.
/// The item id is distinct from call_id. Call status is in_progress or completed;
/// failure belongs to the corresponding apply_patch_call_output, not this call.</summary>
public sealed class ResponseApplyPatchCallItem : ResponseInputItem
{
    public ResponseApplyPatchCallItem() => Type = "apply_patch_call";

    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("call_id")]
    public string CallId { get; set; } = default!;

    [JsonPropertyName("operation")]
    public ResponseApplyPatchOperation Operation { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "completed";

    [JsonPropertyName("caller")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseCaller? Caller { get; set; }

    [JsonPropertyName("created_by")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; set; }
}

/// <summary>Native result, correlated using call_id. Status is completed or failed.</summary>
public sealed class ResponseApplyPatchCallOutputItem : ResponseInputItem
{
    public ResponseApplyPatchCallOutputItem() => Type = "apply_patch_call_output";

    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("call_id")]
    public string CallId { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "completed";

    [JsonPropertyName("output")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Output { get; set; }

    [JsonPropertyName("caller")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseCaller? Caller { get; set; }

    [JsonPropertyName("created_by")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; set; }
}

[JsonConverter(typeof(ResponseApplyPatchOperationJsonConverter))]
public abstract class ResponseApplyPatchOperation
{
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    [JsonPropertyName("path")]
    public string Path { get; set; } = default!;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public sealed class ResponseApplyPatchCreateFile : ResponseApplyPatchOperation
{
    [JsonPropertyName("type")]
    public override string Type => "create_file";

    [JsonPropertyName("diff")]
    public string Diff { get; set; } = default!;
}

public sealed class ResponseApplyPatchUpdateFile : ResponseApplyPatchOperation
{
    [JsonPropertyName("type")]
    public override string Type => "update_file";

    [JsonPropertyName("diff")]
    public string Diff { get; set; } = default!;
}

public sealed class ResponseApplyPatchDeleteFile : ResponseApplyPatchOperation
{
    [JsonPropertyName("type")]
    public override string Type => "delete_file";
}

/// <summary>Preserves future operations without making them executable.</summary>
public sealed class ResponseUnknownApplyPatchOperation : ResponseApplyPatchOperation
{
    [JsonIgnore]
    public string Discriminator { get; set; } = default!;

    [JsonPropertyName("type")]
    public override string Type => Discriminator;
}

public sealed class ResponseApplyPatchOperationJsonConverter : JsonConverter<ResponseApplyPatchOperation>
{
    public override ResponseApplyPatchOperation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var type = root.GetProperty("type").GetString() ?? throw new JsonException("Missing patch operation type.");
        return type switch
        {
            "create_file" => root.Deserialize<ResponseApplyPatchCreateFile>(options)!,
            "update_file" => root.Deserialize<ResponseApplyPatchUpdateFile>(options)!,
            "delete_file" => root.Deserialize<ResponseApplyPatchDeleteFile>(options)!,
            _ => ReadUnknown(root, type, options)
        };
    }

    private static ResponseUnknownApplyPatchOperation ReadUnknown(JsonElement root, string type, JsonSerializerOptions options)
    {
        var value = root.Deserialize<ResponseUnknownApplyPatchOperation>(options)!;
        value.Discriminator = type;
        return value;
    }

    public override void Write(Utf8JsonWriter writer, ResponseApplyPatchOperation value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, value.GetType(), options);
}
