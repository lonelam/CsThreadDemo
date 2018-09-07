using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CommandWrapper.MVVM.ViewModel
{
    public class PoolFoobarViewModel : FoobarViewModelBase
    {
        [FoobarMethod]
        public void UseThreadPoolMain()
        {
            new Thread(() =>
            {
                const int numberOfOperations = 5000;
                var sw = new Stopwatch();
                sw.Start();
                UseThreads(numberOfOperations);
                sw.Stop();
                long threadTime = sw.ElapsedMilliseconds;
                WriteLine($"Execution time using threads: {sw.ElapsedMilliseconds}");

                sw.Reset();
                sw.Start();
                UseThreads(numberOfOperations);
                sw.Stop();
                WriteLine($"Execution time using thread pool: {sw.ElapsedMilliseconds}");
                WriteLine("-------------summary-----------");
                WriteLine($"Execution time using threads: {threadTime}");
                WriteLine($"Execution time using thread pool: {sw.ElapsedMilliseconds}");
            }).Start();
            
        }

        [FoobarMethod]
        public void CancellationTokenTest()
        {
            new Thread(() =>
            {
                using (var cts = new CancellationTokenSource())
                {
                    CancellationToken token = cts.Token;
                    ThreadPool.QueueUserWorkItem(_ => AsyncOperation1(token));
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    cts.Cancel();
                }
                using (var cts = new CancellationTokenSource())
                {
                    CancellationToken token = cts.Token;
                    ThreadPool.QueueUserWorkItem(_ => AsyncOperation2(token));
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    cts.Cancel();
                }
                using (var cts = new CancellationTokenSource())
                {
                    CancellationToken token = cts.Token;
                    ThreadPool.QueueUserWorkItem(_ => AsyncOperation3(token));
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    cts.Cancel();
                }
            }).Start();
        }

        [FoobarMethod]
        public void RunPoolWorkerOperation()
        {
            new Thread(() =>
            {
                RunOperations(TimeSpan.FromSeconds(5));
                RunOperations(TimeSpan.FromSeconds(7));
            }).Start();
        }

        [FoobarMethod]
        public void BackgroundWorkerTest()
        {
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += Worker_DoWork;
            bw.ProgressChanged += Worker_ProgressChanged;
            bw.RunWorkerCompleted += Worker_Completed;
            bw.RunWorkerAsync();
        }

        [FoobarMethod]
        public void TaskTester()
        {
            var t1 = new Task(()=> TaskMethod("Task 1"));
            var t2 = new Task(()=> TaskMethod("Task 2"));
            t2.Start();
            t1.Start();
            Task.Run(() => TaskMethod("Task 3"));
            Task.Factory.StartNew(() => TaskMethod("Task 4"));
            Task.Factory.StartNew(() => TaskMethod("Task 5"), TaskCreationOptions.LongRunning);
        }

        [FoobarMethod]
        public void Calculate42()
        {
            new Thread(() =>
            {
                IntTaskMethod("Main Thread Task");
                Task<int> task = CreateTask("Task 1");
                task.Start();
                int result = task.Result;
                WriteLine($"Result is: {result}");
                task = CreateTask("Task 2");
                task.RunSynchronously();
                result = task.Result;
                WriteLine($"Result is: {result}");
                task = CreateTask("Task 3");

                WriteLine(task.Status);
                task.Start();

                while (!task.IsCompleted)
                {
                    WriteLine(task.Status);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }

                WriteLine(task.Status);
                result = task.Result;
                WriteLine($"Result is: {result}");
            }).Start();
        }

        [FoobarMethod]
        public void MulCalculation()
        {
            new Thread(() =>
            {
                var firstTask = new Task<int>(() => MulTaskMethod("First Task", 3));
                var secondTask = new Task<int>(() => MulTaskMethod("Second Task", 2));
                firstTask.ContinueWith(t => WriteLine($"The first answer is {t.Result}. Thread id: {Thread.CurrentThread.ManagedThreadId}, is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}"),
                    TaskContinuationOptions.OnlyOnRanToCompletion);
                firstTask.Start();
                secondTask.Start();
                Thread.Sleep(TimeSpan.FromSeconds(10));
                Task continuation = secondTask.ContinueWith(
                    t => WriteLine($"The second answer is {t.Result}. Thread id: {Thread.CurrentThread.ManagedThreadId}, is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}."),
                    TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
                continuation.GetAwaiter().OnCompleted(
                    () => WriteLine($"Continuation Task Completed! Thread id {Thread.CurrentThread.ManagedThreadId}, is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}"));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                WriteLine();
                firstTask = new Task<int>(() =>
                {
                    var innerTask = Task.Factory.StartNew(() => MulTaskMethod("Second Task", 5), TaskCreationOptions.AttachedToParent);
                    innerTask.ContinueWith(t => MulTaskMethod("third Task", 2), TaskContinuationOptions.AttachedToParent);
                    return MulTaskMethod("First Task", 2);
                });
                firstTask.Start();
                while (!firstTask.IsCompleted)
                {
                    WriteLine(firstTask.Status);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
                WriteLine(firstTask.Status);
            }).Start();
        }
        

        private int MulTaskMethod(string name, int seconds)
        {
            WriteLine($"Task {name} is running on a thread id: {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            return 42 * seconds;
        }

        private Task<int> CreateTask(string name)
        {
            return new Task<int>(() => IntTaskMethod(name));
        }

        private int IntTaskMethod(string name)
        {
            WriteLine($"Task {name} is running on a thread id: {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            return 42;
        }

        private void TaskMethod(string name)
        {
            WriteLine($"Task {name} is running on a thread id: {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
        }

        void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WriteLine($"DoWork thread pool thread id: {Thread.CurrentThread.ManagedThreadId}");
            var bw = (BackgroundWorker) sender;
            for (int i = 1; i <= 100; i++)
            {
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                if (i % 10 == 0)
                {
                    bw.ReportProgress(i);
                }

                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }

            e.Result = 42;
        }

        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            WriteLine($"{e.ProgressPercentage} % Completed. " + $"Progress thread pool thread id : {Thread.CurrentThread.ManagedThreadId}");
        }

        void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            WriteLine($"Completed thread pool thread id: {Thread.CurrentThread.ManagedThreadId}");
            if (e.Error != null)
            {
                WriteLine($"Exception {e.Error.Message} has occured. ");
            }
            else if (e.Cancelled)
            {
                WriteLine($"Operation has been canceled. ");
            }
            else
            {
                WriteLine($"The answer is : {e.Result}");
            }
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

        void AsyncOperation1(CancellationToken token)
        {
            WriteLine("Starting the first task");
            for (int i = 0; i < 5; i++)
            {
                if (token.IsCancellationRequested)
                {
                    WriteLine("The first task has been cancelled");
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            WriteLine("The first task has completed successfully");
        }

        void AsyncOperation2(CancellationToken token)
        {
            try
            {
                WriteLine("Starting the second task");
                for (int i = 0; i < 5; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
                WriteLine("The second task has completed successfully");
            }
            catch (OperationCanceledException)
            {
                Write("The second task has been cancelled");
            }
        }

        void AsyncOperation3(CancellationToken token)
        {
            bool cancellationFlag = false;
            token.Register(() => cancellationFlag = true);
            WriteLine("Starting the third task");
            for (int i = 0; i < 5; i++)
            {
                if (cancellationFlag)
                {
                    WriteLine("The third task has been canceled");
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            WriteLine("The third task has completed successfully");
        }

        public void RunOperations(TimeSpan workerOperationTimeOut)
        {
            using (var evt = new ManualResetEvent(false))
            using (var cts = new CancellationTokenSource())
            {
                WriteLine("Registering timeout operation...");
                var worker = ThreadPool.RegisterWaitForSingleObject(evt, (state, isTimeOut) => WorkerOperationWait(cts, isTimeOut), null, workerOperationTimeOut, true);
                WriteLine("Starting long running operation...");
                ThreadPool.QueueUserWorkItem(_ => WorkerOperation(cts.Token, evt));
                Thread.Sleep(workerOperationTimeOut.Add(TimeSpan.FromSeconds(2)));
                worker.Unregister(evt);
            }
        }

        public void WorkerOperation(CancellationToken token, ManualResetEvent evt)
        {
            for (int i = 0; i < 6; i++)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            evt.Set();
        }

        void WorkerOperationWait(CancellationTokenSource cts, bool isTimeOut)
        {
            if (isTimeOut)
            {
                cts.Cancel();
                WriteLine("Worker operation timed out and was canceled.");
            }
            else
            {
                WriteLine("Worker operation succeeded.");
            }
        }
    }
}
