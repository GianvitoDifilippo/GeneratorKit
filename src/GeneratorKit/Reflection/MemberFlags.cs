using System;

namespace GeneratorKit.Reflection;

[Flags]
internal enum MemberFlags
{
  None = 0,
  Fields = 1,
  Properties = 2,
  Methods = 4,
  Constructors = 8,
  Events = 16,
  NestedTypes = 32,
  AllMembers = Fields | Properties | Methods | Constructors | Events | NestedTypes,

  DeepSearch = 64
}
