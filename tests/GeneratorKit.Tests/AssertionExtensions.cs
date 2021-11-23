﻿using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Primitives;
using System.Collections.Generic;

namespace GeneratorKit;

public static class AssertionsExtensions
{
  public static AndConstraint<TAssertions> Equal<TSubject, TAssertions>(this ReferenceTypeAssertions<TSubject, TAssertions> assertions, TSubject? other, IEqualityComparer<TSubject> comparer, string because = "", params object[] becauseArgs)
    where TAssertions : ReferenceTypeAssertions<TSubject, TAssertions>
  {
    return assertions.Match(x => comparer.Equals(x, other), because, becauseArgs);
  }

  public static AndConstraint<ObjectAssertions> Equal<TSubject>(this ObjectAssertions assertions, TSubject? other, IEqualityComparer<TSubject> comparer, string because = "", params object[] becauseArgs)
    where TSubject : class
  {
    return assertions.Match(x => comparer.Equals(x as TSubject, other), because, becauseArgs);
  }

  public static AndConstraint<TAssertions> BeEquivalentTo<TCollection, T, TAssertions>(this GenericCollectionAssertions<TCollection, T, TAssertions> assertions, IEnumerable<T> expectation, IEqualityComparer<T> comparer, string because = "", params object[] becauseArgs)
    where TCollection : IEnumerable<T>
    where TAssertions : GenericCollectionAssertions<TCollection, T, TAssertions>
  {
    return assertions.BeEquivalentTo(expectation, opt => opt.Using(comparer), because, becauseArgs);
  }
}