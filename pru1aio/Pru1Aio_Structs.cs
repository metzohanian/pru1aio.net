using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;


namespace Pru1Aio
{
    public class Pru1AioEventArgs : EventArgs
    {
        public readonly MessageType Type;
        public readonly Reading MeanReading;
        public readonly bool WarmUp;
        public readonly int BufferStartIndex;
        public readonly int BufferSize;

        public Pru1AioEventArgs(MessageType Type, Reading MeanReading, bool WarmUp, int BufferStartIndex, int BufferSize)
        {
            this.Type = Type;
            this.MeanReading = MeanReading;
            this.WarmUp = WarmUp;
            this.BufferStartIndex = BufferStartIndex;
            this.BufferSize = BufferSize;
        }
    }

	[StructLayout (LayoutKind.Sequential, Pack=1)]
    public unsafe struct PruControl
    {
		public uint CurrentBuffer;			// 0x0		Complete
		public uint BufferCount;				// 0x4		Complete
		public byte BufferSize;				// 0x8		Complete
		public byte ChannelEnabledMask;		// 0x9		Complete
		public byte SampleAverage;			// 0xA		Complete
		public byte SampleSoc;				// 0xB		Complete
		public uint SampleRate;				// 0xC		Complete
		public byte SampleMode;				// 0x10		Complete
		public byte ChannelCount;				// 0x11		Complete
		public uint BufferMemoryBytes;		// 0X12		Complete
		public uint BufferPosition;			// 0X16		Complete
		public uint ReadCount;				// 0x1A		Complete
		public uint IepClockCount;			// 0x1E
		public uint WriteMask;				// 0x22
		public uint DigitalOut;				// 0x26
        [MarshalAs(UnmanagedType.ByValArray)]
		public unsafe fixed byte Scratch[14];			// 0x2A
	}


	[StructLayout (LayoutKind.Sequential, Pack=1)]
    public unsafe struct PruSharedMemory
    {
		public PruControl Control;
		public ushort* Buffer1;
		public ushort* Buffer2;
	}

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Pru1AioConditional
    {
        [MarshalAs(UnmanagedType.ByValArray)]
		public unsafe fixed char Name[33];
		public Comparator Condition;
		public Signal Signal;
		public ushort Comp1;
		public ushort Comp2;

		public int LastSignal;

		public TriggerState Triggered;
		public ushort TriggerCount;
	}

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Pru1AioConditions
    {
		public char Count;
		public Pru1AioConditional *Conditionals;
	}

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct Pru1AioReading {
		public byte Buffer;
        [MarshalAs(UnmanagedType.ByValArray)]
        public unsafe fixed ushort Readings[8];
		public ushort DigitalIn;
	}

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CallState
    {
		public Pru1AioReading *Readings;
		public Pru1AioReading BufferMean;
		public int Records;
		public int MaximumRecords;
		public int Signals;
		public IntPtr CallerState;
		public Pru1AioConditions *Conditions;
	}

    public class Reading
    {
        public byte Buffer;
        public ushort[] Readings;
        public ushort DigitalIn;

        public Reading()
        {
            Readings = new ushort[8];
        }
    }

    public struct Conditional
    {
        public string Name;
        public Comparator Condition;
        public Signal Signal;
        public ushort Comparator1;
        public ushort Comparator2;
        public TriggerState Triggered;
        public int Triggers;
        public uint LastSignal;

        public override string ToString()
        {
            return Name + ":\n\t" + Condition.ToString() + "\n\t" + Signal + "\n\t" + Comparator1 + "\n\t" + Comparator2;
        }
    }

}
