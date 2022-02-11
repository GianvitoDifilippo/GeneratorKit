using System;

namespace GeneratorKit.Interpret;

internal static class BinaryOperations
{
  public static object Add(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));
    
    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left     + (int)right,
      TypeCode.UInt32  => (uint)left    + (uint)right,
      TypeCode.Int64   => (long)left    + (long)right,
      TypeCode.UInt64  => (ulong)left   + (ulong)right,
      TypeCode.Single  => (float)left   + (float)right,
      TypeCode.Double  => (double)left  + (double)right,
      TypeCode.Decimal => (decimal)left + (decimal)right,
      TypeCode.String  => (string)left  + (string)right,
      _                => throw new ArgumentException($"Add (+) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object Subtract(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left     - (int)right,
      TypeCode.UInt32  => (uint)left    - (uint)right,
      TypeCode.Int64   => (long)left    - (long)right,
      TypeCode.UInt64  => (ulong)left   - (ulong)right,
      TypeCode.Single  => (float)left   - (float)right,
      TypeCode.Double  => (double)left  - (double)right,
      TypeCode.Decimal => (decimal)left - (decimal)right,
      _                => throw new ArgumentException($"Subtract (-) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object Multiply(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left     * (int)right,
      TypeCode.UInt32  => (uint)left    * (uint)right,
      TypeCode.Int64   => (long)left    * (long)right,
      TypeCode.UInt64  => (ulong)left   * (ulong)right,
      TypeCode.Single  => (float)left   * (float)right,
      TypeCode.Double  => (double)left  * (double)right,
      TypeCode.Decimal => (decimal)left * (decimal)right,
      _                => throw new ArgumentException($"Multiply (*) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object Divide(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left     * (int)right,
      TypeCode.UInt32  => (uint)left    * (uint)right,
      TypeCode.Int64   => (long)left    * (long)right,
      TypeCode.UInt64  => (ulong)left   * (ulong)right,
      TypeCode.Single  => (float)left   * (float)right,
      TypeCode.Double  => (double)left  * (double)right,
      TypeCode.Decimal => (decimal)left * (decimal)right,
      _                => throw new ArgumentException($"Divide (/) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object Remainder(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left     % (int)right,
      TypeCode.UInt32  => (uint)left    % (uint)right,
      TypeCode.Int64   => (long)left    % (long)right,
      TypeCode.UInt64  => (ulong)left   % (ulong)right,
      TypeCode.Single  => (float)left   % (float)right,
      TypeCode.Double  => (double)left  % (double)right,
      TypeCode.Decimal => (decimal)left % (decimal)right,
      _                => throw new ArgumentException($"Divide (/) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object LeftShift(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left   << (int)right,
      TypeCode.UInt32  => (uint)left  << (int)right,
      TypeCode.Int64   => (long)left  << (int)right,
      TypeCode.UInt64  => (ulong)left << (int)right,
      _                => throw new ArgumentException($"Left shift (<<) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object RightShift(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left   >> (int)right,
      TypeCode.UInt32  => (uint)left  >> (int)right,
      TypeCode.Int64   => (long)left  >> (int)right,
      TypeCode.UInt64  => (ulong)left >> (int)right,
      _                => throw new ArgumentException($"Right shift (>>) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object And(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Boolean => (bool)left  & (bool)right,
      TypeCode.Int32   => (int)left   & (int)right,
      TypeCode.UInt32  => (uint)left  & (uint)right,
      TypeCode.Int64   => (long)left  & (long)right,
      TypeCode.UInt64  => (ulong)left & (ulong)right,
      _                => throw new ArgumentException($"And (&) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object Or(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Boolean => (bool)left  | (bool)right,
      TypeCode.Int32   => (int)left   | (int)right,
      TypeCode.UInt32  => (uint)left  | (uint)right,
      TypeCode.Int64   => (long)left  | (long)right,
      TypeCode.UInt64  => (ulong)left | (ulong)right,
      _                => throw new ArgumentException($"Or (|) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object ExclusiveOr(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Boolean => (bool)left  ^ (bool)right,
      TypeCode.Int32   => (int)left   ^ (int)right,
      TypeCode.UInt32  => (uint)left  ^ (uint)right,
      TypeCode.Int64   => (long)left  ^ (long)right,
      TypeCode.UInt64  => (ulong)left ^ (ulong)right,
      _                => throw new ArgumentException($"Exclusive or (^) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object ConditionalAnd(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Boolean => (bool)left && (bool)right,
      _                => throw new ArgumentException($"Conditional and (&&) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object ConditionalOr(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Boolean => (bool)left || (bool)right,
      _                => throw new ArgumentException($"Conditional and (&&) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object Equal(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left     == (int)right,
      TypeCode.UInt32  => (uint)left    == (uint)right,
      TypeCode.Int64   => (long)left    == (long)right,
      TypeCode.UInt64  => (ulong)left   == (ulong)right,
      TypeCode.Single  => (float)left   == (float)right,
      TypeCode.Double  => (double)left  == (double)right,
      TypeCode.Decimal => (decimal)left == (decimal)right,
      _                => throw new ArgumentException($"Equals (==) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object NotEqual(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));

    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left     != (int)right,
      TypeCode.UInt32  => (uint)left    != (uint)right,
      TypeCode.Int64   => (long)left    != (long)right,
      TypeCode.UInt64  => (ulong)left   != (ulong)right,
      TypeCode.Single  => (float)left   != (float)right,
      TypeCode.Double  => (double)left  != (double)right,
      TypeCode.Decimal => (decimal)left != (decimal)right,
      _                => throw new ArgumentException($"Not equals (!=) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object LessThan(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));
    
    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left     < (int)right,
      TypeCode.UInt32  => (uint)left    < (uint)right,
      TypeCode.Int64   => (long)left    < (long)right,
      TypeCode.UInt64  => (ulong)left   < (ulong)right,
      TypeCode.Single  => (float)left   < (float)right,
      TypeCode.Double  => (double)left  < (double)right,
      TypeCode.Decimal => (decimal)left < (decimal)right,
      _                => throw new ArgumentException($"Less than (<) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object LessThanOrEqual(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));
    
    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left     <= (int)right,
      TypeCode.UInt32  => (uint)left    <= (uint)right,
      TypeCode.Int64   => (long)left    <= (long)right,
      TypeCode.UInt64  => (ulong)left   <= (ulong)right,
      TypeCode.Single  => (float)left   <= (float)right,
      TypeCode.Double  => (double)left  <= (double)right,
      TypeCode.Decimal => (decimal)left <= (decimal)right,
      _                => throw new ArgumentException($"Less than or equal (<=) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object GreaterThanOrEqual(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));
    
    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left     >= (int)right,
      TypeCode.UInt32  => (uint)left    >= (uint)right,
      TypeCode.Int64   => (long)left    >= (long)right,
      TypeCode.UInt64  => (ulong)left   >= (ulong)right,
      TypeCode.Single  => (float)left   >= (float)right,
      TypeCode.Double  => (double)left  >= (double)right,
      TypeCode.Decimal => (decimal)left >= (decimal)right,
      _                => throw new ArgumentException($"Greater than or equal (>=) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }

  public static object GreaterThan(object? left, object? right)
  {
    if (left is null)
      throw new ArgumentNullException(nameof(left));
    if (right is null)
      throw new ArgumentNullException(nameof(right));
    
    return Convert.GetTypeCode(left) switch
    {
      TypeCode.Int32   => (int)left     > (int)right,
      TypeCode.UInt32  => (uint)left    > (uint)right,
      TypeCode.Int64   => (long)left    > (long)right,
      TypeCode.UInt64  => (ulong)left   > (ulong)right,
      TypeCode.Single  => (float)left   > (float)right,
      TypeCode.Double  => (double)left  > (double)right,
      TypeCode.Decimal => (decimal)left > (decimal)right,
      _                => throw new ArgumentException($"Greater than (>) operation was supplied an invalid target of type {left.GetType()}.", nameof(left))
    };
  }
}