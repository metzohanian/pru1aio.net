using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Pru1Aio
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void AsynchronousCallBack(uint BufferCount, ushort BufferSize, Reading* CapturedBuffer, CallState* CallState, PruSharedMemory* PruMemory);

	public partial class Pru1Aio
	{
        [DllImport("libprussdrv.so")]
        private static extern int prussdrv_init();

        [DllImport("libprussdrv.so")]
        private static extern IntPtr prussdrv_strversion(int version);

		[DllImport ("libpru1aio.so")]
		private unsafe static extern void pru_rta_configure(PruControl *Control);
		[DllImport ("libpru1aio.so")]
		private static extern IntPtr pru_rta_init();
        [DllImport("libpru1aio.so")]
        private unsafe static extern IntPtr pru_rta_init_capture_buffer(PruSharedMemory* PruMemory);

		[DllImport ("libpru1aio.so")]
		private static extern void pru_rta_clear_pru(int PRU);

		[DllImport ("libpru1aio.so")]
		private static extern void pru_rta_start_firmware();

		[DllImport ("libpru1aio.so")]
        private unsafe static extern void pru_rta_start_capture(PruSharedMemory* PruMemory, Reading* Buffer, CallState* CallState, AsynchronousCallBack CallBack);
		[DllImport ("libpru1aio.so")]
		private unsafe static extern void pru_rta_pause_capture(PruSharedMemory *PruMemory);
		[DllImport ("libpru1aio.so")]
		private unsafe static extern void pru_rta_stop_capture(PruSharedMemory *PruMemory);
        [DllImport("libpru1aio.so")]
        private unsafe static extern void pru_rta_set_digital_out(PruSharedMemory* PruMemory, uint WriteMask, uint DigitalOut);

		[DllImport ("libpru1aio.so")]
		private unsafe static extern void print_pru_map(PruSharedMemory *PruMemory);
		[DllImport ("libpru1aio.so")]
		private unsafe static extern void print_pru_map_address(PruSharedMemory *PruMemory);

        [DllImport("libpru1aio.so")]
        private unsafe static extern IntPtr pru_rta_init_call_state();
        [DllImport("libpru1aio.so")]
        private unsafe static extern void pru_rta_free_call_state(CallState* CallState);

        [DllImport("libpru1aio.so")]
		private static extern void pru_rta_add_condition(ref Conditions Conditions, string Name, Comparator Condition, Signal Signal, ushort Comp1, ushort Comp2);

		[DllImport ("libpru1aio.so")]
		private static extern void pru_printf_hello(string world);

        [DllImport("libpru1aio.so")]
//        private unsafe static extern void pru_rta_test_callback(PruSharedMemory* PruMemory, [MarshalAs(UnmanagedType.LPArray)] Reading[] Buffer, CallState* CallState, AsynchronousCallBack CallBac);
        private unsafe static extern void pru_rta_test_callback(PruSharedMemory* PruMemory, Reading* Buffer, CallState* CallState, AsynchronousCallBack CallBac);
	}
}
