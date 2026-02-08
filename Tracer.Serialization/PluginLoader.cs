using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization
{
    public class PluginLoader
    {
        public List<ITraceResultSerializer> LoadPlugins(string path)
        {
            var serializers = new List<ITraceResultSerializer>();

            if (!Directory.Exists(path))
                return serializers;

            var files = Directory.GetFiles(path, "*.dll");

            foreach (var file in files)
            {
                try
                {
                    // Загружаем сборку
                    var assembly = Assembly.LoadFrom(file);

                    // Ищем типы, реализующие наш интерфейс
                    var types = assembly.GetTypes()
                        .Where(t => typeof(ITraceResultSerializer).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (var type in types)
                    {
                        if (Activator.CreateInstance(type) is ITraceResultSerializer instance)
                        {
                            serializers.Add(instance);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading plugin {file}: {ex.Message}");
                }
            }

            return serializers;
        }
    }
}
