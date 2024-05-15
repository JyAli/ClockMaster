using ClockMaster;
using System.IO.Ports;

internal class Program
{
    private static List<Task> tasks = new List<Task>();
    private static SerialPort Port;

    private static List<Slave> Slaves = new List<Slave>();

    private static void Main(string[] args)
    {
        //Display all port names connected to the device
        foreach (string portName in SerialPort.GetPortNames())
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

        Port.Close();
    }

    private static void StartReceivingData()
    {
        tasks.Add(startListening());
    }

    private static async Task startListening()
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
                        if (message.StartsWith("ID"))
                        {
                            string ID = message.Split(":")[1].Trim();
                            Slave? slave = Slaves.Find(x => x.ID == ID);

                            if (slave == null)
                            {
                                Slaves.Add(new Slave(ID, Port.PortName));
                            }
                        }
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