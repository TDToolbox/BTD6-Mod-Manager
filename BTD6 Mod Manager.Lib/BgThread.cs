using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BTD6_Mod_Manager.Lib
{
    /// <summary>
    /// This delegate is used to pass functions into the ThreadQueue
    /// </summary>
    public delegate void funcDelegate();

    /// <summary>
    /// Background thread class. Used to manage threading in the application
    /// </summary>
    public class BgThread
    {
        #region Properties

        private static BgThread instance;

        /// <summary>
        /// Singleton instance of this class
        /// </summary>
        public static BgThread Instance
        {
            get
            {
                if (instance == null)
                    instance = new BgThread();
                return instance;
            }
        }

        /// <summary>
        /// The thread this class uses to do threading
        /// </summary>
        public static Thread ThreadInstance { get; private set; }

        /// <summary>
        /// A queue of threads that need to be ran
        /// </summary>
        public static Queue<Thread> ThreadQueue { get; private set; }

        /// <summary>
        /// A queue of threads that need to be ran. Used as a temp queue for adding threads to the front of the queue.
        /// </summary>
        private static Queue<Thread> addToFrontThreadQueue;

        private static Queue<Thread> AddToFrontThreadQueue
        {
            get
            {
                if (addToFrontThreadQueue == null)
                    addToFrontThreadQueue = new Queue<Thread>();
                return addToFrontThreadQueue;
            }
            set { addToFrontThreadQueue = value; }
        }
        #endregion


        /// <summary>
        /// Check if the background thread is in use or not
        /// </summary>
        /// <returns>Whether or not the background thread is in use</returns>
        public static bool IsThreadRunning() => !(ThreadInstance == null || !ThreadInstance.IsAlive);

        /// <summary>
        /// Add a function to the ThreadQueue. It will execute the thread immediately if the thread Instance for this class isn't running
        /// </summary>
        /// <param name="func">The function to be added to the TheadQueue and run as a thread</param>
        public static void AddToQueue(funcDelegate func, ApartmentState threadType = ApartmentState.Unknown, bool join = false) => AddToQueue(new Thread(() => func()), threadType, join);

        /// <summary>
        /// Add a thread to the ThreadQueue. It will execute the thread immediately if the thread Instance for this class isn't running
        /// </summary>
        /// <param name="thread">Thread to be added to the ThreadQueue</param>
        public static void AddToQueue(Thread thread, ApartmentState threadType = ApartmentState.Unknown, bool join = false)
        {
            if (ThreadQueue == null)
                ThreadQueue = new Queue<Thread>();

            thread.SetApartmentState(threadType);
            ThreadQueue.Enqueue(thread);

            ThreadingEventArgs args = new ThreadingEventArgs();
            args.Thread = thread;
            Instance.ItemAddedToThreadQueue(args);

            if (!IsThreadRunning())
                Instance.ExecuteQueue(join);
        }

        /// <summary>
        /// Add a list of threads to the ThreadQueue. It will execute them immediately if the thread Instance for this class isn't running
        /// </summary>
        /// <param name="threads"></param>
        public static void AddToQueue(List<Thread> threads, ApartmentState threadType = ApartmentState.Unknown, bool join = false)
        {
            if (threads == null || threads.Count() <= 0)
            {
                Logger.Log("Error! Tried adding an empty thread list to the Thread Queue");
                return;
            }

            foreach (var thread in threads)
                AddToQueue(thread, threadType, join);
        }

        /*public static void AddToFrontOfQueue(funcDelegate func) => AddToFrontOfQueue(new Thread(() => func()));

        public static void AddToFrontOfQueue(Thread thread)
        {
            if (AddToFrontThreadQueue == null)
                AddToFrontThreadQueue = new Queue<Thread>();

            var tempQueue = new Queue<Thread>();
            tempQueue = AddToFrontThreadQueue;

            AddToFrontThreadQueue = new Queue<Thread>();
            AddToFrontThreadQueue.Enqueue(thread);

            foreach (var tempT in tempQueue)
                AddToFrontThreadQueue.Enqueue(tempT);
        }*/


        int runningThreads = 0;
        /// <summary>
        /// Run the first thread in the ThreadQueue
        /// </summary>
        private void RunThread(bool nullifyThreadInst = false)
        {
            ThreadQueue.Peek().IsBackground = true;
            ThreadQueue.Peek().Start();
            runningThreads++;

            if (runningThreads >= 3)
            {
                ThreadQueue.Peek().Join();
                runningThreads--;
            }

            ThreadingEventArgs removeArgs = new ThreadingEventArgs();
            removeArgs.Thread = ThreadQueue.Peek();

            ThreadQueue.Dequeue();

            ItemRemovedFromThreadQueue(removeArgs);
        }

        /// <summary>
        /// Excecutes threads on queue until none are left
        /// </summary>
        /// <param name="join">Wait for ThreadInstance to finish before re-joining with the main thread?</param>
        private void RunThreads(bool join = false)
        {
            if (ThreadQueue.Count() <= 0)
                return;

            ThreadInstance = new Thread(() =>
            {
                ThreadingEventArgs args = new ThreadingEventArgs();
                args.ThreadQueue = ThreadQueue;
                OnThreadQueueStarted(args);

                while (ThreadQueue.Count > 0)
                    RunThread();

                OnThreadQueueFinished(null);
            });

            ThreadInstance.IsBackground = true;
            ThreadInstance.Start();
            if (join)
                ThreadInstance.Join();
        }

        /// <summary>
        /// Calls RunThreads to excecutes threads on queue until none are left
        /// </summary>
        public void ExecuteQueue(bool join = false) => RunThreads(join);

        /// <summary>
        /// Get the background thread instance of the class
        /// </summary>
        /// <returns>the background thread instance</returns>
        public static Thread GetThreadInst() => ThreadInstance;

        /// <summary>
        /// Get the instance of BgThread class
        /// </summary>
        /// <returns></returns>
        public static BgThread GetInstance() => Instance;


        #region Events
        public static event EventHandler<ThreadingEventArgs> ThreadQueueFinished;
        public static event EventHandler<ThreadingEventArgs> ThreadQueueStarted;
        public static event EventHandler<ThreadingEventArgs> ThreadQueueItemAdded;
        public static event EventHandler<ThreadingEventArgs> ThreadQueueItemRemoved;

        public class ThreadingEventArgs : EventArgs
        {
            public Queue<Thread> ThreadQueue { get; set; }
            public Thread Thread { get; set; }
        }

        /// <summary>
        /// Event fired when a thread has been added to the ThreadQueue
        /// </summary>
        /// <param name="e">Passes current Queue as argument</param>
        public void OnThreadQueueStarted(ThreadingEventArgs e)
        {
            EventHandler<ThreadingEventArgs> handler = ThreadQueueStarted;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Event fired when ThreadQueue finished executing all threads
        /// </summary>
        /// <param name="e">Can be null</param>
        public void OnThreadQueueFinished(ThreadingEventArgs e)
        {
            EventHandler<ThreadingEventArgs> handler = ThreadQueueFinished;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Event fired when an item is added to the ThreadQueue
        /// </summary>
        /// <param name="e">The thread that was added to the queue is passed as an arg</param>
        public void ItemAddedToThreadQueue(ThreadingEventArgs e)
        {
            EventHandler<ThreadingEventArgs> handler = ThreadQueueItemAdded;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Event fired when an item is removed from ThreadQueue
        /// </summary>
        /// <param name="e">The thread that was removed from queue is passed as arg</param>
        public void ItemRemovedFromThreadQueue(ThreadingEventArgs e)
        {
            EventHandler<ThreadingEventArgs> handler = ThreadQueueItemRemoved;
            if (handler != null)
                handler(this, e);
        }
        #endregion
    }
}
