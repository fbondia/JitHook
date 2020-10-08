using System;

using Bugscout.Agent.Core;

namespace Bugscout.Agent.Hook
{

    [Serializable]
    public unsafe delegate int HookDelegate(
        Native.CompileMethodDeclaration compileMethod,
        IntPtr thisPtr,
        IntPtr corJitInfo,
        Headers.CorMethodInfo* methodInfoPtr,
        Headers.CorJitFlag flags,
        IntPtr nativeEntry,
        IntPtr nativeSizeOfCode);

}