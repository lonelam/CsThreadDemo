/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/


using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows.Documents;
using GalaSoft.MvvmLight;

namespace CommandWrapper.MVVM.ViewModel
{
    using GalaSoft.MvvmLight.Ioc;
    using CommonServiceLocator;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<BasicThreadFoobarViewModel>();
            SimpleIoc.Default.Register<PoolFoobarViewModel>();
            SimpleIoc.Default.Register<GoTapFoobarViewModel>();
            TabViewModels.Add(GoTapFoobar);
            TabViewModels.Add(PoolFoobar);
            TabViewModels.Add(BasicThreadFoobar);
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public GoTapFoobarViewModel GoTapFoobar => ServiceLocator.Current.GetInstance<GoTapFoobarViewModel>();

        public PoolFoobarViewModel PoolFoobar
        {
            get { return ServiceLocator.Current.GetInstance<PoolFoobarViewModel>(); }
        }

        public BasicThreadFoobarViewModel BasicThreadFoobar
        {
            get { return ServiceLocator.Current.GetInstance<BasicThreadFoobarViewModel>(); }
        }

        public List<ViewModelBase> TabViewModels { get; set; } = new List<ViewModelBase>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}