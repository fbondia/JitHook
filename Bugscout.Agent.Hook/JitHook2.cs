using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Bugscout.Agent.Core;

namespace Bugscout.Agent.Hook
{


	[Serializable]
	public class JitHook2
	{

		readonly NativeInterface native;

		internal HookDelegate2 hook = null;

		internal Native.CompileMethodDelegate realCompileMethod = null;
		internal Native.CompileMethodDelegate hookedCompileMethod = null;


		internal IntPtr jitVTable = default;
		internal IntPtr pointerToCompileMethod = default;
		internal IntPtr pointerToVersionIdentifierMethod = default;

		internal System.Guid jitVersion = default;

		// Used vtable indices for ICorJitCompiler
		private const int ICorJitCompiler_compileMethod_index = 0;
		private const int ICorJitCompiler_getVersionIdentifier_index = 4;




		public unsafe JitHook2(Headers.Platform platform)
		{

			this.native = NativeFactory.getInstance(platform);
			this.hookedCompileMethod = HookedCompileMethodDelegate;

			this.jitVTable = Marshal.ReadIntPtr(native.getJit());

			this.pointerToCompileMethod = this.jitVTable;
			//this.pointerToCompileMethod = Marshal.ReadIntPtr(this.jitVTable, IntPtr.Size * ICorJitCompiler_compileMethod_index);

			this.pointerToVersionIdentifierMethod = Marshal.ReadIntPtr(this.jitVTable, IntPtr.Size * ICorJitCompiler_getVersionIdentifier_index);

			Native.GetVersionIdentifierDelegate getVersionIdentifier = (Native.GetVersionIdentifierDelegate)Marshal.GetDelegateForFunctionPointer(this.pointerToVersionIdentifierMethod, typeof(Native.GetVersionIdentifierDelegate));
			getVersionIdentifier(this.jitVTable, out this.jitVersion);


    		String[] methods = new string[] { "InstallHook", "Stop", "HookedCompileMethodDelegate" };

			foreach (String methodName in methods)
			{
				MethodInfo methodInfo = this.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				RuntimeHelpers.PrepareMethod(methodInfo.MethodHandle);
			}

		}

		public void InstallHook(HookDelegate2 hook)
		{
			RuntimeHelpers.PrepareDelegate(hook);
			this.hook = hook;
		}

		internal unsafe int HookedCompileMethodDelegate(
			IntPtr thisPtr,
			IntPtr comp, // ICorJitInfo* comp, /* IN */
			ref Native.CORINFO_METHOD_INFO info, // struct CORINFO_METHOD_INFO  *info,               /* IN */
			uint flags, // unsigned /* code:CorJitFlag */   flags,          /* IN */
			IntPtr nativeEntry, // BYTE                        **nativeEntry,       /* OUT */
			int nativeSizeOfCode // ULONG* nativeSizeOfCode    /* OUT */
        )
		{

			if (this.hook != null)
			{
				return this.hook(this.realCompileMethod, thisPtr, comp, ref info, flags, nativeEntry, nativeSizeOfCode);
			}
			else
			{
				return this.realCompileMethod(thisPtr, comp, ref info, flags, nativeEntry, nativeSizeOfCode);
			}

		}

		public unsafe void Start()
		{

			if (this.realCompileMethod == null)
			{
				uint oldProtection = 0u;

				if (!native.VirtualProtect(this.pointerToCompileMethod, (uint)IntPtr.Size, Headers.Protection.READ_WRITE, ref oldProtection))
				{
					Environment.Exit(-1);
				}

				Headers.ProtectionWindows protection = (Headers.ProtectionWindows)Enum.Parse(typeof(Headers.Protection), oldProtection.ToString());

				this.realCompileMethod = (Native.CompileMethodDelegate)Marshal.GetDelegateForFunctionPointer(this.pointerToCompileMethod, typeof(Native.CompileMethodDelegate));

				RuntimeHelpers.PrepareDelegate(this.realCompileMethod);
				RuntimeHelpers.PrepareDelegate(this.hookedCompileMethod);

				IntPtr realCompileMethodPointer = Marshal.GetFunctionPointerForDelegate(this.realCompileMethod);
				IntPtr hookedCompileMethodPointer = Marshal.GetFunctionPointerForDelegate(this.hookedCompileMethod);

                /*
				// 2) Build a trampoline that will allow to simulate a call from native to our delegate
				var trampolinePtr = AllocateTrampoline(hookedCompileMethodPointer2);
				var trampoline = (Native.CompileMethodDelegate)Marshal.GetDelegateForFunctionPointer(trampolinePtr, typeof(Native.CompileMethodDelegate));

				// 3) Call our trampoline
				IntPtr value = default;
				int size = default;
				var emptyInfo = default(Native.CORINFO_METHOD_INFO);
				trampoline(IntPtr.Zero, IntPtr.Zero, ref emptyInfo, 0, value, size);
				FreeTrampoline(trampolinePtr);

				// 4) Once our `CompileMethodDelegate` can be accessible from native code, we can install it
				InstallManagedJit(_overrideCompileMethodPtr);
                */


				//Marshal.WriteIntPtr(this.pointerToCompileMethod, realCompileMethodPointer);
				Marshal.WriteIntPtr(this.pointerToCompileMethod, hookedCompileMethodPointer);

				native.VirtualProtect(this.pointerToCompileMethod, (uint)IntPtr.Size, Headers.Protection.READ_WRITE, ref oldProtection);
			}

		}


		//[DllImport("kernel32", EntryPoint = "VirtualAlloc")]
		//private static extern IntPtr VirtualAlloc(IntPtr lpAddress, int dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

		//[DllImport("kernel32", EntryPoint = "VirtualFree")]
		//private static extern int VirtualFree(IntPtr lpAddress, IntPtr dwSize, FreeType freeType);

        /*
		private IntPtr AllocateTrampoline(IntPtr ptr)
		{
			// Create an executable region of code in memory
			var jmpNative = VirtualAlloc(IntPtr.Zero, DelegateTrampolineCode.Length, Native.AllocationType.Commit, Native.MemoryProtection.ExecuteReadWrite);
			// Copy our trampoline code there
			Marshal.Copy(DelegateTrampolineCode, 0, jmpNative, DelegateTrampolineCode.Length);
			// Setup the delegate we want to call as part of a reverse P/Invoke call
			Marshal.WriteIntPtr(jmpNative, 2, ptr);
			return jmpNative;
		}

		private void FreeTrampoline(IntPtr ptr)
		{
			VirtualFree(ptr, new IntPtr(DelegateTrampolineCode.Length), Native.FreeType.Release);
		}

		private void InstallManagedJit(IntPtr compileMethodPtr)
		{
			// We need to unprotect the JitVtable as it is by default not read-write
			// It is usually a C++ VTable generated at compile time and placed into a read-only section in the shared library
			VirtualProtect(this.jitVTable + ICorJitCompiler_compileMethod_index, new IntPtr(IntPtr.Size), Native.MemoryProtection.ReadWrite, out var oldFlags);
			Marshal.WriteIntPtr(this.jitVTable, ICorJitCompiler_compileMethod_index, compileMethodPtr);
			VirtualProtect(this.jitVTable + ICorJitCompiler_compileMethod_index, new IntPtr(IntPtr.Size), oldFlags, out oldFlags);
		}
        */


		public unsafe void Stop()
		{
			if (this.realCompileMethod != null)
			{
				uint oldProtection = 0u;

				if (!native.VirtualProtect(this.pointerToCompileMethod, (uint)IntPtr.Size, Headers.Protection.READ_WRITE, ref oldProtection))
				{
					Environment.Exit(-1);
				}

				Headers.Protection protection = (Headers.Protection)Enum.Parse(typeof(Headers.Protection), oldProtection.ToString());

				Marshal.WriteIntPtr(this.pointerToCompileMethod, Marshal.GetFunctionPointerForDelegate((Delegate)this.realCompileMethod));

				native.VirtualProtect(pointerToCompileMethod, (uint)IntPtr.Size, Headers.Protection.READ_WRITE, ref oldProtection);

				this.realCompileMethod = null;
			}
		}

	}

}