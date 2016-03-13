using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace AsyncClientServer
{


	public class Server
	{
		public TcpListener Listener;
		private volatile bool Running;
		private List<StreamWriter> writers = new List<StreamWriter>();
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
			var writer = new StreamWriter(connection.GetStream());
			writer.AutoFlush = true;

			lock (writers) {
				writers.Add (writer);
			}
			using(var stream = new StreamReader(connection.GetStream())) {
				while (Running && connection.Connected) {
					var line = await stream.ReadLineAsync ();
					ProcessCommand (connection, writer, line);
				}
				connection.Close ();
			}
			lock (writers) {
				writers.Remove (writer);
			}
		}


		private void ProcessCommand(TcpClient connection, StreamWriter writer, String line) {
			var info = connection.Client.RemoteEndPoint as IPEndPoint;
			var response = String.Format ("{1}:{2}: {0}", line, info.Address.ToString(), info.Port);
			Console.WriteLine (response);
			lock (writers) {
				foreach (var w in writers) {
					if (w != null) {
						try {
							w.WriteLineAsync (response);
						} catch {
						
						}
					}
				}
			}
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

