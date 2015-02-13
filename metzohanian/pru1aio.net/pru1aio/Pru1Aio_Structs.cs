using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Pru1Aio
{
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

	public enum Comparator {
		Greater = 0,
		GreaterEq,
		Less,
		LessEq,
		Equal,
		RisingEdge,
		FallingEdge
	}

	public enum Signal {
		CHANNEL_0 = 0,
		CHANNEL_1,
		CHANNEL_2,
		CHANNEL_3,
		CHANNEL_4,
		CHANNEL_5,
		CHANNEL_6,
		CHANNEL_7,
		CHANNEL_DIO
	}

	public enum TriggerState {
		NOT_TRIGGERED = 0,
		TRIGGERED
	}

	[StructLayout (LayoutKind.Sequential, Pack=1)]
    public unsafe struct PruSharedMemory
    {
		public PruControl Control;
		public ushort* Buffer1;
		public ushort* Buffer2;
	}

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Conditional
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
    public unsafe struct Conditions
    {
		public char Count;
		public Conditional *Conditionals;
	}

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct Reading {
		public byte Buffer;
        [MarshalAs(UnmanagedType.ByValArray)]
        public unsafe fixed ushort Readings[8];
		public ushort DigitalIn;
	}

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CallState
    {
		public Reading *Readings;
		public Reading BufferMean;
		public int Records;
		public int MaximumRecords;
		public int Signals;
		public IntPtr CallerState;
		public Conditions *Conditions;
	}

}
