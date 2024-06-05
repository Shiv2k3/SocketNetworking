using Core.Util;

namespace Core.Multiplayer.DataTransmission
{
#nullable enable
    /// <summary>
    /// Query for lobby search
    /// </summary>
    public class LobbyQuery : Transmission
    {
        public ByteString? Search;
        public StringArray? Lobbies;

        /// <summary>
        /// Creates query, client-side
        /// </summary>
        /// <param name="search">Lobby name</param>
        public LobbyQuery(string search) : base(typeof(LobbyQuery), OL.GetByteStringLength(search))
        {
            Search = new(search, Body, 0);
        }

        /// <summary>
        /// Creates query reply, server-side, only has Lobbies
        /// </summary>
        /// <param name="trms"></param>
        public LobbyQuery(Transmission trms) : base(trms)
        {
            Lobbies = new(Body, 0);
        }
    }

}