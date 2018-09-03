using System.Windows;

namespace CommandWrapper.MVVM.View
{
    using System.Windows.Controls;
    using CommandWrapper.MVVM.ViewModel;

    /// <summary>
    /// Interaction logic for FoobarControl.xaml
    /// </summary>
    public partial class FoobarControl : UserControl
    {
        public FoobarViewModelBase Context
        {
            get
            {
                return DataContext as FoobarViewModelBase;
            }
        }

        public FoobarControl()
        {
            InitializeComponent();
        }

        private void FoobarControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            Context.PropertyChanged += (o, args) =>
            {
                if (args.PropertyName == nameof(FoobarViewModelBase.ConsoleText))
                {
                    ConsoleViewer.ScrollToEnd();
                }
            };
        }
    }
}
