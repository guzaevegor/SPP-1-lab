using System.IO;
using System.Linq;
using System.Text.Json; // Встроен в .NET
using Tracer.Core;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization.Json
{
    public class JsonTraceResultSerializer : ITraceResultSerializer
    {
        public string Format => "json";

        public void Serialize(TraceResult traceResult, Stream to)
        {

            var dto = new
            {
                threads = traceResult.Threads.Select(t => new
                {
                    id = t.ThreadId,
                    time = $"{t.TimeMs}ms",
                    methods = t.Methods.Select(m => MapMethod(m)).ToList()
                }).ToList()
            };

            var options = new JsonSerializerOptions { WriteIndented = true };

            // Используем Utf8JsonWriter для записи в Stream
            using var writer = new Utf8JsonWriter(to, new JsonWriterOptions { Indented = true });
            JsonSerializer.Serialize(writer, dto, options);
        }

        private object MapMethod(MethodTraceResult method)
        {
            return new
            {
                name = method.MethodName,
                @class = method.ClassName,
                time = $"{method.TimeMs}ms",
                methods = method.Methods.Select(m => MapMethod(m)).ToList()
            };
        }
    }
}
