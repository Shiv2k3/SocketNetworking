using Core.Util;
using System;

namespace Core.Multiplayer.DataTransmission
{
    /// <summary>
    /// A transmission to request to host a lobby
    /// </summary>
    internal class HostRequest : Transmission
    {
        private new const int HEADERSIZE = 1; // 7b maxClients + 1b publicVisible
        private const int MaskPublic = 128;

        public bool PublicVisible
        {
            get => (Body[0] & MaskPublic) == MaskPublic;
            set => Body[0] = (byte)(Body[0] | (value ? MaskPublic : 0));
        }
        public byte MaxClients
        {
            get => (byte)(Body[0] & ~MaskPublic);
            set => Body[0] = (byte)(value & ~MaskPublic | Body[0] & MaskPublic);
        }

        public TString Name;
        public TString Password;

        /// <summary>
        /// Creates a transmission for requesting to host (Client-Side)
        /// </summary>
        /// <param name="name">The lobby name, 5 <= Length <= 16</param>
        /// <param name="password">The lobby password used to authenticate clients, 5 < Length < 16</param>
        /// <param name="publicVisible">Is the lobby publicly searchable</param>
        /// <param name="maxClients">Max number of player, must be less than 128</param>
        public HostRequest(string name, string password, bool publicVisible, byte maxClients) : base(typeof(HostRequest), (ushort)(HEADERSIZE + OL.GetTStringLength(name, password)))
        {
            if (name.Length < 5 || name.Length > 16)
                throw new ArgumentOutOfRangeException($"Lobby name length {name.Length} is out of range");
            if (password.Length < 5 || password.Length > 16)
                throw new ArgumentOutOfRangeException($"Lobby password length {password.Length} is out of range");
            if ((maxClients & MaskPublic) == MaskPublic)
                throw new ArgumentException("Last bit was set");

            // Setup header
            PublicVisible = publicVisible;
            MaxClients = maxClients;

            // Setup name & pass
            int start = HEADERSIZE;
            Name = new(name, Body, start);
            Password = new(password, Body, start + Name.Length);
        }
    }

}