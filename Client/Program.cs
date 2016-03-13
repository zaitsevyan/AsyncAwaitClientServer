using System;

namespace Client
{
	class MainClassClient
	{
		public static void Main (string[] args)
		{
			var client = new Client ();
			client.Start (AsyncClientServer.MainClass.Port);
			while (true) {
				var line = Console.ReadLine ();
				client.Send (line);
			}
		}
	}
}
