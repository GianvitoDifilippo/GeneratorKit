using System;

namespace GeneratorKit.Interpret;

public class ArrayInitializerContext
{
  public ArrayInitializerContext(Array array, int[] indices, int dimension)
  {
    Array = array;
    Indices = indices;
    Dimension = dimension;
  }

  public Array Array { get; }
  public int[] Indices { get; }
  public int Dimension { get; set; }

  public void Deconstruct(out Array array, out int[] indices, out int dimension) => (array, indices, dimension) = (Array, Indices, Dimension);
}