using System;

using Rocket.API;

namespace GunGame.API
{
    public interface IGunGameCommand
    {
        void Execute(IRocketPlayer caller, string[] args);

        string Help { get; }

        EPermissionLevel PermissionLevel { get; }

        ECommandTiming CommandTiming { get; }
    }

    public enum EPermissionLevel : byte
    {
        NONE,
        LOW,
        MEDIUM,
        HIGH
    }

    [Flags]
    public enum ECommandTiming : byte
    {
        RUNNING = 1,
        WAITING = 2,
        STOPPED = 4,
    }
}

namespace GunGame.API.Exceptions
{
    public class GunGameException : Exception
    {
        public EExceptionType type;
        public EPermissionLevel level;
        public ECommandTiming timing;

        public GunGameException(EExceptionType type)
        {
            this.type = type;
        }

        public GunGameException(EPermissionLevel level)
        {
            type = EExceptionType.INVALID_PERMS;
            this.level = level;
        }

        public GunGameException(ECommandTiming timing)
        {
            type = EExceptionType.INVALID_TIME;
            this.timing = timing;
        }
    }

    public enum EExceptionType
    {
        INVALID_CMD,
        INVALID_ARGS,
        INVALID_PERMS,
        INVALID_TIME
    }
}