
using System.Collections.Generic;

namespace Tracer.Core
{
    public class MethodTraceResult
    {
        public string MethodName { get; }
        public string ClassName { get; }
        public long TimeMs { get; }
        public IReadOnlyList<MethodTraceResult> Methods { get; }

        public MethodTraceResult(string methodName, string className, long timeMs, List<MethodTraceResult> methods)
        {
            MethodName = methodName;
            ClassName = className;
            TimeMs = timeMs;
            Methods = methods.AsReadOnly();
        }
    }
}
