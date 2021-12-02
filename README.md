# GeneratorKit

[![build](https://github.com/GianvitoDifilippo/GeneratorKit/actions/workflows/ci.yml/badge.svg)](https://github.com/GianvitoDifilippo/GeneratorKit/actions?query=workflow%3ACI)

## About

The aim of this tool is to improve the Developer Experience of writing C# source generators, as well as the final User Experience. It enables the usage of reflection in alternative of the semantic model and the execution of the user's code without compiling it. This means users can configure the behavior of the generator with code, instead of, say, attributes.

## Usage

Make your generator class inherit from `RuntimeSourceGenerator` instead of implementing the `ISourceGenerator` interface, then override the `Execute` and `Initialize` methods. The `Initialize` method is the same method you would implement normally, while the `Execute` method contains an extra parameter - an instance of `IGeneratorRuntime`.

```csharp
using GeneratorKit;

[Generator]
public class MySourceGenerator : RuntimeSourceGenerator
{
    public override void Execute(GeneratorExecutionContext context, IGeneratorRuntime runtime)
    {
        // Execution logic goes here
    }

    public override void Initialize(GeneratorInitializationContext context)
    {
        // Initialization logic goes here
    }
}
```

The `IGeneratorRuntime` interface exposes a `CompilationAssembly` property which represents the assembly of the current compilation. This assembly can be used to query types and most of the things you would normally do with an `Assembly` object.

Another thing you can do with the `IGeneratorRuntime` is creating reflection delegators from `ISymbol`'s. For example:


```csharp
ITypeSymbol myTypeSymbol = ...;
Type myType = runtime.CreateTypeDelegator(myType);
```

Same goes for `IPropertySymbol`/`PropertyInfo`, `IMethodSymbol`/`MethodInfo`, and so on.

## Executing user's code (preview)

> Note: This is how this feature should look like when finished.

Once you got your type delegator, you can use the activator class to instantiate it.

Suppose you have a base class `GeneratorConfiguration` which exposes a set of methods to configure the source generator. So, the user would extend this class and use those methods, say, in the class constructor:

```csharp
public class MyGeneratorConfiguration : GeneratorConfiguration
{
    public MyGeneratorConfiguration()
    {
        // Configuration logic goes here
    }
}
```

The source generator can instantiate this class (or, rather, a class that mirrors it) and then access the result of the user's action.

```csharp
Assembly assembly = runtime.CompilationAssembly;
Type baseConfigurationType = runtime.TypeOf<GeneratorConfiguration>();
Type configurationType = assembly
    .GetTypes()
    .FirstOrDefault(type => baseConfigurationType.IsAssignableFrom(type));

if (configurationType != null)
{
    GeneratorConfiguration configuration = (GeneratorConfiguration)Activator.CreateInstance(configurationType);
    // Do stuff with your instance
}
```