
using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;

namespace CommandWrapper.MVVM.Model
{
    public class FoobarRunner : ObservableObject
    {
        private int _callCount = 0;

        public int CallCount
        {
            get => this._callCount;
            set { this.Set(() => this.CallCount, ref this._callCount, value); }
        }

        private string _functionName;

        public string FunctionName
        {
            get => this._functionName;
            set { this.Set(() => this.FunctionName, ref this._functionName, value); }
        }

        private RelayCommand _relatedCmd;

        public RelayCommand RelatedCmd
        {
            get => this._relatedCmd;
            set { this.Set(() => this.RelatedCmd, ref this._relatedCmd, value); }
        }

        public FoobarRunner(string name, Action solveAction)
        {
            FunctionName = name;
            CallCount = 0;
            RelatedCmd = new RelayCommand(() =>
            {
                solveAction.Invoke();
                CallCount += 1;
            }, true);
        }
    }
}
