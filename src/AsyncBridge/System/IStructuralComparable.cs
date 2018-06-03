// https://github.com/dotnet/coreclr/blob/v2.1.0/src/mscorlib/shared/System/Collections/IStructuralComparable.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
using System;

namespace System.Collections
{
    public interface IStructuralComparable
    {
        Int32 CompareTo(Object other, IComparer comparer);
    }
}
#endif
