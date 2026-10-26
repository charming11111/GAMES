using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

// Loaded only after the native entry point confirms .NET Framework 4.8+.
// The actual launcher is executed as an ordinary managed assembly, in the same
// OS process and on the STA thread established by the native CLR host.
public static class BootstrapEntry
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CommandLineToArgvW(string commandLine, out int count);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr memory);

    public static int Start(string commandLine)
    {
        if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            throw new InvalidOperationException("GAMES must start on an STA thread.");

        int count;
        IntPtr nativeArgs = CommandLineToArgvW(commandLine, out count);
        if (nativeArgs == IntPtr.Zero || count == 0)
            throw new InvalidOperationException("Cannot read the launcher command line.");

        string launcher;
        string[] arguments = new string[count - 1];
        try
        {
            launcher = Marshal.PtrToStringUni(Marshal.ReadIntPtr(nativeArgs));
            for (int i = 1; i < count; i++)
                arguments[i - 1] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(nativeArgs, i * IntPtr.Size));
        }
        finally { LocalFree(nativeArgs); }

        string payload = Path.Combine(Path.GetDirectoryName(typeof(BootstrapEntry).Assembly.Location), "GAMES.Core.exe");
        var setup = new AppDomainSetup
        {
            ApplicationBase = Path.GetDirectoryName(Path.GetFullPath(launcher)),
            ApplicationName = "GAMES"
        };
        AppDomain application = AppDomain.CreateDomain("GAMES.Launcher", null, setup);
        // The native entry exits this OS process immediately afterward. Avoid
        // unloading a WPF domain while an asynchronous cover request is in IO.
        return application.ExecuteAssembly(payload, arguments);
    }
}
