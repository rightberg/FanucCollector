using Newtonsoft.Json;

public struct Device
{
	public string address;
	public int port;
	public string series;
}

public class CollectorData
{
	public List<Collector> collectors { get; set; }
}

class Program
{
	public static void Main(string[] args)
	{
		Console.OutputEncoding = System.Text.Encoding.UTF8;

		if (args.Length > 0)
		{
			string jsonString = args[0];
			try
			{
				List<Device> devices = JsonConvert.DeserializeObject<List<Device>>(jsonString);
				List<Collector> collectors = new();
				foreach (var device in devices)
				{
					Collector collector = Fanuc.GetCollector(device.address, (ushort)device.port, 10, device.series);
					collector.device = device;
					collectors.Add(collector);
				}
				var data = new CollectorData { collectors = collectors };

				string output = JsonConvert.SerializeObject(data);
				Console.WriteLine(output);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Ошибка JSON: " + ex.Message);
			}
		}
		else
			return;
	}
}
