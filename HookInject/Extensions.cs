using System.Collections.Generic;

namespace HookInject
{
    public static class Extensions
    {
        public static T PopFirstElement<T>(this List<T> list)
        {
            T t = list[0];
            list.RemoveAt(0);
            return t;
        }

        public static bool IsEmpty<T>(this List<T> list)
        {
            return list.Count == 0;
        }
    }
}
