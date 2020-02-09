﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Operator
{
    public static class Extensions
    {
        public static Task WaitAsync(this CancellationToken token)
        {
            var tsc = new TaskCompletionSource<bool>();
            token.Register(CancellationTokenCallback, tsc);

            return token.IsCancellationRequested ? Task.CompletedTask : tsc.Task;
        }

        public static void CancellationTokenCallback(object taskCompletionSource)
        {
            ((TaskCompletionSource<bool>)taskCompletionSource).SetResult(true);
        }
    }
}
