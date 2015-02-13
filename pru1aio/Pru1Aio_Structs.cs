using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Pru1Aio
{
	[StructLayout (LayoutKind.Sequential, Pack=1)]
	public unsafe struct PruControl {
		uint CurrentBuffer;			// 0x0		Complete
		uint BufferCount;				// 0x4		Complete
		byte BufferSize;				// 0x8		Complete
		byte ChannelEnabledMask;		// 0x9		Complete
		byte SampleAverage;			// 0xA		Complete
		byte SampleSoc;				// 0xB		Complete
		uint SampleRate;				// 0xC		Complete
		byte sample_mode;				// 0x10		Complete
		byte ChannelCount;				// 0x11		Complete
		uint BufferMemoryBytes;		// 0X12		Complete
		uint BufferPosition;			// 0X16		Complete
		uint ReadCount;				// 0x1A		Complete
		uint IepClockCount;			// 0x1E
		uint WriteMask;				// 0x22
		uint DigitalOut;				// 0x26
		fixed byte Scratch[14];			// 0x2A
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
	public unsafe struct PruSharedMemory {
		PruControl Control;
		ushort* Buffer1;
		ushort* Buffer2;
	}

	public unsafe struct Conditional {
		fixed char Name[33];
		Comparator Condition;
		Signal Signal;
		ushort Comp1;
		ushort Comp2;

		int LastSignal;

		TriggerState Triggered;
		ushort TriggerCount;
	}

	public unsafe struct Conditions {
		char Count;
		Conditional *Conditionals;
	}

	public unsafe struct Reading {
		byte Buffer;
		fixed ushort Readings[8];
		ushort DigitalIn;
	}

	public unsafe struct CallState {
		Reading *Readings;
		Reading BufferMean;
		int Records;
		int MaximumRecords;
		int Signals;
		IntPtr CallerState;
		Conditions *Conditions;
	}

}
