using System;

namespace Core.Multiplayer.DataTransmission
{
    /// <summary>
    /// Wraps the serialization and deserialization of a byte into an arr
    /// </summary>
    public class ByteMember
    {
        public byte Value { get; private set; }
        public ByteMember(in ArraySegment<byte> body, int index, byte value) => Value = body[index] = value;
        public ByteMember(in ArraySegment<byte> body, int index) => Value = body[index];
    }
}