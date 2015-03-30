namespace Gauge.CSharp.Runner.Exceptions
{
    public class NotAValidGaugeProjectException : System.Exception
    {
        private const string InvalidProjectMessage = "This is not a valid Gauge Project";
        public NotAValidGaugeProjectException()
            : base(InvalidProjectMessage)
        {
        }
    }
}