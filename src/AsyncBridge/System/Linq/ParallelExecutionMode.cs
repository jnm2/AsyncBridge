// https://github.com/dotnet/corefx/blob/v2.1.0/src/System.Linq.Parallel/src/System/Linq/ParallelExecutionMode.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ParallelQueryExecutionMode.cs
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Linq
{
    /// <summary>
    /// The query execution mode is a hint that specifies how the system should handle
    /// performance trade-offs when parallelizing queries.
    /// </summary>
    public enum ParallelExecutionMode
    {
        /// <summary>
        /// By default, the system will use algorithms for queries
        /// that are ripe for parallelism and will avoid algorithms with high 
        /// overheads that will likely result in slow downs for parallel execution. 
        /// </summary>
        Default = 0,

        /// <summary>
        /// Parallelize the entire query, even if that means using high-overhead algorithms.
        /// </summary>
        ForceParallelism = 1,
    }
}
#endif
