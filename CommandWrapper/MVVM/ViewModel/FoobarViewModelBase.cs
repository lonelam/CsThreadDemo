
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using CommandWrapper.MVVM.Model;
using GalaSoft.MvvmLight.Threading;

namespace CommandWrapper.MVVM.ViewModel
{
    using GalaSoft.MvvmLight;

    public class FoobarViewModelBase : ViewModelBase
    {
        #region util
        public string ChapName
        {
            get
            {
                var name = GetType().Name;
                return name.Length > 15 ? name.Substring(0, name.Length - 15) : name;
            }
        }
        

        private string _consoleText = string.Empty;

        public string ConsoleText
        {
            get => this._consoleText;
            set { this.Set(() => this.ConsoleText, ref this._consoleText, value); }
        }

        private ObservableCollection<FoobarRunner> _cmdList = new ObservableCollection<FoobarRunner>();

        public ObservableCollection<FoobarRunner> CmdList
        {
            get => this._cmdList;
            set { this.Set(() => this.CmdList, ref this._cmdList, value); }
        }

        public void WriteLine()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ConsoleText += "\n";
            });
        }

        public void WriteLine(object str)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ConsoleText += str.ToString();
                ConsoleText += "\n";
            });
        }

        public void Write(object str)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ConsoleText += str.ToString();
            });
        }

        public void InitMethods()
        {
            var type = GetType();
            var methods = from method in type.GetMethods()
                where method.GetCustomAttributes(typeof(FoobarMethodAttribute), false).Length > 0
                select method;
            foreach (var method in methods)
            {
                if (method.GetCustomAttributes(typeof(AsyncStateMachineAttribute), false).Length > 0 || method.GetCustomAttributes(typeof(AsyncFoobarMethodAttribute), false).Length > 0)
                {
                    WriteLine($"{method.Name} is async method");
                    CmdList.Add(new FoobarRunner(method.Name, () =>
                    {
                        Thread t = new Thread(() => method.Invoke(this, null));
                        t.Start();
                    }));
                }
                else
                {
                    CmdList.Add(new FoobarRunner(method.Name, () => method.Invoke(this, null)));
                }
            }

            WriteLine("viewmodel initialized\n");
        }

        public FoobarViewModelBase()
        {
            InitMethods();
        }
        #endregion
    }
}
