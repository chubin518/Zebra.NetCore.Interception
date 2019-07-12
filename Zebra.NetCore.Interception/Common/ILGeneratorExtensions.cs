using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Zebra.NetCore.Interception.Common
{
    public static class ILGeneratorExtensions
    {
        private static int GetTypeCode(Type type)
        {
            if (type == null)
                return 0;   // TypeCode.Empty;

            if (type == typeof(Boolean))
                return 3;   // TypeCode.Boolean;

            if (type == typeof(Char))
                return 4;   // TypeCode.Char;

            if (type == typeof(SByte))
                return 5;   // TypeCode.SByte;

            if (type == typeof(Byte))
                return 6;   // TypeCode.Byte;

            if (type == typeof(Int16))
                return 7;   // TypeCode.Int16;

            if (type == typeof(UInt16))
                return 8;   // TypeCode.UInt16;

            if (type == typeof(Int32))
                return 9;   // TypeCode.Int32;

            if (type == typeof(UInt32))
                return 10;  // TypeCode.UInt32;

            if (type == typeof(Int64))
                return 11;  // TypeCode.Int64;

            if (type == typeof(UInt64))
                return 12;  // TypeCode.UInt64;

            if (type == typeof(Single))
                return 13;  // TypeCode.Single;

            if (type == typeof(Double))
                return 14;  // TypeCode.Double;

            if (type == typeof(Decimal))
                return 15;  // TypeCode.Decimal;

            if (type == typeof(DateTime))
                return 16;  // TypeCode.DateTime;

            if (type == typeof(String))
                return 18;  // TypeCode.String;

            if (type.GetTypeInfo().IsEnum)
                return GetTypeCode(Enum.GetUnderlyingType(type));

            return 1;   // TypeCode.Object;
        }

        private static OpCode[] s_ldindOpCodes = new OpCode[] {
                OpCodes.Nop,//Empty = 0,
                OpCodes.Nop,//Object = 1,
                OpCodes.Nop,//DBNull = 2,
                OpCodes.Ldind_I1,//Boolean = 3,
                OpCodes.Ldind_I2,//Char = 4,
                OpCodes.Ldind_I1,//SByte = 5,
                OpCodes.Ldind_U1,//Byte = 6,
                OpCodes.Ldind_I2,//Int16 = 7,
                OpCodes.Ldind_U2,//UInt16 = 8,
                OpCodes.Ldind_I4,//Int32 = 9,
                OpCodes.Ldind_U4,//UInt32 = 10,
                OpCodes.Ldind_I8,//Int64 = 11,
                OpCodes.Ldind_I8,//UInt64 = 12,
                OpCodes.Ldind_R4,//Single = 13,
                OpCodes.Ldind_R8,//Double = 14,
                OpCodes.Nop,//Decimal = 15,
                OpCodes.Nop,//DateTime = 16,
                OpCodes.Nop,//17
                OpCodes.Ldind_Ref,//String = 18,
            };

        private static OpCode[] s_stindOpCodes = new OpCode[] {
                OpCodes.Nop,//Empty = 0,
                OpCodes.Nop,//Object = 1,
                OpCodes.Nop,//DBNull = 2,
                OpCodes.Stind_I1,//Boolean = 3,
                OpCodes.Stind_I2,//Char = 4,
                OpCodes.Stind_I1,//SByte = 5,
                OpCodes.Stind_I1,//Byte = 6,
                OpCodes.Stind_I2,//Int16 = 7,
                OpCodes.Stind_I2,//UInt16 = 8,
                OpCodes.Stind_I4,//Int32 = 9,
                OpCodes.Stind_I4,//UInt32 = 10,
                OpCodes.Stind_I8,//Int64 = 11,
                OpCodes.Stind_I8,//UInt64 = 12,
                OpCodes.Stind_R4,//Single = 13,
                OpCodes.Stind_R8,//Double = 14,
                OpCodes.Nop,//Decimal = 15,
                OpCodes.Nop,//DateTime = 16,
                OpCodes.Nop,//17
                OpCodes.Stind_Ref,//String = 18,
            };


        public static void Ldind(this ILGenerator il, Type type)
        {
            OpCode opCode = s_ldindOpCodes[GetTypeCode(type)];
            if (!opCode.Equals(OpCodes.Nop))
            {
                il.Emit(opCode);
            }
            else
            {
                il.Emit(OpCodes.Ldobj, type);
            }
        }

        public static void Stind(this ILGenerator il, Type type)
        {
            OpCode opCode = s_stindOpCodes[GetTypeCode(type)];
            if (!opCode.Equals(OpCodes.Nop))
            {
                il.Emit(opCode);
            }
            else
            {
                il.Emit(OpCodes.Stobj, type);
            }
        }
    }
}
