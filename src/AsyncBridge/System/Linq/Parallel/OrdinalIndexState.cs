// https://github.com/dotnet/corefx/blob/v2.1.0/src/System.Linq.Parallel/src/System/Linq/Parallel/QueryOperators/OrdinalIndexState.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// OrdinalIndexState.cs
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

namespace System.Linq.Parallel
{
    /// <summary>
    /// Describes the state of order preservation index associated with an enumerator. 
    /// </summary>
    internal enum OrdinalIndexState : byte
    {
        Indexable = 0,   // Indices of elements are 0,1,2,... An element with any index can be accessed in constant time.
        Correct = 1,     // Indices of elements are 0,1,2,... Within each partition, elements are in the correct order.
        Increasing = 2,  // Indices of elements are increasing. Within each partition, elements are in the correct order.
        Shuffled = 3,    // Indices are of arbitrary type. Elements appear in an arbitrary order in each partition.
    }
}
#endif
