// https://github.com/dotnet/corefx/blob/v2.1.0/src/System.Linq.Parallel/src/System/Linq/Parallel/Partitioning/OrderedHashRepartitionStream.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// OrderedHashRepartitionStream.cs
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Threading;

namespace System.Linq.Parallel
{
    internal class OrderedHashRepartitionStream<TInputOutput, THashKey, TOrderKey> : HashRepartitionStream<TInputOutput, THashKey, TOrderKey>
    {
        internal OrderedHashRepartitionStream(
            PartitionedStream<TInputOutput, TOrderKey> inputStream, Func<TInputOutput, THashKey> hashKeySelector,
            IEqualityComparer<THashKey> hashKeyComparer, IEqualityComparer<TInputOutput> elementComparer, CancellationToken cancellationToken)
            : base(inputStream.PartitionCount, inputStream.KeyComparer, hashKeyComparer, elementComparer)
        {
            _partitions =
                new OrderedHashRepartitionEnumerator<TInputOutput, THashKey, TOrderKey>[inputStream.PartitionCount];

            // Initialize state shared among the partitions. A latch and a matrix of buffers. Note that
            // the actual elements in the buffer array are lazily allocated if needed.
            CountdownEvent barrier = new CountdownEvent(inputStream.PartitionCount);
            ListChunk<Pair<TInputOutput, THashKey>>[][] valueExchangeMatrix =
                JaggedArray<ListChunk<Pair<TInputOutput, THashKey>>>.Allocate(inputStream.PartitionCount, inputStream.PartitionCount);
            ListChunk<TOrderKey>[][] keyExchangeMatrix =
                JaggedArray<ListChunk<TOrderKey>>.Allocate(inputStream.PartitionCount, inputStream.PartitionCount);

            // Now construct each partition object.
            for (int i = 0; i < inputStream.PartitionCount; i++)
            {
                _partitions[i] = new OrderedHashRepartitionEnumerator<TInputOutput, THashKey, TOrderKey>(
                    inputStream[i], inputStream.PartitionCount, i, hashKeySelector, this, barrier,
                    valueExchangeMatrix, keyExchangeMatrix, cancellationToken);
            }
        }
    }
}
#endif
