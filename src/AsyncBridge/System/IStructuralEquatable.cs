// https://github.com/dotnet/coreclr/blob/v2.1.0/src/mscorlib/shared/System/Collections/IStructuralEquatable.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/coreclr/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
namespace System.Collections
{
    public interface IStructuralEquatable
    {
        Boolean Equals(Object other, IEqualityComparer comparer);
        int GetHashCode(IEqualityComparer comparer);
    }
}
#endif
