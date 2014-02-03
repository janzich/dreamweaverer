using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamWeaverer
{
    public static class Utils
    {

        public static Tuple<string, string> RemoveCommonPrefix(string str1, string str2)
        {

            if (str1 == null) throw new ArgumentNullException("str1");
            if (str2 == null) throw new ArgumentNullException("str2");

            int i = 0;
            while (i < str1.Length && i < str2.Length && str1[i] == str2[i])
            {
                i++;
            }

            return Tuple.Create(
                str1.Substring(i),
                str2.Substring(i));

        }

    }
}
