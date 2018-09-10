using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents.Serialization;
using System.Windows.Media.Media3D;
using static System.Threading.Thread;

namespace CommandWrapper.MVVM.ViewModel
{
    public class AsyncKeywordFoobarViewModel : FoobarViewModelBase
    {
        [FoobarMethod]
        public async void AwaitOperator()
        {
            Task t = AsynchronyWithAwait();
            t.Wait();
            t = AsynchronyWithTPL();
            t.Wait();
        }

        [FoobarMethod]
        public async void AwaitLambda()
        {
            Task t = AsynchronousProcessing();
            while (!t.IsCompleted)
            {
                WriteLine(t.Status);
                Sleep(TimeSpan.FromSeconds(0.5));
            }
            WriteLine(t.Status);
            t.Wait();
        }

        [FoobarMethod]
        public async Task AsynchronySequential()
        {
            Task t = AsynchronyWithTPLSequantial();
            await t;
            t = AsynchronyWithAwaitSequential();
            await t;
        }

        [FoobarMethod]
        public void AsynchronyParallel()
        {
            Task t = AsynchronousProcessingParallel();
            WriteLine(t.Status);
        }

        [FoobarMethod]
        public void AsynchronousException()
        {
            Task t = AsynchronousExceptionProcessing();
        }

        async Task AsynchronousExceptionProcessing()
        {
            WriteLine("1. Single exception");
            try
            {
                string result = await GetExceptionAsync("Task 1", 2);
                WriteLine(result);
            }
            catch (Exception e)
            {
                WriteLine($"Exception details: {e}");
            }
            WriteLine();
            WriteLine("2. Multiple exceptions");
            Task<string> t1 = GetExceptionAsync("Task 1", 3);
            Task<string> t2 = GetExceptionAsync("Task 2", 2);
            try
            {
                string [] results = await Task.WhenAll(t1, t2);
                WriteLine(results.Length);
            }
            catch (Exception e)
            {
                WriteLine($"Exception details: {e}");
            }

            WriteLine();
            WriteLine("3. Multiple exceptions with AggregateException");
            t1 = GetExceptionAsync("Task 1", 3);
            t2 = GetExceptionAsync("Task 2", 2);
            Task<string[]> t3 = Task.WhenAll(t1, t2);
            try
            {
                string[] results = await t3;
                WriteLine(results.Length);
            }
            catch
            {
                var ae = t3.Exception.Flatten();
                var exceptions = ae.InnerExceptions;
                WriteLine($"Exception caught: {exceptions.Count}");
                foreach (var exception in exceptions)
                {
                    WriteLine($"Exception details: {exception}");
                    WriteLine();
                }
            }
            WriteLine();
            WriteLine("4. await in catch and finally blocks");
            try
            {
                String result = await GetExceptionAsync("Task 1", 2);
                WriteLine(result);
            }
            catch (Exception e)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                WriteLine($"Catch block with await: Exception details: {e}");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                WriteLine("Finally block");
            }
        }

        async Task<string> GetExceptionAsync(string name, int seconds)
        {
            WriteLine($"Task {name} Started");
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            throw new Exception($"Boom from {name}");
        }

        async Task AsynchronousProcessingParallel()
        {
            Task<string> t1 = GetNothingAsync("Task 1", 3);
            Task<string> t2 = GetNothingAsync("Task 2", 5);
            string[] results = await Task.WhenAll(t1, t2);
            foreach (var result in results)
            {
                WriteLine(result);
            }
        }

        async Task<string> GetNothingAsync(string name, int seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            return $"Task {name} is running on thread id {CurrentThread.ManagedThreadId}";
        }

        Task AsynchronyWithTPLSequantial()
        {
            var containerTask = new Task(() =>
            {
                Task<string> t = GetInfoAsync("TPL 1");
                t.ContinueWith(task =>
                {
                    WriteLine(t.Result);
                    Task<string> t2 = GetInfoAsync("TPL 2");
                    t2.ContinueWith(innerTask => WriteLine(innerTask.Result), TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.AttachedToParent);
                    t2.ContinueWith(innerTask => WriteLine(innerTask.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.AttachedToParent);
                }, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.AttachedToParent);
                t.ContinueWith(task => WriteLine(t.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.AttachedToParent);
            });
            containerTask.Start();
            return containerTask;
        }


        async Task AsynchronyWithAwaitSequential()
        {
            try
            {
                string result = await GetInfoAsync("Async 1");
                WriteLine(result);
                result = await GetInfoAsync("Async 2");
                WriteLine(result);
            }
            catch (Exception e)
            {
                WriteLine(e);
            }
        }

        async Task AsynchronousProcessing()
        {
            Func<string, Task<string>> asyncLambda = async name =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                return $"Task {name} is running on a thread id {CurrentThread.ManagedThreadId}." +
                       $"Is thread pool thread: {CurrentThread.IsThreadPoolThread}";
            };
            string result = await asyncLambda("async lambda");
            WriteLine(result);
        }

        Task AsynchronyWithTPL()
        {
            Task<string> t = GetInfoAsync("Task 1");
            Task t2 = t.ContinueWith(task => WriteLine(t.Result), TaskContinuationOptions.NotOnFaulted);
            Task t3 = t.ContinueWith(task => WriteLine(t.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted);
            return Task.WhenAny(t2, t3);
        }

        async Task AsynchronyWithAwait()
        {
            try
            {
                string result = await GetInfoAsync("Task 2");
                WriteLine(result);
            }
            catch (Exception e)
            {
                WriteLine(e);
            }
        }

        async Task<string> GetInfoAsync(string name)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            return $"Task {name} is running on a thread id {CurrentThread.ManagedThreadId}." +
                   $" Is thread pool thread: {CurrentThread.IsThreadPoolThread}";
        }
    }
}
