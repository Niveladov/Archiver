using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace archiver
{
    public static class ThreadExtension
    {
        //public static void StartAll(this IEnumerable<Thread> threads)
        //{
        //    if (threads != null)
        //    {
        //        foreach (var thread in threads)
        //        {
        //            thread.Start();
        //        }
        //    }
        //}

        public static void WaitAll(this IEnumerable<Thread> threads)
        {
            if (threads != null)
            {
                foreach (var thread in threads)
                {
                    thread.Join();
                }
            }
        }
    }
    
}
