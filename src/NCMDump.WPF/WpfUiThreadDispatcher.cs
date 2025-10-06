using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NCMDump.WPF
{
    public class WpfUiThreadDispatcher : IUiThreadDispatcher
    {
        private readonly Dispatcher _dispatcher;

        public WpfUiThreadDispatcher(Dispatcher dispatcher) =>
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

        public Task InvokeAsync(Action action) =>
            _dispatcher.InvokeAsync(action).Task;
    }
}