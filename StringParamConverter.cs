using main;

namespace gauge_csharp
{
    internal class StringParamConverter : IParamConverter
    {
        public object Convert(Parameter parameter)
        {
            return parameter.Value;
        }
    }
}