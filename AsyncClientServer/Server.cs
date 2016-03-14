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
		private List<BinaryWriter> writers = new List<BinaryWriter>();
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
			var writer = new BinaryWriter(connection.GetStream());

			lock (writers) {
				writers.Add (writer);
			}
			using(var stream = new BinaryReader(connection.GetStream())) {
				while (Running && connection.Connected) {
					await Task.Factory.StartNew (() => {
						var count = stream.ReadInt32();
						var data = stream.ReadBytes (count);
						ProcessCommand (connection, writer, data);
					});
				}
				connection.Close ();
			}
			lock (writers) {
				writers.Remove (writer);
			}
		}


		private void ProcessCommand(TcpClient connection, BinaryWriter writer, byte[] data) {
			var info = connection.Client.RemoteEndPoint as IPEndPoint;
			var line = System.Text.Encoding.UTF8.GetString (data);
			var response = String.Format ("{1}:{2}: {0}", line, info.Address.ToString(), info.Port);
			Console.WriteLine (response);
			lock (writers) {
				foreach (var w in writers) {
					if (w != null) {
						try {
							w.Write((Int32)data.Length);
							w.Write(data);
							w.Flush();
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

