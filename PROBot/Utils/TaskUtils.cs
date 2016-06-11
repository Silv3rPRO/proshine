using System;
using System.Threading;
using System.Threading.Tasks;

namespace PROBot.Utils
{
    internal class TaskUtils
    {
        internal static void CallActionWithTimeout(Action action, Action error, int timeout)
        {
            CancellationTokenSource cancelToken = new CancellationTokenSource();
            CancellationToken token = cancelToken.Token;
            Task<Exception> task = Task.Run(delegate
            {
                try
                {
                    Thread thread = Thread.CurrentThread;
                    using (token.Register(thread.Abort))
                    {
                        action();
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }, token);
            int index = Task.WaitAny(task, Task.Delay(timeout));
            if (index != 0)
            {
                cancelToken.Cancel();
                error();
            }
            else if (task.Result != null)
            {
                throw task.Result;
            }
        }
    }
}
