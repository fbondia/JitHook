using System;
using System.Reflection.Emit;

namespace Bugscout.Agent.Runtime
{

	[Serializable]
	internal sealed class TrampolineMethodInfo
	{

		internal MethodBuilder Builder;
		internal string MethodName;
		internal int PatchOffset;

		internal TrampolineMethodInfo(MethodBuilder builder, string methodName, int patchOffset)
		{
			Builder = builder;
			MethodName = methodName;
			PatchOffset = patchOffset;
		}

	}

}
