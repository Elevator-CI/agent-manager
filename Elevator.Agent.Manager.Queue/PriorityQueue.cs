using System.Collections.Generic;
using Elevator.Agent.Manager.Queue.Models;

namespace Elevator.Agent.Manager.Queue
{
    public class PriorityQueue<T> where T: class
    {
        private readonly Queue<T> normalPriorityQueue;
        private readonly Queue<T> highPriorityQueue;

        public PriorityQueue()
        {
            normalPriorityQueue = new();
            highPriorityQueue = new();
        }

        public void Enqueue(T t, Priority priority)
        {
            switch (priority)
            {
                case Priority.Normal:
                    normalPriorityQueue.Enqueue(t);
                    break;
                case Priority.High:
                    highPriorityQueue.Enqueue(t);
                    break;
            }
        }

        public bool TryDequeue(out T t)
        {
            if (highPriorityQueue.Count > 0)
            {
                lock (highPriorityQueue)
                {
                    t = highPriorityQueue.Dequeue();
                    return true;
                }
            }

            if (normalPriorityQueue.Count > 0)
            {
                lock (normalPriorityQueue)
                {
                    t = normalPriorityQueue.Dequeue();
                    return true;
                }
            }

            t = default;
            return false;
        }
    }
}
