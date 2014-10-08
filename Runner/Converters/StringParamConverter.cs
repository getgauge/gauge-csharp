using main;

namespace Gauge.CSharp.Runner.Converters
{
    public class StringParamConverter : IParamConverter
    {
        public object Convert(Parameter parameter)
        {
            return parameter.Value;
        }
    }
}