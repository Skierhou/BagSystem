using System;
using System.Collections.Generic;

namespace SkierFramework
{
    public class IdGenerate
    {
        private ulong id;

        public IdGenerate()
        {
            id = (ulong)System.DateTime.Now.Ticks;
        }

        public ulong GenerateId()
        {
            return ++id;
        }
    }
}
