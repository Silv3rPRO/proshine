using System;
using System.Threading;
using System.Threading.Tasks;

namespace PROBot.Utils
{
    internal class TaskUtils
    {
        internal static void CallActionWithTimeout(Action action, Action error, int timeout)
        {
            var cancelToken = new CancellationTokenSource();
            var token = cancelToken.Token;
            var task = Task.Run(delegate
            {
                try
                {
                    var thread = Thread.CurrentThread;
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
            var index = Task.WaitAny(task, Task.Delay(timeout));
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