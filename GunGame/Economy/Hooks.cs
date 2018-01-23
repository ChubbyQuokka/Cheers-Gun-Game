using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using GunGame.API;
using GunGame.Managers;

using Rocket.API;

namespace GunGame.Economy
{
    public class UconomyHook : IEconomyHook
    {
        MethodInfo increaseBalanceMethod;
        FieldInfo uconomyInstanceField;
        FieldInfo databaseField;

        public string DeterminingAssembly => "Uconomy";

        public void Initialize()
        {
            increaseBalanceMethod = EconomyManager.EconomyAssembly.GetType("fr34kyn01535.Uconomy.DatabaseManager").GetMethod("IncreaseBalance", BindingFlags.Public | BindingFlags.Instance);
            uconomyInstanceField = EconomyManager.EconomyAssembly.GetType("fr34kyn01535.Uconomy.Uconomy").GetField("Instance", BindingFlags.Public | BindingFlags.Static);
            databaseField = EconomyManager.EconomyAssembly.GetType("fr34kyn01535.Uconomy.Uconomy").GetField("Database", BindingFlags.Public | BindingFlags.Instance);
        }

        public void IncreaseBalance(IRocketPlayer p, decimal amt)
        {
            object uconomyInstance = uconomyInstanceField.GetValue(null);
            object databaseInstance = databaseField.GetValue(uconomyInstance);
            increaseBalanceMethod.Invoke(databaseField, new object[] { p.Id, amt});
        }
    }

    public class AviEconomyHook : IEconomyHook
    {
        MethodInfo payAsServerMethod;

        public string DeterminingAssembly => "AviEconomy";

        public void Initialize()
        {
            payAsServerMethod = EconomyManager.EconomyAssembly.GetType("com.aviadmini.rocketmod.AviEconomy.Bank").GetMethod("PayAsServer", BindingFlags.Public | BindingFlags.Static);
        }

        public void IncreaseBalance(IRocketPlayer p, decimal increaseBy)
        {
            decimal final = 0;
            payAsServerMethod.Invoke(null, new object[] {p.Id, increaseBy, false, final, string.Empty});
        }
    }
}
