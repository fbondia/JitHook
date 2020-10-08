using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Bugscout.Agent.Core;

namespace Bugscout.Agent.Runtime
{

	[Serializable]
	public class RuntimeDispatcher
	{

		private String runtimeDispatcherClass = "Bugscout.Agent.Runtime.Dispatcher";
		private String runtimeDispatcherMethod = "DispatchCallback";

		readonly NativeInterface native;

		internal AssemblyBuilder dynamicAssembly;

		internal ModuleBuilder dynamicModule;

		internal int inCompileMethod;

		internal int index;

		internal List<MethodFilter> filters;

		internal Dictionary<string, MethodInfo> signatures;

		public RuntimeDispatcher(Headers.Platform platform)
		{

			this.native = NativeFactory.getInstance(platform);

			//_dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.RunAndCollect);
			//_dynamicModule = _dynamicAssembly.DefineDynamicModule("dispatcherMethods", "DispatcherMethods.dll");

			this.dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.RunAndCollect);
			this.dynamicModule = dynamicAssembly.DefineDynamicModule("dispatcherMethods");


			this.inCompileMethod = 0;
			this.index = 0;

			this.filters = new List<MethodFilter>();
			this.signatures = new Dictionary<string, MethodInfo>();

			MethodInfo compileMethod = this.GetType().GetMethod("CompileMethod");
			RuntimeHelpers.PrepareMethod(compileMethod.MethodHandle);

		}

		public void AddFilter(Type invokerType, string methodName)
		{
			filters.Add(new MethodFilter(invokerType, methodName));
		}

		internal string CreateMethodSignature(MethodBase methodBase)
		{

			// return type info

			MethodInfo methodInfo = methodBase as MethodInfo;
			ConstructorInfo constructorInfo = methodBase as ConstructorInfo;

			string returnType;

			if (methodInfo != null)
			{
				returnType = methodInfo.ReturnType.GetHashCode().ToString();
			}
			else if (constructorInfo != null)
			{
				returnType = (typeof(object)).GetHashCode().ToString();
			}
			else
			{
				returnType = (typeof(void)).GetHashCode().ToString(); ;
			}



			// instance type info

			string instanceType = ((!methodBase.IsStatic) ? typeof(object) : typeof(void)).GetHashCode().ToString();



			// parameters info

			StringBuilder parameters = new StringBuilder();

			for (int i = 0; i < methodBase.GetParameters().Length; i++)
			{
				if (i > 0) parameters.Append(",");
				ParameterInfo p = methodBase.GetParameters()[i];
				parameters.Append(p.ParameterType.GetHashCode().ToString());
			}


			return returnType + instanceType + parameters.ToString();

		}

		public unsafe int CompileMethod(
			Native.CompileMethodDeclaration realCompileMethod,
			IntPtr thisPtr,
			IntPtr corJitInfo,
			Headers.CorMethodInfo* methodInfoPtr,
			Headers.CorJitFlag flags,
			IntPtr nativeEntry,
			IntPtr nativeSizeOfCode)
		{

			if (Interlocked.CompareExchange(ref this.inCompileMethod, 1, 0) == 0)
			{

				Headers.CorMethodInfo methodInfo = *methodInfoPtr;

				FilteredMethod filteredMethod = null;

				foreach (MethodFilter filter in this.filters)
				{
					filteredMethod = filter.GetMethod(methodInfo);

					if (filteredMethod != null)
					{
						break;
					}
				}

				if (filteredMethod != null)
				{
					try
					{

						MethodInfo dispatcherMethod = this.GenerateDispatcherMethod(filteredMethod);

						ModuleBuilder dynamicModule = this.dynamicAssembly.DefineDynamicModule("MODULE" + (this.index++).ToString() + this.dynamicAssembly.FullName);
						TypeBuilder typeBuilder = dynamicModule.DefineType("TYPE" + Guid.NewGuid().ToString());

						TrampolineMethodInfo trampolineMethod = this.GenerateTrampolineMethod(filteredMethod, typeBuilder, dispatcherMethod);
						Type trampolineType = typeBuilder.CreateType();

						MethodInfo trampolineMethodInfo = trampolineType.GetMethod(trampolineMethod.MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

						MethodBody trampolineMethodBody = trampolineMethodInfo.GetMethodBody();

						byte[] patchedMsilCode = this.PatchMethodBody(trampolineMethodBody, filteredMethod, trampolineMethod.PatchOffset);

						byte[] trampolineMsil = patchedMsilCode;
						byte[] code = this.MergeCode(methodInfo, trampolineMsil);

						MethodBase method = filteredMethod.Method;

						this.FixEHClausesIfNecessary(methodInfo, method, patchedMsilCode.Length);

						GCHandle ilMem = GCHandle.Alloc(code, GCHandleType.Pinned);

						methodInfo.IlCode = ilMem.AddrOfPinnedObject();
						methodInfo.IlCodeSize = (uint)code.Length;

						ushort maxStack = methodInfo.MaxStack;
						ushort num = 10;

						methodInfo.MaxStack = (ushort)(maxStack + num);
						*methodInfoPtr = methodInfo;


					}
					catch (Exception ex)
					{
						//Console.WriteLine(ex2.ToString());
					}
				}

				Interlocked.Exchange(ref inCompileMethod, 0);

			}

			return realCompileMethod(thisPtr, corJitInfo, methodInfoPtr, flags, nativeEntry, nativeSizeOfCode);

		}

		internal MethodInfo ResolveDispatcherMethod(MethodBase methodBase)
		{
			string signature = CreateMethodSignature(methodBase);

			if (this.signatures.ContainsKey(signature))
			{
				return signatures[signature];
			}

			return null;

		}

		internal MethodInfo GenerateDispatcherMethod(FilteredMethod filteredMethod)
		{

			MethodInfo methodInfo = ResolveDispatcherMethod(filteredMethod.Method);

			if (methodInfo != null)
			{
				return methodInfo;
			}



			List<Type> argumentTypes = new List<Type>();

			if (!filteredMethod.Method.IsStatic)
			{
				argumentTypes.Add(typeof(Object));
			}

			foreach (ParameterInfo p in filteredMethod.Method.GetParameters())
			{
				argumentTypes.Add(p.ParameterType);
			}




			string dynamicMethodName = filteredMethod.Method.Name + "_Dispatcher" + index.ToString();
			string dynamicTypeName = filteredMethod.Method.Name + "_Type" + index.ToString();

			index++;

			TypeBuilder dynamicType = dynamicModule.DefineType(dynamicTypeName);
			MethodBuilder dynamicMethod = dynamicType.DefineMethod(dynamicMethodName, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, CallingConventions.Standard, typeof(void), argumentTypes.ToArray());


			// generate method IL
			ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
			ilGenerator.Emit(OpCodes.Nop);

			// push the location of the assembly to load containing the monitors
			string assemblyLocation = filteredMethod.Filter.Invoker==null ? String.Empty : filteredMethod.Filter.Invoker.Assembly.Location;
			ilGenerator.Emit(OpCodes.Ldstr, assemblyLocation);

			// create argv array
			ilGenerator.Emit(OpCodes.Ldc_I4, filteredMethod.NumOfArgumentsToPushInTheStack);
			ilGenerator.Emit(OpCodes.Newarr, typeof(object));

			// fill the argv array
			if (filteredMethod.NumOfArgumentsToPushInTheStack > 0)
			{

				List<Type> types = new List<Type>();

				foreach (ParameterInfo p in filteredMethod.Method.GetParameters())
				{
					types.Add(p.ParameterType);
				}

				for (int i=0; i < filteredMethod.NumOfArgumentsToPushInTheStack; i++)
				{

					ilGenerator.Emit(OpCodes.Dup);
					ilGenerator.Emit(OpCodes.Ldc_I4, i);
					ilGenerator.Emit(OpCodes.Ldarg, i);

					// chyeck if I have to box the value
					if (filteredMethod.Method.IsStatic || i > 0)
					{

                        int paramIndex = (!filteredMethod.Method.IsStatic) ? (i - 1) : i;

						if (types[paramIndex].IsEnum)
						{
							// consider all enum as Int32 type to avoid access problems     
							ilGenerator.Emit(OpCodes.Box, typeof(int));
						}
						else if (types[paramIndex].IsValueType)
						{
							ilGenerator.Emit(OpCodes.Box, types[paramIndex]);
						}

					}

					// store the element in the array
					ilGenerator.Emit(OpCodes.Stelem_Ref);

				}

			}

			// emit call to dispatchCallback
			MethodInfo dispatchCallbackMethod = Type.GetType(this.runtimeDispatcherClass).GetMethod(this.runtimeDispatcherMethod, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			ilGenerator.EmitCall(OpCodes.Call, dispatchCallbackMethod, null);
			ilGenerator.Emit(OpCodes.Ret);



            // create type and method
			Type createdType = dynamicType.CreateType();
			MethodInfo createdMethod = createdType.GetMethod(dynamicMethodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			string signature = CreateMethodSignature(filteredMethod.Method);
			signatures.Add(signature, createdMethod);

			RuntimeHelpers.PrepareMethod(createdMethod.MethodHandle);

            GC.KeepAlive(createdMethod);

            return createdMethod;

		}

		internal TrampolineMethodInfo GenerateTrampolineMethod(FilteredMethod filteredMethod, TypeBuilder typeBuilder, MethodInfo dispatcherMethod)
		{

			// this method is in charge for the creation of the MSIL code to be added to the MSIL that will be compiled. 
			// It is in charge to call the dynamic method wich in turn will dispatch the call to the method monitors

			List<Type> dispatcherArgs = new List<Type>();
			for (int i=0; i<filteredMethod.NumOfArgumentsToPushInTheStack - 1; i++)
			{
				dispatcherArgs.Add(typeof(Object));
			}


			// retrieve the necessary object to create the new IL        
			int functionAddress = dispatcherMethod.MethodHandle.GetFunctionPointer().ToInt32();
			MethodBuilder methodBuilder = typeBuilder.DefineMethod("CONTAINER_" + Guid.NewGuid().ToString(), MethodAttributes.Static, CallingConventions.Standard, typeof(void), dispatcherArgs.ToArray());

			// create method body 
			ILGenerator ilGenerator = methodBuilder.GetILGenerator();
			ilGenerator.Emit(OpCodes.Nop);

			// load all arguments in the stack
			if (filteredMethod.NumOfArgumentsToPushInTheStack > 0)
			{
				for (int i = 0; i < filteredMethod.NumOfArgumentsToPushInTheStack; i++)
				{
					ilGenerator.Emit(OpCodes.Ldarg, i);
				}
			}


			// emit calli instruction with a pointer to the hook method.
            // the token used by the calli is not important as I'll modify it soon
			ilGenerator.Emit(OpCodes.Ldc_I4, functionAddress);

			//ilGenerator.EmitCalli(OpCodes.Calli, CallingConventions.Standard, dispatcherMethod.ReturnType, dispatcherArgs.ToArray(), null);
			ilGenerator.EmitCalli(OpCodes.Calli, CallingConvention.StdCall, dispatcherMethod.ReturnType, dispatcherArgs.ToArray());

			// this index allow to modify the right byte
			int patchOffset = ilGenerator.ILOffset - 4;
			ilGenerator.Emit(OpCodes.Nop);

			MethodBase method = filteredMethod.Method;
			MethodInfo methodInfo = method as MethodInfo;

			if (methodInfo != null)
			{
				if (!methodInfo.ReturnType.Name.Equals((typeof(void)).Name))
				{
					ilGenerator.Emit(OpCodes.Pop);
				}
			}

			// end method
			ilGenerator.Emit(OpCodes.Ret);

            return new TrampolineMethodInfo(methodBuilder, methodBuilder.Name, patchOffset);

		}

		internal byte[] PatchMethodBody(MethodBody methodBody, FilteredMethod filteredMethod, int patchOffset)
		{

			byte[] trampolineMsil = methodBody.GetILAsByteArray();

			short num = (short)(filteredMethod.TokenNum & -256);

			short b2 = (short)(filteredMethod.TokenNum & 0xFF);
			short b3 = (short)(num >> (8 & 0xF));

			trampolineMsil[patchOffset + 0] = (byte)b2;
			trampolineMsil[patchOffset + 1] = (byte)b3;
			trampolineMsil[patchOffset + 3] = 6;

			return trampolineMsil;

		}

		internal byte[] MergeCode(Headers.CorMethodInfo methodInfo, byte[] trampolineMsil)
		{

			byte[] origMsil = new byte[(int)methodInfo.IlCodeSize];

			Marshal.Copy(methodInfo.IlCode, origMsil, 0, origMsil.Length);

			int newLen = origMsil.Length + trampolineMsil.Length;

			byte[] newMsil = new byte[newLen];

			Array.Copy(trampolineMsil, newMsil, trampolineMsil.Length);

            Array.Copy(origMsil, 0, newMsil, trampolineMsil.Length, origMsil.Length);

            if (newMsil[trampolineMsil.Length - 1] == (byte)OpCodes.Ret.Value)
			{
				newMsil[trampolineMsil.Length - 1] = (byte)OpCodes.Nop.Value;
			}

            return newMsil;

		}

		internal unsafe void FixEHClausesIfNecessary(Headers.CorMethodInfo methodInfo, MethodBase methodBase, int additionalCodeLength)
		{

			IList<ExceptionHandlingClause> clauses = methodBase.GetMethodBody().ExceptionHandlingClauses;

			if (clauses.Count==0)
			{
				return;
			}

			int codeSizeAligned = ((int)methodInfo.IlCodeSize % 4 != 0) ? (4 - (int)methodInfo.IlCodeSize % 4) : 0;

			IntPtr startEHClauses = (IntPtr)(void*)((long)methodInfo.IlCode + (long)new IntPtr((int)methodInfo.IlCodeSize + codeSizeAligned));

            byte kind = Marshal.ReadByte(startEHClauses);

            bool isFat = (kind & 0x40) != 0;

            startEHClauses = (IntPtr)(void*)((long)startEHClauses + (long)new IntPtr(4));



            for(int i=0; i<clauses.Count; i++)
            {

				if (isFat)
				{

					IntPtr ehFatClausePointer = startEHClauses;

					Headers.CorILMethodSectEhFat ehFatClause = *(Headers.CorILMethodSectEhFat*)(long)ehFatClausePointer;
					ehFatClause.HandlerOffset += (uint)additionalCodeLength;
					ehFatClause.TryOffset += (uint)additionalCodeLength;

                    uint oldProtection2 = 0u;

					int memSize = Marshal.SizeOf(typeof(Headers.CorILMethodSectEhFat));

                    if (!native.VirtualProtect(startEHClauses, (uint)memSize, Headers.Protection.READ_WRITE, ref oldProtection2))
					{
						Environment.Exit(-1);
					}

                    Headers.Protection protection2 = (Headers.Protection)Enum.Parse(typeof(Headers.Protection), oldProtection2.ToString());

                    *(Headers.CorILMethodSectEhFat*)(long)ehFatClausePointer = ehFatClause;

                    native.VirtualProtect(startEHClauses, (uint)memSize, protection2, ref oldProtection2);

					startEHClauses = (IntPtr)(void*)((long)startEHClauses + (long)new IntPtr(memSize));

				}
				else
				{

					IntPtr ehSmallClausePointer = startEHClauses;

					Headers.CorILMethodSectEhSmall ehSmallClause = *(Headers.CorILMethodSectEhSmall*)(long)ehSmallClausePointer;
                    ehSmallClause.HandlerOffset = (ushort)(ehSmallClause.HandlerOffset + (ushort)additionalCodeLength);
					ehSmallClause.TryOffset = (ushort)(ehSmallClause.TryOffset + (ushort)additionalCodeLength);

					uint oldProtection = 0u;

                    int memSize = Marshal.SizeOf(typeof(Headers.CorILMethodSectEhSmall));

                    if (!native.VirtualProtect(startEHClauses, (uint)memSize, Headers.Protection.READ_WRITE, ref oldProtection))
					{
						Environment.Exit(-1);
					}

					Headers.Protection protection = (Headers.Protection)Enum.Parse(typeof(Headers.Protection), oldProtection.ToString());

                    *(Headers.CorILMethodSectEhSmall*)(long)ehSmallClausePointer = ehSmallClause;

                    native.VirtualProtect(startEHClauses, (uint)memSize, protection, ref oldProtection);

                    startEHClauses = (IntPtr)(void*)((long)startEHClauses + (long)new IntPtr(memSize));

				}

			}

		}
	}

}