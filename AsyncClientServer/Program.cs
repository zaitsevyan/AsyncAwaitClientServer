using System;

namespace AsyncClientServer
{
	public static class MainClass
	{
		public static readonly int Port = 54232;
		public static void Main (string[] args)
		{
			var server = new Server (Port);
			server.Start ();
		}
	}
}
