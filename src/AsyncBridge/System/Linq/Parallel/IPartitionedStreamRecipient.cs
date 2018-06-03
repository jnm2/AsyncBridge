// https://github.com/dotnet/corefx/blob/v2.1.0/src/System.Linq.Parallel/src/System/Linq/Parallel/Partitioning/IPartitionedStreamRecipient.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// IPartitionedStreamRecipient.cs
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

namespace System.Linq.Parallel
{
    /// <summary>
    /// IPartitionedStreamRecipient is essentially a generic action on a partitioned stream,
    /// whose generic type parameter is the type of the order keys in the partitioned stream.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    interface IPartitionedStreamRecipient<TElement>
    {
        void Receive<TKey>(PartitionedStream<TElement, TKey> partitionedStream);
    }
}
#endif
