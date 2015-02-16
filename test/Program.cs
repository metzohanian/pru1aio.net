using System;

namespace test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            Pru1Aio.Pru1Aio.Initialize();

            Pru1Aio.Pru1Aio.Configure(20, Pru1Aio.Channels.AllChannels, 15, 16, 2000);
            Pru1Aio.Pru1Aio.PrintControl();
            Pru1Aio.Pru1Aio.Start(1000);
            if (Pru1Aio.Pru1Aio.DroppedBuffers.Count > 0)
                Pru1Aio.Pru1Aio.DroppedBuffers.ForEach(Console.WriteLine);

            Pru1Aio.Pru1Aio.Configure(40, Pru1Aio.Channels.AllChannels, 15, 16, 4000);
            Pru1Aio.Pru1Aio.PrintControl();
            Pru1Aio.Pru1Aio.Start(1000);
            if (Pru1Aio.Pru1Aio.DroppedBuffers.Count > 0)
                Pru1Aio.Pru1Aio.DroppedBuffers.ForEach(Console.WriteLine);

            Pru1Aio.Pru1Aio.Configure(40, Pru1Aio.Channels.AllChannels, 15, 16, 4000);
            Pru1Aio.Pru1Aio.PrintControl();
            Pru1Aio.Pru1Aio.Start(1000, 200, Pru1Aio.BufferMode.Ring);
            if (Pru1Aio.Pru1Aio.DroppedBuffers.Count > 0)
                Pru1Aio.Pru1Aio.DroppedBuffers.ForEach(Console.WriteLine);

            Pru1Aio.Pru1Aio.Configure(10, Pru1Aio.Channels.AllChannels, 15, 16, 1000);
            Pru1Aio.Pru1Aio.PrintControl();
            Pru1Aio.Pru1Aio.Start(1000);
            if (Pru1Aio.Pru1Aio.DroppedBuffers.Count > 0)
                Pru1Aio.Pru1Aio.DroppedBuffers.ForEach(Console.WriteLine);

            Pru1Aio.Pru1Aio.PrintControl();
        }
	}
}
