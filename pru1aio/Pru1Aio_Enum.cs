using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Pru1Aio
{
    public enum UnderflowMode
    {
        Continue,
        Panic
    }

    public enum BufferMode
    {
        Fill,
        Overflow,
        Ring
    }

    public enum PRU_NUM
    {
        PRU0 = 0,
        PRU1 = 1
    }

    public enum Channels
    {
        Channel0 = 0x1,
        Channel1 = 0x2,
        Channel2 = 0x4,
        Channel3 = 0x8,
        Channel4 = 0x10,
        Channel5 = 0x20,
        Channel6 = 0x40,
        AllChannels = 0x7F
    }

    public enum Comparator
    {
        Greater = 0,
        GreaterEq,
        Less,
        LessEq,
        Equal,
        RisingEdge,
        FallingEdge
    }

    public enum Signal
    {
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

    public enum TriggerState
    {
        NOT_TRIGGERED = 0,
        TRIGGERED
    }

}