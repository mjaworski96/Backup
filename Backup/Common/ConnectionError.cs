namespace Common
{
    public class ConnectionError
    {
        public ErrorCodes ErrorCode { get; set; }
        public object ExpectedParameterValue { get; set; }
    }
}
