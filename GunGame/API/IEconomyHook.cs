using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rocket.API;

namespace GunGame.API
{
    public interface IEconomyHook
    {
        string DeterminingAssembly { get; }
        void Initialize();
        void IncreaseBalance(IRocketPlayer p, decimal amt);
    }
}
