using System;

namespace Core.Multiplayer.Connections
{
    public partial class Network
    {

        public class InvalidAction : Exception
        {
            public InvalidAction(string message) : base(message) { }
        }
    }
}