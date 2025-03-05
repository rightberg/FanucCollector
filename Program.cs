using Newtonsoft.Json;
using OpcUaDeviceControlServer;

public class Device
{
	public string address { get; set; }
	public int port { get; set; }
}

public class Devices
{
	public List<FanucCollector> collectors { get; set; }
}

class Program
{
	public static void Main(string[] args)
	{
		Console.OutputEncoding = System.Text.Encoding.UTF8;

		if (args.Length == 0)
			return;

		if (args.Length > 0)
		{
			string jsonString = args[0];
			try
			{
				Fanuc fanuc = new();

				List<Device> devices = JsonConvert.DeserializeObject<List<Device>>(jsonString);
				List<FanucCollector> collectors = new();
				foreach (var device in devices)
				{
					FanucCollector collector = fanuc.GetCollector(device.address, (ushort)device.port, 10);
					collectors.Add(collector);
				}
				var device_list = new Devices { collectors = collectors };

				string output = JsonConvert.SerializeObject(device_list);
				Console.WriteLine(output);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Ошибка JSON: " + ex.Message);
			}
		}
	}
}
