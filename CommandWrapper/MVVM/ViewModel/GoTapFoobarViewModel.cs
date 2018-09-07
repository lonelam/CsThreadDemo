using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace CommandWrapper.MVVM.ViewModel
{
    public class GoTapFoobarViewModel : FoobarViewModelBase
    {
        [FoobarMethod]
        public void ApmToTap()
        {
            new Thread(() =>
            {
                int threadId;
                AsynchronousTask d = Test;
                IncompatibleAsynchronousTask e = Test;
                WriteLine("Option 1");
                Task<string> task = Task<string>.Factory.FromAsync(d.BeginInvoke("AsyncTaskThread", Callback, "a delegate asynchronous call"), d.EndInvoke);
                task.ContinueWith(t => WriteLine($"Callback is finished now running a continuation! Result: {t.Result}"));
                while (!task.IsCompleted)
                {
                    WriteLine(task.Status);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
                WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(1));
                WriteLine("-------------------");
                WriteLine();
                WriteLine("Option 2");
                task = Task<string>.Factory.FromAsync(d.BeginInvoke, d.EndInvoke, "AsyncTaskThread", "a delegate asynchronout call");
                task.ContinueWith(t => WriteLine($"Task is Completed, now running a continuation! Result: {t.Result}"));
                while (!task.IsCompleted)
                {
                    WriteLine(task.Status);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }

                WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(1));

                WriteLine("------------------------");
                WriteLine();
                WriteLine("Option 3");

                IAsyncResult ar = e.BeginInvoke(out threadId, Callback, "a delegate asynchronous call");
                task = Task<string>.Factory.FromAsync(ar, _ => e.EndInvoke(out threadId, ar));

                task.ContinueWith(t => WriteLine($"Task is completed, now running a continuation! Result: {t.Result}, Thread Id: {threadId}"));

                while (!task.IsCompleted)
                {
                    WriteLine(task.Status);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
                WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(1));

            }).Start();
        }

        delegate string AsynchronousTask(string threadName);

        delegate string IncompatibleAsynchronousTask(out int threadId);

        public void Callback(IAsyncResult ar)
        {
            WriteLine("Starting a callback...");
            WriteLine($"State passed to 2 callbak: {ar.AsyncState}");
            WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            WriteLine($"Thread pool worker thread id {Thread.CurrentThread.ManagedThreadId}");
        }

        public string Test(string threadName)
        {
            WriteLine("Starting...");
            WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Thread.CurrentThread.Name = threadName;
            return $"Thread Name: {Thread.CurrentThread.Name}";
        }

        public string Test(out int threadId)
        {
            WriteLine("Starting...");
            WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            threadId = Thread.CurrentThread.ManagedThreadId;
            return $"Thread pool worker thread id was: {threadId}";
        }

        //4.6 将EAP转换为任务
        [FoobarMethod]
        public async void EapToTap()
        {
            var tcs = new TaskCompletionSource<int>();
            var worker = new BackgroundWorker();
            worker.DoWork += (sender, eventArgs) => { eventArgs.Result = TaskMethod("Background worker", 5); };

            worker.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                {
                    tcs.SetException(args.Error);
                }
                else if (args.Cancelled)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    tcs.SetResult((int) args.Result);
                }
            };
            worker.RunWorkerAsync();
            while (!tcs.Task.IsCompleted)
            {
                WriteLine(tcs.Task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            WriteLine(tcs.Task.Status);
            int result = tcs.Task.Result;
            WriteLine($"Result is : {result}");
        }
        
        int TaskMethod(string name, int seconds)
        {
            WriteLine($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            return seconds * 42;
        }
    }
}
