using System;

using JitHook.Agent.Core;

namespace JitHook.Agent.Hook
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