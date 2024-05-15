using System.IO.Ports;
using Timer = System.Timers.Timer;

namespace ClockMaster
{
    public class Slave
    {
        public string ID { get; private set; }
        public SerialPort Port { get; private set; }

        private List<Task> tasks = new List<Task>();
        private Timer StatusTimer;

        public event EventHandler OnPortDisconnected;
        public Slave(string id, SerialPort port)
        {
            ID = id;
            Port = port;
            Console.WriteLine($"{port.PortName}: Slave Found!");
            //InitStatusTimer();
            StartReceivingData();
        }

        public void SetPosition(int directPos, int indirectPos)
        {
            string direct = directPos.ToString().PadLeft(4,'0');
            string indirect = indirectPos.ToString().PadLeft(4,'0');
            Port.WriteLine($"setPosition: {direct},{indirect}");
        }

        private void InitStatusTimer()
        {
            StatusTimer = new Timer();
            StatusTimer.Elapsed += delegate { CheckStatus(); };
            StatusTimer.Interval = 1000; // in miliseconds
            StatusTimer.AutoReset = true;
            StatusTimer.Start();
        }

        private void CheckStatus()
        {
            string[] systemPorts = SerialPort.GetPortNames();
            if (!systemPorts.Any(x => x == Port.PortName))
            {
                Port.Close();
                OnPortDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        private void StartReceivingData()
        {
            tasks.Add(StartListening());
        }

        private async Task StartListening()
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    while (Port.IsOpen)
                    {
                        try
                        {
                            string message = Port.ReadLine();

                            //ReceivedMessages.Items.Add(message);
                            Console.WriteLine(message);
                        }
                        catch (TimeoutException)
                        {
                            //There will be a timeout/Operation timed out exception going mental here so nothing is really needed here...
                            Console.WriteLine("Time Out!");
                        }
                        catch { /*there's some IO exception here because of the Task.WhenAll(tasks);*/ }
                    }
                });
            }
            catch { }
        }
    }
}
