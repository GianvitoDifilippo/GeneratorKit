using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GeneratorKit;

public record CompilationOutput(bool IsValid, Compilation Compilation, ImmutableArray<Diagnostic> Diagnostics, Assembly? Assembly)
{
  public static CompilationOutput Create(string source, string assemblyName, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary, IEnumerable<Assembly>? referencedAssemblies = null)
  {
    referencedAssemblies ??= Enumerable.Empty<Assembly>();

    List<MetadataReference> references = referencedAssemblies
      .Distinct()
      .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
      .Append(typeof(object).Assembly)
      .Select(x => (MetadataReference)MetadataReference.CreateFromFile(x.Location))
      .ToList();

    CSharpCompilation compilation = CSharpCompilation.Create(
      assemblyName,
      new SyntaxTree[] { CSharpSyntaxTree.ParseText(source) },
      references,
      new CSharpCompilationOptions(outputKind));

    using (MemoryStream stream = new MemoryStream())
    {
      EmitResult result = compilation.Emit(stream);

      if (result.Success)
      {
        stream.Position = 0;
        return new CompilationOutput(true, compilation, result.Diagnostics, Assembly.Load(stream.ToArray()));
      }
      else
      {
        return new CompilationOutput(false, compilation, result.Diagnostics, null);
      }
    }
  }
}