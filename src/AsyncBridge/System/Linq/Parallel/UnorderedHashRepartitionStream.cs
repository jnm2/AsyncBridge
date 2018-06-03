// https://github.com/dotnet/corefx/blob/v2.1.0/src/System.Linq.Parallel/src/System/Linq/Parallel/Partitioning/UnorderedHashRepartitionStream.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// UnorderedHashRepartitionStream.cs
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Threading;

namespace System.Linq.Parallel
{
    internal class UnorderedHashRepartitionStream<TInputOutput, THashKey, TIgnoreKey> : HashRepartitionStream<TInputOutput, THashKey, int>
    {
        //---------------------------------------------------------------------------------------
        // Creates a new partition exchange operator.
        //

        internal UnorderedHashRepartitionStream(
            PartitionedStream<TInputOutput, TIgnoreKey> inputStream,
            Func<TInputOutput, THashKey> keySelector, IEqualityComparer<THashKey> keyComparer, IEqualityComparer<TInputOutput> elementComparer,
            CancellationToken cancellationToken)
            : base(inputStream.PartitionCount, Util.GetDefaultComparer<int>(), keyComparer, elementComparer)
        {
            // Create our array of partitions.
            _partitions = new HashRepartitionEnumerator<TInputOutput, THashKey, TIgnoreKey>[inputStream.PartitionCount];

            // Initialize state shared among the partitions. A latch and a matrix of buffers. Note that
            // the actual elements in the buffer array are lazily allocated if needed.
            CountdownEvent barrier = new CountdownEvent(inputStream.PartitionCount);
            ListChunk<Pair<TInputOutput, THashKey>>[][] valueExchangeMatrix =
                JaggedArray<ListChunk<Pair<TInputOutput, THashKey>>>.Allocate(inputStream.PartitionCount, inputStream.PartitionCount);

            // Now construct each partition object.
            for (int i = 0; i < inputStream.PartitionCount; i++)
            {
                _partitions[i] = new HashRepartitionEnumerator<TInputOutput, THashKey, TIgnoreKey>(
                    inputStream[i], inputStream.PartitionCount, i, keySelector, this,
                    barrier, valueExchangeMatrix, cancellationToken);
            }
        }
    }
}
#endif
