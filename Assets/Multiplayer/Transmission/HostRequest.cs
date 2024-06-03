﻿using Core.Util;
using System;
using System.Text;

namespace Core.Multiplayer.DataTransmission
{
    /// <summary>
    /// A transmission to request to host a lobby
    /// </summary>
    internal class HostRequest : Transmission
    {
        private new const int HEADERSIZE = 1 + 1; // 7b maxClients + 1b publicVisible + 4b name Length + 4b password length
        private const int MaskPublic = 0b1000_0000;
        private const int PasswordMask = 0b1111_0000;

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
        private int NameLength
        {
            // Left 4 bits
            get => Body[1] >> 4;
            set => Body[1] = (byte)(Body[1] | (((byte)value << 4) & PasswordMask));
        }

        private int PasswordLength
        {
            // Right 4 bits
            get => Body[1] & ~PasswordMask;
            set => Body[1] = (byte)(Body[1] | ((byte)value & ~PasswordMask));
        }
        public string Name
        {
            get => OL.StringFromSpan(Body.Slice(HEADERSIZE, NameLength));
        }
        public string Password
        {
            get => OL.StringFromSpan(Body.Slice(HEADERSIZE + NameLength, PasswordLength));
        }

        /// <summary>
        /// Creates a transmission for requesting to host (Client-Side)
        /// </summary>
        /// <param name="name">The lobby name, 5 <= Length <= 16</param>
        /// <param name="password">The lobby password used to authenticate clients, 5 < Length < 16</param>
        /// <param name="publicVisible">Is the lobby publicly searchable</param>
        /// <param name="maxClients">Max number of player, must be less than 128</param>
        public HostRequest(string name, string password, bool publicVisible, byte maxClients) : base(typeof(HostRequest), (ushort)(HEADERSIZE + OL.GetWithinLength(name, password) + 1))
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
            NameLength = name.Length;
            PasswordLength = password.Length;

            // Copy name && pass
            var nameBody = Body.AsMemory(HEADERSIZE, name.Length);
            Encoding.ASCII.GetBytes(name).CopyTo(nameBody);
            var passBody = Body.AsMemory(HEADERSIZE + name.Length, password.Length);
            Encoding.ASCII.GetBytes(password).CopyTo(passBody);
        }
    }

}