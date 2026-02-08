using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Tracer.Core; // Убедись, что этот namespace доступен

namespace Tracer.Core.Tests
{
    public class TracerTests
    {
        // --- Тест 1: Простой одиночный метод ---
        [Fact]
        public void SingleMethod_Trace_ShouldCaptureMethodNameAndClass()
        {
            // Arrange
            ITracer tracer = new Tracer();
            var testClass = new TestClass(tracer);

            // Act
            testClass.MethodA();
            var result = tracer.GetTraceResult();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Threads); // Должен быть 1 поток

            var threadResult = result.Threads[0];
            Assert.Single(threadResult.Methods); // 1 метод верхнего уровня

            var method = threadResult.Methods[0];
            Assert.Equal(nameof(TestClass.MethodA), method.MethodName);
            Assert.Equal(nameof(TestClass), method.ClassName);
            Assert.True(method.TimeMs >= 50, "Time should be at least 50ms");
            Assert.Empty(method.Methods); // Вложенных методов нет
        }

        // --- Тест 2: Вложенные методы ---
        [Fact]
        public void NestedMethods_Trace_ShouldBuildCorrectHierarchy()
        {
            // Arrange
            ITracer tracer = new Tracer();
            var testClass = new TestClass(tracer);

            // Act
            testClass.RootMethod(); // Root -> Child -> Leaf
            var result = tracer.GetTraceResult();

            // Assert
            var threadResult = result.Threads[0];
            Assert.Single(threadResult.Methods);

            // Проверяем уровень 1 (Root)
            var root = threadResult.Methods[0];
            Assert.Equal(nameof(TestClass.RootMethod), root.MethodName);
            Assert.Single(root.Methods);

            // Проверяем уровень 2 (Child)
            var child = root.Methods[0];
            Assert.Equal(nameof(TestClass.ChildMethod), child.MethodName);
            Assert.Single(child.Methods);

            // Проверяем уровень 3 (Leaf)
            var leaf = child.Methods[0];
            Assert.Equal(nameof(TestClass.LeafMethod), leaf.MethodName);
            Assert.Empty(leaf.Methods);
        }

        // --- Тест 3: Многопоточность ---
        [Fact]
        public void MultiThreaded_Trace_ShouldSeparateThreads()
        {
            // Arrange
            ITracer tracer = new Tracer();
            var testClass = new TestClass(tracer);
            var thread1 = new Thread(testClass.MethodA);
            var thread2 = new Thread(testClass.MethodB);

            // Act
            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            var result = tracer.GetTraceResult();

            // Assert
            Assert.Equal(2, result.Threads.Count); // Должно быть 2 разных потока

            foreach (var thread in result.Threads)
            {
                Assert.Single(thread.Methods);
                // Проверяем, что id потока в результате > 0 (реальный id)
                Assert.True(thread.ThreadId > 0);
            }
        }

        // --- Вспомогательный класс для тестов ---
        private class TestClass
        {
            private readonly ITracer _tracer;

            public TestClass(ITracer tracer)
            {
                _tracer = tracer;
            }

            public void MethodA()
            {
                _tracer.StartTrace();
                Thread.Sleep(50);
                _tracer.StopTrace();
            }

            public void MethodB()
            {
                _tracer.StartTrace();
                Thread.Sleep(50);
                _tracer.StopTrace();
            }

            public void RootMethod()
            {
                _tracer.StartTrace();
                ChildMethod();
                _tracer.StopTrace();
            }

            public void ChildMethod()
            {
                _tracer.StartTrace();
                LeafMethod();
                _tracer.StopTrace();
            }

            public void LeafMethod()
            {
                _tracer.StartTrace();
                Thread.Sleep(10);
                _tracer.StopTrace();
            }
        }
    }
}
