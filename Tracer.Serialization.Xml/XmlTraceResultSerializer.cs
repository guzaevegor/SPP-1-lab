using System.IO;
using System.Linq;
using System.Xml.Linq;
using Tracer.Core;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization.Xml
{
    public class XmlTraceResultSerializer : ITraceResultSerializer
    {
        public string Format => "xml";

        public void Serialize(TraceResult traceResult, Stream to)
        {
            var root = new XElement("root",
                traceResult.Threads.Select(t => new XElement("thread",
                    new XAttribute("id", t.ThreadId),
                    new XAttribute("time", $"{t.TimeMs}ms"),
                    t.Methods.Select(MapMethod)
                ))
            );

            root.Save(to);
        }

        private XElement MapMethod(MethodTraceResult method)
        {
            return new XElement("method",
                new XAttribute("name", method.MethodName),
                new XAttribute("time", $"{method.TimeMs}ms"),
                new XAttribute("class", method.ClassName),
                method.Methods.Select(MapMethod)
            );
        }
    }
}
