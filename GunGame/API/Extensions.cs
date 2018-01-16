using GunGame;
using GunGame.API;

using Rocket.Unturned.Player;

using Steamworks;

namespace Rocket.API
{
    public static class IRocketPlayerExtensions
    {
        public static bool HasPermissionFor(this IRocketPlayer player, IGunGameCommand command)
        {
            return (byte)command.PermissionLevel <= (byte)((UnturnedPlayer)player).GunGamePlayer().pLevel;
        }
    }
}

namespace Rocket.Unturned.Player
{
    public static class UnturnedPlayerExtensions
    {
        public static GunGamePlayerComponent GunGamePlayer(this UnturnedPlayer player)
        {
            return player.GetComponent<GunGamePlayerComponent>();
        }
    }
}

namespace System
{
    public static class UInt64Extensions
    {
        public static UnturnedPlayer GetPlayer(this ulong id)
        {
            return UnturnedPlayer.FromCSteamID(new CSteamID(id));
        }
    }

    public static class EnumExtensions
    {
        public static bool HasFlags(this Enum x, Enum y)
        {
            byte _x = Convert.ToByte(x);
            byte _y = Convert.ToByte(y);

            return (_x & _y) == _y;
        }
    }
}