using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZusiStart.Miscellaneous
{
#if false
    class LockableObject<T>
    {
        private readonly object _lock = new object();
        private T _value;

        public T Value
        {
            get
            {
                lock (_lock)
                {
                    return _value;
                }
            }
            set
            {
                lock (_lock)
                {
                    _value = value;
                }
            }
        }

        public static implicit operator T(LockableObject<T> obj)
        {
            return obj != null ? obj.Value : default;
        }
    }
#endif
}
