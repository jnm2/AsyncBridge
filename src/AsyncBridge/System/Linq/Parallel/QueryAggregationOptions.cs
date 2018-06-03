// https://github.com/dotnet/corefx/blob/v2.1.0/src/System.Linq.Parallel/src/System/Linq/Parallel/Enumerables/QueryAggregationOptions.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// QueryAggregationOptions.cs
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

namespace System.Linq.Parallel
{
    /// <summary>
    /// An enum to specify whether an aggregate operator is associative, commutative,
    /// neither, or both. This influences query analysis and execution: associative
    /// aggregations can run in parallel, whereas non-associative cannot; non-commutative
    /// aggregations must be run over data in input-order. 
    /// </summary>
    [Flags]
    internal enum QueryAggregationOptions
    {
        None = 0,
        Associative = 1,
        Commutative = 2,
        AssociativeCommutative = (Associative | Commutative) // For convenience.
        // If you change the members, make sure you update IsDefinedQueryAggregationOptions() below.
    }

    internal static class QueryAggregationOptionsExtensions
    {
        // This helper is a workaround for the fact that Enum.Defined() does not work on non-public enums.
        // There is a custom attribute in System.Reflection.Metadata.Controls that would make it work
        // but we don't want to introduce a dependency on that contract just to support two asserts.
        public static bool IsValidQueryAggregationOption(this QueryAggregationOptions value)
        {
            return value == QueryAggregationOptions.None
                   || value == QueryAggregationOptions.Associative
                   || value == QueryAggregationOptions.Commutative
                   || value == QueryAggregationOptions.AssociativeCommutative;
        }
    }
}
#endif
