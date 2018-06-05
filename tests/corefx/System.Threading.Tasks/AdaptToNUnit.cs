// Because there is no version of xUnit which runs on .NET Framework 2.0 and has MemberData, some sort of porting is necessary.

extern alias NUnit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit::NUnit.Framework.Internal;
using NUnitFramework = NUnit::NUnit.Framework;

namespace Xunit
{
    public static class Assert
    {
        public static void Equal<T>(T expected, T actual)
        {
            NUnitFramework.Assert.AreEqual(expected, actual);
        }

        public static void NotEqual<T>(T expected, T actual)
        {
            NUnitFramework.Assert.AreNotEqual(expected, actual);
        }

        public static void Same(object expected, object actual)
        {
            NUnitFramework.Assert.AreSame(expected, actual);
        }

        public static void True(bool condition)
        {
            NUnitFramework.Assert.True(condition);
        }

        public static void True(bool condition, string userMessage)
        {
            NUnitFramework.Assert.True(condition, userMessage);
        }

        public static void False(bool condition, string userMessage)
        {
            NUnitFramework.Assert.False(condition, userMessage);
        }

        public static void Null(object @object)
        {
            NUnitFramework.Assert.Null(@object);
        }

        public static void NotNull(object @object)
        {
            NUnitFramework.Assert.NotNull(@object);
        }

        public static T Throws<T>(NUnitFramework.TestDelegate testCode) where T : Exception
        {
            return NUnitFramework.Assert.Throws<T>(testCode);
        }

        public static async Task<T> ThrowsAsync<T>(Func<Task> testCode) where T : Exception
        {
            using (new TestExecutionContext.IsolatedContext())
            {
                try
                {
                    await testCode.Invoke().ConfigureAwait(false);
                    return null;
                }
                catch (T ex)
                {
                    return ex;
                }
            }
        }

        public static void Contains<T>(T expected, IEnumerable<T> collection)
        {
            NUnitFramework.Assert.That(collection, NUnitFramework.Does.Contain(expected));
        }

        public static void DoesNotContain<T>(T expected, IEnumerable<T> collection)
        {
            NUnitFramework.Assert.That(collection, NUnitFramework.Does.Not.Contain(expected));
        }

        public static void Superset<T>(ISet<T> expectedSubset, ISet<T> actual)
        {
            NUnitFramework.CollectionAssert.IsSupersetOf(actual, expectedSubset);
        }

        public static T IsType<T>(object @object)
        {
            NUnitFramework.Assert.That(@object, NUnitFramework.Is.TypeOf(typeof(T)));
            return (T)@object;
        }

        public static void Matches(string expectedRegexPattern, string actualString)
        {
            NUnitFramework.StringAssert.IsMatch(expectedRegexPattern, actualString);
        }

        public static void NotEmpty(IEnumerable collection)
        {
            NUnitFramework.Assert.IsNotEmpty(collection);
        }

        public static T ThrowsAny<T>(Action testCode) where T : Exception
        {
            return (T)NUnitFramework.Assert.Catch(typeof(T), testCode.Invoke);
        }

        public static T ThrowsAny<T>(Func<object> testCode) where T : Exception
        {
            return (T)NUnitFramework.Assert.Catch(typeof(T), () => testCode.Invoke());
        }
    }

    public sealed class FactAttribute : NUnitFramework.TestAttribute
    {
    }

    public sealed class ConditionalFactAttribute : NUnitFramework.TestAttribute
    {
        public ConditionalFactAttribute(string obj1)
        {
        }
    }

    public sealed class TheoryAttribute : NUnitFramework.TheoryAttribute
    {
    }

    public sealed class InlineDataAttribute : NUnitFramework.TestCaseAttribute
    {
        public InlineDataAttribute(params object[] arguments) : base(arguments)
        {
        }
    }

    public sealed class MemberDataAttribute : NUnitFramework.TestCaseSourceAttribute
    {
        public MemberDataAttribute(string sourceName) : base(sourceName)
        {
        }
    }
}



public sealed class OuterLoopAttribute : Attribute
{
}

public sealed class SkipOnTargetFrameworkAttribute : Attribute
{
    public SkipOnTargetFrameworkAttribute(TargetFrameworkMonikers obj1, string obj2)
    {
    }
}

public sealed class ActiveIssueAttribute : Attribute
{
    public ActiveIssueAttribute(string obj1, TargetFrameworkMonikers obj2)
    {
    }
}

public enum TargetFrameworkMonikers
{
    NetFramework,
    UapAot
}

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Indicates that a method is an extension method, or that a class or assembly contains extension methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    internal sealed class ExtensionAttribute : Attribute { }
}
