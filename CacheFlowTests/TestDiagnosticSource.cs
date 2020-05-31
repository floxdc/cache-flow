using System.Diagnostics;

namespace CacheFlowTests
{
    public class TestDiagnosticSource : DiagnosticSource
    {
        public override void Write(string name, object value)
        {
        }


        public override bool IsEnabled(string name) => true;
    }
}
