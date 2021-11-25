using Microsoft.CodeAnalysis;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal class SymbolParameterType : SymbolType
{
  private readonly SymbolType _type;
  private readonly IParameterSymbol _symbol;

  public SymbolParameterType(GeneratorRuntime runtime, Compilation compilation, SymbolType type, IParameterSymbol symbol)
    : base(runtime, compilation)
  {
    _type = type;
    _symbol = symbol;
  }

  public override ITypeSymbol Symbol => _type.Symbol;


  // System.Type overrides

  public override string FullName => _type.FullName;

  public override string Name => _type.Name;

  protected override TypeAttributes GetAttributeFlagsImpl() => _type.Attributes;

  protected override bool HasElementTypeImpl() => _type.HasElementType;

  protected override bool IsArrayImpl() => _type.IsArray;

  protected override bool IsByRefImpl()
  {
    return _symbol.RefKind is not RefKind.None;
  }

  protected override bool IsPointerImpl() => _type.IsPointer;

  protected override bool IsPrimitiveImpl() => _type.IsPrimitive;


  // SymbolTypeBase overrides

  protected override SymbolAssembly AssemblyCore => _runtime.CreateAssemblyDelegator(_symbol.ContainingAssembly);

  protected override SymbolType[] GenericTypeArgumentsCore => throw new System.NotImplementedException();

  protected override SymbolModule ModuleCore => _runtime.CreateModuleDelegator(_symbol.ContainingModule);

  public override string Namespace => _symbol.ContainingNamespace.ToDisplayString(s_namespaceFormat);

  protected override SymbolType? GetElementTypeCore() => _type.GetElementType();

  protected override SymbolType[] GetGenericArgumentsCore() => _type.GetGenericArguments();

  protected override SymbolType[] GetGenericParameterConstraintsCore() => _type.GetGenericParameterConstraints();
}