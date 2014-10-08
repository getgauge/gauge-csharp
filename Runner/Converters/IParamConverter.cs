using System;
using main;

namespace Gauge.CSharp.Runner.Converters
{
    internal interface IParamConverter
    {
        Object Convert(Parameter parameter);
    }
}