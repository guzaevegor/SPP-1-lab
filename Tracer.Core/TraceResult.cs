using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Tracer.Core
{
    public class TraceResult
    {
        public IReadOnlyList<ThreadTraceResult> Threads { get; }

        internal TraceResult(ConcurrentDictionary<int, List<MethodTraceResult>> threadMethods)
        {
            var resultList = new List<ThreadTraceResult>();
            foreach (var threadId in threadMethods.Keys)
            {
                resultList.Add(new ThreadTraceResult(threadId, threadMethods[threadId]));
            }
            Threads = resultList.AsReadOnly();
        }
    }
}
