using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bugscout.Agent.Test
{
    #region Example

    /// <summary>
    /// A simple program to test Injection
    /// </summary>
    class ProgramReplaceMethod
    {


        // https://github.com/juliusfriedman/net7mma/blob/master/Concepts/Classes/MethodHelper.cs

        public static void Execute()
        {
            Target targetInstance = new Target();

            System.Type targetType = typeof(Target);

            System.Type destinationType = targetType;

            targetInstance.test();

            //Injection.install(1);

            MethodHelper.Redirect(targetType, "targetMethod1", targetType, "injectionMethod1");

            //Injection.install(2);

            MethodHelper.Redirect(targetType, "targetMethod2", targetType, "injectionMethod2");

            //Injection.install(3);

            MethodHelper.Redirect(targetType, "targetMethod3", targetType, "injectionMethod3");

            //Injection.install(4);

            MethodHelper.Redirect(targetType, "targetMethod4", targetType, "injectionMethod4");

            targetInstance.test();

            Console.Read();
        }
    }

    internal class Target
    {
        public void test()
        {
            targetMethod1();
            System.Diagnostics.Debug.WriteLine(targetMethod2());
            targetMethod3("Test");
            targetMethod4();
        }

        private void targetMethod1()
        {
            System.Diagnostics.Debug.WriteLine("Target.targetMethod1()");

        }

        private string targetMethod2()
        {
            System.Diagnostics.Debug.WriteLine("Target.targetMethod2()");
            return "Not injected 2";
        }

        public void targetMethod3(string text)
        {
            System.Diagnostics.Debug.WriteLine("Target.targetMethod3(" + text + ")");
        }

        private void targetMethod4()
        {
            System.Diagnostics.Debug.WriteLine("Target.targetMethod4()");
        }

        private void injectionMethod1()
        {
            System.Diagnostics.Debug.WriteLine("Injection.injectionMethod1");
        }

        private string injectionMethod2()
        {
            System.Diagnostics.Debug.WriteLine("Injection.injectionMethod2");
            return "Injected 2";
        }

        private void injectionMethod3(string text)
        {
            System.Diagnostics.Debug.WriteLine("Injection.injectionMethod3 " + text);
        }

        private void injectionMethod4()
        {
            System.Diagnostics.Process.Start("calc");
        }
    }

    #endregion


}
