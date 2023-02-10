using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;

public class CmdWatchdogService : ServiceBase
{
    private Timer timer = null;
    private Process cmdProcess = null;

    public CmdWatchdogService()
    {
        this.ServiceName = "Cmd Watchdog Service";
    }

    // The OnStart method is called when the service is started
    protected override void OnStart(string[] args)
    {
        // Initialize a Timer object
        timer = new Timer();

        // Set the timer interval to 10 seconds
        timer.Interval = 10000;

        // Attach an event handler to the Timer's Elapsed event
        timer.Elapsed += Timer_Elapsed;

        // Start the timer
        timer.Start();
    }

    // The Timer_Elapsed event handler is called every time the timer interval elapses
    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            // Check if the cmd.exe process is running
            if (cmdProcess == null || cmdProcess.HasExited)
            {
                // If the cmd.exe process is not running, start it
                cmdProcess = Process.Start("cmd.exe");
            }
        }
        catch (Exception ex)
        {
            // Log the error if an exception is thrown while starting the cmd.exe process
        }
    }

    // The OnStop method is called when the service is stopped
    protected override void OnStop()
    {
        // Stop the timer
        timer.Stop();

        // Kill the cmd.exe process if it's still running
        if (cmdProcess != null && !cmdProcess.HasExited)
        {
            cmdProcess.Kill();
        }
    }

    public static void Main(string[] args)
    {
        // Check if the program is running as a service or as a standalone application
        if (Environment.UserInteractive)
        {
            // If the program is running as a standalone application, install the service
            Console.WriteLine("Installing service...");
            ManagedInstallerClass.InstallHelper(new string[] { "/i", System.Reflection.Assembly.GetExecutingAssembly().Location });
            Console.WriteLine("Service installed successfully");

            // Start the service after installation
            ServiceController sc = new ServiceController("Cmd Watchdog Service");
            sc.Start();
            Console.WriteLine("Service started successfully");
        }
        else
        {
            // If the program is running as a service, start the service
            ServiceBase.Run(new CmdWatchdogService());
        }
    }
}
