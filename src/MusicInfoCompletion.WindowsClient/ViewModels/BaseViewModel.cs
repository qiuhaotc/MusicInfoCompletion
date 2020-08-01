using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using ReactiveUI;

namespace MusicInfoCompletion.WindowsClient
{
    public class BaseViewModel : ReactiveObject, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChange([CallerMemberName] string memberName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }

        public void NotifyPropertyChange(Func<string> propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName.Invoke()));
        }

        public static void InvokeDispatcher(Action action, Dispatcher dispatcher, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
        {
            dispatcher?.BeginInvoke(dispatcherPriority, action);
        }
    }
}
