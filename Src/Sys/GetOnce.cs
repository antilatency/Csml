

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;



namespace Csml {

    /*[PSerializable]
    public class GetOnceAttribute : LocationInterceptionAspect {
        Dictionary<Type,object> Backup; //For generic types
        //object Backup;
        public override void OnGetValue(LocationInterceptionArgs args) {

            StackTrace stackTrace = new StackTrace();
            if (Backup == null) Backup = new Dictionary<Type, object>();
            if (Backup.ContainsKey(args.Location.PropertyInfo.PropertyType)) {
                args.Value = Backup[args.Location.PropertyInfo.PropertyType];
                return;
            }


            args.ProceedGetValue();
            Backup.Add(args.Location.PropertyInfo.PropertyType, args.Value);

            if (args.Value is IPropertyInitializer) {
                
                (args.Value as IPropertyInitializer).AfterInitialization(args.Instance, args.LocationName, args.Location.PropertyInfo);
            }
        }
    }

    public interface IPropertyInitializer {
        void AfterInitialization(object parent, string propertyName, PropertyInfo propertyInfo);
    }*/

    /*public class GetOnce {

        public interface IStaticPropertyInitializer {
            void AfterInitialization(PropertyInfo propertyInfo);
        }

        public static Harmony harmony = new Harmony("com.antilatency.csml");

        public class Gen<T, I> {
            public static PropertyInfo PropertyInfo;
            public static T Backup;
            public static bool Initialized = false;
            public static bool Prefix(ref T __result) {
                Console.WriteLine($"Prefix of {PropertyInfo.Name} initialized:{Initialized}");
                if (Initialized) {
                    __result = Backup;
                }
                return !Initialized;
            }
            public static void Postfix(T __result) {
                Console.WriteLine($"Postfix of {PropertyInfo.Name} initialized:{Initialized}");
                if (!Initialized) {
                    Backup = __result;
                    Initialized = true;
                    if (Backup != null) {
                        if (Backup is IStaticPropertyInitializer) {
                            (Backup as IStaticPropertyInitializer).AfterInitialization(PropertyInfo);
                        }
                    }
                }
            }
        }


        private static Dictionary<Type, Type> currentType = new Dictionary<Type, Type>();
        public static Type GetNewType(Type propertyType) {
            if (!currentType.ContainsKey(propertyType)) {
                currentType.Add(propertyType, typeof(int));
            }
            var lastType = currentType[propertyType];
            var result = typeof(Gen<,>).MakeGenericType(propertyType, lastType);
            currentType[propertyType] = result;
            return result;
        }

        public static void WrapPropertyGetter(PropertyInfo propertyInfo) {

            var wrapper = GetNewType(propertyInfo.PropertyType);
            wrapper.GetField("PropertyInfo", BindingFlags.Static | BindingFlags.Public).SetValue(null, propertyInfo);

            Console.WriteLine($"WrapPropertyGetter of {propertyInfo.Name} wrapper type is:{wrapper.FullName}");

            var classProcessor = harmony.CreateProcessor(propertyInfo.GetMethod);

            var prefix = wrapper.GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var postfix = wrapper.GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            classProcessor.AddPrefix(prefix);
            classProcessor.AddPostfix(postfix);
            classProcessor.Patch();

        }
    }*/
    public class GetOnce {
        public static bool Log = false;
        public interface IStaticPropertyInitializer {
            void AfterInitialization(PropertyInfo propertyInfo);
        }
        public class Gen<T, I> {
            public static PropertyInfo PropertyInfo;
            public static object Backup;
            public static MethodInfo MethodInfo = null;

            public static T GetValue() {                
                if (Backup == null) {
                    if (Log) Console.WriteLine($"{PropertyInfo.DeclaringType.Name}.{PropertyInfo.Name} Call original getter.");
                    Backup = MethodInfo.Invoke(null, new object[] { });
                    if (Backup != null) {
                        if (Backup is IStaticPropertyInitializer) {
                            (Backup as IStaticPropertyInitializer).AfterInitialization(PropertyInfo);
                        }
                    }
                }

                if (Log) Console.WriteLine($"{PropertyInfo.DeclaringType.Name}.{PropertyInfo.Name} Use previously returned value.");
                return (T)Backup;
            }
        }


        private static Dictionary<Type, Type> currentType = new Dictionary<Type, Type>();
        public static Type GetNewType(Type propertyType) {
            if (!currentType.ContainsKey(propertyType)) {
                currentType.Add(propertyType, typeof(int));
            }
            var lastType = currentType[propertyType];
            var result = typeof(Gen<,>).MakeGenericType(propertyType, lastType);
            currentType[propertyType] = result;
            return result;
        }

        /*[MethodImpl(MethodImplOptions.NoInlining, MethodCodeType = MethodCodeType.IL)]
        public static object A() {
            Console.WriteLine("A");
            return A();
        }*/

        public static void WrapPropertyGetter(PropertyInfo propertyInfo) {
            var wrapper = GetNewType(propertyInfo.PropertyType);
            wrapper.GetField("PropertyInfo", BindingFlags.Static | BindingFlags.Public).SetValue(null, propertyInfo);
            var a = wrapper.GetMethod("GetValue", BindingFlags.Static | BindingFlags.Public);
            var b = propertyInfo.GetMethod;
            SwapMethods<long>(a, b);
            wrapper.GetField("MethodInfo", BindingFlags.Static | BindingFlags.Public).SetValue(null, a);

        }

        private unsafe static void SwapMethods<T>(MethodInfo a, MethodInfo b) where T : struct {
            RuntimeHelpers.PrepareMethod(a.MethodHandle);
            RuntimeHelpers.PrepareMethod(b.MethodHandle);

            var methodHandleA = (IntPtr*)a.MethodHandle.Value.ToPointer();
            var methodHandleB = (IntPtr*)b.MethodHandle.Value.ToPointer();
            var functionPointerA = (IntPtr)a.MethodHandle.GetFunctionPointer().ToPointer();
            var functionPointerB = (IntPtr)b.MethodHandle.GetFunctionPointer().ToPointer();

            //Console.WriteLine("A flags " + (*methodHandleA).ToString("X16"));
            //Console.WriteLine("B flags " + (*methodHandleB).ToString("X16"));
            IntPtr* pFunctionPointerA = methodHandleA + 1;
            IntPtr* pFunctionPointerB = methodHandleB + 1;
            int maxOffset = 100;
            for (int i = 0; i <= maxOffset; i++) {
                if (*pFunctionPointerA == functionPointerA) {
                    //Console.WriteLine("A found at "+i);
                    break;
                }
                pFunctionPointerA++;
            }
            for (int i = 0; i <= maxOffset; i++) {
                if (*pFunctionPointerB == functionPointerB) {
                    //Console.WriteLine("B found at " + i);
                    break;
                }
                pFunctionPointerB++;
            }


#if DEBUG
            int* pOffsetA = (int*)(((byte*)functionPointerA) + 1);
            int* pOffsetB = (int*)(((byte*)functionPointerB) + 1);
            //Console.WriteLine("A offset " + (*pOffsetA).ToString("X16"));
            //Console.WriteLine("B offset " + (*pOffsetB).ToString("X16"));

            int temp = (int)((long)functionPointerA + *pOffsetA - (long)functionPointerB);
            *pOffsetA = (int)((long)functionPointerB + *pOffsetB - (long)functionPointerA);
            *pOffsetB = temp;


#else
            //Console.WriteLine("\nVersion x64 Release\n");
            //Console.WriteLine(*pFunctionPointerA);
            //Console.WriteLine(*pFunctionPointerB);
            //Console.WriteLine("a GetFunctionPointer "+ (long)a.MethodHandle.GetFunctionPointer().ToPointer());
                    
            long temp = *pFunctionPointerB;
            *pFunctionPointerB = *pFunctionPointerA;
            *pFunctionPointerA = temp;

            //Console.WriteLine("a GetFunctionPointer " + (long)a.MethodHandle.GetFunctionPointer().ToPointer());
#endif


        }
    }
}

