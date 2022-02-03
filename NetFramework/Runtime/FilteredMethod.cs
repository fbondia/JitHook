using System;
using System.Reflection;

namespace JitHook.Agent.Runtime
{

	[Serializable]
	public class FilteredMethod
	{

		public short TokenNum;
		public int NumOfArgumentsToPushInTheStack;
		public MethodBase Method;
		public bool IsConstructor;
		public MethodFilter Filter;

		public FilteredMethod(short tokenNum, int numOfArgs, MethodBase method, bool isConstructor, MethodFilter filter)
		{
			this.TokenNum = tokenNum;
			this.NumOfArgumentsToPushInTheStack = numOfArgs;
			this.Method = method;
			this.IsConstructor = isConstructor;
			this.Filter = filter;
		}

	}

}