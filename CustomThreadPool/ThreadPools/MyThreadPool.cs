using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CustomThreadPool
{
    public class MyThreadPool : IThreadPool
    {
        private long processedTask;
        private Queue<Action> publicQueue = new Queue<Action>();
        private Dictionary<int, WorkStealingQueue<Action>> queues = new Dictionary<int, WorkStealingQueue<Action>>();
        public long GetTasksProcessedCount() => processedTask;

        public MyThreadPool()
        {
            void Action()
            {
                while (true)
                {
                    Action task = null;
                    if (queues[Thread.CurrentThread.ManagedThreadId].LocalPop(ref task))
                    {
                        task();
                        Interlocked.Increment(ref processedTask);
                    }
                    else
                    {
                        lock (publicQueue)
                        {
                            if (publicQueue.TryDequeue(out task))
                                queues[Thread.CurrentThread.ManagedThreadId].LocalPush(task);
                            else if (!queues.Any(id =>
                                    id.Key != Thread.CurrentThread.ManagedThreadId && !id.Value.IsEmpty))
                                Monitor.Wait(publicQueue);
                        }

                        if (task != null) 
                            continue;

                        var queueToSteal = queues
                            .FirstOrDefault(id =>
                                id.Key != Thread.CurrentThread.ManagedThreadId && !id.Value.IsEmpty).Value;

                        if (queueToSteal is null || !queueToSteal.TrySteal(ref task)) 
                            continue;

                        task();
                        Interlocked.Increment(ref processedTask);
                    }
                }
            }

            var threads = CreateBackThreads(Action, Environment.ProcessorCount * 3);

            foreach (var thread in threads)
                queues[thread.ManagedThreadId] = new WorkStealingQueue<Action>();

            foreach (var thread in threads)
                thread.Start();
        }

        public void EnqueueAction(Action action)
        {
            if (queues.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                queues[Thread.CurrentThread.ManagedThreadId].LocalPush(action);
            else
            {
                lock (publicQueue)
                {
                    publicQueue.Enqueue(action);
                    Monitor.Pulse(publicQueue);
                }
            }
        }

        private Thread[] CreateBackThreads(Action action, int count) =>
            Enumerable
                .Range(0, count)
                .Select(_ => new Thread(() => action()) { IsBackground = true })
                .ToArray();
    }
}