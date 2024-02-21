namespace ToSic.Eav.WebApi.Dto;

/// <summary>
/// Idea: Most WebAPIs should return something, but it's better to return the data as a property of an envelope - like this.
/// Reason is that we may want to add more data later on, and then the client needs to be adapted after making the backend change, without
/// breaking the client in the meantime.
/// </summary>
/// <remarks>
/// Created 2024-02-21 by 2dm, ATM not widely used.
/// </remarks>
public class WebApiResultDto
{
    /// <summary>
    /// To be used when just returning a true/false result
    /// </summary>
    [JsonPropertyName("ok")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Ok { get; set; }

    /// <summary>
    /// Additional message, if not i18n
    /// </summary>
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Message { get; set; }

    /// <summary>
    /// Message key i18n to lookup in translations
    /// </summary>
    [JsonPropertyName("messageKey")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string MessageKey { get; set; }

    [JsonPropertyName("timeTaken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal TimeTaken { get; set; }
}