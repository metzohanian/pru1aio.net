using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

namespace Pru1Aio
{

	public partial class Pru1Aio
	{
        static unsafe AsynchronousCallBack AsynchCall;

        static int Calls;
        static int RunTimeMs;
        static int CurrentRecord;
        static int Signals;
        static int LastBufferCount;
        static List<int> _DroppedBuffers;

        static InitMode _Status = InitMode.NotReady;
        static InitMode Status
        {
            get
            {
                return _Status;
            }
        }

        static bool _capturing;
        static bool _IsCapturing
        {
            get
            {
                return _capturing;
            }
            set
            {
                _capturing = value;
            }
        }

        static BufferMode Mode;
        static bool RunLimit;

        static Reading[] Readings;

        static unsafe PruSharedMemory* PruMemory;
        static unsafe CallState* CallState;
        static unsafe Reading* Buffer;

        static unsafe IntPtr IPruMem;
        static unsafe IntPtr ICallState;
        static unsafe IntPtr IBuffer;

        public static unsafe Pru1Aio Aio;

        private static readonly object Locker = new object();

		private Pru1Aio ()
		{
            // forces libprussdrv to load
            Pru1Aio.prussdrv_strversion(1);
            _Status = InitMode.NotReady;
        }

        public static unsafe void Start(int RunTimeMs = 2000, int Readings = 0, BufferMode Mode = BufferMode.Fill)
        {
            if (Status != InitMode.Configured)
                throw new Exception("Pru1Aio must be configured prior to operation.");

            lock (Pru1Aio.Locker)
            {
                Pru1Aio.Mode = Mode;
                Pru1Aio.RunTimeMs = RunTimeMs;
                RunLimit = false;

                if (Readings % PruMemory->Control.BufferSize != 0)
                    throw new Exception("Readings must be a multiple of the buffer size.  Inter-buffer reading limits are not supported.");

                if (Pru1Aio.Mode == BufferMode.Fill || RunTimeMs > 0)
                {
                    RunLimit = true;
                    if (RunTimeMs == 0 && Readings == 0)
                    {
                        Pru1Aio.pru_rta_stop_capture(PruMemory);
                        Pru1Aio.pru_rta_clear_pru((int)PRU_NUM.PRU1);
                        Pru1Aio.pru_rta_clear_pru((int)PRU_NUM.PRU0);
                        return;
                    }
                    if (RunTimeMs > 0 && Readings == 0)
                    {
                        Pru1Aio.Readings = new Reading[RunTimeMs / 1000 * PruMemory->Control.SampleRate];
                        Calls = (int)(PruMemory->Control.SampleRate / PruMemory->Control.BufferSize * RunTimeMs / 1000);
                    }
                    else if (RunTimeMs == 0 && Readings > 0)
                    {
                        Pru1Aio.Readings = new Reading[Readings];
                        Calls = Readings / PruMemory->Control.BufferSize;
                    }
                    else if (RunTimeMs > 0 && Readings > 0)
                    {
                        Pru1Aio.Readings = new Reading[Readings];
                        Calls = (int)(PruMemory->Control.SampleRate / PruMemory->Control.BufferSize * RunTimeMs / 1000);
                    }
                }

                _DroppedBuffers = new List<int>();

                Pru1Aio.pru_rta_configure(&(PruMemory->Control));

                Pru1Aio.pru_rta_start_firmware();

                Signals = 0;
                _IsCapturing = true;
                _Status = InitMode.Running;
                Pru1Aio.pru_rta_start_capture(PruMemory, Buffer, CallState, AsynchCall);
            }
        }

        public static unsafe void PrintControl(bool Addresses = false)
        {
            if (Status == InitMode.NotReady)
                throw new Exception("Pru1Aio not Initialized()");

            if (Addresses)
                Pru1Aio.print_pru_map_address(PruMemory);
            else
                Pru1Aio.print_pru_map(PruMemory);
        }

        public static unsafe void Initialize()
        {
            if (Status != InitMode.NotReady)
                throw new Exception("Pru1Aio may only be Initialized once.");

            lock (Locker)
            {
                if (Pru1Aio.Aio == null)
                    Pru1Aio.Aio = new Pru1Aio();

                IPruMem = Pru1Aio.pru_rta_init();
                ICallState = Pru1Aio.pru_rta_init_call_state();
                PruMemory = (PruSharedMemory*)IPruMem.ToPointer();
                CallState = (CallState*)ICallState.ToPointer();
                _IsCapturing = false;

                _Status = InitMode.Initialized;
            }
        }

        public static unsafe void Configure(int BufferSize, Channels ChannelEnabledMask, int SampleSoc, int SampleAverage, int SampleRate)
        {
            if (Status != InitMode.Initialized)
                throw new Exception("Pru1Aio must be initialized prior to Configuration.");

            lock (Locker)
            {
                _IsCapturing = false;

                AsynchCall = new AsynchronousCallBack(CallBack);

                PruMemory->Control.BufferCount = 0;
                PruMemory->Control.BufferSize = (byte)BufferSize; // 40;
                PruMemory->Control.ChannelEnabledMask = (byte)ChannelEnabledMask; // 0x7F;
                PruMemory->Control.SampleSoc = (byte)SampleSoc; // 15;
                PruMemory->Control.SampleAverage = (byte)SampleAverage; // 16;
                PruMemory->Control.SampleRate = (uint)SampleRate; // 1000;

                IBuffer = Pru1Aio.pru_rta_init_capture_buffer(PruMemory);
                Buffer = (Reading*)IBuffer.ToPointer();

                _Status = InitMode.Configured;
            }
        }

        public static unsafe void Stop()
        {
            _IsCapturing = false;
            _Status = InitMode.Initialized;
            Pru1Aio.pru_rta_stop_capture(PruMemory);
        }

        private static unsafe void CallBack(uint BufferCount, ushort BufferSize, Reading* CapturedBuffer, CallState* CallState, PruSharedMemory* PruMemory)
        {
            Signals++;
            LastBufferCount = (int)PruMemory->Control.BufferCount;

            if (Signals < LastBufferCount)
            {
                do
                {
                    _DroppedBuffers.Add(Signals);
                    Signals++;
                } while (Signals < LastBufferCount);
            }
            
            Pru1Aio.pru_rta_set_digital_out(PruMemory, 0xF, 0xA);

            if (IsCapturing && Status == InitMode.Running)
            {
                for (int record = 0; record < BufferSize; record++)
                {
                    if (CurrentRecord >= Readings.Length)
                    {
                        if (Mode == BufferMode.Ring)
                            CurrentRecord = 0;
                        else
                            break;
                    }
                    for (int channel = 0; channel < PruMemory->Control.ChannelCount; channel++)
                    {
                        fixed (Reading* readings = Readings)
                        {
                            readings[CurrentRecord].Readings[channel] = CapturedBuffer[record].Readings[channel];
                        }
                    }
                    Readings[CurrentRecord].DigitalIn = CapturedBuffer[record].DigitalIn;
                    Readings[CurrentRecord].Buffer = CapturedBuffer[record].Buffer;
                    CurrentRecord++;
                }
            }

            if (RunLimit)
            {
                _IsCapturing = false;

                Calls--;
                if (Calls == 0)
                {
                    Pru1Aio.pru_rta_stop_capture(PruMemory);
                    _Status = InitMode.Initialized;
                }

                if (CurrentRecord >= Readings.Length)
                {
                    Pru1Aio.pru_rta_stop_capture(PruMemory);
                    _Status = InitMode.Initialized;
                }
            }
            
            return;
        }

		public static void ClearPru(int PRU) {
            Pru1Aio.Stop();
			Pru1Aio.pru_rta_clear_pru (PRU);
		}

		public static void Hello(string World) {
			Pru1Aio.pru_printf_hello (World);
		}

	}
}

