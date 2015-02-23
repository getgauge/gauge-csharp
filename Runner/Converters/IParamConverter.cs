using System;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Converters
{
    internal interface IParamConverter
    {
        Object Convert(Parameter parameter);
    }
}