using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using JitHook.Agent.Core;

namespace JitHook.Agent.Hook
{


	[Serializable]
	public class JitHook
	{

		readonly NativeInterface native;

		internal HookDelegate hook = null;

		internal Native.CompileMethodDeclaration realCompileMethod = null;
		internal Native.CompileMethodDeclaration hookedCompileMethod = null;


		internal IntPtr jitVTable = default;
		internal IntPtr pointerToCompileMethod = default;
		internal IntPtr pointerToVersionIdentifierMethod = default;

		internal System.Guid jitVersion = default;

		// Used vtable indices for ICorJitCompiler
		private const int ICorJitCompiler_compileMethod_index = 0;
		private const int ICorJitCompiler_getVersionIdentifier_index = 4;




		public unsafe JitHook(Headers.Platform platform)
		{

			this.native = NativeFactory.getInstance(platform);
			this.hookedCompileMethod = HookedCompileMethodDelegate;

			this.jitVTable = Marshal.ReadIntPtr(native.getJit());

			this.pointerToCompileMethod = this.jitVTable;
			//this.pointerToCompileMethod = Marshal.ReadIntPtr(this.jitVTable, IntPtr.Size * ICorJitCompiler_compileMethod_index);

			this.pointerToVersionIdentifierMethod = Marshal.ReadIntPtr(this.jitVTable, IntPtr.Size * ICorJitCompiler_getVersionIdentifier_index);

			//Native.GetVersionIdentifierDelegate getVersionIdentifier = (Native.GetVersionIdentifierDelegate)Marshal.GetDelegateForFunctionPointer(this.pointerToVersionIdentifierMethod, typeof(Native.GetVersionIdentifierDelegate));
			//getVersionIdentifier(this.jitVTable, out this.jitVersion);


			// pre-compile hook methods before jit instrumentalization
    		String[] methods = new string[] { "InstallHook", "Stop", "HookedCompileMethodDelegate" };

			foreach (String methodName in methods)
			{
				MethodInfo methodInfo = this.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				RuntimeHelpers.PrepareMethod(methodInfo.MethodHandle);
			}

		}

		public void InstallHook(HookDelegate hook)
		{
			RuntimeHelpers.PrepareDelegate(hook);
			this.hook = hook;
		}

		internal unsafe int HookedCompileMethodDelegate(
			IntPtr thisPtr,
			IntPtr corJitInfo,
			Headers.CorMethodInfo* methodInfoPtr,
			Headers.CorJitFlag flags,
			IntPtr nativeEntry,
			IntPtr nativeSizeOfCode)
		{

            if (thisPtr==IntPtr.Zero && corJitInfo==IntPtr.Zero)
            {
				return 0;
            }

			if (this.hook != null)
			{
				return this.hook(this.realCompileMethod, thisPtr, corJitInfo, methodInfoPtr, flags, nativeEntry, nativeSizeOfCode);
			}
			else
			{
				return this.realCompileMethod(thisPtr, corJitInfo, methodInfoPtr, flags, nativeEntry, nativeSizeOfCode);
			}

		}

		public unsafe void Start()
		{

			if (this.realCompileMethod == null)
			{
				uint oldProtection = 0u;

				IntPtr p = Marshal.ReadIntPtr(native.getJit());

				if (!native.VirtualProtect(p, (uint)IntPtr.Size, Headers.Protection.READ_WRITE, ref oldProtection))
				{
					Environment.Exit(-1);
				}


				Headers.ProtectionWindows protection = (Headers.ProtectionWindows)Enum.Parse(typeof(Headers.Protection), oldProtection.ToString());
				//Headers.ProtectionPosix protection = (Headers.ProtectionPosix)Enum.Parse(typeof(Headers.Protection), oldProtection.ToString());

				this.realCompileMethod = (Native.CompileMethodDeclaration)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(p), typeof(Native.CompileMethodDeclaration));

				RuntimeHelpers.PrepareDelegate(this.realCompileMethod);
				RuntimeHelpers.PrepareDelegate(this.hookedCompileMethod);

				GC.KeepAlive(this.hookedCompileMethod);

				//this.hookedCompileMethod(IntPtr.Zero, IntPtr.Zero, null, 0x0, IntPtr.Zero, IntPtr.Zero);

				IntPtr realCompileMethodPointer = Marshal.GetFunctionPointerForDelegate(this.realCompileMethod);
				IntPtr hookedCompileMethodPointer = Marshal.GetFunctionPointerForDelegate(this.hookedCompileMethod);

				//Marshal.WriteIntPtr(p, realCompileMethodPointer);
				Marshal.WriteIntPtr(p, hookedCompileMethodPointer);

				native.VirtualProtect(p, (uint)IntPtr.Size, Headers.Protection.READ_WRITE, ref oldProtection);
			}

		}

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