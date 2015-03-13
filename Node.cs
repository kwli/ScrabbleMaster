using System.Collections.Generic;
using System.Text;

namespace ScrabbleMaster
{
    public class Node : Dictionary<Character, Node>
    {
        public bool Terminator;
        private static readonly Encoding _encoding = Encoding.GetEncoding("Windows-1250");

        public Node()
        {
            Terminator = false;
        }

        public static string DisplayCharacters(Character[] chars)
        {
            byte[] bytes = new byte[chars.Length];
            for (int i = 0; i < chars.Length; i++)
            {
                bytes[i] = (byte)chars[i];
            }
            return _encoding.GetString(bytes);
        }

        public static string DisplayCharacters(List<Character> chars)
        {
            byte[] bytes = new byte[chars.Count];
            int i = 0;
            foreach (Character c in chars)
            {
                bytes[i++] = (byte)c;
            }
            return _encoding.GetString(bytes);
        }
    }
}
