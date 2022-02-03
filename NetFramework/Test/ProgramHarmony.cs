using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JitHook.Agent.Test
{

    public class ProgramHarmony
    {

        public static void Execute()
        {
            var harmony = new Harmony("xxx.JitHook.test");

            var mOriginal = AccessTools.Method(typeof(Console), "ReadKey");
            var mPrefix = SymbolExtensions.GetMethodInfo(() => MyPrefix());
            var mPostfix = SymbolExtensions.GetMethodInfo(() => MyPostfix());

            harmony.Patch(mOriginal, new HarmonyMethod(mPrefix), new HarmonyMethod(mPostfix));
        }

        public static void MyPrefix()
        {
            Console.WriteLine("prefixing");
        }

        public static void MyPostfix()
        {
            Console.WriteLine("postfixing");
        }


    }

}
