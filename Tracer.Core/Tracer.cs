using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Tracer.Core
{
    public class Tracer : ITracer
    {
        private class MethodContext
        {
            public string MethodName { get; set; }
            public string ClassName { get; set; }
            public Stopwatch Stopwatch { get; set; }
            public List<MethodTraceResult> Children { get; } = new List<MethodTraceResult>();
        }

        private readonly ConcurrentDictionary<int, Stack<MethodContext>> _activeThreads = new();

        private readonly ConcurrentDictionary<int, List<MethodTraceResult>> _completedThreadMethods = new();

        private readonly object _lock = new();

        public void StartTrace()
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;

            var stackTrace = new StackTrace(1, false);
            var frame = stackTrace.GetFrame(0);
            var method = frame?.GetMethod();

            var context = new MethodContext
            {
                MethodName = method?.Name ?? "Unknown",
                ClassName = method?.DeclaringType?.Name ?? "Unknown",
                Stopwatch = Stopwatch.StartNew()
            };

            var stack = _activeThreads.GetOrAdd(threadId, _ => new Stack<MethodContext>());
            stack.Push(context);
        }

        public void StopTrace()
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;

            if (!_activeThreads.TryGetValue(threadId, out var stack) || stack.Count == 0)
                return;

            var context = stack.Pop();
            context.Stopwatch.Stop();

            var result = new MethodTraceResult(
                context.MethodName,
                context.ClassName,
                context.Stopwatch.ElapsedMilliseconds,
                context.Children
            );

            if (stack.Count > 0)
            {
                var parent = stack.Peek();
                parent.Children.Add(result);
            }
            else
            {
                var list = _completedThreadMethods.GetOrAdd(threadId, _ => new List<MethodTraceResult>());
                lock (list) 
                {
                    list.Add(result);
                }
            }
        }

        public TraceResult GetTraceResult()
        {
            return new TraceResult(_completedThreadMethods);
        }
    }
}
