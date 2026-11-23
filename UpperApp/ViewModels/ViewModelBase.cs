using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ReactiveUI;

namespace UpperApp.ViewModels
{
    internal class ViewModelBase : ReactiveObject
    {
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.RaisePropertyChanged(name);
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
