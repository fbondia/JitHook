using System;
using System.Reflection;
using System.Runtime.InteropServices;

using Bugscout.Agent.Core;

namespace Bugscout.Agent.Runtime
{

	[Serializable]
	public class MethodFilter
	{
		public string MethodNameFilter;
		public Guid Id;
		public Type Invoker;

		public MethodFilter(Type invokerType, string methodNameFilter)
		{
			this.MethodNameFilter = methodNameFilter;
			this.Id = Guid.NewGuid();
			this.Invoker = invokerType;
		}

		public FilteredMethod GetMethod(Headers.CorMethodInfo methodInfo)
		{

			FilteredMethod info = null;

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Module assemblyModule in assembly.GetModules())
				{
					if (info == null)
					{
						info = GetMethodInfoFromModule(methodInfo, assemblyModule);
					}
				}
			}

			return info;

		}

		internal bool IsMonitoredMethod(MethodBase methodBase)
		{
			string fullName = $"{methodBase.DeclaringType.FullName}.{methodBase.Name}";
			return fullName.StartsWith(MethodNameFilter, StringComparison.OrdinalIgnoreCase);
		}

		internal FilteredMethod GetMethodInfoFromModule(Headers.CorMethodInfo methodInfo, Module assemblyModule)
		{

			try
			{

				FieldInfo mPtr = assemblyModule.ModuleHandle.GetType().GetField("m_ptr", BindingFlags.Instance | BindingFlags.NonPublic);
				object mPtrValue = mPtr.GetValue(assemblyModule.ModuleHandle);
				FieldInfo mpData = mPtrValue.GetType().GetField("m_pData", BindingFlags.Instance | BindingFlags.NonPublic);

				if (mpData == null)
				{
					return null;
				}

				IntPtr mpDataValue = (IntPtr)mpData.GetValue(mPtrValue);
				if (mpDataValue != methodInfo.ModuleHandle)
				{
					return null;
				}

				short tokenNum = Marshal.ReadInt16(methodInfo.MethodHandle);

				MethodBase methodBase = assemblyModule.ResolveMethod(0x06000000 + tokenNum);
				Type declaringType = methodBase.DeclaringType;

				if (!IsMonitoredMethod(methodBase))
				{
					return null;
				}

				int numOfParameters = methodBase.GetParameters().Length;

				if (!methodBase.IsStatic)
				{
					numOfParameters++;
				}

				return new FilteredMethod(tokenNum, numOfParameters, methodBase, methodBase is ConstructorInfo, this);

			}
			catch (Exception ex)
			{
				//Console.Write(obj2.ToString());
				return null;
			}

		}
	}

}