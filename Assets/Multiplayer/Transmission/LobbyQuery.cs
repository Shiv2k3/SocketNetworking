using Core.Util;

namespace Core.Multiplayer.DataTransmission
{
    public class LobbyQuery : Transmission
    {
        public TString Search;
        // TODO:
        // think of some query parameters
        public LobbyQuery(Transmission trms) : base(trms)
        {
            // Ensure all parameters are valid
        }
    }

}