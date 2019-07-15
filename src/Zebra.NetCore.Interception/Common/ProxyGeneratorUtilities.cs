using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Zebra.NetCore.Interception.Common
{
    internal class ProxyGeneratorUtilities
    {

        private const string ASSEMBLY_NAME = "Zebra.NetCore.Interception.Proxy";
        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _moduleBuilder;

        public ProxyGeneratorUtilities()
        {
#if NETCOREAPP2_1

            this._assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(ASSEMBLY_NAME) { Version = new Version(1, 0, 0) }, AssemblyBuilderAccess.RunAndCollect);
            this._moduleBuilder = this._assemblyBuilder.DefineDynamicModule(ASSEMBLY_NAME);
#else
            this._assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(ASSEMBLY_NAME) { Version = new Version(1, 0, 0) }, AssemblyBuilderAccess.RunAndSave);
            this._moduleBuilder = this._assemblyBuilder.DefineDynamicModule(ASSEMBLY_NAME, ASSEMBLY_NAME + ".dll");
#endif
        }

        public Type CreateProxy(Type service)
        {
            Type resultType = null;
            if (service.IsInterface)
            {
                resultType = CreateInterfaceProxy(service);
            }
            else
            {
                resultType = CreateClassProxy(service);
            }
            return resultType;
        }

        public Type CreateInterfaceProxy(Type @interface)
        {
            if (!@interface.IsInterface)
            {
                throw new ArgumentException($"InterfaceType_Must_Be_Interface, {@interface.FullName}", "T");
            }
            TypeBuilder builder = this._moduleBuilder.DefineType($"{@interface.Name}_Proxy", TypeAttributes.Public, typeof(object), new Type[] { @interface, typeof(IProxy) });
            ProxyBuilder proxyBuilder = new ProxyBuilder(builder, @interface);

            proxyBuilder.DefineClassConstructor();

            proxyBuilder.DefineProxyMethod();

            foreach (var method in @interface.GetMethods().Where(method => !method.IsSpecialName))
            {
                proxyBuilder.DefineInterceptorMethod(method);
            }

            foreach (var property in @interface.GetProperties())
            {
                proxyBuilder.DefineProperty(property);
            }

            return builder.CreateTypeInfo();
        }

        public Type CreateClassProxy(Type @class)
        {
            if (@class.IsSealed)
            {
                throw new ArgumentException($"BaseType_Cannot_Be_Sealed, {@class.FullName}", "TProxy");
            }
            var builder = _moduleBuilder.DefineType($"{@class.Name}_Proxy", TypeAttributes.Public | TypeAttributes.Sealed, @class, new Type[] { typeof(IProxy) });
            ProxyBuilder proxyBuilder = new ProxyBuilder(builder, @class);

            proxyBuilder.DefineSubClassConstructor();

            proxyBuilder.DefineProxyMethod();

            foreach (var methodInfo in @class.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(method => method.DeclaringType != typeof(object)))
            {
                if (!methodInfo.IsSpecialName && !methodInfo.IsFinal && methodInfo.IsVirtual)
                {
                    proxyBuilder.DefineInterceptorMethod(methodInfo);
                }
            }

            foreach (var property in @class.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.GetProperty))
            {
                proxyBuilder.DefineProperty(property);
            }

            return builder.CreateTypeInfo();
        }

        public void Save()
        {
#if NET461
            this._assemblyBuilder.Save(ASSEMBLY_NAME + ".dll");
#endif
        }

        private class ProxyBuilder
        {

            const MethodAttributes OverrideMethodAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final;
            const MethodAttributes InterfaceMethodAttributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

            private TypeBuilder _builder;
            private Type _targetType;
            private FieldBuilder _targetField;
            private FieldBuilder _invokerField;

            public ProxyBuilder(TypeBuilder builder, Type targetType)
            {
                _builder = builder ?? throw new ArgumentNullException(nameof(builder));
                _targetType = targetType.GetTypeInfo() ?? throw new ArgumentNullException(nameof(targetType));
                _targetField = builder.DefineField("_target", targetType, FieldAttributes.Private);
                _invokerField = builder.DefineField("_invoker", typeof(InterceptorInvoker), FieldAttributes.Private);

                if (_targetType.GetTypeInfo().IsGenericTypeDefinition)
                {
                    var arguments = _targetType.GetTypeInfo().GetGenericArguments();
                    var builders = _builder.DefineGenericParameters(arguments.Select(item => item.Name).ToArray());
                    DefineGenericParameters(arguments, builders);
                }
            }

            public void DefineClassConstructor()
            {
                var constructor = _builder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, Array.Empty<Type>());
                var ilGen = constructor.GetILGenerator();
                //Call object's constructor.
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Call, typeof(object).GetTypeInfo().DeclaredConstructors.First());
                //Return
                ilGen.Emit(OpCodes.Ret);
            }

            public void DefineSubClassConstructor()
            {
                var constructors = _targetType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

                foreach (var constructor in constructors)
                {
                    var parameters = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
                    var constructorBuilder = _builder.DefineConstructor(constructor.Attributes, constructor.CallingConvention, parameters);
                    var ilGen = constructorBuilder.GetILGenerator();
                    //Call object's constructor.
                    ilGen.Emit(OpCodes.Ldarg_0);
                    for (int i = 1; i <= parameters.Length; i++)
                    {
                        ilGen.Emit(OpCodes.Ldarg_S, i);
                    }
                    ilGen.Emit(OpCodes.Call, constructor);
                    //Return
                    ilGen.Emit(OpCodes.Ret);
                }
            }

            public void DefineProxyMethod()
            {
                MethodInfo method = ReflectionUtilities.GetMethod<IProxy>(nameof(IProxy.SetProxy));
                MethodBuilder methodBuilder = _builder.DefineMethod(method.Name, InterfaceMethodAttributes, typeof(void), new Type[] { typeof(object), typeof(InterceptorInvoker) });
                var ilGen = methodBuilder.GetILGenerator();
                // _target = (target as IProductGeneric<T>);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Isinst, _targetType);
                ilGen.Emit(OpCodes.Stfld, _targetField);
                // _invoker = invoker;
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_2);
                ilGen.Emit(OpCodes.Stfld, _invokerField);
                ilGen.Emit(OpCodes.Ret);
            }

            public void DefineProperty(PropertyInfo property)
            {
                PropertyBuilder propertyBuilder = _builder.DefineProperty(property.Name, property.Attributes, property.PropertyType, property.GetIndexParameters().Select(it => it.ParameterType).ToArray());
                MethodInfo getMethod = property.GetMethod;
                if (getMethod != null && !getMethod.IsFinal)
                {
                    propertyBuilder.SetGetMethod(this.DefineInterceptorMethod(getMethod));
                }
                MethodInfo setMethod = property.SetMethod;
                if (setMethod != null && !setMethod.IsFinal)
                {
                    propertyBuilder.SetSetMethod(this.DefineInterceptorMethod(setMethod));
                }
            }

            public MethodBuilder DefineInterceptorMethod(MethodInfo method)
            {
                var parameters = method.GetParameters();

                var methodBuilder = this._builder.DefineMethod(method.Name, GetMethodAttributes(method), method.ReturnType, parameters.Select(item => item.ParameterType).ToArray());

                if (method.ContainsGenericParameters)
                {
                    var arguments = method.GetGenericArguments();
                    DefineGenericParameters(arguments, methodBuilder.DefineGenericParameters(arguments.Select(item => item.Name).ToArray()));
                }

                for (int index = 0; index < parameters.Length; index++)
                {
                    var parameter = parameters[index];
                    methodBuilder.DefineParameter(index + 1, parameter.Attributes, parameter.Name);
                }

                var ilGen = methodBuilder.GetILGenerator();
                // 定义本地变量
                ilGen.DeclareLocal(typeof(MethodBase)); //index 0
                ilGen.DeclareLocal(typeof(InvocationContext));//index 1

                //加载并存储当前执行方法 并赋值给索引为0的本地变量
                ilGen.Emit(OpCodes.Ldtoken, method);
                if (_targetType.IsGenericTypeDefinition)
                {
                    ilGen.Emit(OpCodes.Ldtoken, method.DeclaringType);
                    ilGen.Emit(OpCodes.Call, ReflectionUtilities.GetMethod<MethodBase>(_ => MethodBase.GetMethodFromHandle(default(RuntimeMethodHandle), default(RuntimeTypeHandle))));
                }
                else
                {
                    ilGen.Emit(OpCodes.Call, ReflectionUtilities.GetMethod<MethodBase>(_ => MethodBase.GetMethodFromHandle(default(RuntimeMethodHandle))));
                }
                ilGen.Emit(OpCodes.Stloc_0);

                // 	InvocationContext invocationContext = new InvocationContext(this, _target, method, new object[1]
                // 	{
                // 		id
                // 	});

                ilGen.Emit(OpCodes.Ldarg_0);

                //构造函数第一个参数 proxy
                ilGen.Emit(OpCodes.Ldarg_0);
                //构造函数第二个参数 target
                ilGen.Emit(OpCodes.Ldfld, _targetField);
                //构造函数第三个参数 method
                ilGen.Emit(OpCodes.Ldloc_0);
                //构造函数第三个参数 arguments
                ilGen.Emit(OpCodes.Ldc_I4, parameters.Length);
                ilGen.Emit(OpCodes.Newarr, typeof(object));
                for (int i = 0; i < parameters.Length; i++)
                {
                    Type parameterType = parameters[i].ParameterType;
                    ilGen.Emit(OpCodes.Dup);
                    ilGen.Emit(OpCodes.Ldc_I4, i);
                    ilGen.Emit(OpCodes.Ldarg, i + 1);
                    if (parameters[i].IsOut || parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();
                        ilGen.Ldind(parameterType);
                    }
                    if (parameterType.GetTypeInfo().IsValueType || parameterType.GetTypeInfo().IsGenericParameter)
                    {
                        ilGen.Emit(OpCodes.Box, parameterType);
                    }
                    ilGen.Emit(OpCodes.Stelem_Ref);
                }

                ilGen.Emit(OpCodes.Newobj, ReflectionUtilities.GetConstructor(() => new InvocationContext(default(object), default(object), default(MethodBase), default(object[]))));
                ilGen.Emit(OpCodes.Stloc_1);

                // _invoker.Invoke<object>(Get, invocationContext);

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, _invokerField);
                ilGen.Emit(OpCodes.Ldarg_0);
                MethodInfo targetMethod = DefineTargetMethod(method);
                if (method.ContainsGenericParameters)
                {
                    targetMethod = targetMethod.MakeGenericMethod(methodBuilder.GetGenericArguments());
                }
                ilGen.Emit(OpCodes.Ldftn, targetMethod);
                ilGen.Emit(OpCodes.Newobj, typeof(InterceptorDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
                ilGen.Emit(OpCodes.Ldloc_1);

                MethodInfo invokerMethod;

                if (method.IsVoid())
                {
                    invokerMethod = ReflectionUtilities.GetMethod<InterceptorInvoker>(nameof(InterceptorInvoker.Invoke)).MakeGenericMethod(typeof(object));
                }
                else if (method.IsTask())
                {
                    invokerMethod = ReflectionUtilities.GetMethod<InterceptorInvoker>(nameof(InterceptorInvoker.InvokeAsync)).MakeGenericMethod(typeof(object));
                }
                else if (method.IsTaskWithResult())
                {
                    var returnType = method.ReturnType.GetTypeInfo().GetGenericArguments().First();
                    invokerMethod = ReflectionUtilities.GetMethod<InterceptorInvoker>(nameof(InterceptorInvoker.InvokeAsync)).MakeGenericMethod(returnType);
                }
                else
                {
                    invokerMethod = ReflectionUtilities.GetMethod<InterceptorInvoker>(nameof(InterceptorInvoker.Invoke)).MakeGenericMethod(method.ReturnType);
                }
                ilGen.Emit(OpCodes.Callvirt, invokerMethod);

                var returnValue = default(LocalBuilder);
                if (!method.IsVoid())
                {
                    returnValue = ilGen.DeclareLocal(method.ReturnType);
                    ilGen.Emit(OpCodes.Stloc, returnValue);
                }
                else
                {
                    ilGen.Emit(OpCodes.Pop);
                }

                // age = (int)invocationContext.Arguments[1];
                for (int index = 0; index < parameters.Length; index++)
                {
                    Type parameterType = parameters[index].ParameterType;
                    if (parameters[index].IsOut || parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();

                        ilGen.Emit(OpCodes.Ldarg, index + 1);
                        ilGen.Emit(OpCodes.Ldloc, 1);
                        ilGen.Emit(OpCodes.Callvirt, ReflectionUtilities.GetProperty<InvocationContext, object[]>(context => context.Arguments).GetMethod);
                        ilGen.Emit(OpCodes.Ldc_I4, index);
                        ilGen.Emit(OpCodes.Ldelem_Ref);
                        if (parameterType.IsValueType || parameterType.IsGenericParameter)
                        {
                            ilGen.Emit(OpCodes.Unbox_Any, parameterType);
                            ilGen.Stind(parameterType);
                        }
                        else
                        {
                            ilGen.Emit(OpCodes.Castclass, parameterType);
                            ilGen.Emit(OpCodes.Stind_Ref);
                        }
                    }
                }
                if (returnValue != null)
                {
                    ilGen.Emit(OpCodes.Ldloc, returnValue);
                }
                ilGen.Emit(OpCodes.Ret);
                return methodBuilder;
            }


            private MethodAttributes GetMethodAttributes(MethodInfo method)
            {
                if (method.DeclaringType.IsInterface)
                {
                    return InterfaceMethodAttributes;
                }
                else
                {
                    var attributes = OverrideMethodAttributes;
                    if (method.Attributes.HasFlag(MethodAttributes.Public))
                    {
                        return attributes | MethodAttributes.Public;
                    }

                    if (method.Attributes.HasFlag(MethodAttributes.Family))
                    {
                        return attributes | MethodAttributes.Family;
                    }

                    if (method.Attributes.HasFlag(MethodAttributes.FamORAssem))
                    {
                        return attributes | MethodAttributes.FamORAssem;
                    }

                    if (method.Attributes.HasFlag(MethodAttributes.FamANDAssem))
                    {
                        return attributes | MethodAttributes.FamANDAssem;
                    }
                    return attributes;
                }
            }

            private MethodBuilder DefineTargetMethod(MethodInfo method)
            {
                var parameters = method.GetParameters();
                var methodBuilder = _builder.DefineMethod(method.Name + "_" + string.Join("_", parameters.Select(item => item.Name)) + "_" + method.MetadataToken, MethodAttributes.Private | MethodAttributes.HideBySig, typeof(Task), new Type[] { typeof(InvocationContext) });
                if (method.ContainsGenericParameters)
                {
                    var arguments = method.GetGenericArguments();
                    DefineGenericParameters(arguments, methodBuilder.DefineGenericParameters(arguments.Select(item => item.Name).ToArray()));
                }
                methodBuilder.DefineParameter(1, ParameterAttributes.None, "context");
                var ilGen = methodBuilder.GetILGenerator();

                ilGen.DeclareLocal(typeof(object[]));//index 0
                foreach (var item in parameters)
                {
                    Type parameterType = item.ParameterType;
                    if (item.IsOut || parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();
                    }
                    ilGen.DeclareLocal(parameterType);
                }
                ilGen.Emit(OpCodes.Nop);
                // object[] arguments = context.Arguments;
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Callvirt, ReflectionUtilities.GetProperty<InvocationContext, object[]>(context => context.Arguments).GetMethod);
                ilGen.Emit(OpCodes.Stloc_0);

                // long arg0 = (long)arguments[0];
                for (int i = 0; i < parameters.Length; i++)
                {
                    ilGen.Emit(OpCodes.Ldloc_0);
                    ilGen.Emit(OpCodes.Ldc_I4, i);
                    ilGen.Emit(OpCodes.Ldelem_Ref);
                    Type parameterType = parameters[i].ParameterType;
                    if (parameters[i].IsOut || parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();
                    }
                    if (parameterType.IsValueType || parameterType.IsGenericParameter)
                    {
                        ilGen.Emit(OpCodes.Unbox_Any, parameterType);
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Castclass, parameterType);
                    }
                    ilGen.Emit(OpCodes.Stloc, i + 1);
                }
                // context.Return = _target.Update(arg0, ref arg, out arg2, out arg3);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, _targetField);
                for (int index = 0; index < parameters.Length; index++)
                {
                    if (parameters[index].IsOut || parameters[index].ParameterType.IsByRef)
                    {
                        ilGen.Emit(OpCodes.Ldloca_S, index + 1);
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Ldloc_S, index + 1);
                    }
                }
                if (method.ContainsGenericParameters)
                {
                    var genericMethod = method.MakeGenericMethod(methodBuilder.GetGenericArguments());
                    ilGen.Emit(OpCodes.Call, genericMethod);
                }
                else
                {
                    ilGen.Emit(OpCodes.Call, method);
                }

                if (!method.IsVoid())
                {
                    if (method.ReturnType.IsValueType || method.ReturnType.IsGenericParameter)
                    {
                        ilGen.Emit(OpCodes.Box, method.ReturnType);
                    }
                    ilGen.Emit(OpCodes.Callvirt, ReflectionUtilities.GetProperty<InvocationContext, object>(context => context.Return).SetMethod);
                }
                else
                {
                    ilGen.Emit(OpCodes.Pop);
                }

                // arguments[1] = arg;
                for (int index = 0; index < parameters.Length; index++)
                {
                    Type parameterType = parameters[index].ParameterType;
                    if (parameters[index].IsOut || parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();

                        ilGen.Emit(OpCodes.Ldloc, 0);
                        ilGen.Emit(OpCodes.Ldc_I4, index);
                        ilGen.Emit(OpCodes.Ldloc, index + 1);
                        if (parameterType.IsValueType || parameterType.IsGenericParameter)
                        {
                            ilGen.Emit(OpCodes.Box, parameterType);
                        }
                        ilGen.Emit(OpCodes.Stelem_Ref);
                    }
                }

                //return Task.CompletedTask
                ilGen.Emit(OpCodes.Call, ReflectionUtilities.GetProperty<Task, Task>(_ => Task.CompletedTask).GetMethod);
                ilGen.Emit(OpCodes.Ret);

                return methodBuilder;
            }

            private void DefineGenericParameters(Type[] arguments, GenericTypeParameterBuilder[] builders)
            {
                for (int index = 0; index < arguments.Length; index++)
                {
                    var builder = builders[index];
                    var argument = arguments[index];
                    if (!argument.IsGenericParameter)
                    {
                        continue;
                    }
                    builder.SetGenericParameterAttributes(argument.GenericParameterAttributes);
                    var interfaceConstraints = new List<Type>();
                    foreach (var constraint in argument.GetGenericParameterConstraints())
                    {
                        if (constraint.IsClass)
                            builder.SetBaseTypeConstraint(constraint);
                        if (constraint.IsInterface)
                            interfaceConstraints.Add(constraint);
                    }
                    if (interfaceConstraints.Count > 0)
                    {
                        builder.SetInterfaceConstraints(interfaceConstraints.ToArray());
                    }
                }
            }

        }
    }
}
