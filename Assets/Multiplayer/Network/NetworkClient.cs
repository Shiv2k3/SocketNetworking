using Core.Multiplayer.DataTransmission;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Core.Multiplayer
{
    public class NetworkClient
    {
        public static readonly Queue<Transmission> Broadcasts = new();

        public readonly Socket Socket;

        public enum TransmisionState 
        {
            None,
            Receiving,
            Complete
        }
        public Transmission CurrentTransmission;
        public TransmisionState state;

        public bool TransmissionAvailable => Socket.Available >= Transmission.HEADERSIZE;

        public NetworkClient(Socket socket)
        {
            Socket = socket;
        }

        public void Disconnect()
        {
            Socket.Close();
        }

        public async Task<Transmission> TryGetTransmission()
        {
            if (state == TransmisionState.Receiving)
            {
                return await CompleteTransmission(CurrentTransmission);
            }

            if (state == TransmisionState.None || state == TransmisionState.Complete && TransmissionAvailable)
            {
                byte[] header = new byte[Transmission.HEADERSIZE];
                await Receive(header);
                CurrentTransmission = new Transmission(header);
                state = TransmisionState.Receiving;

                return await CompleteTransmission(CurrentTransmission);
            }

            return null;
        }

        private async Task<Transmission> CompleteTransmission(Transmission transmission)
        {
            if (Socket.Available < transmission.Length)
                return null;

            byte[] data = new byte[transmission.Length];
            await Receive(data);
            state = TransmisionState.Complete;

            return CurrentTransmission = new Transmission(transmission.Payload, data);
        }

        public async Task Send(byte[] payload)
        {
            int count = 0;
            do
            {
                if (count > payload.Length) throw new Exception("Count exceeded Length during Send()");
                var segment = new ArraySegment<byte>(payload, count, payload.Length - count);
                count += await Socket.SendAsync(segment, SocketFlags.None);
            }
            while (count != payload.Length);
        }

        public async Task Receive(byte[] arr)
        {
            int count = 0;
            do
            {
                if (count > arr.Length) throw new Exception("Count exceeded Length during Receive()");
                var segment = new ArraySegment<byte>(arr, count, arr.Length - count);
                count += await Socket.ReceiveAsync(segment, SocketFlags.None);
            }
            while (count != arr.Length);
        }
    }
}
