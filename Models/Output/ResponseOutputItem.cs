using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHappey.Responses.Output;

/// <summary>Extension data shared by the opt-in output models, including nested objects.</summary>
public abstract class ResponseOutputObject
{
    [JsonExtensionData]
    public virtual Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public abstract class ResponseOutputVariant : ResponseOutputObject
{
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    internal virtual void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, this, GetType(), options);
}

/// <summary>
/// Opt-in output union. Does not replace ResponseResult.Output or alter ResponseJson.Default.
/// Covers the 28-member official output union, with existing complete item contracts
/// exposed through ResponseOutputItem&lt;TItem&gt;. New item and content types are never
/// registered with the existing input or streaming converters.
/// Status strings remain open-ended, as in the existing contracts, to allow future
/// API values. Unknown variants and additional fields are retained for round trips.
/// Schemas: https://github.com/openai/openai-node/blob/aab204589e6a512883aa4d1e63dcfafbb4490f62/src/resources/responses/responses.ts
/// </summary>
[JsonConverter(typeof(ResponseOutputItemJsonConverter))]
public abstract class ResponseOutputItem : ResponseOutputVariant;

/// <summary>
/// Reuses a fully represented existing item without duplicating its contract.
/// Serialized directly as the item's wire JSON, not as an envelope.
/// The item retains its existing optional-property and serialization semantics.
/// </summary>
[JsonConverter(typeof(ResponseOutputItemJsonConverter))]
public sealed class ResponseOutputItem<TItem>(TItem item) : ResponseOutputItem
    where TItem : ResponseInputItem
{
    [JsonIgnore]
    public TItem Item { get; } = item ?? throw new ArgumentNullException(nameof(item));

    [JsonPropertyName("type")]
    public override string Type => Item.Type ?? throw new JsonException("Output item is missing 'type'.");

    [JsonExtensionData]
    public override Dictionary<string, JsonElement>? AdditionalProperties
    {
        get => Item.AdditionalProperties;
        set => Item.AdditionalProperties = value;
    }

    internal override void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, Item, options);
}

/// <summary>Retains the discriminator and all fields of a future output item.</summary>
public sealed class ResponseUnknownOutputItem : ResponseOutputItem
{
    [JsonIgnore]
    public string Discriminator { get; set; } = default!;

    [JsonPropertyName("type")]
    public override string Type => Discriminator;
}

public sealed class ResponseOutputItemJsonConverter : JsonConverter<ResponseOutputItem>
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(ResponseOutputItem)
            || (typeToConvert.IsGenericType
                && typeToConvert.GetGenericTypeDefinition() == typeof(ResponseOutputItem<>));

    public override ResponseOutputItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var discriminator = ResponseOutputDiscriminators.Read(root);
        ResponseOutputItem result = discriminator switch
        {
            "message" => root.Deserialize<ResponseOutputMessage>(options)!,
            "reasoning" => root.Deserialize<ResponseOutputReasoningItem>(options)!,
            "file_search_call" => root.Deserialize<ResponseOutputFileSearchCall>(options)!,
            "web_search_call" => root.Deserialize<ResponseOutputWebSearchCall>(options)!,
            "computer_call" => root.Deserialize<ResponseOutputComputerCall>(options)!,
            "computer_call_output" => root.Deserialize<ResponseOutputComputerCallResult>(options)!,
            "code_interpreter_call" => root.Deserialize<ResponseOutputCodeInterpreterCall>(options)!,
            "image_generation_call" => root.Deserialize<ResponseOutputImageGenerationCall>(options)!,
            "mcp_call" => root.Deserialize<ResponseOutputMcpCall>(options)!,
            "mcp_list_tools" => root.Deserialize<ResponseOutputMcpListTools>(options)!,
            "local_shell_call" => root.Deserialize<ResponseOutputLocalShellCall>(options)!,
            "function_call_output" => root.Deserialize<ResponseOutputFunctionCallResult>(options)!,
            "custom_tool_call_output" => root.Deserialize<ResponseOutputCustomToolCallResult>(options)!,
            "additional_tools" => root.Deserialize<ResponseOutputAdditionalTools>(options)!,
            "function_call" => Reuse<ResponseFunctionCallItem>(root, options),
            "custom_tool_call" => Reuse<ResponseCustomToolCallItem>(root, options),
            "compaction" => Reuse<ResponseCompactionItem>(root, options),
            "program" => Reuse<ResponseProgramItem>(root, options),
            "program_output" => Reuse<ResponseProgramOutputItem>(root, options),
            "shell_call" => root.Deserialize<ResponseOutputShellCall>(options)!,
            "shell_call_output" => root.Deserialize<ResponseOutputShellCallResult>(options)!,
            "apply_patch_call" => Reuse<ResponseApplyPatchCallItem>(root, options),
            "apply_patch_call_output" => Reuse<ResponseApplyPatchCallOutputItem>(root, options),
            "mcp_approval_request" => Reuse<ResponseMcpApprovalRequestItem>(root, options),
            "mcp_approval_response" => Reuse<ResponseMcpApprovalResponseItem>(root, options),
            "local_shell_call_output" => Reuse<ResponseLocalShellCallOutputItem>(root, options),
            "tool_search_call" => root.Deserialize<ResponseOutputToolSearchCall>(options)!,
            "tool_search_output" => root.Deserialize<ResponseOutputToolSearchResult>(options)!,
            _ => Unknown(root, discriminator, options)
        };
        if (!typeToConvert.IsInstanceOfType(result))
            throw new JsonException($"Output type '{discriminator}' is not compatible with {typeToConvert.Name}.");
        return result;
    }

    private static ResponseOutputItem<T> Reuse<T>(JsonElement root, JsonSerializerOptions options)
        where T : ResponseInputItem => new(root.Deserialize<T>(options)!);

    private static ResponseUnknownOutputItem Unknown(JsonElement root, string discriminator, JsonSerializerOptions options)
    {
        var result = root.Deserialize<ResponseUnknownOutputItem>(options)!;
        result.Discriminator = discriminator;
        return result;
    }

    public override void Write(Utf8JsonWriter writer, ResponseOutputItem value, JsonSerializerOptions options)
        => value.Write(writer, options);
}
