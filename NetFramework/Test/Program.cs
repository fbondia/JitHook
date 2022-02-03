using System;
using MockLibrary;

using JitHook.Agent.Core;
using JitHook.Agent.Hook;
using JitHook.Agent.Runtime;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Security.Cryptography;
using System.Linq;

namespace JitHook.Agent.Test
{
    class Program
    {

        static void Main(string[] args)
        {
            ProgramHarmony.Execute();


            Console.WriteLine("PRESS...");
            Console.ReadKey();

            Console.ReadLine();

            //ProgramAnathema.Execute();
            //ProgramReplaceMethod.Execute();

        }
    }
}
