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
            else
            {
                throw new NotImplementedException();
            }
        }
    }

}