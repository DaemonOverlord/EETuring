using EETuring.Physics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EETuring
{
    public class Turing : IDisposable
    {
        private Thread thread;
        private WorkbackProcessor processor;

        public WorldData World { get; private set; }

        public event ProgressCallback OnProgress;
        public event TestCompleteCallback OnComplete;

        private void OnProcessorProgress(double percentProgress)
        {
            if (OnProgress != null)
            {
                OnProgress(percentProgress);
            }
        }

        private void OnTuringTestComplete(Path path, bool isPossible)
        {
            if (OnComplete != null)
            {
                OnComplete(path, isPossible);
            }
        }

        /// <summary>
        /// Start the turing test
        /// </summary>
        /// <param name="a">Start point indexed at 0</param>
        /// <param name="b">Goal point indexed at 0</param>
        public void SearchAsync(Point a, Point b)
        {
            if (thread != null)
            {
                if (thread.ThreadState == ThreadState.Running)
                {
                    throw new InvalidOperationException("Already searching. Wait for the Search to finish.");
                }
            }

            thread = new Thread(() => processor.Search(a, b));
            thread.IsBackground = true;
            thread.Name = "TuringSearchThread";
            thread.Start();
        }

        /// <summary>
        /// Disposes and stops the search
        /// </summary>
        public void Dispose()
        {
            if (thread != null)
            {
                if (thread.ThreadState == ThreadState.Running)
                {
                    thread.Interrupt();
                }
            }

            processor.Dispose();
        }

        public Turing(WorldData world)
        {
            World = world;
            processor = new WorkbackProcessor(world, OnProcessorProgress, OnTuringTestComplete);
        }
    }
}
