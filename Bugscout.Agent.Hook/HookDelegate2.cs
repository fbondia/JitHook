using System;

using Bugscout.Agent.Core;

namespace Bugscout.Agent.Hook
{

    [Serializable]
    public unsafe delegate int HookDelegate2(
        Native.CompileMethodDelegate compileMethod,
        IntPtr thisPtr,
        IntPtr comp,                            // ICorJitInfo* comp,                               /* IN */
        ref Native.CORINFO_METHOD_INFO info,    // struct CORINFO_METHOD_INFO  *info,               /* IN */
        uint flags,                             // unsigned /* code:CorJitFlag */   flags,          /* IN */
        IntPtr nativeEntry,                     // BYTE                        **nativeEntry,       /* OUT */
        int nativeSizeOfCode                    // ULONG* nativeSizeOfCode                          /* OUT */
    );

}