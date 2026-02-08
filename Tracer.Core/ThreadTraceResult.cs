using System.Collections.Generic;
using System.Linq;

namespace Tracer.Core
{
    public class ThreadTraceResult
    {
        public int ThreadId { get; }
        public long TimeMs { get; }
        public IReadOnlyList<MethodTraceResult> Methods { get; }

        public ThreadTraceResult(int threadId, List<MethodTraceResult> methods)
        {
            ThreadId = threadId;
            Methods = methods.AsReadOnly();
            TimeMs = methods.Sum(m => m.TimeMs);
        }
    }
}
