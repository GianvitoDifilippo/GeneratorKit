using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Expressions;

internal interface IExpressionContext
{
  IExpression GetExpression(IOperation operation);
  IParameterExpression GetParameter(IParameterSymbol parameter);

  Type GetDelegateType(IMethodSymbol symbol);
  Type GetType(ITypeSymbol symbol);
  MethodInfo GetMethodInfo(IMethodSymbol symbol);
  ConstructorInfo GetConstructorInfo(IMethodSymbol symbol);
  MemberInfo GetPropertyInfo(IPropertySymbol symbol);
  MemberInfo GetFieldInfo(IFieldSymbol symbol);
  bool IsNullableType(ITypeSymbol symbol);
}
