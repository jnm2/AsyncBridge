// https://github.com/dotnet/corefx/blob/v2.1.0/src/System.Linq.Parallel/src/System/Linq/Parallel/Scheduling/QueryLifecycle.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// QueryLifecycle.cs
//
// A convenient place to put things associated with entire queries and their lifecycle events.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq.Parallel
{
    internal static class QueryLifecycle
    {
        // This method is called once per execution of a logical query.
        // (It is not called multiple time if repartitionings occur)
        internal static void LogicalQueryExecutionBegin(int queryID)
        {
            //We call NOCTD to inform the debugger that multiple threads will most likely be required to 
            //execute this query.  We do not attempt to run the query even if we think we could, for simplicity and consistency.
            DebuggerEx.NotifyOfCrossThreadDependency();
        }


        // This method is called once per execution of a logical query.
        // (It is not called multiple time if repartitionings occur)
        internal static void LogicalQueryExecutionEnd(int queryID)
        {
        }
    }
}
#endif
