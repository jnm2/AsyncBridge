// https://github.com/dotnet/corefx/blob/v2.1.0/src/System.Linq.Parallel/src/System/Linq/Parallel/Merging/IMergeHelper.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ImergeHelper.cs
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Used as a stand-in for replaceable merge algorithms. Alternative implementations
    /// are chosen based on the style of merge required. 
    /// </summary>
    /// <typeparam name="TInputOutput"></typeparam>
    internal interface IMergeHelper<TInputOutput>
    {
        // Begins execution of the merge.
        void Execute();

        // Return an enumerator that yields the merged output.
        IEnumerator<TInputOutput> GetEnumerator();

        // Returns the merged output as an array.
        TInputOutput[] GetResultsAsArray();
    }
}
#endif
