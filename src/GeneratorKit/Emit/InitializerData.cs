using Microsoft.CodeAnalysis;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

public record struct InitializerData(FieldBuilder Field, IOperation InitOperation);