using System;
using System.IO;

namespace ScrabbleMaster
{
    public class ScrabbleDictionary
    {
        public Node Root;

        public ScrabbleDictionary(string fileName, int fileLength)
        {
            using (FileStream fs = new FileStream(String.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, fileName), FileMode.Open))
            {
                byte[] buffer = new byte[fileLength];
                fs.Read(buffer, 0, fileLength);
                Root = new Node();
                Node current = Root;
                for (int i = 0; i < fileLength; i++)
                {
                    if (RecursiveAdd((Character)buffer[i], ref current))
                    {
                        current = Root;
                    }
                }
            }
        }

        public bool WordExists(string word)
        {
            byte[] bytes = Scrabble.Encoding.GetBytes(word);
            Character[] characters = new Character[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                characters[i] = (Character)bytes[i];
            }
            return RecursiveSearch(Root, characters);
        }

        public bool SequenceExists(Character[] word)
        {
            return RecursiveSearch(Root, word);
        }

        private bool RecursiveSearch(Node node, Character[] word)
        {
            if (word.Length == 0)
            {
                return node.Terminator;
            }
            if (!node.ContainsKey(word[0])) return false;
            Character[] tmp = new Character[word.Length - 1];
            Array.Copy(word, 1, tmp, 0, word.Length - 1);
            return RecursiveSearch(node[word[0]], tmp);
        }

        private bool RecursiveAdd(Character readByte, ref Node node)
        {
            if (readByte == Character.CARRIAGE_RETURN)
            {
                node.Terminator = true;
                return true;
            }
            if (readByte == Character.NEWLINE)
            {
                return false;
            }
            if (!node.ContainsKey(readByte))
            {
                Node child = new Node();
                node.Add(readByte, child);
                node = child;
            }
            else
            {
                node = node[readByte];
            }
            return false;
        }
    }
}