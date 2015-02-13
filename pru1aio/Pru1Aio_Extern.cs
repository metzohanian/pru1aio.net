using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Pru1Aio
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void SynchronousCallBack(uint BufferCount, ushort BufferSize, ref Reading[] CapturedBuffer, ref CallState CallState, ref PruSharedMemory PruMemory);

	public partial class Pru1Aio
	{
        [DllImport("libprussdrv.so")]
        private static extern int prussdrv_init();

        [DllImport("libprussdrv.so")]
        private static extern IntPtr prussdrv_strversion(int version);

		[DllImport ("libpru1aio.so")]
		private static extern void pru_rta_configure(ref PruControl Control);
		[DllImport ("libpru1aio.so")]
		[return : MarshalAs(UnmanagedType.LPStruct)]
		private static extern PruSharedMemory pru_rta_init();

		[DllImport ("libpru1aio.so")]
		private static extern void pru_rta_clear_pru(int PRU);

		[DllImport ("libpru1aio.so")]
		private static extern void pru_rta_start_firmware();

		[DllImport ("libpru1aio.so")]
		// Incomplete
		private static extern void pru_rta_start_capture(ref PruSharedMemory PruMemory, SynchronousCallBack CallBack, ref CallState CallState);
		[DllImport ("libpru1aio.so")]
		private static extern void pru_rta_pause_capture(ref PruSharedMemory PruMemory);
		[DllImport ("libpru1aio.so")]
		private static extern void pru_rta_stop_capture(ref PruSharedMemory PruMemory);

		[DllImport ("libpru1aio.so")]
		private static extern void print_pru_map(ref PruSharedMemory PruMemory);
		[DllImport ("libpru1aio.so")]
		private static extern void print_pru_map_address(ref PruSharedMemory PruMemory);

		[DllImport ("libpru1aio.so")]
		[return : MarshalAs(UnmanagedType.LPStruct)]
		private static extern Conditions pru_rta_init_conditions();
		[DllImport ("libpru1aio.so")]
		private static extern void pru_rta_destroy_conditions(ref Conditions Conditions);
		[DllImport ("libpru1aio.so")]
		private static extern void pru_rta_add_condition(ref Conditions Conditions, string Name, Comparator Condition, Signal Signal, ushort Comp1, ushort Comp2);

		[DllImport ("libpru1aio.so")]
		private static extern void pru_printf_hello(string world);


	}
}
