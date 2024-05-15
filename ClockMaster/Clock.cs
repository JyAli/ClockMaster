using ClockMaster;
using System.IO.Ports;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

internal class Clock
{
    private static List<SerialPort> Ports = new List<SerialPort>();
    private static Timer ScanTimer;
    private static Timer IDTimer;

    private static List<Slave> Slaves = new List<Slave>();
    private static List<Task> tasks = new List<Task>();

    private static void Main(string[] args)
    {
        InitScanTimer();
        InitIDTimer();
        StartReceivingData();
        Console.ReadLine();
        //Display all port names connected to the device
        /*foreach (string portName in SerialPort.GetPortNames())
        {
            Console.WriteLine("   {0}", portName);
        }


        Port = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
        Port.NewLine = Environment.NewLine;

        Port.Open();
        StartReceivingData();

        string? input = "";

        //Keeps asking user for input and sends the input to the port
        while (input != "End")
        {
            input = Console.ReadLine();
            Port.WriteLine(input);
        }

        Task.WhenAll(tasks);

        Port.Close();*/
    }

    private static void InitScanTimer()
    {
        ScanTimer = new Timer();
        ScanTimer.Elapsed += delegate { ScanPorts(); };
        ScanTimer.Interval = 5000; // in miliseconds
        ScanTimer.AutoReset = true;
        ScanTimer.Start();
    }
    private static void InitIDTimer()
    {
        IDTimer = new Timer();
        IDTimer.Elapsed += delegate { RequestIDs(); };
        IDTimer.Interval = 5000; // in miliseconds
        IDTimer.AutoReset = true;
        IDTimer.Start();
    }

    private static void RequestIDs()
    {
        //Filters out the ports that are slaves already
        SerialPort[] nonSlavePorts = Ports.Where(x => Slaves.All(y => x.PortName != y.Port.PortName)).ToArray();
        foreach (SerialPort port in nonSlavePorts)
        {
            port.WriteLine(Commands.REQUEST_ID_COMMAND);
        }
    }

    private static void ScanPorts()
    {
        string[] systemPortNames = SerialPort.GetPortNames();
        SerialPort[] deadPorts = Ports.Where(x => systemPortNames.All(y => x.PortName != y)).ToArray();
        foreach (SerialPort port in deadPorts)
        {
            Ports.Remove(port);
            Slaves.RemoveAll(x => x.Port.PortName == port.PortName);
        }

        Console.WriteLine("Scanning...");
        foreach (string portName in systemPortNames)
        {
            Console.WriteLine("   {0}", portName);
            if (Ports.All(x => x.PortName != portName))
            {
                SerialPort newPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
                newPort.Open();
                Ports.Add(newPort);
            }
        }
    }

    private static void StartReceivingData()
    {
        tasks.Add(StartListening());
    }

    private static async Task StartListening()
    {
        try
        {
            await Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    foreach (SerialPort port in Ports)
                    {
                        if (Slaves.Any(x => x.Port.PortName == port.PortName)) continue;

                        try
                        {
                            string message = port.ReadLine();
                            Console.WriteLine($"{port.PortName}: {message}");

                            if (message.StartsWith("ID:"))
                            {
                                string id = message.Split(':')[1].Trim();
                                Slave newSlave = new Slave(id, port);
                                newSlave.OnPortDisconnected += NewSlave_OnSlaveDisconnected; ;
                                Slaves.Add(newSlave);
                            }
                        }
                        catch (TimeoutException)
                        {
                            //There will be a timeout/Operation timed out exception going mental here so nothing is really needed here...
                            Console.WriteLine($"{port.PortName}: Time Out!");
                        }
                        catch { /*there's some IO exception here because of the Task.WhenAll(tasks);*/ }
                    }
                }
            });
        }
        catch { }
    }

    private static void NewSlave_OnSlaveDisconnected(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}