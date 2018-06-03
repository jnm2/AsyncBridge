// https://github.com/dotnet/corefx/blob/v2.1.0/src/System.Linq.Parallel/src/System/Linq/Parallel/QueryOperators/ListQueryResults.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ListQueryResults.cs
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Class to represent an IList{T} as QueryResults{T} 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ListQueryResults<T> : QueryResults<T>
    {
        private IList<T> _source;
        private int _partitionCount;
        private bool _useStriping;

        internal ListQueryResults(IList<T> source, int partitionCount, bool useStriping)
        {
            _source = source;
            _partitionCount = partitionCount;
            _useStriping = useStriping;
        }

        internal override void GivePartitionedStream(IPartitionedStreamRecipient<T> recipient)
        {
            PartitionedStream<T, int> partitionedStream = GetPartitionedStream();
            recipient.Receive<int>(partitionedStream);
        }

        internal override bool IsIndexible
        {
            get { return true; }
        }

        internal override int ElementsCount
        {
            get { return _source.Count; }
        }

        internal override T GetElement(int index)
        {
            return _source[index];
        }

        internal PartitionedStream<T, int> GetPartitionedStream()
        {
            return ExchangeUtilities.PartitionDataSource(_source, _partitionCount, _useStriping);
        }
    }
}
#endif
