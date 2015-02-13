using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Pru1Aio
{

	public partial class Pru1Aio
	{
        public SynchronousCallBack SynchCall;

		public Pru1Aio ()
		{
            Pru1Aio.prussdrv_strversion(1);
		}

        public void Initialize()
        {


            SynchCall = new SynchronousCallBack(CallBack);

            PruSharedMemory PruMemory = Pru1Aio.pru_rta_init();

            PruMemory.Control.BufferSize = 40;
            PruMemory.Control.ChannelEnabledMask = 0x7F;
            PruMemory.Control.SampleSoc = 15;
            PruMemory.Control.SampleAverage = 16;
            PruMemory.Control.SampleRate = 1000;

            Pru1Aio.pru_rta_configure(ref PruMemory.Control);

            Pru1Aio.print_pru_map(ref PruMemory);

            Pru1Aio.print_pru_map_address(ref PruMemory);
        }

        public void CallBack(uint BufferCount, ushort BufferSize, ref Reading[] CapturedBuffer, ref CallState CallState, ref PruSharedMemory PruMemory) {
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

