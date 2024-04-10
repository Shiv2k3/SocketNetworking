using System.Text;
using UnityEngine.Assertions;

namespace Core.Multiplayer.Data
{
    public class TextMessage : PayloadData
    {
        public string Message;
        public TextMessage(string msg)
        {
            Message = msg;
            payload = new(Payload.DataType.Text, Encoding.UTF8.GetBytes(msg));
        }
        public TextMessage(in Payload payload)
        {
            Assert.IsTrue(payload.Type == Payload.DataType.Text, "Incorrect payload type");
            Message = Encoding.UTF8.GetString(payload.Data);
            this.payload = payload;
        }
    }
}
