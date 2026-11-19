namespace Finance.Application.Exceptions
{
    /// <summary>The caller is authenticated but not allowed to do this (mapped to 403 with a stable code).</summary>
    public class ForbiddenException : Exception
    {
        public string Code { get; }

        public ForbiddenException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}
