using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIHappey.Responses;

public sealed class TruncationStrategyJsonConverter : JsonConverter<TruncationStrategy>
{
    public override TruncationStrategy Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => reader.GetString() switch
        {
            "auto" => TruncationStrategy.Auto,
            "disabled" => TruncationStrategy.Disabled,
            var value => throw new JsonException($"Unknown truncation strategy: '{value}'.")
        };

    public override void Write(
        Utf8JsonWriter writer,
        TruncationStrategy value,
        JsonSerializerOptions options)
        => writer.WriteStringValue(value switch
        {
            TruncationStrategy.Auto => "auto",
            TruncationStrategy.Disabled => "disabled",
            _ => throw new JsonException($"Unknown truncation strategy value: '{value}'.")
        });
}

public sealed class ResponseRoleJsonConverter : JsonConverter<ResponseRole>
{
    public override ResponseRole Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => reader.GetString() switch
        {
            "user" => ResponseRole.User,
            "assistant" => ResponseRole.Assistant,
            "system" => ResponseRole.System,
            "developer" => ResponseRole.Developer,
            var value => throw new JsonException($"Unknown Responses role: '{value}'.")
        };

    public override void Write(
        Utf8JsonWriter writer,
        ResponseRole value,
        JsonSerializerOptions options)
        => writer.WriteStringValue(value switch
        {
            ResponseRole.User => "user",
            ResponseRole.Assistant => "assistant",
            ResponseRole.System => "system",
            ResponseRole.Developer => "developer",
            _ => throw new JsonException($"Unknown Responses role value: '{value}'.")
        });
}
