using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

namespace Pru1Aio
{

	public partial class Pru1Aio
	{
        public unsafe AsynchronousCallBack AsynchCall;

        int Calls;
        int RunTimeMs;
        int CurrentRecord;
        int Signals;
        int LastBufferCount;
        List<int> _DroppedBuffers;

        bool _CaptureData;
        bool CaptureData
        {
            get
            {
                return _CaptureData;
            }
            set
            {
                _CaptureData = value;
            }
        }

        bool _capturing;
        bool _IsCapturing
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

        BufferMode Mode;
        bool RunLimit;

        Reading[] Readings;

        unsafe PruSharedMemory* PruMemory;
        unsafe CallState* CallState;
        unsafe Reading* Buffer;

        unsafe IntPtr IPruMem;
        unsafe IntPtr ICallState;
        unsafe IntPtr IBuffer;

		public Pru1Aio ()
		{
            // forces libprussdrv to load
            Pru1Aio.prussdrv_strversion(1);
            CaptureData = true;
        }

        public unsafe void Start(int RunTimeMs = 2000, int Readings = 0, BufferMode Mode = BufferMode.Fill)
        {
            this.Mode = Mode;
            this.RunTimeMs = RunTimeMs;
            RunLimit = false;

            if (Readings % PruMemory->Control.BufferSize != 0)
                throw new Exception("Readings must be a multiple of the buffer size.  Inter-buffer reading limits are not supported.");

            if (this.Mode == BufferMode.Fill || RunTimeMs > 0)
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
                    this.Readings = new Reading[RunTimeMs / 1000 * PruMemory->Control.SampleRate];
                    Calls = (int)(PruMemory->Control.SampleRate / PruMemory->Control.BufferSize * RunTimeMs / 1000);
                }
                else if (RunTimeMs == 0 && Readings > 0)
                {
                    this.Readings = new Reading[Readings];
                    Calls = Readings / PruMemory->Control.BufferSize;
                }
                else if (RunTimeMs > 0 && Readings > 0)
                {
                    this.Readings = new Reading[Readings];
                    Calls = (int)(PruMemory->Control.SampleRate / PruMemory->Control.BufferSize * RunTimeMs / 1000);
                }
            }

            _DroppedBuffers = new List<int>();

            Pru1Aio.pru_rta_configure(&(PruMemory->Control));

            Pru1Aio.pru_rta_start_firmware();

            Signals = 0;
            _IsCapturing = true;
            Pru1Aio.pru_rta_start_capture(PruMemory, Buffer, CallState, AsynchCall);
        }

        public unsafe void PrintControl(bool Addresses = false)
        {
            if (Addresses)
                Pru1Aio.print_pru_map_address(PruMemory);
            else
                Pru1Aio.print_pru_map(PruMemory);
        }

        public unsafe void Initialize()
        {

            IPruMem = Pru1Aio.pru_rta_init();
            ICallState = Pru1Aio.pru_rta_init_call_state();
            PruMemory = (PruSharedMemory*)IPruMem.ToPointer();
            CallState = (CallState*)ICallState.ToPointer();
            _IsCapturing = false;
        }

        public unsafe void Configure(int BufferSize, Channels ChannelEnabledMask, int SampleSoc, int SampleAverage, int SampleRate)
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

        }

        public unsafe void Stop()
        {
            _IsCapturing = false;
            CaptureData = false;
            Pru1Aio.pru_rta_stop_capture(PruMemory);
        }

        public unsafe void CallBack(uint BufferCount, ushort BufferSize, Reading* CapturedBuffer, CallState* CallState, PruSharedMemory* PruMemory)
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

            if (IsCapturing && CaptureData)
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
                CaptureData = false;

                Calls--;
                if (Calls == 0)
                    Pru1Aio.pru_rta_stop_capture(PruMemory);

                if (CurrentRecord >= Readings.Length)
                    Pru1Aio.pru_rta_stop_capture(PruMemory);
            }
            
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

