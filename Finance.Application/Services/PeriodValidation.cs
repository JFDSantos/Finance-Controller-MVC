using FluentValidation;
using FluentValidation.Results;

namespace Finance.Application.Services
{
    public static class PeriodValidation
    {
        public const int MinYear = 2000, MaxYear = 2100, MaxSeriesMonths = 24;

        private static ValidationException Fail(string property, string message, string code)
            => new(new[] { new ValidationFailure(property, message) { ErrorCode = code } });

        public static void ValidateMonth(int? month)
        {
            if (month is null or < 1 or > 12)
                throw Fail("month", "Month must be between 1 and 12.", "INVALID_MONTH");
        }

        public static void ValidateYear(int? year)
        {
            if (year is null or < MinYear or > MaxYear)
                throw Fail("year", $"Year must be between {MinYear} and {MaxYear}.", "INVALID_YEAR");
        }

        public static void ValidateMonths(int months)
        {
            if (months < 1 || months > MaxSeriesMonths)
                throw Fail("months", $"Months must be between 1 and {MaxSeriesMonths}.", "INVALID_RANGE");
        }
    }
}
