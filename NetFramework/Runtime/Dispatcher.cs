using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Bugscout.Agent.Runtime
{

	public static class Dispatcher
	{

		public static void DispatchCallback(string assemblyLocation, object[] argv)
		{
			if (File.Exists(assemblyLocation))
			{
				MethodBase methodBase;

				try
				{
					StackTrace stackTrace = new StackTrace();
					StackFrame[] frames = stackTrace.GetFrames();

                    methodBase = frames[2].GetMethod();
				}
				catch (Exception ex)
				{
					//Console.Write(obj.ToString());
					methodBase = null;
				}

				Type[] types = Assembly.Load(File.ReadAllBytes(assemblyLocation)).GetTypes();

				foreach (Type type in types)
				{
					try
					{
						if (type.Name.EndsWith("Monitor") && !type.IsAbstract)
						{
							ConstructorInfo monitorConstructor = type.GetConstructor(new Type[2]
							{
							    typeof(MethodBase),
							    typeof(object[])
							});

							monitorConstructor.Invoke(new object[2]
							{
								methodBase,
								argv
							});

						}
					}
					catch (Exception ex)
					{
						// Console.Write(obj4.ToString());
					}

				}

			}

		}

	}

}