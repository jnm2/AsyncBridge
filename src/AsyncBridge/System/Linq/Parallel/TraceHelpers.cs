// https://github.com/dotnet/corefx/blob/v2.1.0/src/System.Linq.Parallel/src/System/Linq/Parallel/Utils/TraceHelpers.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// TraceHelpers.cs
//
//
// Common routines used to trace information about execution, the state of things, etc.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Diagnostics;

namespace System.Linq.Parallel
{
    internal static class TraceHelpers
    {
        [Conditional("PFXTRACE")]
        internal static void TraceInfo(string msg, params object[] args)
        {
            Debug.WriteLine(string.Format(msg, args));
        }
    }
}
#endif
