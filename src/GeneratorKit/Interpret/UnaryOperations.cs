using System;

namespace GeneratorKit.Interpret;

internal static class UnaryOperations
{
  public static object Increment(object? target)
  {
    if (target is null)
      throw new ArgumentNullException(nameof(target));

    return Convert.GetTypeCode(target) switch
    {
      TypeCode.Char    => (char)((char)target + 1),
      TypeCode.Byte    => (byte)((byte)target + 1),
      TypeCode.SByte   => (sbyte)((sbyte)target + 1),
      TypeCode.Int16   => (short)((short)target + 1),
      TypeCode.UInt16  => (ushort)((ushort)target + 1),
      TypeCode.Int32   => (int)target + 1,
      TypeCode.UInt32  => (uint)target + 1,
      TypeCode.Int64   => (long)target + 1,
      TypeCode.UInt64  => (ulong)target + 1,
      TypeCode.Single  => (float)target + 1.0f,
      TypeCode.Double  => (double)target + 1.0,
      TypeCode.Decimal => (decimal)target + 1.0m,
      _                => throw new ArgumentException($"Increment (++) operation was supplied an invalid target of type {target.GetType()}.", nameof(target))
    };
  }

  public static object Decrement(object? target)
  {
    if (target is null)
      throw new ArgumentNullException(nameof(target));

    return Convert.GetTypeCode(target) switch
    {
      TypeCode.Char    => (char)((char)target - 1),
      TypeCode.Byte    => (byte)((byte)target - 1),
      TypeCode.SByte   => (sbyte)((sbyte)target - 1),
      TypeCode.Int16   => (short)((short)target - 1),
      TypeCode.UInt16  => (ushort)((ushort)target - 1),
      TypeCode.Int32   => (int)target - 1,
      TypeCode.UInt32  => (uint)target - 1,
      TypeCode.Int64   => (long)target - 1,
      TypeCode.UInt64  => (ulong)target - 1,
      TypeCode.Single  => (float)target - 1.0f,
      TypeCode.Double  => (double)target - 1.0,
      TypeCode.Decimal => (decimal)target - 1.0m,
      _                => throw new ArgumentException($"Decrement (--) operation was supplied an invalid target of type {target.GetType()}.", nameof(target))
    };
  }

  public static object BitwiseNegation(object? target)
  {
    if (target is null)
      throw new ArgumentNullException(nameof(target));

    return Convert.GetTypeCode(target) switch
    {
      TypeCode.Char    => ~(char)target,
      TypeCode.Byte    => ~(byte)target,
      TypeCode.SByte   => ~(sbyte)target,
      TypeCode.Int16   => ~(short)target,
      TypeCode.UInt16  => ~(ushort)target,
      TypeCode.Int32   => ~(int)target,
      TypeCode.UInt32  => ~(uint)target,
      TypeCode.Int64   => ~(long)target,
      _                => throw new ArgumentException($"Bitwise negation (~) operation was supplied an invalid target of type {target.GetType()}.", nameof(target))
    };
  }

  public static object Not(object? target)
  {
    if (target is null)
      throw new ArgumentNullException(nameof(target));

    return Convert.GetTypeCode(target) switch
    {
      TypeCode.Boolean => !(bool)target,
      _                => throw new ArgumentException($"Not (!) operation was supplied an invalid target of type {target.GetType()}.", nameof(target))
    };
  }

  public static object Plus(object? target)
  {
    if (target is null)
      throw new ArgumentNullException(nameof(target));

    return Convert.GetTypeCode(target) switch
    {
      TypeCode.Char    => +(char)target,
      TypeCode.Byte    => +(byte)target,
      TypeCode.SByte   => +(sbyte)target,
      TypeCode.Int16   => +(short)target,
      TypeCode.UInt16  => +(ushort)target,
      TypeCode.Int32   => +(int)target,
      TypeCode.UInt32  => +(uint)target,
      TypeCode.Int64   => +(long)target,
      TypeCode.UInt64  => +(ulong)target,
      TypeCode.Single  => +(float)target,
      TypeCode.Double  => +(double)target,
      TypeCode.Decimal => +(decimal)target,
      _                => throw new ArgumentException($"Plus (+) operation was supplied an invalid target of type {target.GetType()}.", nameof(target))
    };
  }

  public static object Minus(object? target)
  {
    if (target is null)
      throw new ArgumentNullException(nameof(target));

    return Convert.GetTypeCode(target) switch
    {
      TypeCode.Char    => -(char)target,
      TypeCode.Byte    => -(byte)target,
      TypeCode.SByte   => -(sbyte)target,
      TypeCode.Int16   => -(short)target,
      TypeCode.UInt16  => -(ushort)target,
      TypeCode.Int32   => -(int)target,
      TypeCode.UInt32  => -(uint)target,
      TypeCode.Int64   => -(long)target,
      TypeCode.Single  => -(float)target,
      TypeCode.Double  => -(double)target,
      TypeCode.Decimal => -(decimal)target,
      _                => throw new ArgumentException($"Minus (-) operation was supplied an invalid target of type {target.GetType()}.", nameof(target))
    };
  }

  public static object Hat(object? target)
  {
    if (target is null)
      throw new ArgumentNullException(nameof(target));

    return Convert.GetTypeCode(target) switch
    {
      TypeCode.Char   => ^(char)target,
      TypeCode.Byte   => ^(byte)target,
      TypeCode.SByte  => ^(sbyte)target,
      TypeCode.Int16  => ^(short)target,
      TypeCode.UInt16 => ^(ushort)target,
      TypeCode.Int32  => ^(int)target,
      _               => throw new ArgumentException($"Hat (^) operation was supplied an invalid target of type {target.GetType()}.", nameof(target))
    };
  }
}