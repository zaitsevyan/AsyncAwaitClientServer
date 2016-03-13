using System;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;

namespace Client
{
	public class Client
	{
		private TcpClient Connection;
		private volatile bool Running;
		private StreamWriter Writer;
		public Client ()
		{
			Connection = new TcpClient ();
		}

		public async Task Start(int port) {

			Connection.Connect ("localhost", port);
			Console.WriteLine ("Connected");
			Running = true;
			Writer = new StreamWriter (Connection.GetStream ());

			using(var stream = new StreamReader(Connection.GetStream())) {
				while (Running && Connection.Connected) {
					var line = await stream.ReadLineAsync ();
					ProcessCommand (line);
				}
				Stop ();
			}
		}

		public async Task Send(String line) {
			if (Writer == null)
				return;
			await Writer.WriteLineAsync (line);
			await Writer.FlushAsync ();
		}

		private void ProcessCommand(String line) {
			Console.WriteLine (line); // Обработка данных
		}

		public void Stop() {
			Running = false;
			Writer.Close ();
			Writer = null;
			Connection.Close ();
		
		}
		~Client() {
			Stop ();
		}
	}
}

