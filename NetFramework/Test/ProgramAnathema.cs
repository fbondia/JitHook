﻿using System;
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
    class ProgramAnathema
    {

        //private static Headers.Platform platform = Headers.Platform.MACOS;
        private static Headers.Platform platform = Headers.Platform.WIN;

        private unsafe static JitHook CreateSut(String methodNameFilter, Type monitorType = null)
        {
            var sut = new RuntimeDispatcher(platform);
            var hook = new JitHook(platform);
            hook.InstallHook(new HookDelegate(sut.CompileMethod));
            sut.AddFilter(monitorType ?? typeof(TestMethodMonitor), methodNameFilter);

            return hook;
        }

        private static void MyInterceptor()
        {
            
            var sut = CreateSut("System.Console.ReadLine", typeof(SayHelloMonitor));

            sut.Start();
            String line = Console.ReadLine();
            sut.Stop();

        }

        private static void Intercept_constructor_method()
        {
            var sut = CreateSut("MockLibrary.SayHello..ctor");

            sut.Start();
            var mock = new SayHello("World");
            sut.Stop();
        }

        private static void Intercept_instance_method()
        {
            var sut = CreateSut("MockLibrary.SayHello.Speak", typeof(SayHelloMonitor));

            sut.Start();
            var mock = new SayHello("World");
            mock.Speak();
            sut.Stop();
        }

        private static void Intercept_a_static_method_with_no_parameters()
        {
            var sut = CreateSut("MockLibrary.StaticClass.Method1");

            sut.Start();
            StaticClass.Method1();
            sut.Stop();
        }

        private static void Intercept_a_static_method_with_one_parameters()
        {
            var sut = CreateSut("MockLibrary.StaticClass.Method2");

            sut.Start();
            StaticClass.Method2(new object());
            sut.Stop();
        }

        private static void Intercept_a_static_method_with_two_parameters_of_different_type()
        {
            var sut = CreateSut("MockLibrary.StaticClass.Method3");

            sut.Start();
            StaticClass.Method3("Hello", 31337);
            sut.Stop();
        }

        private static void Intercept_a_static_method_with_one_parameters_and_monitor()
        {
            var sut = CreateSut("MockLibrary.StaticClass.Method2Monitored");

            sut.Start();
            StaticClass.Method2(new object());
            sut.Stop();
        }

        private static void Intercept_a_static_method_which_raise_an_Exception()
        {
            var sut = CreateSut("MockLibrary.StaticClass.Method4");

            sut.Start();
            StaticClass.Method4();
            sut.Stop();
        }

        private static void Intercept_a_static_method_that_return_a_Boolean()
        {
            var sut = CreateSut("MockLibrary.StaticClass.Method5");

            sut.Start();
            StaticClass.Method5();
            sut.Stop();
        }

        private static void Intercept_instance_method_return_boolean()
        {
            var sut = CreateSut("MockLibrary.SayHello.IsEven", typeof(SayHelloMonitor));

            sut.Start();
            var mock = new SayHello("World");
            Console.WriteLine(mock.IsEven(2));
            sut.Stop();
        }

        private static void Intercept_constructor_two_parameters()
        {            
            var sut = CreateSut("MockLibrary.SimpleClass..ctor", typeof(SayHelloMonitor));

            sut.Start();
            var mock = new SimpleClass("World", 2);
            sut.Stop();
        }

        private static void Intercept_superclass_constructor()
        {
            var sut = CreateSut("MockLibrary.SaySuperHello..ctor", typeof(SayHelloMonitor));

            sut.Start();
            var mock = new SaySuperHello(2, "World");
            mock.Speak();
            sut.Stop();
        }

        private static void Intercept_instance_virtual_method()
        {
            var sut = CreateSut("MockLibrary.SaySuperHello.Speak", typeof(SayHelloMonitor));

            sut.Start();
            var mock = new SaySuperHello(2, "World");
            mock.Speak();
            sut.Stop();
        }

        private static void Intercept_set_property()
        {
            var sut = CreateSut("MockLibrary.SayHello.set_Prop1", typeof(SayHelloMonitor));

            sut.Start();
            var mock = new SayHello("World");
            mock.Prop1 = 1234.56;
            sut.Stop();
        }

        private static void Intercept_method_receiving_a_user_defined_class()
        {
            var sut = CreateSut("MockLibrary.StaticClass.Method6_B");

            sut.Start();
            StaticClass.Method6_A();
            sut.Stop();
        }

        private static void Intercept_method_receiving_enum()
        {
            var sut = CreateSut("MockLibrary.SayHello.ReceiveEnum", typeof(SayHelloMonitor));

            sut.Start();
            var mock = new SayHello("World");
            mock.PassEnum();
            sut.Stop();
        }

        private static void Intercept_method_receiving_struct()
        {
            var sut = CreateSut("MockLibrary.StaticClass.Method7");

            sut.Start();
            var s = new MyStruct(10.50, "some string");
            StaticClass.Method7(s);
            sut.Stop();
        }

        public static void Execute()
        {

            var sut = CreateSut("System.Console.ReadLine", typeof(SayHelloMonitor));

            var a1 = AppDomain.CurrentDomain.GetAssemblies();

            AppDomain.CurrentDomain.AssemblyLoad += delegate (object sender, AssemblyLoadEventArgs aargs)
            {
                Console.WriteLine(aargs.ToString());
            };

            AssemblyName fullName = new AssemblyName("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            fullName.ProcessorArchitecture = ProcessorArchitecture.MSIL;

            var assembly = Assembly.Load(fullName);

            Console.WriteLine("Public types in assembly {0}:", assembly.FullName);

            foreach (var t in assembly.GetTypes().Where(x=>x.Name.Contains("Console"))) {
                Console.WriteLine("   {0}", t.FullName);
            }

            var a2 = AppDomain.CurrentDomain.GetAssemblies();


            sut.Start();
            String line = Console.ReadLine();
            sut.Stop();




            // don't modify the execution order, otherwise it may affect the test result            
            /*
            Intercept_a_static_method_with_no_parameters();
            Intercept_a_static_method_with_one_parameters();
            Intercept_a_static_method_with_one_parameters_and_monitor();
            Intercept_a_static_method_with_two_parameters_of_different_type();
            Intercept_constructor_method();
            Intercept_instance_method();
            Intercept_a_static_method_which_raise_an_Exception();
            Intercept_a_static_method_that_return_a_Boolean();
            Intercept_instance_method_return_boolean();
            Intercept_constructor_two_parameters();
            Intercept_superclass_constructor();
            Intercept_instance_virtual_method();
            Intercept_set_property();
            Intercept_method_receiving_a_user_defined_class();
            Intercept_method_receiving_enum();
            Intercept_method_receiving_struct();
            */

            System.Console.WriteLine("Hey, Press <ENTER> to finish...");
            System.Console.ReadLine();

        }
    }
}
