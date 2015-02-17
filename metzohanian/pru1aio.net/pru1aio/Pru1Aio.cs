using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace Pru1Aio
{

    unsafe delegate void ProcessBufferDelegate(uint BufferCount, ushort BufferSize, Pru1AioReading* CapturedBuffer, CallState* CallState, PruSharedMemory* PruMemory);

    public partial class Pru1Aio
    {
        public static event EventHandler Message = delegate { };

        static unsafe AsynchronousCallBack AsynchCall;

        public static int Calls
        {
            get
            {
                return _Calls;
            }
        }
        public static int TotalRecords
        {
            get
            {
                return _TotalRecords;
            }
        }
        public static InitMode Status
        {
            get
            {
                return _Status;
            }
        }
        public static List<int> DroppedBuffers
        {
            get
            {
                return _DroppedBuffers;
            }
        }

        public static bool IsCapturing
        {
            get
            {
                return _IsCapturing;
            }
        }

        public static int Readings
        {
            get
            {
                if (Status == InitMode.NotReady)
                    throw new Exception("Pru1Aio not ready.");
                int readings = 0;
                unsafe
                {
                    readings = (int)Pru1Aio.PruMemory->Control.ReadCount;
                }
                return readings;
            }
        }

        public static uint DigitalOutput
        {
            set
            {
                DigitalOutputLocker.EnterWriteLock();
                _DigitalOutput = value;
                DigitalOutputLocker.ExitWriteLock();
            }
            get
            {
                uint dout;
                DigitalOutputLocker.EnterReadLock();
                dout = _DigitalOutput;
                DigitalOutputLocker.ExitReadLock();
                return dout;
            }
        }

        public static Reading MeanReading
        {
            get
            {
                return _MeanReading;
            }
        }

        private static Reading _MeanReading
        {
            get
            {
                Reading mread;
                MeanReadingLocker.EnterReadLock();
                mread = _MeanReading_;
                MeanReadingLocker.ExitReadLock();
                return mread;
            }
            set
            {
                MeanReadingLocker.EnterWriteLock();
                _MeanReading_ = value;
                MeanReadingLocker.ExitWriteLock();
            }
        }
        static uint _DigitalOutput;
        static ReaderWriterLockSlim DigitalOutputLocker = new ReaderWriterLockSlim();
        static Reading _MeanReading_;
        static ReaderWriterLockSlim MeanReadingLocker = new ReaderWriterLockSlim();
        static int _Calls;
        static int _TotalRecords;
        static int RunTimeMs;
        static int CurrentBufferIndex;
        static int Signals;
        static int LastBufferCount;
        static List<int> _DroppedBuffers;
        static bool _WarmUp = false;

        static InitMode _Status = InitMode.NotReady;

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

        //static Pru1AioReading[] _Buffer;
        public static Reading[] Buffer
        {
            get
            {
                return _Buffer;
            }
        }
        static Reading[] _Buffer;

        static unsafe PruSharedMemory* PruMemory;
        static unsafe CallState* CallState;
        static unsafe Pru1AioReading* CaptureBuffer;

        static unsafe IntPtr IPruMem;
        static unsafe IntPtr ICallState;
        static unsafe IntPtr IBuffer;

        private static unsafe Pru1Aio Aio;

        private static readonly object Locker = new object();

        private Pru1Aio()
        {
            // forces libprussdrv to load
            Pru1Aio.prussdrv_strversion(1);
            _Status = InitMode.NotReady;
        }

        public static void Start(int RunTimeMs = 2000, int Readings = 0, BufferMode Mode = BufferMode.Fill)
        {
            if (Status != InitMode.Configured)
                throw new Exception("Pru1Aio must be configured prior to operation.");

            lock (Pru1Aio.Locker)
            {
                Pru1Aio.Mode = Mode;
                Pru1Aio.RunTimeMs = RunTimeMs;
                RunLimit = false;

                unsafe
                {
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
                            //Pru1Aio._Buffer = new Pru1AioReading[RunTimeMs / 1000 * PruMemory->Control.SampleRate];
                            Pru1Aio._Buffer = Pru1Aio.InitializeArray<Reading>((int)(RunTimeMs / 1000 * PruMemory->Control.SampleRate));
                            _Calls = (int)(PruMemory->Control.SampleRate / PruMemory->Control.BufferSize * RunTimeMs / 1000);
                        }
                        else if (RunTimeMs == 0 && Readings > 0)
                        {
                            //Pru1Aio._Buffer = new Pru1AioReading[Readings];
                            Pru1Aio._Buffer = Pru1Aio.InitializeArray<Reading>((int)(Readings));
                            _Calls = Readings / PruMemory->Control.BufferSize;
                        }
                        else if (RunTimeMs > 0 && Readings > 0)
                        {
                            //Pru1Aio._Buffer = new Pru1AioReading[Readings];
                            Pru1Aio._Buffer = Pru1Aio.InitializeArray<Reading>((int)(Readings));
                            _Calls = (int)(PruMemory->Control.SampleRate / PruMemory->Control.BufferSize * RunTimeMs / 1000);
                        }
                    }
                    else if (Pru1Aio.Mode == BufferMode.Ring)
                    {
                        //Pru1Aio._Buffer = new Pru1AioReading[Readings];
                        Pru1Aio._Buffer = Pru1Aio.InitializeArray<Reading>((int)(Readings));
                        Pru1Aio.RunLimit = false;
                    }

                    _DroppedBuffers = new List<int>();

                    Pru1Aio.pru_rta_configure(&(PruMemory->Control));

                    Pru1Aio.pru_rta_start_firmware();

                    Signals = 0;
                    _IsCapturing = true;
                    _Status = InitMode.Running;
                    MessageInvoker(MessageType.Start, new Reading(), 0, 0);
                    Pru1Aio.pru_rta_start_capture(PruMemory, CaptureBuffer, CallState, AsynchCall);
                }
            }
        }

        public static void PrintControl(bool Addresses = false)
        {
            if (Status == InitMode.NotReady)
                throw new Exception("Pru1Aio not Initialized()");

            unsafe
            {
                if (Addresses)
                    Pru1Aio.print_pru_map_address(PruMemory);
                else
                    Pru1Aio.print_pru_map(PruMemory);
            }
        }

        public static void Initialize()
        {
            if (Status != InitMode.NotReady)
                throw new Exception("Pru1Aio may only be Initialized once.");

            lock (Locker)
            {
                unsafe
                {
                    if (Pru1Aio.Aio == null)
                        Pru1Aio.Aio = new Pru1Aio();

                    IPruMem = Pru1Aio.pru_rta_init();
                    ICallState = Pru1Aio.pru_rta_init_call_state();
                    PruMemory = (PruSharedMemory*)IPruMem.ToPointer();
                    CallState = (CallState*)ICallState.ToPointer();
                }

                _IsCapturing = false;
                _Status = InitMode.Initialized;
            }
        }

        public static void Configure(int BufferSize, Channels ChannelEnabledMask, int SampleSoc, int SampleAverage, int SampleRate)
        {
            if (Status != InitMode.Initialized)
                throw new Exception("Pru1Aio must be initialized prior to Configuration.");

            lock (Locker)
            {
                unsafe
                {
                    AsynchCall = new AsynchronousCallBack(ProcessBuffer);

                    PruMemory->Control.BufferCount = 0;
                    PruMemory->Control.BufferSize = (byte)BufferSize; // 40;
                    PruMemory->Control.ChannelEnabledMask = (byte)ChannelEnabledMask; // 0x7F;
                    PruMemory->Control.SampleSoc = (byte)SampleSoc; // 15;
                    PruMemory->Control.SampleAverage = (byte)SampleAverage; // 16;
                    PruMemory->Control.SampleRate = (uint)SampleRate; // 1000;

                    IBuffer = Pru1Aio.pru_rta_init_capture_buffer(PruMemory);
                    CaptureBuffer = (Pru1AioReading*)IBuffer.ToPointer();
                }

                _IsCapturing = false;
                _Status = InitMode.Configured;
                _TotalRecords = 0;
                CurrentBufferIndex = 0;
            }
        }

        public static void Stop()
        {
            _IsCapturing = false;
            _Status = InitMode.Initialized;
            unsafe
            {
                Pru1Aio.pru_rta_stop_capture(PruMemory);
                MessageInvoker(MessageType.Stop, new Reading(), 0, 0);
            }
        }

        private static unsafe void ProcessBuffer(uint BufferCount, ushort BufferSize, Pru1AioReading* CapturedBuffer, CallState* CallState, PruSharedMemory* PruMemory)
        {
            Signals++;
            
            LastBufferCount = (int)PruMemory->Control.BufferCount;

            Reading mread = new Reading();

            for (int i = 0; i < 8; i++)
            {
                mread.Readings[i] = CallState->BufferMean.Readings[i];
            }
            mread.Buffer = CallState->BufferMean.Buffer;
            mread.DigitalIn = CallState->BufferMean.DigitalIn;
            _MeanReading = mread;

            int LastGoodBuffer = Signals;

            if (Signals < LastBufferCount)
            {
                do
                {
                    DropBuffer(Signals, mread, LastGoodBuffer, (int)BufferSize);
                    Signals++;
                } while (Signals < LastBufferCount);
            }

            MessageInvoker(MessageType.Notification, mread, CurrentBufferIndex, (int)BufferSize);

            Pru1Aio.pru_rta_set_digital_out(PruMemory, 0xF, _DigitalOutput);

            if (IsCapturing && Status == InitMode.Running)
            {
                for (int record = 0; record < BufferSize; record++)
                {
                    if (CurrentBufferIndex >= _Buffer.Length)
                    {
                        if (Mode == BufferMode.Ring)
                        {
                            CurrentBufferIndex = 0;
                            MessageInvoker(MessageType.Ring, mread, CurrentBufferIndex, (int)BufferSize);
                        }
                        else
                        {
                            MessageInvoker(MessageType.Overflow, mread, CurrentBufferIndex, (int)BufferSize);
                            _IsCapturing = false;
                            break;
                        }
                    }
                    for (int channel = 0; channel < PruMemory->Control.ChannelCount; channel++)
                    {
                        /*
                        fixed (Pru1AioReading* readings = _Buffer)
                        {
                            readings[CurrentBufferIndex].Readings[channel] = CapturedBuffer[record].Readings[channel];
                        }
                         */
                        _Buffer[CurrentBufferIndex].Readings[channel] = CapturedBuffer[record].Readings[channel];
                    }
                    _Buffer[CurrentBufferIndex].DigitalIn = CapturedBuffer[record].DigitalIn;
                    _Buffer[CurrentBufferIndex].Buffer = CapturedBuffer[record].Buffer;
                    CurrentBufferIndex++;
                    _TotalRecords++;
                }
            }

            if (RunLimit)
            {

                _Calls--;
                if (_Calls == 0)
                {
                    Pru1Aio.pru_rta_stop_capture(PruMemory);
                    _Status = InitMode.Initialized;
                    MessageInvoker(MessageType.Stop, new Reading(), 0, 0);
                }

                if (CurrentBufferIndex >= _Buffer.Length && Pru1Aio.Mode == BufferMode.Fill)
                {
                    Pru1Aio.pru_rta_stop_capture(PruMemory);
                    _Status = InitMode.Initialized;
                    MessageInvoker(MessageType.Stop, new Reading(), 0, 0);
                }

            }

            return;
        }

        public static void WarmUp()
        {
            _WarmUp = true;
            Pru1Aio.Configure(10, Channels.AllChannels, 15, 16, 1000);
            Pru1Aio.Start(500, 100, BufferMode.Ring);
            _WarmUp = false;
        }

        public static void ClearPru(int PRU)
        {
            Pru1Aio.Stop();
            Pru1Aio.pru_rta_clear_pru(PRU);
        }

        public static void Hello(string World)
        {
            Pru1Aio.pru_printf_hello(World);
        }

        private static void DropBuffer(int Buffer, Reading MeanReading, int LastGoodBuffer, int BufferCount)
        {
            _DroppedBuffers.Add(Buffer);
            MessageInvoker(MessageType.Underflow, MeanReading, 0, 0);
        }

        private static void MessageInvoker(MessageType Type, Reading MeanReading, int BufferStartIndex, int BufferCount)
        {
            Delegate[] subscribers = Message.GetInvocationList();
            try
            {
                for (int i = 0; i < subscribers.Length; i++)
                {
                    EventArgs args = new EventArgs();
                    EventHandler eventHandler = (EventHandler)subscribers[i];
                    eventHandler.BeginInvoke(null, new Pru1AioEventArgs(Type, MeanReading, _WarmUp, BufferStartIndex, BufferCount), EndAsyncEvent, null);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static void EndAsyncEvent(IAsyncResult iar)
        {

            var ar = (System.Runtime.Remoting.Messaging.AsyncResult)iar;
            var invokedMethod = (EventHandler)ar.AsyncDelegate;

            try
            {
                invokedMethod.EndInvoke(iar);
            }
            catch
            {

            }
        }

        static T[] InitializeArray<T>(int length) where T : new()
        {
            T[] array = new T[length];
            for (int i = 0; i < length; ++i)
            {
                array[i] = new T();
            }

            return array;
        }
    }

}

