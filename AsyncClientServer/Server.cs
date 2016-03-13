using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace AsyncClientServer
{


	public class Server
	{
		public TcpListener Listener;
		private volatile bool Running;
		public Server (int port)
		{
			Listener = new TcpListener (IPAddress.Any, port);
		}

		public void Start() {
			Listener.Start (10);
			Running = true;

			while (Running) {
				var connection = Listener.AcceptTcpClient ();
				ProcessConnection (connection);//Будет работать асинхронно, так как мы не написали await перед вызовом
			}
		}


		public async Task ProcessConnection(TcpClient connection) { //Будет выполнятся в отдельном потоке
			using(var writer = new StreamWriter(connection.GetStream()))
			using(var stream = new StreamReader(connection.GetStream())) {
				while (Running && connection.Connected) {
					var line = await stream.ReadLineAsync ();
					ProcessCommand (connection, writer, line);
				}
				connection.Close ();
			}
		}


		private void ProcessCommand(TcpClient connection, StreamWriter writer, String line) {
			var info = connection.Client.RemoteEndPoint as IPEndPoint;
			Console.WriteLine ("{1}:{2}: {0}", line, info.Address.ToString(), info.Port); // Обработка данных
			writer.WriteLineAsync (String.Format("Received {0}", line.Length));//Не пишем await, так как нам не нужно ждать завершение отправки
			writer.Flush();
		}


		public void Stop() {
			Running = false;
			Listener.Stop ();
		}
		~Server() {
			Running = false;
			Listener.Stop ();
		}
	}
}

