using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace archiver
{
    public sealed class Block
    {
        public int Id { get; }
        public byte[] Buffer { get; }

        public Block(int id, byte[] buffer)
        {
            Id = id;
            Buffer = buffer;
        }
    }

    public sealed class ProducerConsumer
    {
        private object _locker = new object();
        private bool isStoped = false;
        private Queue<Block> queue = new Queue<Block>();

        public void Enqueue(Block _block)
        {
            if (_block == null)
            {
                throw new ArgumentNullException("_block");
            }
            lock (_locker)
            {
                if (isStoped)
                {
                    throw new InvalidOperationException("Queue already stopped");
                }
                queue.Enqueue(_block);
                Monitor.Pulse(_locker);
            }
        }

        public Block Dequeue()
        {
            lock (_locker)
            {
                while (queue.Count == 0 && !isStoped)
                {
                    Monitor.Wait(_locker);
                }

                if (queue.Count == 0)
                {
                    return null;
                }

                return queue.Dequeue();
            }
        }

        public void Stop()
        {
            lock (_locker)
            {
                isStoped = true;
                Monitor.PulseAll(_locker);
            }
        }
    }
}
