using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace test
{
	class MainClass
	{
        public static int OverflowNotified;
        public static int RingNotified;

        public delegate void TestStopDelegate();

        public static void Reset()
        {
            Console.WriteLine("ON: " + OverflowNotified);
            Console.WriteLine("RN: " + RingNotified);
            OverflowNotified = 0;
            RingNotified = 0;
        }

        public static void Subscriber(object sender, Pru1Aio.Pru1AioEventArgs e)
        {
            if (e.WarmUp)
                return;

            switch (e.Type)
            {
                case Pru1Aio.MessageType.Underflow:
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n" + e.Type);
                    Console.ResetColor();
                    break;
                case Pru1Aio.MessageType.Overflow:
                    if (OverflowNotified == 0)
                    {
                        Console.WriteLine(e.Type);
                    }
                    OverflowNotified++;
                    break;
                case Pru1Aio.MessageType.Ring:
                    if (RingNotified == 0)
                    {
                        Console.WriteLine(e.Type);
                    }
                    RingNotified++;
                    break;
                case Pru1Aio.MessageType.Notification:
                    break;
                default:
                    Console.WriteLine(e.Type);
                    break;
            }
        }

        public static void TestStop()
        {
            Console.WriteLine("TestStop()");
            while (Pru1Aio.Pru1Aio.Status != Pru1Aio.InitMode.Running)
            {
                System.Threading.Thread.Sleep(50);
            }
            while (Pru1Aio.Pru1Aio.TotalRecords < 1000)
            {
                if (Pru1Aio.Pru1Aio.TotalRecords > 400)
                    Pru1Aio.Pru1Aio.DigitalOutput = 0xFF;
                System.Threading.Thread.Sleep(50);
            }
            Console.WriteLine("Pru1Aio.Stop()");
            Pru1Aio.Pru1Aio.Stop();
            Console.WriteLine();
            Pru1Aio.Pru1Aio.PrintControl();
        }

		public static void Main (string[] args)
		{
            Pru1Aio.Pru1Aio.Initialize();

            Pru1Aio.Pru1Aio.Message += (sender, e) =>
            {
                Subscriber(sender, (Pru1Aio.Pru1AioEventArgs)e);
            };

            Pru1Aio.Pru1Aio.WarmUp();
            /*
            Reset();
            Pru1Aio.Pru1Aio.Configure(10, Pru1Aio.Channels.AllChannels, 15, 16, 1000);
            Pru1Aio.Pru1Aio.PrintControl();
            Console.WriteLine("Calls: " + Pru1Aio.Pru1Aio.Calls);
            Pru1Aio.Pru1Aio.Start(1000);
            if (Pru1Aio.Pru1Aio.DroppedBuffers.Count > 0)
                Pru1Aio.Pru1Aio.DroppedBuffers.ForEach(Console.WriteLine);
            Console.WriteLine("Total Records: " + Pru1Aio.Pru1Aio.TotalRecords);

            Reset();
            Pru1Aio.Pru1Aio.Configure(40, Pru1Aio.Channels.AllChannels, 15, 16, 4000);
            Pru1Aio.Pru1Aio.PrintControl();
            Console.WriteLine("Calls: " + Pru1Aio.Pru1Aio.Calls);
            Pru1Aio.Pru1Aio.Start(1000);
            if (Pru1Aio.Pru1Aio.DroppedBuffers.Count > 0)
                Pru1Aio.Pru1Aio.DroppedBuffers.ForEach(Console.WriteLine);
            Console.WriteLine("Total Records: " + Pru1Aio.Pru1Aio.TotalRecords);

            Reset();
            Pru1Aio.Pru1Aio.Configure(40, Pru1Aio.Channels.AllChannels, 15, 16, 4000);
            Pru1Aio.Pru1Aio.PrintControl();
            Console.WriteLine("Calls: " + Pru1Aio.Pru1Aio.Calls);
            Pru1Aio.Pru1Aio.Start(4000, 400, Pru1Aio.BufferMode.Ring);
            if (Pru1Aio.Pru1Aio.DroppedBuffers.Count > 0)
                Pru1Aio.Pru1Aio.DroppedBuffers.ForEach(Console.WriteLine);
            Console.WriteLine("Total Records: " + Pru1Aio.Pru1Aio.TotalRecords);

            Reset();
            Pru1Aio.Pru1Aio.DigitalOutput = 0x0;
            Pru1Aio.Pru1Aio.Configure(10, Pru1Aio.Channels.AllChannels, 15, 16, 1000);
            Pru1Aio.Pru1Aio.PrintControl();
            Console.WriteLine("Calls: " + Pru1Aio.Pru1Aio.Calls);
            Pru1Aio.Pru1Aio.Start(1000, 200, Pru1Aio.BufferMode.Overflow);
            if (Pru1Aio.Pru1Aio.DroppedBuffers.Count > 0)
                Pru1Aio.Pru1Aio.DroppedBuffers.ForEach(Console.WriteLine);
            Console.WriteLine("Total Records: " + Pru1Aio.Pru1Aio.TotalRecords);
            */
            Pru1Aio.Conditions PruTriggers = new Pru1Aio.Conditions();
            PruTriggers.Add(new Pru1Aio.Conditional()
            {
                Name = "Check AI[0] for > 3800",
                Condition = Pru1Aio.Comparator.Greater,
                Comparator1 = 3800,
                Signal = Pru1Aio.Signal.CHANNEL_0
            });
            PruTriggers.Add(new Pru1Aio.Conditional()
            {
                Name = "Check for DI[0] Rising",
                Condition = Pru1Aio.Comparator.RisingEdge,
                Comparator1 = 0,
                Signal = Pru1Aio.Signal.CHANNEL_DIO
            });
            PruTriggers.Triggered += PruTriggers_Triggered;
            PruTriggers.CheckEach = true;

            Reset();
            TestStopDelegate dlgt = new TestStopDelegate(TestStop);
            dlgt.BeginInvoke(null, null);
            Pru1Aio.Pru1Aio.Configure(40, Pru1Aio.Channels.AllChannels, 15, 16, 1000);
            Pru1Aio.Pru1Aio.PrintControl();
            Console.WriteLine("Calls: " + Pru1Aio.Pru1Aio.Calls);
            Pru1Aio.Pru1Aio.Start(0, 1000, Pru1Aio.BufferMode.Ring);
            if (Pru1Aio.Pru1Aio.DroppedBuffers.Count > 0)
                Pru1Aio.Pru1Aio.DroppedBuffers.ForEach(Console.WriteLine);
            Console.WriteLine("Total Records: " + Pru1Aio.Pru1Aio.TotalRecords);

            Reset();
        }

        static void PruTriggers_Triggered(List<Pru1Aio.Conditional> ConditionsTriggered)
        {
            Console.WriteLine(ConditionsTriggered[0].Name + " triggered [" + ConditionsTriggered[0].Triggers + "]");
        }
	}

}
