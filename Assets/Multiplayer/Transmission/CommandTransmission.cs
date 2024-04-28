using System;

namespace Core.Multiplayer.DataTransmission
{
    public class CommandTransmission : Transmission
    {
        public enum Command : byte
        {
            Create,
            Destroy
        }

        public Command MCommand { get => (Command)Data[0]; set => Data[0] = (byte)value; }
        public ushort Index { get => (ushort)(Data[1] << 8 | Data[2]); }

        public CommandTransmission(Command cmd, ushort index) : base(typeof(CommandTransmission), 3)
        {
            MCommand = cmd;
            Data[1] = (byte)(index << 8);
            Data[2] = (byte)index;
        }
    }
}