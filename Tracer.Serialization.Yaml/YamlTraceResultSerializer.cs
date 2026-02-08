using System.IO;
using System.Linq;
using Tracer.Core;
using Tracer.Serialization.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tracer.Serialization.Yaml
{
    public class YamlTraceResultSerializer : ITraceResultSerializer
    {
        public string Format => "yaml";

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

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            using var writer = new StreamWriter(to);
            serializer.Serialize(writer, dto);
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
