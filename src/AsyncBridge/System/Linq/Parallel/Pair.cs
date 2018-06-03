// https://github.com/dotnet/corefx/blob/v2.1.0/src/System.Linq.Parallel/src/System/Linq/Parallel/Utils/Pair.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// Pair.cs
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

namespace System.Linq.Parallel
{
    /// <summary>
    /// A pair just wraps two bits of data into a single addressable unit. This is a
    /// value type to ensure it remains very lightweight, since it is frequently used
    /// with other primitive data types as well.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    internal struct Pair<T, U>
    {
        // The first and second bits of data.
        internal T _first;
        internal U _second;

        //-----------------------------------------------------------------------------------
        // A simple constructor that initializes the first/second fields.
        //

        public Pair(T first, U second)
        {
            _first = first;
            _second = second;
        }

        //-----------------------------------------------------------------------------------
        // Accessors for the left and right data.
        //

        public T First
        {
            get { return _first; }
            set { _first = value; }
        }

        public U Second
        {
            get { return _second; }
            set { _second = value; }
        }
    }
}
#endif
