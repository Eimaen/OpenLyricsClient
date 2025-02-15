using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Threading;
using OpenLyricsClient.Backend;
using Xilium.CefGlue.Common;

namespace OpenLyricsClient
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .With(new Win32PlatformOptions
                {
                    UseWindowsUIComposition = false
                })
                .AfterSetup(t =>
                {
                    CefRuntimeLoader.Initialize();
                    
                    new Core();
                })
                .UseReactiveUI();
    }
}
