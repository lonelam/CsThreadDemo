using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommandWrapper.MVVM.ViewModel
{
    public class PoolFoobarViewModel : FoobarViewModelBase
    {
        [FoobarMethod]
        public void UseThreadPoolMain()
        {
            const int numberOfOperations = 500;
            var sw = new Stopwatch();
            sw.Start();
            UseThreads(numberOfOperations);
            sw.Stop();
            WriteLine($"Execution time using threads: {sw.ElapsedMilliseconds}");

            sw.Reset();
            sw.Start();
        }

        public void UseThreads(int numberOfOperations)
        {
            using (var countdown = new CountdownEvent(numberOfOperations))
            {
                WriteLine("Scheduling work by creating threads");
                for (int i = 0; i < numberOfOperations; i++)
                {
                    var thread = new Thread(() =>
                    {
                        Write($"{Thread.CurrentThread.ManagedThreadId}, ");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countdown.Signal();
                    });
                    thread.Start();
                }

                countdown.Wait();
                WriteLine();
            }
        }

        void UseThreadPool(int numberOfOperations)
        {
            using (var countdown = new CountdownEvent(numberOfOperations))
            {
                WriteLine("Starting work on a thread pool");
                for (int i = 0; i < numberOfOperations; i++)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        Write($"{Thread.CurrentThread.ManagedThreadId}, ");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countdown.Signal();
                    });
                }

                countdown.Wait();
                WriteLine();
            }
        }
    }
}
