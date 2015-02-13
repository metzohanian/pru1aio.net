using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Pru1Aio
{

	public partial class Pru1Aio
	{
        public unsafe AsynchronousCallBack AsynchCall;

        int Calls;
        int CallsPerUpdate;
        int CurrentRecord;
        Reading[] Readings;

		public Pru1Aio ()
		{
            Pru1Aio.prussdrv_strversion(1);
		}

        public unsafe void Initialize()
        {

            AsynchCall = new AsynchronousCallBack(CallBack);

            IntPtr prumem = Pru1Aio.pru_rta_init();
            IntPtr cstate = Pru1Aio.pru_rta_init_call_state();
            PruSharedMemory* PruMemory = (PruSharedMemory*)prumem.ToPointer();
            CallState* CallState = (CallState*)cstate.ToPointer();
            IntPtr cbuffer = Pru1Aio.pru_rta_init_capture_buffer(PruMemory);
            Reading* Buffer = (Reading*)cbuffer.ToPointer();

            PruMemory->Control.BufferSize = 40;
            PruMemory->Control.ChannelEnabledMask = 0x7F;
            PruMemory->Control.SampleSoc = 15;
            PruMemory->Control.SampleAverage = 16;
            PruMemory->Control.SampleRate = 2000;

            int seconds = 5;
            Readings = new Reading[seconds * PruMemory->Control.SampleRate];

            Calls = (int)(PruMemory->Control.SampleRate / PruMemory->Control.BufferSize * seconds);
            CallsPerUpdate = (int)((PruMemory->Control.SampleRate / PruMemory->Control.BufferSize) / 2);

            Pru1Aio.pru_rta_configure(&(PruMemory->Control));

            Pru1Aio.print_pru_map(PruMemory);
            Pru1Aio.print_pru_map_address(PruMemory);

            Console.WriteLine("Start Firmware\n");
            Pru1Aio.pru_rta_start_firmware();

            Console.WriteLine("Start Capture\n");
            Pru1Aio.pru_rta_start_capture(PruMemory, Buffer, CallState, AsynchCall);
        }

        public unsafe void CallBack(uint BufferCount, ushort BufferSize, Reading* CapturedBuffer, CallState* CallState, PruSharedMemory* PruMemory)
        {
            Pru1Aio.pru_rta_set_digital_out(PruMemory, 0xF, 0xA);

            for (int record = 0; record < BufferSize; record++)
            {
                for (int channel = 0; channel < PruMemory->Control.ChannelCount; channel++)
                {
                    fixed (Reading* readings = Readings) {
                        readings[CurrentRecord].Readings[channel] = CapturedBuffer[record].Readings[channel];
                    }
                }
                Readings[CurrentRecord].DigitalIn = CapturedBuffer[record].DigitalIn;
                Readings[CurrentRecord].Buffer = CapturedBuffer[record].Buffer;
                CurrentRecord++;
            }
            Calls--;

            string output = "";
            if (Calls % CallsPerUpdate == 0)
            {
                for (int channel = 0; channel < PruMemory->Control.ChannelCount; channel++)
                {
                    output += channel + ":" + CallState->BufferMean.Readings[channel].ToString("X") + ",";
                }
                output += "D:" + CallState->BufferMean.DigitalIn.ToString("X");
                Console.WriteLine(output);
            }

            if (Calls == 0 )
                Pru1Aio.pru_rta_stop_capture(PruMemory);

            return;
        }

		public void ClearPru(int PRU) {
			Pru1Aio.pru_rta_clear_pru (PRU);
		}

		public void Hello(string World) {
			Pru1Aio.pru_printf_hello (World);
		}

	}
}

