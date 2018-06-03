// https://raw.githubusercontent.com/dotnet/corefx/v2.1.0/src/System.Linq.Parallel/src/System/Linq/Parallel/Utils/Shared.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/corefx/blob/v2.1.0/LICENSE.TXT

#if NET20 || NET35
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// Shared.cs
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

namespace System.Linq.Parallel
{
    /// <summary>
    /// A very simple primitive that allows us to share a value across multiple threads.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Shared<T>
    {
        internal T Value;

        internal Shared(T value)
        {
            this.Value = value;
        }
    }
}
#endif
