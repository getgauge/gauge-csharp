using main;

namespace Gauge.CSharp.Runner.Converters
{
    internal class StringParamConverter : IParamConverter
    {
        public object Convert(Parameter parameter)
        {
            return parameter.Value;
        }
    }
}