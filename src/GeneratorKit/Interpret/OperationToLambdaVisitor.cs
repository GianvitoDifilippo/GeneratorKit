#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Interpret.Frame;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GeneratorKit.Interpret;

internal class OperationToLambdaVisitor : OperationVisitor<object?, Expression>
{
}