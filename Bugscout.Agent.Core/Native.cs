using System;
using System.Runtime.InteropServices;

namespace Bugscout.Agent.Core
{

	public static class Native
	{

        // https://github.com/dotnet/coreclr/blob/bb01fb0d954c957a36f3f8c7aad19657afc2ceda/src/inc/corjit.h#L391-L445

		[Serializable]
		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		public unsafe delegate int CompileMethodDeclaration(
            IntPtr thisPtr,
            IntPtr corJitInfo,
            Headers.CorMethodInfo* methodInfoPtr,
            Headers.CorJitFlag flags,
            IntPtr nativeEntry,
            IntPtr nativeSizeOfCode);

        /*
         * There are 2 DLL’s used in every standard .NET application, either Mscorjit.dll or Clrjit.dll depending
         * on what .NET version the assembly is targetting. Mscorjit targets 2.0 and below while Clrjit.dll
         * target 4.0 and above.
         */
        [DllImport("Clrjit.dll", EntryPoint = "getJit", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr getJitWindows();

        [DllImport("libclrjit.dylib", EntryPoint = "getJit", CallingConvention = CallingConvention.StdCall, PreserveSig = true)]
        internal static extern IntPtr getJitMacOs();

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, Headers.ProtectionWindows flNewProtect, ref uint lpflOldProtect);

        [DllImport("/usr/lib/libc.dylib", SetLastError = true)]
        internal static extern int getpid();

        [DllImport("/usr/lib/libc.dylib", SetLastError = true)]
        internal static extern int sysconf(int name);

        [DllImport("/usr/lib/libc.dylib", SetLastError = true)]
        internal static extern int getpagesize();

        [DllImport("/usr/lib/libc.dylib", SetLastError = true)]
        internal static extern int mprotect(IntPtr address, UInt32 length, int prot);



        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GetVersionIdentifierDelegate(IntPtr thisPtr, out Guid versionIdentifier /* OUT */);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int GetMethodDefFromMethodDelegate(IntPtr thisPtr, IntPtr hMethodHandle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr GetMethodClassDelegate(IntPtr thisPtr, IntPtr methodHandle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr GetMethodNameDelegate(IntPtr thisPtr, IntPtr ftn, out IntPtr moduleName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int CompileMethodDelegate(
                IntPtr thisPtr,
                IntPtr comp,                    // ICorJitInfo* comp,                               /* IN */
                ref CORINFO_METHOD_INFO info,   // struct CORINFO_METHOD_INFO  *info,               /* IN */
                uint flags,                     // unsigned /* code:CorJitFlag */   flags,          /* IN */
                IntPtr nativeEntry,             // BYTE                        **nativeEntry,       /* OUT */
                int nativeSizeOfCode            // ULONG* nativeSizeOfCode                          /* OUT */
            );

        public struct CORINFO_METHOD_INFO
        {
            public IntPtr ftn;
            public IntPtr scope;
            public IntPtr ILCode;
            public int ILCodeSize;
            public uint maxStack;
            public uint EHcount;
            public CorInfoOptions options;
            public CorInfoRegionKind regionKind;
            public CORINFO_SIG_INFO args;
            public CORINFO_SIG_INFO locals;
        };

        public enum CorInfoOptions
        {
            CORINFO_OPT_INIT_LOCALS = 0x00000010, // zero initialize all variables

            CORINFO_GENERICS_CTXT_FROM_THIS = 0x00000020, // is this shared generic code that access the generic context from the this pointer?  If so, then if the method has SEH then the 'this' pointer must always be reported and kept alive.
            CORINFO_GENERICS_CTXT_FROM_METHODDESC = 0x00000040, // is this shared generic code that access the generic context from the ParamTypeArg(that is a MethodDesc)?  If so, then if the method has SEH then the 'ParamTypeArg' must always be reported and kept alive. Same as CORINFO_CALLCONV_PARAMTYPE
            CORINFO_GENERICS_CTXT_FROM_METHODTABLE = 0x00000080, // is this shared generic code that access the generic context from the ParamTypeArg(that is a MethodTable)?  If so, then if the method has SEH then the 'ParamTypeArg' must always be reported and kept alive. Same as CORINFO_CALLCONV_PARAMTYPE
            CORINFO_GENERICS_CTXT_MASK = (CORINFO_GENERICS_CTXT_FROM_THIS |
                                          CORINFO_GENERICS_CTXT_FROM_METHODDESC |
                                          CORINFO_GENERICS_CTXT_FROM_METHODTABLE),
            CORINFO_GENERICS_CTXT_KEEP_ALIVE = 0x00000100, // Keep the generics context alive throughout the method even if there is no explicit use, and report its location to the CLR

        };

        public enum CorInfoRegionKind
        {
            CORINFO_REGION_NONE,
            CORINFO_REGION_HOT,
            CORINFO_REGION_COLD,
            CORINFO_REGION_JIT,
        };

        public struct CORINFO_SIG_INFO
        {
            public CorInfoCallConv callConv;
            public IntPtr retTypeClass;   // if the return type is a value class, this is its handle (enums are normalized)
            public IntPtr retTypeSigClass;// returns the value class as it is in the sig (enums are not converted to primitives)
            public CorInfoType retType;
            public byte flags;
            public ushort numArgs;
            public CORINFO_SIG_INST sigInst;  // information about how type variables are being instantiated in generic code
            public IntPtr args;
            public IntPtr pSig;
            public uint cbSig;
            public IntPtr scope;          // passed to getArgClass
            public uint token;
            public long garbage;
        };

        public struct CORINFO_SIG_INST
        {
            public uint classInstCount;
            public IntPtr classInst; // (representative, not exact) instantiation for class type variables in signature
            public uint methInstCount;
            public IntPtr methInst; // (representative, not exact) instantiation for method type variables in signature
        };

        public enum CorInfoType : byte
        {
            CORINFO_TYPE_UNDEF = 0x0,
            CORINFO_TYPE_VOID = 0x1,
            CORINFO_TYPE_BOOL = 0x2,
            CORINFO_TYPE_CHAR = 0x3,
            CORINFO_TYPE_BYTE = 0x4,
            CORINFO_TYPE_UBYTE = 0x5,
            CORINFO_TYPE_SHORT = 0x6,
            CORINFO_TYPE_USHORT = 0x7,
            CORINFO_TYPE_INT = 0x8,
            CORINFO_TYPE_UINT = 0x9,
            CORINFO_TYPE_LONG = 0xa,
            CORINFO_TYPE_ULONG = 0xb,
            CORINFO_TYPE_NATIVEINT = 0xc,
            CORINFO_TYPE_NATIVEUINT = 0xd,
            CORINFO_TYPE_FLOAT = 0xe,
            CORINFO_TYPE_DOUBLE = 0xf,
            CORINFO_TYPE_STRING = 0x10,         // Not used, should remove
            CORINFO_TYPE_PTR = 0x11,
            CORINFO_TYPE_BYREF = 0x12,
            CORINFO_TYPE_VALUECLASS = 0x13,
            CORINFO_TYPE_CLASS = 0x14,
            CORINFO_TYPE_REFANY = 0x15,

            // CORINFO_TYPE_VAR is for a generic type variable.
            // Generic type variables only appear when the JIT is doing
            // verification (not NOT compilation) of generic code
            // for the EE, in which case we're running
            // the JIT in "import only" mode.

            CORINFO_TYPE_VAR = 0x16,
            CORINFO_TYPE_COUNT,                         // number of jit types
        };

        public enum CorInfoCallConv
        {
            // These correspond to CorCallingConvention

            CORINFO_CALLCONV_DEFAULT = 0x0,
            CORINFO_CALLCONV_C = 0x1,
            CORINFO_CALLCONV_STDCALL = 0x2,
            CORINFO_CALLCONV_THISCALL = 0x3,
            CORINFO_CALLCONV_FASTCALL = 0x4,
            CORINFO_CALLCONV_VARARG = 0x5,
            CORINFO_CALLCONV_FIELD = 0x6,
            CORINFO_CALLCONV_LOCAL_SIG = 0x7,
            CORINFO_CALLCONV_PROPERTY = 0x8,
            CORINFO_CALLCONV_NATIVEVARARG = 0xb,    // used ONLY for IL stub PInvoke vararg calls

            CORINFO_CALLCONV_MASK = 0x0f,     // Calling convention is bottom 4 bits
            CORINFO_CALLCONV_GENERIC = 0x10,
            CORINFO_CALLCONV_HASTHIS = 0x20,
            CORINFO_CALLCONV_EXPLICITTHIS = 0x40,
            CORINFO_CALLCONV_PARAMTYPE = 0x80,     // Passed last. Same as CORINFO_GENERICS_CTXT_FROM_PARAMTYPEARG
        };

        [Flags]
        public enum FreeType
        {
            Decommit = 0x4000,
            Release = 0x8000,
        }

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

    }

    public interface NativeInterface
    {

        IntPtr getJit();

        bool VirtualProtect(IntPtr lpAddress, uint dwSize, Headers.Protection flNewProtect, ref uint lpflOldProtect);

    }

    public class NativeFactory
    {
        public static NativeInterface getInstance(Headers.Platform platform)
        {
            if (platform == Headers.Platform.WIN)
            {
                return new NativeWin();
            }
            else if (platform == Headers.Platform.MACOS)
            {
                return new NativeMacOs();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

}