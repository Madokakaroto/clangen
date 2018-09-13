using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clangen
{
    class StringTools
    {
        public static string RemoveFirstOf(string to_remove, string sub)
        {
            int index = to_remove.IndexOf(sub);
            return index < 0 ? to_remove : to_remove.Remove(index, sub.Length).TrimStart();
        }
    }
}
