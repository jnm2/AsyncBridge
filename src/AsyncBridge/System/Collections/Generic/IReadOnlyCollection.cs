// https://github.com/dotnet/corefx/blob/v2.1.0/src/System.Collections.Concurrent/src/System/Collections/Concurrent/IProducerConsumerCollection.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
using System;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    // Provides a read-only, covariant view of a generic list.
    public interface IReadOnlyCollection<T> : IEnumerable<T>
    {
        int Count { get; }
    }
}
#endif
