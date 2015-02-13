using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Pru1Aio
{

	public partial class Pru1Aio
	{
		public Pru1Aio ()
		{
		}

		public void ClearPru(int PRU) {
			Pru1Aio.pru_rta_clear_pru (PRU);
		}

		public void Hello(string World) {
			Pru1Aio.pru_printf_hello (World);
		}

	}
}

