using UnityEngine.Assertions;

namespace Core.Multiplayer.Data
{
    public class TimeMessage : PayloadData
    {
        public int TickRate { get; }
        public int CurrentTick { get; }

        public TimeMessage(int tickRate, int currentTick)
        {
            TickRate = tickRate;
            CurrentTick = currentTick;
            payload = new(Payload.DataType.Time, new byte[2] { (byte)tickRate, (byte)currentTick });
        }

        public TimeMessage(in Payload payload)
        {
            Assert.IsTrue(payload.Type == Payload.DataType.Time, "Incorrect payload type");
            
            TickRate = payload.Data[0];
            CurrentTick = payload.Data[1];

            this.payload = payload;
        }

    }
}
