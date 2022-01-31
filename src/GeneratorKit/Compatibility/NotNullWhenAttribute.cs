// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies that when a method returns System.Diagnostics.CodeAnalysis.NotNullWhenAttribute.ReturnValue,
/// the parameter will not be null even if the corresponding type allows it.
/// </summary>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
internal sealed class NotNullWhenAttribute : Attribute
{
  /// <summary>
  /// Initializes the attribute with the specified return value condition.
  /// </summary>
  /// <param name="returnValue">
  /// The return value condition. If the method returns this value, the associated
  /// parameter will not be null.
  /// </param>
  public NotNullWhenAttribute(bool returnValue)
  {
    ReturnValue = returnValue;
  }

  /// <summary>
  /// Gets the return value condition.
  /// </summary>
  /// <returns>
  /// The return value condition. If the method returns this value, the associated
  /// parameter will not be null.
  /// </returns>
  public bool ReturnValue { get; }
}