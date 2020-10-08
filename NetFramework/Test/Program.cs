using System;
using MockLibrary;

using Bugscout.Agent.Core;
using Bugscout.Agent.Hook;
using Bugscout.Agent.Runtime;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Security.Cryptography;
using System.Linq;

namespace Bugscout.Agent.Test
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
