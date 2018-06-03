#if NET40 || PORTABLE
using System.Reflection;

namespace System.Threading.Tasks
{
    public static class TaskCompletionSourceExtensions
    {
        private static class PerTResult<TResult>
        {
            public static readonly Action<TaskCompletionSource<TResult>, CancellationToken> SetCanceledAction = (Action<TaskCompletionSource<TResult>, CancellationToken>)
                typeof(TaskCompletionSource<TResult>)
                    .GetMethod("SetCanceled", new[] { typeof(CancellationToken) })
                    ?.CreateDelegate(typeof(Action<TaskCompletionSource<TResult>, CancellationToken>));

            public static readonly Func<TaskCompletionSource<TResult>, CancellationToken, bool> TrySetCanceledFunc = (Func<TaskCompletionSource<TResult>, CancellationToken, bool>)
                typeof(TaskCompletionSource<TResult>)
                    .GetMethod("TrySetCanceled", new[] { typeof(CancellationToken) })
                    ?.CreateDelegate(typeof(Func<TaskCompletionSource<TResult>, CancellationToken, bool>));
        }

        public static void SetCanceled<TResult>(this TaskCompletionSource<TResult> taskCompletionSource, CancellationToken cancellationToken)
        {
            if (PerTResult<TResult>.SetCanceledAction != null)
            {
                PerTResult<TResult>.SetCanceledAction.Invoke(taskCompletionSource, cancellationToken);
            }
            else
            {
                taskCompletionSource.SetCanceled();
            }
        }

        public static bool TrySetCanceled<TResult>(this TaskCompletionSource<TResult> taskCompletionSource, CancellationToken cancellationToken)
        {
            if (PerTResult<TResult>.TrySetCanceledFunc != null)
            {
                return PerTResult<TResult>.TrySetCanceledFunc.Invoke(taskCompletionSource, cancellationToken);
            }
            else
            {
                return taskCompletionSource.TrySetCanceled();
            }
        }
    }
}
#endif
