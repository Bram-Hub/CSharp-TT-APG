using System;
using System.Diagnostics;
using System.Threading;

namespace TruthTree2
{
    public class WorkFinishedEventArgs<T> : EventArgs
    {
        public bool Cancelled { get; private set; }
        public bool Completed { get; private set; }
        public T Result { get; private set; }
        public TimeSpan Time { get; private set; }

        public WorkFinishedEventArgs(T r, bool co, bool ca, TimeSpan ts)
        {
            Result = r;
            Completed = co;
            Cancelled = ca;
            Time = ts;
        }
    }

    public class Worker<T>
    {
        public delegate void WorkFinishedEventHandler(object sender, WorkFinishedEventArgs<T> args);
        public event WorkFinishedEventHandler WorkFinished;

        public T Result { get; private set; }
        public bool IsAlive { get; private set; }
        public bool Completed { get; private set; }
        public bool Cancelled { get; private set; }
        public TimeSpan Time { get; private set; }

        private Thread thread;
        private Func<T> work;
        private Stopwatch stopwatch;

        public Worker(Func<T> fun)
        {
            Result = default(T);
            IsAlive = false;
            Completed = false;
            Cancelled = false;
            work = fun;
            stopwatch = new Stopwatch();
        }

        public void Start()
        {
            if (IsAlive) { return; }

            thread = new Thread(
                () =>
                {
                    try
                    {
                        stopwatch.Restart();
                        IsAlive = true;
                        Result = work();
                        IsAlive = false;
                        Completed = true;
                        Cancelled = false;
                    }
                    catch (ThreadAbortException)
                    {
                        IsAlive = false;
                        Completed = false;
                        Cancelled = true;
                        Result = default(T);
                    }
                    catch (Exception)
                    {
                        IsAlive = false;
                        Completed = false;
                        Cancelled = false;
                    }
                    finally
                    {
                        stopwatch.Stop();
                        Time = stopwatch.Elapsed;
                        raiseFinishedEvent();
                    }
                });
            thread.Start();
        }

        protected virtual void raiseFinishedEvent()
        {
            WorkFinishedEventHandler handler = WorkFinished;

            if (handler == null) { return; }

            WorkFinishedEventArgs<T> a = new WorkFinishedEventArgs<T>(Result, Completed, Cancelled, Time);
            handler(this, a);
        }

        public void Stop()
        {
            if (thread == null) { return; }
            if (thread.IsAlive) { thread.Abort(); }
        }

        public void Wait()
        {
            if (thread == null || !IsAlive) { return; }

            thread.Join();
        }
    }
}
