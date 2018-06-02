// https://github.com/dotnet/coreclr/blob/v2.1.0/src/mscorlib/src/System/Runtime/CompilerServices/TaskAwaiter.cs
// Original work under MIT license, Copyright (c) .NET Foundation and Contributors https://github.com/dotnet/coreclr/blob/v2.1.0/LICENSE.TXT

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
//
//
// Types for awaiting Task and Task<T>. These types are emitted from Task{<T>}.GetAwaiter 
// and Task{<T>}.ConfigureAwait.  They are meant to be used only by the compiler, e.g.
// 
//   await nonGenericTask;
//   =====================
//       var $awaiter = nonGenericTask.GetAwaiter();
//       if (!$awaiter.IsCompleted)
//       {
//           SPILL:
//           $builder.AwaitUnsafeOnCompleted(ref $awaiter, ref this);
//           return;
//           Label:
//           UNSPILL;
//       }
//       $awaiter.GetResult();
//
//   result += await genericTask.ConfigureAwait(false);
//   ===================================================================================
//       var $awaiter = genericTask.ConfigureAwait(false).GetAwaiter();
//       if (!$awaiter.IsCompleted)
//       {
//           SPILL;
//           $builder.AwaitUnsafeOnCompleted(ref $awaiter, ref this);
//           return;
//           Label:
//           UNSPILL;
//       }
//       result += $awaiter.GetResult();
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

// NOTE: For performance reasons, initialization is not verified.  If a developer
//       incorrectly initializes a task awaiter, which should only be done by the compiler,
//       NullReferenceExceptions may be generated (the alternative would be for us to detect
//       this case and then throw a different exception instead).  This is the same tradeoff
//       that's made with other compiler-focused value types like List<T>.Enumerator.

namespace System.Runtime.CompilerServices
{
    /// <summary>Provides an awaiter for awaiting a <see cref="System.Threading.Tasks.Task"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public readonly struct TaskAwaiter : ICriticalNotifyCompletion, ITaskAwaiter
    {
        // WARNING: Unsafe.As is used to access the generic TaskAwaiter<> as TaskAwaiter.
        // Its layout must remain the same.

        /// <summary>The task being awaited.</summary>
        internal readonly Task m_task;

        /// <summary>Initializes the <see cref="TaskAwaiter"/>.</summary>
        /// <param name="task">The <see cref="System.Threading.Tasks.Task"/> to be awaited.</param>
        internal TaskAwaiter(Task task)
        {
            Debug.Assert(task != null, "Constructing an awaiter requires a task to await.");
            m_task = task;
        }

        /// <summary>Gets whether the task being awaited is completed.</summary>
        /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        public bool IsCompleted
        {
            get { return m_task.IsCompleted; }
        }

        /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.InvalidOperationException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public void OnCompleted(Action continuation)
        {
            OnCompletedInternal(m_task, continuation, continueOnCapturedContext: true, flowExecutionContext: true);
        }

        /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.InvalidOperationException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        [SecurityCritical]
        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompletedInternal(m_task, continuation, continueOnCapturedContext: true, flowExecutionContext: false);
        }

        /// <summary>Ends the await on the completed <see cref="System.Threading.Tasks.Task"/>.</summary>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <exception cref="System.Threading.Tasks.TaskCanceledException">The task was canceled.</exception>
        /// <exception cref="System.Exception">The task completed in a Faulted state.</exception>
        public void GetResult()
        {
            ValidateEnd(m_task);
        }

        /// <summary>
        /// Fast checks for the end of an await operation to determine whether more needs to be done
        /// prior to completing the await.
        /// </summary>
        /// <param name="task">The awaited task.</param>
        internal static void ValidateEnd(Task task)
        {
            // Fast checks that can be inlined.
#if NET40 || PORTABLE
            if (task.Status != TaskStatus.RanToCompletion)
#else
            if (task.IsWaitNotificationEnabledOrNotRanToCompletion)
#endif
            {
                // If either the end await bit is set or we're not completed successfully,
                // fall back to the slower path.
                HandleNonSuccessAndDebuggerNotification(task);
            }
        }

        /// <summary>
        /// Ensures the task is completed, triggers any necessary debugger breakpoints for completing 
        /// the await on the task, and throws an exception if the task did not complete successfully.
        /// </summary>
        /// <param name="task">The awaited task.</param>
        private static void HandleNonSuccessAndDebuggerNotification(Task task)
        {
            // NOTE: The JIT refuses to inline ValidateEnd when it contains the contents
            // of HandleNonSuccessAndDebuggerNotification, hence the separation.

            // Synchronously wait for the task to complete.  When used by the compiler,
            // the task will already be complete.  This code exists only for direct GetResult use,
            // for cases where the same exception propagation semantics used by "await" are desired,
            // but where for one reason or another synchronous rather than asynchronous waiting is needed.
            if (!task.IsCompleted)
            {
#if NET40 || PORTABLE
                bool taskCompleted = task.Wait(Timeout.Infinite, default(CancellationToken));
#else
                bool taskCompleted = task.InternalWait(Timeout.Infinite, default(CancellationToken));
#endif
                Debug.Assert(taskCompleted, "With an infinite timeout, the task should have always completed.");
            }

#if !(NET40 || PORTABLE)
            // Now that we're done, alert the debugger if so requested
            task.NotifyDebuggerOfWaitCompletionIfNecessary();
#endif

            // And throw an exception if the task is faulted or canceled.
#if NET40 || PORTABLE
            if (task.Status != TaskStatus.RanToCompletion) ThrowForNonSuccess(task);
#else
            if (!task.IsCompletedSuccessfully) ThrowForNonSuccess(task);
#endif
        }

        /// <summary>Throws an exception to handle a task that completed in a state other than RanToCompletion.</summary>
        private static void ThrowForNonSuccess(Task task)
        {
            Debug.Assert(task.IsCompleted, "Task must have been completed by now.");
            Debug.Assert(task.Status != TaskStatus.RanToCompletion, "Task should not be completed successfully.");

            // Handle whether the task has been canceled or faulted
            switch (task.Status)
            {
                // If the task completed in a canceled state, throw an OperationCanceledException.
                // This will either be the OCE that actually caused the task to cancel, or it will be a new
                // TaskCanceledException. TCE derives from OCE, and by throwing it we automatically pick up the
                // completed task's CancellationToken if it has one, including that CT in the OCE.
                case TaskStatus.Canceled:
#if !(NET40 || PORTABLE)
                    var oceEdi = task.GetCancellationExceptionDispatchInfo();
                    if (oceEdi != null)
                    {
                        oceEdi.Throw();
                        Debug.Fail("Throw() should have thrown");
                    }
#endif
                    throw new TaskCanceledException(task);

                // If the task faulted, throw its first exception,
                // even if it contained more than one.
                case TaskStatus.Faulted:
#if NET40 || PORTABLE
                    var exception = task.Exception?.InnerException;
                    if (exception != null)
                    {
                        ExceptionDispatchInfo.Throw(exception);
#else
                    var edis = task.GetExceptionDispatchInfos();
                    if (edis.Count > 0)
                    {
                        edis[0].Throw();
#endif
                        Debug.Fail("Throw() should have thrown");
                        break; // Necessary to compile: non-reachable, but compiler can't determine that
                    }
                    else
                    {
                        Debug.Fail("There should be exceptions if we're Faulted.");
                        throw task.Exception;
                    }
            }
        }

        /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
        /// <param name="task">The task being awaited.</param>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <param name="continueOnCapturedContext">Whether to capture and marshal back to the current context.</param>
        /// <param name="flowExecutionContext">Whether to flow ExecutionContext across the await.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        internal static void OnCompletedInternal(Task task, Action continuation, bool continueOnCapturedContext, bool flowExecutionContext)
        {
            if (continuation == null) throw new ArgumentNullException(nameof(continuation));

#if NET40 || PORTABLE
            // Adapted from Task.SetContinuationForAwait

            var scheduler = (TaskScheduler)null;

            // If the user wants the continuation to run on the current "context" if there is one...
            if (continueOnCapturedContext)
            {
                // First try getting the current synchronization context.
                // If the current context is really just the base SynchronizationContext type, 
                // which is intended to be equivalent to not having a current SynchronizationContext at all, 
                // then ignore it.  This helps with performance by avoiding unnecessary posts and queueing
                // of work items, but more so it ensures that if code happens to publish the default context 
                // as current, it won't prevent usage of a current task scheduler if there is one.
                var syncCtx = SynchronizationContext.Current;
                if (syncCtx != null && syncCtx.GetType() != typeof(SynchronizationContext))
                {
                    scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                }
            }

            task.ContinueWith(
                (t, state) => ((Action)state).Invoke(),
                state: continuation,
                CancellationToken.None,
                TaskContinuationOptions.None,
                scheduler ?? TaskScheduler.Current);
#else
            // Set the continuation onto the awaited task.
            task.SetContinuationForAwait(continuation, continueOnCapturedContext, flowExecutionContext);
#endif
        }
    }

    /// <summary>Provides an awaiter for awaiting a <see cref="System.Threading.Tasks.Task{TResult}"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
        public readonly struct TaskAwaiter<TResult> : ICriticalNotifyCompletion, ITaskAwaiter
    {
        // WARNING: Unsafe.As is used to access TaskAwaiter<> as the non-generic TaskAwaiter.
        // Its layout must remain the same.

        /// <summary>The task being awaited.</summary>
        private readonly Task<TResult> m_task;

        /// <summary>Initializes the <see cref="TaskAwaiter{TResult}"/>.</summary>
        /// <param name="task">The <see cref="System.Threading.Tasks.Task{TResult}"/> to be awaited.</param>
        internal TaskAwaiter(Task<TResult> task)
        {
            Debug.Assert(task != null, "Constructing an awaiter requires a task to await.");
            m_task = task;
        }

        /// <summary>Gets whether the task being awaited is completed.</summary>
        /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        public bool IsCompleted
        {
            get { return m_task.IsCompleted; }
        }

        /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public void OnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(m_task, continuation, continueOnCapturedContext: true, flowExecutionContext: true);
        }

        /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        [SecurityCritical]
        public void UnsafeOnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(m_task, continuation, continueOnCapturedContext: true, flowExecutionContext: false);
        }

        /// <summary>Ends the await on the completed <see cref="System.Threading.Tasks.Task{TResult}"/>.</summary>
        /// <returns>The result of the completed <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <exception cref="System.Threading.Tasks.TaskCanceledException">The task was canceled.</exception>
        /// <exception cref="System.Exception">The task completed in a Faulted state.</exception>
        public TResult GetResult()
        {
            TaskAwaiter.ValidateEnd(m_task);
#if NET40 || PORTABLE
            return m_task.Result;
#else
            return m_task.ResultOnSuccess;
#endif
        }
    }

    /// <summary>
    /// Marker interface used to know whether a particular awaiter is either a
    /// TaskAwaiter or a TaskAwaiter`1.  It must not be implemented by any other
    /// awaiters.
    /// </summary>
    internal interface ITaskAwaiter { }

    /// <summary>
    /// Marker interface used to know whether a particular awaiter is either a
    /// CTA.ConfiguredTaskAwaiter or a CTA`1.ConfiguredTaskAwaiter.  It must not
    /// be implemented by any other awaiters.
    /// </summary>
    internal interface IConfiguredTaskAwaiter { }

    /// <summary>Provides an awaitable object that allows for configured awaits on <see cref="System.Threading.Tasks.Task"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public readonly struct ConfiguredTaskAwaitable
    {
        /// <summary>The task being awaited.</summary>
        private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter m_configuredTaskAwaiter;

        /// <summary>Initializes the <see cref="ConfiguredTaskAwaitable"/>.</summary>
        /// <param name="task">The awaitable <see cref="System.Threading.Tasks.Task"/>.</param>
        /// <param name="continueOnCapturedContext">
        /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
        /// </param>
        internal ConfiguredTaskAwaitable(Task task, bool continueOnCapturedContext)
        {
            Debug.Assert(task != null, "Constructing an awaitable requires a task to await.");
            m_configuredTaskAwaiter = new ConfiguredTaskAwaitable.ConfiguredTaskAwaiter(task, continueOnCapturedContext);
        }

        /// <summary>Gets an awaiter for this awaitable.</summary>
        /// <returns>The awaiter.</returns>
        public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
        {
            return m_configuredTaskAwaiter;
        }

        /// <summary>Provides an awaiter for a <see cref="ConfiguredTaskAwaitable"/>.</summary>
        /// <remarks>This type is intended for compiler use only.</remarks>
        public readonly struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion, IConfiguredTaskAwaiter
        {
            // WARNING: Unsafe.As is used to access the generic ConfiguredTaskAwaiter as this.
            // Its layout must remain the same.

            /// <summary>The task being awaited.</summary>
            internal readonly Task m_task;
            /// <summary>Whether to attempt marshaling back to the original context.</summary>
            internal readonly bool m_continueOnCapturedContext;

            /// <summary>Initializes the <see cref="ConfiguredTaskAwaiter"/>.</summary>
            /// <param name="task">The <see cref="System.Threading.Tasks.Task"/> to await.</param>
            /// <param name="continueOnCapturedContext">
            /// true to attempt to marshal the continuation back to the original context captured
            /// when BeginAwait is called; otherwise, false.
            /// </param>
            internal ConfiguredTaskAwaiter(Task task, bool continueOnCapturedContext)
            {
                Debug.Assert(task != null, "Constructing an awaiter requires a task to await.");
                m_task = task;
                m_continueOnCapturedContext = continueOnCapturedContext;
            }

            /// <summary>Gets whether the task being awaited is completed.</summary>
            /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            public bool IsCompleted
            {
                get { return m_task.IsCompleted; }
            }

            /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            public void OnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(m_task, continuation, m_continueOnCapturedContext, flowExecutionContext: true);
            }

            /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            [SecurityCritical]
            public void UnsafeOnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(m_task, continuation, m_continueOnCapturedContext, flowExecutionContext: false);
            }

            /// <summary>Ends the await on the completed <see cref="System.Threading.Tasks.Task"/>.</summary>
            /// <returns>The result of the completed <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <exception cref="System.Threading.Tasks.TaskCanceledException">The task was canceled.</exception>
            /// <exception cref="System.Exception">The task completed in a Faulted state.</exception>
            public void GetResult()
            {
                TaskAwaiter.ValidateEnd(m_task);
            }
        }
    }

    /// <summary>Provides an awaitable object that allows for configured awaits on <see cref="System.Threading.Tasks.Task{TResult}"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public readonly struct ConfiguredTaskAwaitable<TResult>
    {
        /// <summary>The underlying awaitable on whose logic this awaitable relies.</summary>
        private readonly ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter m_configuredTaskAwaiter;

        /// <summary>Initializes the <see cref="ConfiguredTaskAwaitable{TResult}"/>.</summary>
        /// <param name="task">The awaitable <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="continueOnCapturedContext">
        /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
        /// </param>
        internal ConfiguredTaskAwaitable(Task<TResult> task, bool continueOnCapturedContext)
        {
            m_configuredTaskAwaiter = new ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter(task, continueOnCapturedContext);
        }

        /// <summary>Gets an awaiter for this awaitable.</summary>
        /// <returns>The awaiter.</returns>
        public ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter GetAwaiter()
        {
            return m_configuredTaskAwaiter;
        }

        /// <summary>Provides an awaiter for a <see cref="ConfiguredTaskAwaitable{TResult}"/>.</summary>
        /// <remarks>This type is intended for compiler use only.</remarks>
        public readonly struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion, IConfiguredTaskAwaiter
        {
            // WARNING: Unsafe.As is used to access this as the non-generic ConfiguredTaskAwaiter.
            // Its layout must remain the same.

            /// <summary>The task being awaited.</summary>
            private readonly Task<TResult> m_task;
            /// <summary>Whether to attempt marshaling back to the original context.</summary>
            private readonly bool m_continueOnCapturedContext;

            /// <summary>Initializes the <see cref="ConfiguredTaskAwaiter"/>.</summary>
            /// <param name="task">The awaitable <see cref="System.Threading.Tasks.Task{TResult}"/>.</param>
            /// <param name="continueOnCapturedContext">
            /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
            /// </param>
            internal ConfiguredTaskAwaiter(Task<TResult> task, bool continueOnCapturedContext)
            {
                Debug.Assert(task != null, "Constructing an awaiter requires a task to await.");
                m_task = task;
                m_continueOnCapturedContext = continueOnCapturedContext;
            }

            /// <summary>Gets whether the task being awaited is completed.</summary>
            /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            public bool IsCompleted
            {
                get { return m_task.IsCompleted; }
            }

            /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            public void OnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(m_task, continuation, m_continueOnCapturedContext, flowExecutionContext: true);
            }

            /// <summary>Schedules the continuation onto the <see cref="System.Threading.Tasks.Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            /// <exception cref="System.ArgumentNullException">The <paramref name="continuation"/> argument is null (Nothing in Visual Basic).</exception>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            [SecurityCritical]
            public void UnsafeOnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(m_task, continuation, m_continueOnCapturedContext, flowExecutionContext: false);
            }

            /// <summary>Ends the await on the completed <see cref="System.Threading.Tasks.Task{TResult}"/>.</summary>
            /// <returns>The result of the completed <see cref="System.Threading.Tasks.Task{TResult}"/>.</returns>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <exception cref="System.Threading.Tasks.TaskCanceledException">The task was canceled.</exception>
            /// <exception cref="System.Exception">The task completed in a Faulted state.</exception>
            public TResult GetResult()
            {
                TaskAwaiter.ValidateEnd(m_task);
#if NET40 || PORTABLE
                return m_task.Result;
#else
                return m_task.ResultOnSuccess;
#endif
            }
        }
    }
}
