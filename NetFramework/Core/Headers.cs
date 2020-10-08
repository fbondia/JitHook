using System;
using System.Runtime.InteropServices;

namespace Bugscout.Agent.Core
{

	public static class Headers
	{

        public enum Platform
        {
            WIN,
            MACOS,
            LINUX
        }

		[Serializable]
		public enum Protection
		{
            NONE,
            READ,
            WRITE,
            READ_WRITE
		}

		[Serializable]
		public enum ProtectionPosix
		{
            PROT_NONE = 0x0,   // page can not be accessed
            PROT_READ = 0x01,  // pages can be read
            PROT_WRITE = 0x02, // pages can be written
            PROT_EXEC = 0x04,  // pages can be executed
            PROT_SEM = 0x08,   // page may be used for atomic ops
            PROT_SAO = 0x010   // strong access ordering
		}

    	[Serializable]
		public enum ProtectionWindows
		{
			PAGE_NOACCESS = 1,
			PAGE_READONLY = 2,
			PAGE_READWRITE = 4,
			PAGE_WRITECOPY = 8,
			PAGE_EXECUTE = 0x10,
			PAGE_EXECUTE_READ = 0x20,
			PAGE_EXECUTE_READWRITE = 0x40,
			PAGE_EXECUTE_WRITECOPY = 0x80,
			PAGE_GUARD = 0x100,
			PAGE_NOCACHE = 0x200,
			PAGE_WRITECOMBINE = 0x400
		}

		[Serializable]
		public enum CorJitFlag
		{
			CORJIT_FLG_SPEED_OPT = 1,
			CORJIT_FLG_SIZE_OPT = 2,
			CORJIT_FLG_DEBUG_CODE = 4,
			CORJIT_FLG_DEBUG_EnC = 8,
			CORJIT_FLG_DEBUG_INFO = 0x10,
			CORJIT_FLG_LOOSE_EXCEPT_ORDER = 0x20,
			CORJIT_FLG_TARGET_PENTIUM = 0x100,
			CORJIT_FLG_TARGET_PPRO = 0x200,
			CORJIT_FLG_TARGET_P4 = 0x400,
			CORJIT_FLG_TARGET_BANIAS = 0x800,
			CORJIT_FLG_USE_FCOMI = 0x1000,
			CORJIT_FLG_USE_CMOV = 0x2000,
			CORJIT_FLG_USE_SSE2 = 0x4000,
			CORJIT_FLG_PROF_CALLRET = 0x10000,
			CORJIT_FLG_PROF_ENTERLEAVE = 0x20000,
			CORJIT_FLG_PROF_INPROC_ACTIVE_DEPRECATED = 0x40000,
			CORJIT_FLG_PROF_NO_PINVOKE_INLINE = 0x80000,
			CORJIT_FLG_SKIP_VERIFICATION = 0x100000,
			CORJIT_FLG_PREJIT = 0x200000,
			CORJIT_FLG_RELOC = 0x400000,
			CORJIT_FLG_IMPORT_ONLY = 0x800000,
			CORJIT_FLG_IL_STUB = 0x1000000,
			CORJIT_FLG_PROCSPLIT = 0x2000000,
			CORJIT_FLG_BBINSTR = 0x4000000,
			CORJIT_FLG_BBOPT = 0x8000000,
			CORJIT_FLG_FRAMED = 0x10000000,
			CORJIT_FLG_ALIGN_LOOPS = 0x20000000,
			CORJIT_FLG_PUBLISH_SECRET_PARAM = 0x40000000
		}

		[Serializable]
		public enum CorInfoCallConv
		{
			C = 1,
			DEFAULT = 0,
			EXPLICITTHIS = 0x40,
			FASTCALL = 4,
			FIELD = 6,
			GENERIC = 0x10,
			HASTHIS = 0x20,
			LOCAL_SIG = 7,
			MASK = 0xF,
			NATIVEVARARG = 11,
			PARAMTYPE = 0x80,
			PROPERTY = 8,
			STDCALL = 2,
			THISCALL = 3,
			VARARG = 5
		}

		[Serializable]
		public enum CorInfoType : byte
		{
			BOOL = 2,
			BYREF = 18,
			BYTE = 4,
			CHAR = 3,
			CLASS = 20,
			COUNT = 23,
			DOUBLE = 0xF,
			FLOAT = 14,
			INT = 8,
			LONG = 10,
			NATIVEINT = 12,
			NATIVEUINT = 13,
			PTR = 17,
			REFANY = 21,
			SHORT = 6,
			STRING = 0x10,
			UBYTE = 5,
			UINT = 9,
			ULONG = 11,
			UNDEF = 0,
			USHORT = 7,
			VALUECLASS = 19,
			VAR = 22,
			VOID = 1
		}

		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		public struct CorinfoSigInst
		{
			public uint ClassInstCount;
			public IntPtr ClassInst;
			public uint MethInstCount;
			public IntPtr MethInst;
		}

		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		public struct CorinfoSigInfo
		{
			public CorInfoCallConv CallConv;
			public IntPtr RetTypeClass;
			public IntPtr RetTypeSigClass;
			public CorInfoType RetType;
			public byte Flags;
			public ushort NumArgs;
			public CorinfoSigInst SigInst;
			public IntPtr Args;
			public uint Token;
			public IntPtr Sig;
			public IntPtr Scope;
		}

		[Serializable]
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct CorMethodInfo
		{
			public IntPtr MethodHandle;
			public IntPtr ModuleHandle;
			public IntPtr IlCode;
			public uint IlCodeSize;
			public ushort MaxStack;
			public ushort EHCount;
			public uint CorInfoOptions;
			public CorinfoSigInfo Args;
			public CorinfoSigInfo Locals;
		}

		[Serializable]
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct CorILMethodSectEhSmall
		{
			public ushort Flags;
			public ushort TryOffset;
			public byte TryLength;
			public ushort HandlerOffset;
			public byte HandlerLength;
			public uint ClassToken;
		}

		[Serializable]
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct CorILMethodSectEhFat
		{
			public uint Flags;
			public uint TryOffset;
			public uint TryLength;
			public uint HandlerOffset;
			public uint HandlerLength;
			public uint ClassToken;
		}

	}

}