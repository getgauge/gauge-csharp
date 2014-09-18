using System;
using main;

namespace gauge_csharp
{
    internal interface IParamConverter
    {
        Object Convert(Parameter parameter);
    }
}