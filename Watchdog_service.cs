using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;

[RunInstaller(true)]
public class CmdWatchdogServiceInstaller : Installer
{
    public CmdWatchdogServiceInstaller()
    {
        ServiceProcessInstaller processInstaller = new ServiceProcessInstaller();
        ServiceInstaller serviceInstaller = new ServiceInstaller();

        // Service will run under system account
        processInstaller.Account = ServiceAccount.LocalSystem;

        // Service will have Start Type of Manual
        serviceInstaller.StartType = ServiceStartMode.Manual;

        serviceInstaller.ServiceName = "Cmd Watchdog Service";
        serviceInstaller.DisplayName = "Cmd Watchdog Service";
        serviceInstaller.Description = "This service monitors the cmd.exe process and restarts it if it closes";

        Installers.Add(processInstaller);
        Installers.Add(serviceInstaller);
    }
}

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
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { System.Reflection.Assembly.GetExecutingAssembly().Location });
                Console.WriteLine("Service installed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error installing service: " + ex.Message);
            }

            // Start the service after it has been installed
            ServiceController serviceController = new ServiceController("Cmd Watchdog Service");
            serviceController.Start();
            Console.WriteLine("Service started successfully");
        }
        else
        {
            // If the program is running as a service, run the service using the ServiceBase class
            ServiceBase.Run(new CmdWatchdogService());
        }
    }
}
