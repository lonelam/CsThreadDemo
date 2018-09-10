using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using static System.Threading.Thread;

namespace CommandWrapper.MVVM.ViewModel
{
    public class CustomAwaitFoobarViewModel : FoobarViewModelBase
    {
        [FoobarMethod]
        public async void RunAwaitableObj()
        {
            WriteLine("RAn");
            Task t = AsynchronousProcessing();
            t.Wait();
        }

        async Task AsynchronousProcessing()
        {
            var sync = new CustomAwaitable(true);
            string result = await sync;
            WriteLine(result);

            var async = new CustomAwaitable(false);
            result = await async;
            WriteLine(result);
        }

        class CustomAwaitable
        {
            public CustomAwaitable(bool completeSynchronously)
            {
                _completeSynchronously = completeSynchronously;
            }

            public CustomAwaiter GetAwaiter()
            {
                return new CustomAwaiter(_completeSynchronously);
            }

            private readonly bool _completeSynchronously;
        }

        class CustomAwaiter : INotifyCompletion
        {
            private string _result = "Completed synchronously";
            private readonly bool _completeSynchronously;

            public bool IsCompleted => _completeSynchronously;

            public CustomAwaiter(bool completeSynchronously)
            {
                _completeSynchronously = completeSynchronously;
            }

            public string GetResult()
            {
                return _result;
            }

            public void OnCompleted(Action continuation)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    Sleep(TimeSpan.FromSeconds(1));
                    _result = GetInfo();
                    continuation?.Invoke();
                });
            }

            private string GetInfo()
            {
                return $"Task is running on a thread id {CurrentThread.ManagedThreadId}." +
                       $"Is thread Pool thread: {CurrentThread.IsThreadPoolThread}";
            }
        }
    }
}
