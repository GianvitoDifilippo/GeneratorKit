using FluentAssertions;
using GeneratorKit.Comparers;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using Xunit;
using System.Security;

namespace GeneratorKit.Reflection;

public class SymbolAssemblyTests : IClassFixture<SymbolAssemblyFixture>
{
  private const string s_namespace = SymbolAssemblyFixture.Namespace;

  private readonly SymbolAssemblyFixture _fixture;

  public SymbolAssemblyTests(SymbolAssemblyFixture fixture)
  {
    _fixture = fixture;
  }

  [Fact]
  public void CustomAttributes_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    IEnumerable<CustomAttributeData> expected = reference.CustomAttributes;

    // Act
    IEnumerable<CustomAttributeData> actual = sut.CustomAttributes;

    // Assert
    expected.Should().Contain(x => CustomAttributeDataEqualityComparer.Default.Equals(x, _fixture.CustomAttributeData));
    actual.Should().Contain(x => CustomAttributeDataEqualityComparer.Default.Equals(x, _fixture.CustomAttributeData));
  }

  [Fact]
  public void EntryPoint_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    MethodInfo? expected = reference.EntryPoint;

    // Act
    MethodInfo? actual = sut.EntryPoint;

    // Assert
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Fact]
  public void ExportedTypes_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    IEnumerable<Type> expected = reference.ExportedTypes;
    
    // Act
    IEnumerable<Type> actual = sut.ExportedTypes;

    // Assert
    actual.Should().BeEquivalentTo(expected, TypeEqualityComparer.Default);
  }

  [Fact]
  public void FullName_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    string? expected = reference.FullName;

    // Act
    string? actual = sut.FullName;

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void GetCustomAttributesData_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    IList<CustomAttributeData> expected = reference.GetCustomAttributesData();

    // Act
    IList<CustomAttributeData> actual = sut.GetCustomAttributesData();

    // Assert
    expected.Should().Contain(x => CustomAttributeDataEqualityComparer.Default.Equals(x, _fixture.CustomAttributeData));
    actual.Should().Contain(x => CustomAttributeDataEqualityComparer.Default.Equals(x, _fixture.CustomAttributeData));
  }

  [Fact]
  public void GetExportedTypes_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    IEnumerable<Type> expected = reference.GetExportedTypes();

    // Act
    IEnumerable<Type> actual = sut.GetExportedTypes();

    // Assert
    actual.Should().BeEquivalentTo(expected, TypeEqualityComparer.Default);
  }

  [Fact]
  public void GetFile_ShouldThrowFileNotFoundException()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();

    // Act
    Func<FileStream> getFile = () => sut.GetFile("");

    // Assert
    getFile.Should().ThrowExactly<FileNotFoundException>();
  }

  [Fact]
  public void GetFiles_ShouldThrowFileNotFoundException()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();

    // Act
    Func<FileStream[]> getFiles = () => sut.GetFiles();

    // Assert
    getFiles.Should().ThrowExactly<FileNotFoundException>();
  }

  [Fact]
  public void GetFilesWithBoolArg_ShouldThrowFileNotFoundException()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();

    // Act
    Func<FileStream[]> getFiles = () => sut.GetFiles(true);

    // Assert
    getFiles.Should().ThrowExactly<FileNotFoundException>();
  }

  [Fact]
  public void GetLoadedModules_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    Module[] expected = reference.GetLoadedModules();

    // Act
    Module[] actual = sut.GetLoadedModules();

    // Assert
    actual.Should().BeEquivalentTo(expected, ModuleEqualityComparer.Default);
  }

  [Fact]
  public void GetModules_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    Module[] expected = reference.GetModules();

    // Act
    Module[] actual = sut.GetModules();

    // Assert
    actual.Should().BeEquivalentTo(expected, ModuleEqualityComparer.Default);
  }

  [Fact]
  public void GetModule_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    Module? expected = reference.GetModule(SymbolAssemblyFixture.AssemblyName);

    // Act
    Module? actual = sut.GetModule(SymbolAssemblyFixture.AssemblyName);

    // Assert
    actual.Should().Equal(expected, ModuleEqualityComparer.Default);
  }

  [Fact]
  public void GetName_FullName_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    string? expected = reference.GetName().FullName;

    // Act
    string? actual = sut.GetName().FullName;

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void GetTypes_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    IEnumerable<Type> expected = reference.GetTypes();

    // Act
    IEnumerable<Type> actual = sut.GetTypes();

    // Assert
    actual.Should().BeEquivalentTo(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [InlineData(s_namespace + ".BaseClass")]
  [InlineData(s_namespace + ".GenericClass`2")]
  public void GetType_ShouldReturnType_WhenCorrectNameIsProviderAndIsCaseSensitive(string name)
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    Type? expected = reference.GetType(name, false, false);

    // Act
    Type? actual = sut.GetType(name, false, false);

    // Assert
    expected.Should().Equal(actual, TypeEqualityComparer.Default);
  }

  [Fact]
  public void GetType_ShouldReturnNull_WhenIncorrectNameIsProvidedAndIsCaseSensitive()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    Type? expected = reference.GetType("x", false, false);

    // Act
    Type? actual = sut.GetType("x", false, false);

    // Assert
    expected.Should().BeNull();
    actual.Should().BeNull();
  }

  [Fact]
  public void GetType_ShouldReturnType_WhenCorrectNameIsProviderAndIsNotCaseSensitive()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    const string name = s_namespace + ".baseClass";
    Type? expected = reference.GetType(name, false, true);

    // Act
    Type? actual = sut.GetType(name, false, true);

    // Assert
    expected.Should().Equal(actual, TypeEqualityComparer.Default);
  }

  [Fact]
  public void GetType_ShouldThrowTypeLoadException_WhenThrowOnErrorIsTrue()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();

    // Act
    Func<Type?> getType = () => sut.GetType("", true);

    // Assert
    getType.Should().ThrowExactly<TypeLoadException>();
  }

  [Fact]
  public void IsDynamic_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    bool expected = reference.IsDynamic;

    // Act
    bool actual = sut.IsDynamic;

    // Assert
    expected.Should().Be(actual);
  }

  [Fact]
  public void Modules_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    IEnumerable<Module> expected = reference.Modules;

    // Act
    IEnumerable<Module> actual = sut.Modules;

    // Assert
    actual.Should().BeEquivalentTo(expected, ModuleEqualityComparer.Default);
  }

  [Fact]
  public void ReflectionOnly_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    bool expected = reference.ReflectionOnly;

    // Act
    bool actual = sut.ReflectionOnly;

    // Assert
    expected.Should().Be(actual);
  }

  [Fact]
  public void SecurityRuleSet_ShouldBeCorrect()
  {
    // Arrange
    SymbolAssembly sut = _fixture.GetDelegator();
    Assembly reference = _fixture.GetReference();

    SecurityRuleSet expected = reference.SecurityRuleSet;

    // Act
    SecurityRuleSet actual = sut.SecurityRuleSet;

    // Assert
    expected.Should().Be(actual);
  }
}