using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pru1Aio
{
    public delegate void ConditionsTriggeredDelegate(List<Conditional> ConditionsTriggered);

    public class Conditions
    {
        private List<Conditional> Conditionals;
        public bool CheckEach = false;
        public event ConditionsTriggeredDelegate Triggered = delegate { };

        public Conditions()
        {
            Conditionals = new List<Conditional>();
            Pru1Aio.Message += (sender, e) =>
            {
                Subscriber(sender, (Pru1AioEventArgs)e);
            };
        }

        public void Add(Conditional Condition)
        {
            Conditionals.Add(Condition);
        }

        public void Remove(Conditional Condition)
        {
            Conditionals.Remove(Condition);
        }

        public void Clear()
        {
            Conditionals.Clear();
        }

        public void Subscriber(object sender, Pru1AioEventArgs e)
        {
            if (e.WarmUp || e.Type != MessageType.Notification)
                return;

            List<Conditional> ConditionsTriggered = new List<Conditional>();
            for (int i = 0; i < Conditionals.Count; i++)
            {
                Conditional c = Conditionals[i];
                c.Triggers = 0;
                c.Triggered = TriggerState.NOT_TRIGGERED;
                if (CheckEach)
                {
                    for (int readings = e.BufferStartIndex; readings < e.BufferStartIndex + e.BufferSize; readings++)
                    {
                        c.Triggers += CheckCondition(ref c, Pru1Aio.Buffer[readings]) ? 1 : 0;
                    }
                }
                else
                {
                    c.Triggers = CheckCondition(ref c, e.MeanReading) ? 1 : 0;
                }
                c.Triggered = c.Triggers > 0 ? TriggerState.TRIGGERED : TriggerState.NOT_TRIGGERED;
                Conditionals[i] = c;

                if (c.Triggered == TriggerState.TRIGGERED)
                    ConditionsTriggered.Add(c);
            }
            Triggered(ConditionsTriggered);
        }

        private bool CheckCondition(ref Conditional Condition, Reading MeanReading)
        {
            uint signal;
            if (Condition.Signal == Signal.CHANNEL_DIO)
            {
                signal = (uint)((MeanReading.DigitalIn & (0x1 << Condition.Comparator1)) > 0 ? 1 : 0);
            }
            else
            {
                signal = (uint)MeanReading.Readings[(int)Condition.Condition];
            }
            uint lastsignal = Condition.LastSignal;
            Condition.LastSignal = signal;
            switch (Condition.Condition)
            {
                case Comparator.Greater: return signal > Condition.Comparator1;
                case Comparator.GreaterEq: return signal >= Condition.Comparator1;
                case Comparator.Less: return signal < Condition.Comparator1;
                case Comparator.LessEq: return signal <= Condition.Comparator1;
                case Comparator.Equal: return signal == Condition.Comparator1;
                case Comparator.RisingEdge: return signal == 1 && lastsignal == 0;
                case Comparator.FallingEdge: return signal == 0 && lastsignal == 1;
            }
            return false;
        }
    }
}
