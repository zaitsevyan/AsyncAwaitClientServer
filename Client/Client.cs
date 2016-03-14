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
		private BinaryWriter Writer;
		public Client ()
		{
			Connection = new TcpClient ();
		}

		public async Task Start(int port) {

			Connection.Connect ("localhost", port);
			Console.WriteLine ("Connected");
			Running = true;
			Writer = new BinaryWriter (Connection.GetStream ());

			using(var stream = new BinaryReader(Connection.GetStream())) {
				while (Running && Connection.Connected) {
					await Task.Factory.StartNew (() => {
						var count = stream.ReadInt32();
						var data = stream.ReadBytes(count);
						ProcessCommand(data);
					});
				}
				Stop ();
			}
		}

		public async Task Send(String line) {
			if (Writer == null)
				return;
			await Task.Factory.StartNew (() => {
				var data = System.Text.Encoding.UTF8.GetBytes(line);
				Writer.Write((Int32) data.Length);
				Writer.Write(data);
				Writer.Flush();
			});
		}

		private void ProcessCommand(byte[] data) {
			var line = System.Text.Encoding.UTF8.GetString (data);
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

