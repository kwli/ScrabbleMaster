using System;
using System.Collections.Generic;
using System.Linq;

namespace ScrabbleMaster
{
    public class Permutations
    {
        private readonly SortedDictionary<Word, int> _list = new SortedDictionary<Word, int>();
        private Field[,] Board;

        public Permutations(Field[,] board, List<Character> rack, ScrabbleDictionary dictionary)
        {
            Board = board;
            Rec(new List<Character>(), rack, dictionary.Root);
            List<KeyValuePair<Word, int>> dictCopy = new List<KeyValuePair<Word, int>>(_list.Count);
            int i = 1;
            do
            {
                dictCopy.RemoveAll(t => true);
                foreach (KeyValuePair<Word, int> kvp in _list)
                {
                    if (kvp.Key.Length > i)
                    {
                        dictCopy.Add(kvp);
                    }
                }
                foreach (KeyValuePair<Word, int> kvp in dictCopy)
                {
                    Word clone = (Word)kvp.Key.Clone();
                    if (_list.ContainsKey(clone))
                    {
                        if (clone.Points >= _list[clone])
                        {
                            _list.Remove(clone);
                            _list.Add(clone, clone.Points);
                        }
                    }
                }
                i++;
            } while (dictCopy.Count > 0);

            foreach (KeyValuePair<Word, int> kvp in _list)
            {
                Console.WriteLine(kvp.Key.BoardView);
            }
        }

        private void Rec(List<Character> word, List<Character> rack, Node subDict)
        {
            if (subDict.Terminator)
            {
                Word w = new Word(Board, word, Board[7, 7], true);
                if (!_list.ContainsKey(w))
                {
                    _list.Add(w, w.Points);
                }
            }
            if (rack.Count == 0)
            {
                return;
            }
            foreach (Character ch in rack)
            {
                if (subDict.ContainsKey(ch))
                {
                    List<Character> modifiedRack = new List<Character>(rack.Count - 1);
                    foreach (Character c in rack)
                    {
                        if (!c.Equals(ch))
                        {
                            modifiedRack.Add(c);
                        }
                    }
                    word.Add(ch);
                    Rec(word, modifiedRack, subDict[ch]);
                    word.Remove(ch);
                }
            }
        }

        public Word FindBestWord()
        {
            return _list.First().Key;
        }
    }
}
