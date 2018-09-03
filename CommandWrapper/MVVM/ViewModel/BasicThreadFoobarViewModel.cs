

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;

namespace CommandWrapper.MVVM.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using CommandWrapper.MVVM.Model;
    using GalaSoft.MvvmLight;

    public class BasicThreadFoobarViewModel: FoobarViewModelBase
    {
        protected static BasicThreadFoobarViewModel _mainfb;

        public BasicThreadFoobarViewModel() : base()
        {
            if (_mainfb == null) _mainfb = this;
        }

        #region multithread
        [FoobarMethod]
        public void LinqType()
        {
            string [] words = { "apple", "strawberry", "grape", "peach", "banana" };
            var wordQuery = from word in words
                where word[0] == 'g'
                select word;
            foreach (string s in wordQuery)
            {
                WriteLine(s);
            }
        }

        [FoobarMethod]
        public void StartPrintNumbers()
        {
            Thread t = new Thread(PrintNumbers);
            t.Start();
            PrintNumbers();
        }

        [FoobarMethod]
        public void StartPrintNumbersWithDelay()
        {
            Thread t = new Thread(PrintNumbersWithDelay);
            t.Start();
            PrintNumbers();
        }

        [FoobarMethod]
        public async void TestPrintNumbersWithStatus()
        {
            Thread t = new Thread(PrintNumbersWithStatus);
            Thread t2 = new Thread(DoNothing);
            t.Start();
            t2.Start();
            for (int i = 1; i < 30; i++)
            {
                WriteLine(t.ThreadState.ToString());
            }
            Thread.Sleep(TimeSpan.FromSeconds(6));
            t.Abort();
            WriteLine("A thread has been aborted");
            WriteLine(t.ThreadState.ToString());
            WriteLine(t2.ThreadState.ToString());
        }

        [FoobarMethod]
        public void PrioritiesCounter()
        {
            Thread t = new Thread(RunThreads);
            t.Start();
        }

        [FoobarMethod]
        public void TestCounters()
        {
            WriteLine("Incorrect counter");
            var c = new Counter();
            var t1 = new Thread(() => TestCounter(c));
            var t2 = new Thread(() => TestCounter(c));
            var t3 = new Thread(() => TestCounter(c));
            t1.Start();
            t2.Start();
            t3.Start();
            t1.Join();
            t2.Join();
            t3.Join();

            WriteLine($"Total count: {c.Count}");
            WriteLine("---------------------------------------------");
        }

        [FoobarMethod]
        public void MutexInstances()
        {
            Thread t = new Thread(MutexMain);
            t.Start();
        }

        [FoobarMethod]
        public void AutoResetEventTest()
        {
            Thread t = new Thread(AutoResetEventMain);
            t.Start();
        }
        
        [FoobarMethod]
        public void BarrierTest()
        {
            var t1 = new Thread(() => PlayMusic("the guitarist", "Play a amazing solo", 3));
            var t2 = new Thread(() => PlayMusic("the singer", "sing his song", 2));
            t1.Start();
            t2.Start();
        }

        [FoobarMethod]
        public void ThreadPoolTester()
        {
            var mthread = new Thread(() =>
            {
                int threadId = 0;
                RunOnThreadPool poolDelegate = Test;
                var t = new Thread(() => Test(out threadId));
                t.Start();
                t.Join();
                WriteLine($"Thread id : {threadId}");
                var r = poolDelegate.BeginInvoke(out threadId, Callback, "a delegate asynchronous call");
                r.AsyncWaitHandle.WaitOne();
                string result = poolDelegate.EndInvoke(out threadId, r);
                WriteLine($"Thread pool worker thread id: {threadId}");
                WriteLine(result);
            });
            mthread.Start();
        }

        [FoobarMethod]
        public void AddAsyncOperation()
        {
            new Thread(() =>
            {
                const int x = 1;
                const int y = 2;
                const string lambdaState = "lambda state 2";
                ThreadPool.QueueUserWorkItem(AsyncOperation);
                Thread.Sleep(TimeSpan.FromSeconds(1));
                ThreadPool.QueueUserWorkItem(AsyncOperation, "async state");
                Thread.Sleep(TimeSpan.FromSeconds(1));
                ThreadPool.QueueUserWorkItem(state =>
                {
                    WriteLine($"Operation state: {state}");
                    WriteLine($"Worker thread id: {Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }, "lambda state");

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    WriteLine($"Operation state : {x + y}, {lambdaState}");
                    WriteLine($"Worker thread id : {Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }, "lambiowej state");

                Thread.Sleep(TimeSpan.FromSeconds(2));
            }).Start();
        }
        #endregion

        #region helpers

        private static void AsyncOperation(object state)
        {
            _mainfb.WriteLine($"Operation state: {state ?? "(null)"}");
            _mainfb.WriteLine($"Worker thread id: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        public void DoNothing()
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        public void PrintNumbersWithStatus()
        {
            WriteLine("Starting...");
            WriteLine(Thread.CurrentThread.ThreadState.ToString());
            for (int i = 1; i < 10; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
                WriteLine(i);
            }
        }

        public void PrintNumbersWithDelay()
        {
            WriteLine("Starting...");
            for (int i = 1; i < 10; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
                WriteLine(i);
            }
        }

        public void PrintNumbers()
        {
            WriteLine("Starting...");
            for (int i = 1; i < 10; i++)
            {
                WriteLine(i);
            }
        }

        static void RunThreads()
        {
            var sample = new ThreadSample();
            var threadOne = new Thread(sample.CountNumbers);
            threadOne.Name = "ThreadOne";
            var threadTwo = new Thread(sample.CountNumbers);
            threadTwo.Name = "ThreadTwo";
            threadOne.Priority = ThreadPriority.Highest;
            threadTwo.Priority = ThreadPriority.Lowest;
            threadOne.Start();
            threadTwo.Start();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            sample.Stop();
        }

        class ThreadSample
        {
            private bool _isStopped = false;

            public void Stop()
            {
                _isStopped = true;
            }

            public void CountNumbers()
            {
                long counter = 0;
                while (!_isStopped)
                {
                    counter++;
                }

                _mainfb.WriteLine($"{Thread.CurrentThread.Name} with {Thread.CurrentThread.Priority,11} priority has a count = {counter,13:N0}");
            }
        }

        private void RunThreadMain()
        {
            WriteLine($"Current thread priority: {Thread.CurrentThread.Priority}");
            WriteLine("Running on all cores available");
            RunThreads();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            WriteLine($"Running on single core");
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
            RunThreads();
        }

        private static void TestCounter(CounterBase c)
        {
            for (int i = 0; i < 10000000; i++)
            {
                c.Increment();
                c.Decrement();
            }
        }

        class Counter : CounterBase
        {
            public int _count = 0;

            public int Count
            {
                get => _count;
            }

            public override void Increment()
            {
                Interlocked.Increment(ref _count);
            }

            public override void Decrement()
            {
                Interlocked.Decrement(ref _count);
            }
        }

        class CounterWithLock : CounterBase
        {
            private readonly object _syncRoot = new Object();

            public int Count { get; private set; }

            public override void Increment()
            {
                lock (_syncRoot)
                {
                    Count++;
                }
            }

            public override void Decrement()
            {
                lock (_syncRoot)
                {
                    Count--;
                }
            }
        }

        private abstract class CounterBase
        {
            public abstract void Increment();

            public abstract void Decrement();
        }

        private void MutexMain()
        {
            const string MutexName = "CSharpThreadingCookbook";

            using (var m = new Mutex(false, MutexName))
            {
                if (!m.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    WriteLine("Second instance is running");
                }
                else
                {
                    WriteLine("Running!");
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    m.ReleaseMutex();
                }
            }
        }

        private static AutoResetEvent _workerEvent = new AutoResetEvent(false);

        private static AutoResetEvent _mainEvent = new AutoResetEvent(false);

        private void AutoResetEventWorker(int seconds)
        {
            WriteLine("Starting a long running work ...");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            WriteLine("Work is Done!");
            _workerEvent.Set();
            WriteLine("Waiting for a main thread to complete its work");
            _mainEvent.WaitOne();
            WriteLine("Starting second operation...");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            WriteLine("Work is done!");
            _workerEvent.Set();
        }

        public void AutoResetEventMain()
        {
            var t = new Thread(() => AutoResetEventWorker(10));
            t.Start();
            WriteLine("Waiting for another thread to complete work");
            _workerEvent.WaitOne();
            WriteLine("First operation is completed!");
            WriteLine("Performing an operations on a main thread");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            _mainEvent.Set();
            WriteLine("Now running the second operation on a second thread");
            _workerEvent.WaitOne();
            WriteLine("Second operation is completed!");

        }

        private Barrier _barrier = new Barrier(6, b => _mainfb.WriteLine($"End of phase {b.CurrentPhaseNumber + 1}"));

        void PlayMusic(string name, string message, int seconds)
        {
            for (int i = 1; i < 3; i++)
            {
                WriteLine("-------------------------------");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                WriteLine($"{name} starts to {message}");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                WriteLine($"{name} finishes to {message}");
                _barrier.SignalAndWait();
            }
        }

        private delegate string RunOnThreadPool(out int threadId);

        private void Callback(IAsyncResult ar)
        {
            WriteLine("Starting a callback");
            WriteLine($"State Passed to a callback: {ar.AsyncState}");
            WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            WriteLine($"Thread pool worker thread id: {Thread.CurrentThread.ManagedThreadId}");
        }

        private string Test(out int threadId)
        {
            WriteLine("Starting");
            WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            threadId = Thread.CurrentThread.ManagedThreadId;
            return $"Thread pool worker thread id was: {threadId}";
        }
        #endregion

        #region efficient cs

        #endregion
    }

    public class UsefulValues
    {
        public static readonly int StartValue = 5;
        public const int EndValue = 10;
    }
}
