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
        private bool _isStoped = false;
        private int _expectedBlockId = 1;
        private int _countLimit;
        private bool _isFull = false;
        private Queue<Block> _queue = new Queue<Block>();

        public ProducerConsumer(int countLimit)
        {
            _countLimit = countLimit;
        }

        public void Enqueue(Block block)
        {
            if (block == null)
            {
                throw new ArgumentNullException("block");
            }
            lock (_locker)
            {
                if (_isStoped)
                {
                    throw new InvalidOperationException("Очередь уже остановлена");
                }
                while (_expectedBlockId != block.Id)
                {
                    Monitor.Wait(_locker);
                }
                while (_queue.Count >= _countLimit)
                {
                    _isFull = true;
                    Monitor.Wait(_locker);
                }
                _queue.Enqueue(block);
                _expectedBlockId++;
                Monitor.PulseAll(_locker);
            }
        }

        public void Enqueue(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("block");
            }
            lock (_locker)
            {
                if (_isStoped)
                {
                    throw new InvalidOperationException("Очередь уже остановлена");
                }
                while (_queue.Count >= _countLimit)
                {
                    _isFull = true;
                    Monitor.Wait(_locker);
                }
                var block = new Block(_expectedBlockId, buffer);
                _queue.Enqueue(block);
                _expectedBlockId++;
                Monitor.Pulse(_locker);
            }
        }

        public Block Dequeue()
        {
            lock (_locker)
            {
                while (_queue.Count == 0 && !_isStoped)
                {
                    Monitor.Wait(_locker);
                }
                if (_queue.Count == 0) return null;
                var block = _queue.Dequeue();
                if (_isFull)
                {
                    _isFull = false;
                    Monitor.Pulse(_locker);
                }
                return block;
            }
        }

        public void Stop()
        {
            lock (_locker)
            {
                _isStoped = true;
                Monitor.PulseAll(_locker);
            }
        }
    }
}
