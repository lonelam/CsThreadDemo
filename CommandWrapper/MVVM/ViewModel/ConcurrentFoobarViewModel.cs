using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandWrapper.MVVM.ViewModel
{
    public class ConcurrentFoobarViewModel : FoobarViewModelBase
    {
        private readonly string Item = "Dictionary item";
        private const int Iterations = 100000;
        public string CurrentItem;

        [FoobarMethod]
        public void ConcurrentDictUsage()
        {
            var concurrentDictionary = new ConcurrentDictionary<int, string>();
            var dictionary = new Dictionary<int, string>();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < Iterations; i++)
            {
                lock (dictionary)
                {
                    dictionary[i] = Item;
                }
            }

            sw.Stop();
            WriteLine($"Writing to dictionary with a lock : {sw.Elapsed}");
            sw.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                concurrentDictionary[i] = Item;
            }
            sw.Stop();
            WriteLine($"Writing to a concurrent dictionary: {sw.Elapsed}");
            sw.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                lock (dictionary)
                {
                    CurrentItem = dictionary[i];
                }
            }
            sw.Stop();
            WriteLine($"Reading from dictionary with a lock: {sw.Elapsed}");
        }
    }
}
