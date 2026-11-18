namespace Finance.Domain.Models.Enums
{
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public enum EUserRole
    {
        none = 0,
        user = 1,
        admin = 2,
    }
}
