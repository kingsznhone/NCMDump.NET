using System;
using System.Threading.Tasks;

namespace NCMDump.WPF
{
    public interface IUiThreadDispatcher
    {
        Task InvokeAsync(Action action);
    }
}