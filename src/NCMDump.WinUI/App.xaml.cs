using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using NCMDump.Core;
using NCMDump.WinUI.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCMDump.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            MainWindow.Activate();

            AppNotificationManager notificationManager = AppNotificationManager.Default;
            notificationManager.NotificationInvoked += NotificationManager_NotificationInvoked;
            notificationManager.Register();

            var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            var activationKind = activatedArgs.Kind;
            if (activationKind != ExtendedActivationKind.AppNotification)
            {
                LaunchAndBringToForegroundIfNeeded();
            }
            else
            {
                HandleNotification((AppNotificationActivatedEventArgs)activatedArgs.Data);
            }
        }

        public Window MainWindow;

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<NCMDumper>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
        }

        public static TEnum GetEnum<TEnum>(string text) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
            {
                throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
            }
            return (TEnum)Enum.Parse(typeof(TEnum), text);
        }

        private void NotificationManager_NotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            HandleNotification(args);
        }

        private void LaunchAndBringToForegroundIfNeeded()
        {
            if (MainWindow == null)
            {
                MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                MainWindow.Activate();

                // Additionally we show using our helper, since if activated via a app notification, it doesn't
                // activate the window correctly
                WindowHelper.ShowWindow(MainWindow);
            }
            else
            {
                WindowHelper.ShowWindow(MainWindow);
            }
        }

        private void HandleNotification(AppNotificationActivatedEventArgs args)
        {
            // Use the dispatcher from the window if present, otherwise the app dispatcher
            var dispatcherQueue = MainWindow?.DispatcherQueue ?? DispatcherQueue.GetForCurrentThread();

            dispatcherQueue.TryEnqueue(async delegate
            {
                var arguments = args.Arguments;
                switch (args.Arguments["action"])
                {
                    // Send a background message
                    case "clicked":
                        Debug.WriteLine("Clicked");
                        // TODO: Send it

                        // If the UI app isn't open
                        if (MainWindow == null)
                        {
                            // Close since we're done
                            Process.GetCurrentProcess().Kill();
                        }

                        break;

                    // View a message
                    case "viewMessage":

                        // Launch/bring window to foreground
                        LaunchAndBringToForegroundIfNeeded();

                        // TODO: Open the message
                        break;
                }
            });
        }
    }

    internal static class WindowHelper
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void ShowWindow(Window window)
        {
            // Bring the window to the foreground... first get the window handle...
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Restore window if minimized... requires DLL import above
            ShowWindow(hwnd, 0x00000009);

            // And call SetForegroundWindow... requires DLL import above
            SetForegroundWindow(hwnd);
        }
    }
}