using System.Text.Json.Serialization;

namespace AIHappey.Responses.Output;

public sealed class ResponseOutputComputerCall : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "computer_call";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("call_id")]
    public required string CallId { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("pending_safety_checks")]
    public required List<ResponseOutputComputerSafetyCheck> PendingSafetyChecks { get; set; }
    [JsonPropertyName("action")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseOutputComputerAction? Action { get; set; }
    [JsonPropertyName("actions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ResponseOutputComputerAction>? Actions { get; set; }
}

public sealed class ResponseOutputComputerCallResult : ResponseOutputItem
{
    [JsonPropertyName("type")]
    public override string Type => "computer_call_output";
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("call_id")]
    public required string CallId { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    [JsonPropertyName("output")]
    public required ResponseOutputComputerScreenshot Output { get; set; }
    [JsonPropertyName("acknowledged_safety_checks")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ResponseOutputComputerSafetyCheck>? AcknowledgedSafetyChecks { get; set; }
    [JsonPropertyName("created_by")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; set; }
}

public sealed class ResponseOutputComputerSafetyCheck : ResponseOutputObject
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }
}

public sealed class ResponseOutputComputerScreenshot : ResponseOutputVariant
{
    [JsonPropertyName("type")]
    public override string Type => "computer_screenshot";
    [JsonPropertyName("file_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FileId { get; set; }
    [JsonPropertyName("image_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageUrl { get; set; }
}

[JsonConverter(typeof(ResponseOutputVariantJsonConverter<ResponseOutputComputerAction>))]
public abstract class ResponseOutputComputerAction : ResponseOutputVariant;

public sealed class ResponseOutputComputerClick : ResponseOutputComputerAction
{
    [JsonPropertyName("type")]
    public override string Type => "click";
    [JsonPropertyName("button")]
    public required string Button { get; set; }
    [JsonPropertyName("x")]
    public required double X { get; set; }
    [JsonPropertyName("y")]
    public required double Y { get; set; }
    [JsonPropertyName("keys")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Keys { get; set; }
}

public sealed class ResponseOutputComputerDoubleClick : ResponseOutputComputerAction
{
    [JsonPropertyName("type")]
    public override string Type => "double_click";
    [JsonPropertyName("x")]
    public required double X { get; set; }
    [JsonPropertyName("y")]
    public required double Y { get; set; }
    [JsonPropertyName("keys")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required List<string>? Keys { get; set; }
}

public sealed class ResponseOutputComputerDrag : ResponseOutputComputerAction
{
    [JsonPropertyName("type")]
    public override string Type => "drag";
    [JsonPropertyName("path")]
    public required List<ResponseOutputComputerPoint> Path { get; set; }
    [JsonPropertyName("keys")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Keys { get; set; }
}

public sealed class ResponseOutputComputerPoint : ResponseOutputObject
{
    [JsonPropertyName("x")]
    public required double X { get; set; }
    [JsonPropertyName("y")]
    public required double Y { get; set; }
}

public sealed class ResponseOutputComputerKeypress : ResponseOutputComputerAction
{
    [JsonPropertyName("type")]
    public override string Type => "keypress";
    [JsonPropertyName("keys")]
    public required List<string> Keys { get; set; }
}

public sealed class ResponseOutputComputerMove : ResponseOutputComputerAction
{
    [JsonPropertyName("type")]
    public override string Type => "move";
    [JsonPropertyName("x")]
    public required double X { get; set; }
    [JsonPropertyName("y")]
    public required double Y { get; set; }
    [JsonPropertyName("keys")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Keys { get; set; }
}

public sealed class ResponseOutputComputerScreenshotAction : ResponseOutputComputerAction
{
    [JsonPropertyName("type")]
    public override string Type => "screenshot";
}

public sealed class ResponseOutputComputerScroll : ResponseOutputComputerAction
{
    [JsonPropertyName("type")]
    public override string Type => "scroll";
    [JsonPropertyName("scroll_x")]
    public required double ScrollX { get; set; }
    [JsonPropertyName("scroll_y")]
    public required double ScrollY { get; set; }
    [JsonPropertyName("x")]
    public required double X { get; set; }
    [JsonPropertyName("y")]
    public required double Y { get; set; }
    [JsonPropertyName("keys")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Keys { get; set; }
}

public sealed class ResponseOutputComputerType : ResponseOutputComputerAction
{
    [JsonPropertyName("type")]
    public override string Type => "type";
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public sealed class ResponseOutputComputerWait : ResponseOutputComputerAction
{
    [JsonPropertyName("type")]
    public override string Type => "wait";
}

public sealed class ResponseUnknownOutputComputerAction : ResponseOutputComputerAction
{
    [JsonIgnore]
    public string Discriminator { get; set; } = default!;
    [JsonPropertyName("type")]
    public override string Type => Discriminator;
}
