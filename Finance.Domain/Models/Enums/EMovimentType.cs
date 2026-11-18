namespace Finance.Domain.Models.Enums
{
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public enum EMovimentType
    {
        Income = 1,
        Expense = 2
    }
}
