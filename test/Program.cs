using System;

namespace test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Pru1Aio.Pru1Aio p = new Pru1Aio.Pru1Aio ();
            p.Initialize();

            p.Configure(20, Pru1Aio.Channels.AllChannels, 15, 16, 2000);
            p.PrintControl();
            p.Start(1000);
            if (p.DroppedBuffers.Count > 0)
                p.DroppedBuffers.ForEach(Console.WriteLine);

            p.Configure(20, Pru1Aio.Channels.AllChannels, 15, 16, 4000);
            p.PrintControl();
            p.Start(1000);
            if (p.DroppedBuffers.Count > 0)
                p.DroppedBuffers.ForEach(Console.WriteLine);

            p.Configure(20, Pru1Aio.Channels.AllChannels, 15, 16, 4000);
            p.PrintControl();
            p.Start(1000, 200, Pru1Aio.BufferMode.Ring);
            if (p.DroppedBuffers.Count > 0)
                p.DroppedBuffers.ForEach(Console.WriteLine);

            p.Configure(10, Pru1Aio.Channels.AllChannels, 15, 16, 1000);
            p.PrintControl();
            p.Start(1000);
            if (p.DroppedBuffers.Count > 0)
                p.DroppedBuffers.ForEach(Console.WriteLine);
        }
	}
}
