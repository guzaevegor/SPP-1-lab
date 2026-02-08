using System;
using System.IO;
using System.Threading;
using Tracer.Core;
using Tracer.Serialization;

namespace Tracer.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Создаем трейсер
            ITracer tracer = new Core.Tracer();

            // 2. Запускаем нагрузку (пример с многопоточностью)
            var foo = new Foo(tracer);
            Thread t1 = new Thread(foo.MyMethod);
            Thread t2 = new Thread(foo.MyMethod);

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            // 3. Получаем результат
            var result = tracer.GetTraceResult();

            // 4. Загружаем плагины
            // Предполагаем, что сбилженные плагины лежат рядом или в папке plugins
            // Для теста можно скопировать DLL плагинов в bin/Debug/net6.0/
            var pluginLoader = new PluginLoader();
            var serializers = pluginLoader.LoadPlugins(AppDomain.CurrentDomain.BaseDirectory);

            if (serializers.Count == 0)
            {
                Console.WriteLine("No plugins found.");
            }

            // 5. Сохраняем
            foreach (var serializer in serializers)
            {
                var fileName = $"result.{serializer.Format}";
                using var fileStream = new FileStream(fileName, FileMode.Create);
                serializer.Serialize(result, fileStream);
                Console.WriteLine($"Saved to {fileName}");
            }
        }
    }

    public class Foo
    {
        private readonly ITracer _tracer;
        private readonly Bar _bar;

        public Foo(ITracer tracer)
        {
            _tracer = tracer;
            _bar = new Bar(_tracer);
        }

        public void MyMethod()
        {
            _tracer.StartTrace();
            Thread.Sleep(100);
            _bar.InnerMethod();
            _tracer.StopTrace();
        }
    }

    public class Bar
    {
        private readonly ITracer _tracer;
        public Bar(ITracer tracer) => _tracer = tracer;

        public void InnerMethod()
        {
            _tracer.StartTrace();
            Thread.Sleep(50);
            _tracer.StopTrace();
        }
    }
}
