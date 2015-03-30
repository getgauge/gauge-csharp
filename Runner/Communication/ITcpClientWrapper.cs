using System.IO;

namespace Gauge.CSharp.Runner.Communication
{
    public interface ITcpClientWrapper
    {
        bool Connected { get;}
        Stream GetStream();
        void Close();
    }
}