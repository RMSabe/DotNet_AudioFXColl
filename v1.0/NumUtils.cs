/*
	Some Numeric Utilities in C# dotnet core

	Author: Rafael Sabe
	Email: rafaelmsabe@gmail.com
*/

/*
	Description:

	Uxx == xx bits long unsigned integer
	Ixx == xx bits long signed integer
	Fxx == xx bits long floating point variable

	USys == System unsigned integer (64bits for 64bit OS, 32bits for 32bit OS)
	ISys == System signed integer (64bits for 64bit OS, 32bits for 32bit OS)

	LE == Little Endian (default)
	BE == Big Endian

	Warning:
	This code requires unsafe code blocks to be enabled.
*/

using System;

namespace MyLib;

public static class NumUtils
{
	public static nuint USysGetMaximum()
	{
		nuint nsys = 0u;
		nsys = ~nsys;
		return nsys;
	}

	public static nint ISysGetMaximum()
	{
		nuint nsys = USysGetMaximum();
		nsys = (nsys >> 1);
		return (nint) nsys;
	}

	public static nint ISysGetMinimum()
	{
		nuint nsys = (nuint) ISysGetMaximum();
		nsys = ~nsys;
		return (nint) nsys;
	}

	public static ulong U64GetMaximum()
	{
		return 0xffffffffffffffffUL;
	}

	public static long I64GetMaximum()
	{
		return 0x7fffffffffffffffL;
	}

	public static long I64GetMinimum()
	{
		return -0x8000000000000000L;
	}

	public static uint U32GetMaximum()
	{
		return 0xffffffffU;
	}

	public static int I32GetMaximum()
	{
		return 0x7fffffff;
	}

	public static int I32GetMinimum()
	{
		return -0x80000000;
	}

	public static ushort U16GetMaximum()
	{
		return (ushort) 0xffff;
	}

	public static short I16GetMaximum()
	{
		return (short) 0x7fff;
	}

	public static short I16GetMinimum()
	{
		return (short) -0x8000;
	}

	public static byte U8GetMaximum()
	{
		return (byte) 0xff;
	}

	public static sbyte I8GetMaximum()
	{
		return (sbyte) 0x7f;
	}

	public static sbyte I8GetMinimum()
	{
		return (sbyte) -0x80;
	}

	public static ulong BytesToU64LE(byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 8U)) return 0UL;

		return (ulong) ((bytebuf[offset + 7U] << 56) | (bytebuf[offset + 6U] << 48) | (bytebuf[offset + 5U] << 40) | (bytebuf[offset + 4U] << 32) | (bytebuf[offset + 3U] << 24) | (bytebuf[offset + 2U] << 16) | (bytebuf[offset + 1U] << 8) | (bytebuf[offset]));
	}

	public static ulong BytesToU64BE(byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 8U)) return 0UL;

		return (ulong) ((bytebuf[offset] << 56) | (bytebuf[offset + 1U] << 48) | (bytebuf[offset + 2U] << 40) | (bytebuf[offset + 3U] << 32) | (bytebuf[offset + 4U] << 24) | (bytebuf[offset + 5U] << 16) | (bytebuf[offset + 6U] << 8) | (bytebuf[offset + 7U]));
	}

	public static long BytesToI64LE(byte[] bytebuf, nuint offset)
	{
		return (long) BytesToU64LE(bytebuf, offset);
	}

	public static long BytesToI64BE(byte[] bytebuf, nuint offset)
	{
		return (long) BytesToU64BE(bytebuf, offset);
	}

	public static uint BytesToU32LE(byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 4U)) return 0U;

		return (uint) ((bytebuf[offset + 3U] << 24) | (bytebuf[offset + 2U] << 16) | (bytebuf[offset + 1U] << 8) | (bytebuf[offset]));
	}

	public static uint BytesToU32BE(byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 4U)) return 0U;

		return (uint) ((bytebuf[offset] << 24) | (bytebuf[offset + 1U] << 16) | (bytebuf[offset + 2U] << 8) | (bytebuf[offset + 3U]));
	}

	public static int BytesToI32LE(byte[] bytebuf, nuint offset)
	{
		return (int) BytesToU32LE(bytebuf, offset);
	}

	public static int BytesToI32BE(byte[] bytebuf, nuint offset)
	{
		return (int) BytesToU32BE(bytebuf, offset);
	}

	public static ushort BytesToU16LE(byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 2U)) return (ushort) 0U;

		return (ushort) ((bytebuf[offset + 1U] << 8) | (bytebuf[offset]));
	}

	public static ushort BytesToU16BE(byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 2U)) return (ushort) 0U;

		return (ushort) ((bytebuf[offset] << 8) | (bytebuf[offset + 1U]));
	}

	public static short BytesToI16LE(byte[] bytebuf, nuint offset)
	{
		return (short) BytesToU16LE(bytebuf, offset);
	}

	public static short BytesToI16BE(byte[] bytebuf, nuint offset)
	{
		return (short) BytesToU16BE(bytebuf, offset);
	}

	unsafe public static double BytesToF64LE(byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 8U)) return 0.0;

		double f64 = 0.0;
		ulong *pf64 = (ulong*) &f64;

		*pf64 = (ulong) ((bytebuf[offset + 7U] << 56) | (bytebuf[offset + 6U] << 48) | (bytebuf[offset + 5U] << 40) | (bytebuf[offset + 4U] << 32) | (bytebuf[offset + 3U] << 24) | (bytebuf[offset + 2U] << 16) | (bytebuf[offset + 1U] << 8) | (bytebuf[offset]));
		return f64;
	}

	unsafe public static double BytesToF64BE(byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 8U)) return 0.0;

		double f64 = 0.0;
		ulong *pf64 = (ulong*) &f64;

		*pf64 = (ulong) ((bytebuf[offset] << 56) | (bytebuf[offset + 1U] << 48) | (bytebuf[offset + 2U] << 40) | (bytebuf[offset + 3U] << 32) | (bytebuf[offset + 4U] << 24) | (bytebuf[offset + 5U] << 16) | (bytebuf[offset + 6U] << 8) | (bytebuf[offset + 7U]));
		return f64;
	}

	unsafe public static float BytesToF32LE(byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 4U)) return 0.0f;

		float f32 = 0.0f;
		uint *pf32 = (uint*) &f32;

		*pf32 = (uint) ((bytebuf[offset + 3U] << 24) | (bytebuf[offset + 2U] << 16) | (bytebuf[offset + 1U] << 8) | (bytebuf[offset]));
		return f32;
	}

	unsafe public static float BytesToF32BE(byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 4U)) return 0.0f;

		float f32 = 0.0f;
		uint *pf32 = (uint*) &f32;

		*pf32 = (uint) ((bytebuf[offset] << 24) | (bytebuf[offset + 1U] << 16) | (bytebuf[offset + 2U] << 8) | (bytebuf[offset + 3U]));
		return f32;
	}

	public static bool U64ToBytesLE(ulong u64, byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 8U)) return false;

		bytebuf[offset] = (byte) (u64 & 0xff);
		bytebuf[offset + 1U] = (byte) ((u64 >> 8) & 0xff);
		bytebuf[offset + 2U] = (byte) ((u64 >> 16) & 0xff);
		bytebuf[offset + 3U] = (byte) ((u64 >> 24) & 0xff);
		bytebuf[offset + 4U] = (byte) ((u64 >> 32) & 0xff);
		bytebuf[offset + 5U] = (byte) ((u64 >> 40) & 0xff);
		bytebuf[offset + 6U] = (byte) ((u64 >> 48) & 0xff);
		bytebuf[offset + 7U] = (byte) (u64 >> 56);

		return true;
	}

	public static bool U64ToBytesBE(ulong u64, byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 8U)) return false;

		bytebuf[offset] = (byte) (u64 >> 56);
		bytebuf[offset + 1U] = (byte) ((u64 >> 48) & 0xff);
		bytebuf[offset + 2U] = (byte) ((u64 >> 40) & 0xff);
		bytebuf[offset + 3U] = (byte) ((u64 >> 32) & 0xff);
		bytebuf[offset + 4U] = (byte) ((u64 >> 24) & 0xff);
		bytebuf[offset + 5U] = (byte) ((u64 >> 16) & 0xff);
		bytebuf[offset + 6U] = (byte) ((u64 >> 8) & 0xff);
		bytebuf[offset + 7U] = (byte) (u64 & 0xff);

		return true;
	}

	public static bool I64ToBytesLE(long i64, byte[] bytebuf, nuint offset)
	{
		return U64ToBytesLE((ulong) i64, bytebuf, offset);
	}

	public static bool I64ToBytesBE(long i64, byte[] bytebuf, nuint offset)
	{
		return U64ToBytesBE((ulong) i64, bytebuf, offset);
	}

	public static bool U32ToBytesLE(uint u32, byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 4U)) return false;

		bytebuf[offset] = (byte) (u32 & 0xff);
		bytebuf[offset + 1U] = (byte) ((u32 >> 8) & 0xff);
		bytebuf[offset + 2U] = (byte) ((u32 >> 16) & 0xff);
		bytebuf[offset + 3U] = (byte) (u32 >> 24);

		return true;
	}

	public static bool U32ToBytesBE(uint u32, byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 4U)) return false;

		bytebuf[offset] = (byte) (u32 >> 24);
		bytebuf[offset + 1U] = (byte) ((u32 >> 16) & 0xff);
		bytebuf[offset + 2U] = (byte) ((u32 >> 8) & 0xff);
		bytebuf[offset + 3U] = (byte) (u32 & 0xff);

		return true;
	}

	public static bool I32ToBytesLE(int i32, byte[] bytebuf, nuint offset)
	{
		return U32ToBytesLE((uint) i32, bytebuf, offset);
	}

	public static bool I32ToBytesBE(int i32, byte[] bytebuf, nuint offset)
	{
		return U32ToBytesBE((uint) i32, bytebuf, offset);
	}

	public static bool U16ToBytesLE(ushort u16, byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 2U)) return false;

		bytebuf[offset] = (byte) (u16 & 0xff);
		bytebuf[offset + 1U] = (byte) (u16 >> 8);

		return true;
	}

	public static bool U16ToBytesBE(ushort u16, byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 2U)) return false;

		bytebuf[offset] = (byte) (u16 >> 8);
		bytebuf[offset + 1U] = (byte) (u16 & 0xff);

		return true;
	}

	public static bool I16ToBytesLE(short i16, byte[] bytebuf, nuint offset)
	{
		return U16ToBytesLE((ushort) i16, bytebuf, offset);
	}

	public static bool I16ToBytesBE(short i16, byte[] bytebuf, nuint offset)
	{
		return U16ToBytesBE((ushort) i16, bytebuf, offset);
	}

	unsafe public static bool F64ToBytesLE(double f64, byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 8U)) return false;

		ulong *pf64 = (ulong*) &f64;

		bytebuf[offset] = (byte) (*pf64 & 0xff);
		bytebuf[offset + 1U] = (byte) ((*pf64 >> 8) & 0xff);
		bytebuf[offset + 2U] = (byte) ((*pf64 >> 16) & 0xff);
		bytebuf[offset + 3U] = (byte) ((*pf64 >> 24) & 0xff);
		bytebuf[offset + 4U] = (byte) ((*pf64 >> 32) & 0xff);
		bytebuf[offset + 5U] = (byte) ((*pf64 >> 40) & 0xff);
		bytebuf[offset + 6U] = (byte) ((*pf64 >> 48) & 0xff);
		bytebuf[offset + 7U] = (byte) (*pf64 >> 56);

		return true;
	}

	unsafe public static bool F64ToBytesBE(double f64, byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 8U)) return false;

		ulong *pf64 = (ulong*) &f64;

		bytebuf[offset] = (byte) (*pf64 >> 56);
		bytebuf[offset + 1U] = (byte) ((*pf64 >> 48) & 0xff);
		bytebuf[offset + 2U] = (byte) ((*pf64 >> 40) & 0xff);
		bytebuf[offset + 3U] = (byte) ((*pf64 >> 32) & 0xff);
		bytebuf[offset + 4U] = (byte) ((*pf64 >> 24) & 0xff);
		bytebuf[offset + 5U] = (byte) ((*pf64 >> 16) & 0xff);
		bytebuf[offset + 6U] = (byte) ((*pf64 >> 8) & 0xff);
		bytebuf[offset + 7U] = (byte) (*pf64 & 0xff);

		return true;
	}

	unsafe public static bool F32ToBytesLE(float f32, byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 4U)) return false;

		uint *pf32 = (uint*) &f32;

		bytebuf[offset] = (byte) (*pf32 & 0xff);
		bytebuf[offset + 1U] = (byte) ((*pf32 >> 8) & 0xff);
		bytebuf[offset + 2U] = (byte) ((*pf32 >> 16) & 0xff);
		bytebuf[offset + 3U] = (byte) (*pf32 >> 24);

		return true;
	}

	unsafe public static bool F32ToBytesBE(float f32, byte[] bytebuf, nuint offset)
	{
		if(((nuint) bytebuf.Length) < (offset + 4U)) return false;

		uint *pf32 = (uint*) &f32;

		bytebuf[offset] = (byte) (*pf32 >> 24);
		bytebuf[offset + 1U] = (byte) ((*pf32 >> 16) & 0xff);
		bytebuf[offset + 2U] = (byte) ((*pf32 >> 8) & 0xff);
		bytebuf[offset + 3U] = (byte) (*pf32 & 0xff);

		return true;
	}

	public static ulong BytesToU64(byte[] bytebuf, nuint offset)
	{
		return BytesToU64LE(bytebuf, offset);
	}

	public static long BytesToI64(byte[] bytebuf, nuint offset)
	{
		return BytesToI64LE(bytebuf, offset);
	}

	public static uint BytesToU32(byte[] bytebuf, nuint offset)
	{
		return BytesToU32LE(bytebuf, offset);
	}

	public static int BytesToI32(byte[] bytebuf, nuint offset)
	{
		return BytesToI32LE(bytebuf, offset);
	}

	public static ushort BytesToU16(byte[] bytebuf, nuint offset)
	{
		return BytesToU16LE(bytebuf, offset);
	}

	public static short BytesToI16(byte[] bytebuf, nuint offset)
	{
		return BytesToI16LE(bytebuf, offset);
	}

	public static double BytesToF64(byte[] bytebuf, nuint offset)
	{
		return BytesToF64LE(bytebuf, offset);
	}

	public static float BytesToF32(byte[] bytebuf, nuint offset)
	{
		return BytesToF32LE(bytebuf, offset);
	}

	public static bool U64ToBytes(ulong u64, byte[] bytebuf, nuint offset)
	{
		return U64ToBytesLE(u64, bytebuf, offset);
	}

	public static bool I64ToBytes(long i64, byte[] bytebuf, nuint offset)
	{
		return I64ToBytesLE(i64, bytebuf, offset);
	}

	public static bool U32ToBytes(uint u32, byte[] bytebuf, nuint offset)
	{
		return U32ToBytesLE(u32, bytebuf, offset);
	}

	public static bool I32ToBytes(int i32, byte[] bytebuf, nuint offset)
	{
		return I32ToBytesLE(i32, bytebuf, offset);
	}

	public static bool U16ToBytes(ushort u16, byte[] bytebuf, nuint offset)
	{
		return U16ToBytesLE(u16, bytebuf, offset);
	}

	public static bool I16ToBytes(short i16, byte[] bytebuf, nuint offset)
	{
		return I16ToBytesLE(i16, bytebuf, offset);
	}

	public static bool F64ToBytes(double f64, byte[] bytebuf, nuint offset)
	{
		return F64ToBytesLE(f64, bytebuf, offset);
	}

	public static bool F32ToBytes(float f32, byte[] bytebuf, nuint offset)
	{
		return F32ToBytesLE(f32, bytebuf, offset);
	}
}
