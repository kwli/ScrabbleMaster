using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScrabbleMaster
{
    public class Movement
    {
        private readonly Character[] _rack = new Character[7];
        private readonly List<KeyValuePair<Word, int>> List = new List<KeyValuePair<Word, int>>();
        private readonly KeyValuePair<Dictionary<Field, List<Character>>, Dictionary<Field, List<Character>>> _crossChecks;
        public static FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "deb.txt"), FileMode.OpenOrCreate);
        private readonly Field[,] _board;

        public Movement(Field[,] board, KeyValuePair<Dictionary<Field, List<Character>>, Dictionary<Field, List<Character>>> crossChecks, List<Character> rack)
        {
            _crossChecks = crossChecks;
            _board = board;
            _rack = new Character[rack.Count];
            rack.CopyTo(_rack);
            if (rack.Contains(Character.STAR))
            {
                rack.Remove(Character.STAR);
                foreach (Character symbol in Scrabble.Alphabet)
                {
                    rack.Add(symbol);
                    Movement movement = new Movement(board, crossChecks, rack);
                    foreach (KeyValuePair<Word, int> kvp in movement.FullSearch(ScrabbleForm.Dictionary))
                    {
                        InsertWordToList(kvp.Key);
                    }
                    rack.Remove(symbol);
                }
            }
        }

        public List<KeyValuePair<Word, int>> FullSearch(ScrabbleDictionary dictionary)
        {
            try
            {
                foreach (Field field in _board)
                {
                    List<Character> rack = _rack.ToList();
                    int i = field.X - 1;
                    while (i >= 0 && _board[i, field.Y].Content != Character.EMPTY)
                    {
                        i--;
                    }

                    Field start = _board[i + 1, field.Y];
                    int distH = start.DistH;
                    if (distH != -1 && distH <= rack.Count && field.X < 14)
                    {
                        if (!start.Definitive)
                        {
                            IEnumerable<Character> possibleLetters = _crossChecks.Value.ContainsKey(start) ? _crossChecks.Value[start].Intersect(rack) : new List<Character>(rack);
                            foreach (Character c in possibleLetters)
                            {
                                if (c == Character.STAR)
                                {
                                    rack.Remove(c);
                                    foreach (KeyValuePair<Character, Node> kvp in dictionary.Root)
                                    {
                                        RecursiveSearch(_board[field.X + 1, field.Y], rack, kvp.Value, new List<Character> { kvp.Key }, start, true);
                                    }
                                    rack.Add(c);
                                }
                                if (dictionary.Root.ContainsKey(c))
                                {
                                    rack.Remove(c);
                                    RecursiveSearch(_board[field.X + 1, field.Y], rack, dictionary.Root[c],
                                        new List<Character> { c }, start, true);
                                    rack.Add(c);
                                }
                            }
                        }
                        else
                        {
                            if (dictionary.Root.ContainsKey(field.Content))
                                RecursiveSearch(_board[field.X + 1, field.Y], rack, dictionary.Root[field.Content],
                                    new List<Character> { start.Content }, start, true);
                        }
                    }
                }

                foreach (Field field in _board)
                {
                    List<Character> rack = _rack.ToList();
                    int i = field.Y - 1;
                    while (i >= 0 && _board[field.X, i].Content != Character.EMPTY)
                    {
                        i--;
                    }

                    Field start = _board[field.X, i + 1];
                    int distV = start.DistV;
                    if (distV != -1 && distV <= rack.Count && field.Y < 14)
                    {
                        if (!start.Definitive)
                        {
                            IEnumerable<Character> possibleLetters = _crossChecks.Key.ContainsKey(start) ? _crossChecks.Key[start].Intersect(rack) : new List<Character>(rack); 
                            foreach (Character c in possibleLetters)
                            {
                                if (c == Character.STAR)
                                {
                                    rack.Remove(c);
                                    foreach (KeyValuePair<Character, Node> kvp in dictionary.Root)
                                    {
                                        RecursiveSearch(_board[field.X, field.Y + 1], rack, kvp.Value, new List<Character> { kvp.Key }, start, false);
                                    }
                                    rack.Add(c);
                                }
                                if (dictionary.Root.ContainsKey(c))
                                {
                                    rack.Remove(c);
                                    RecursiveSearch(_board[field.X, field.Y + 1], rack, dictionary.Root[c], new List<Character> { c }, start, false);
                                    rack.Add(c);
                                }
                            }
                        }
                        else
                        {
                            if (dictionary.Root.ContainsKey(field.Content))
                                RecursiveSearch(_board[field.X, field.Y + 1], rack, dictionary.Root[field.Content],
                                    new List<Character> { start.Content }, start, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(ex);
            }

            return List;
        }

        public void RecursiveSearch(Field field, List<Character> rack, Node subDict, List<Character> word, Field original, bool horizontal)
        {
            StringBuilder sb = new StringBuilder(field.ToString());
            sb.Append("; ORI: ");
            sb.Append(original.ToString());
            sb.Append("; WORD: ");
            foreach (Character c in word)
            {
                sb.Append(c);
            }
            sb.Append("; ");
            sb.Append(rack.Count);
            if (field.Definitive && field.Content != Character.EMPTY)
            {

                sb.AppendLine(" NOT EMPTY");
                byte[] buffer = Scrabble.Encoding.GetBytes(sb.ToString());
                fs.Write(buffer, 0, buffer.Length);
                if (!subDict.ContainsKey(field.Content)) return;
                //CheckWordFinish(field, subDict, word, original, horizontal);
                word.Add(field.Content);
                if (horizontal && field.X <= Scrabble.BoardSize - 2)
                    RecursiveSearch(_board[field.X + 1, field.Y], rack, subDict[field.Content], word, original, true);
                else if (field.Y <= Scrabble.BoardSize - 2)
                    RecursiveSearch(_board[field.X, field.Y + 1], rack, subDict[field.Content], word, original, false);
                word.RemoveAt(word.Count - 1);
            }
            else
            {

                sb.AppendLine(" EMPTY");
                byte[] buffer = Scrabble.Encoding.GetBytes(sb.ToString());
                fs.Write(buffer, 0, buffer.Length);
                if (rack.Count() < 7)
                {
                    CheckWordFinish(field, subDict, word, original, horizontal);
                }
                foreach (Character c in rack.ToArray())
                {
                    if (horizontal)
                    {
                        if ((!_crossChecks.Value.ContainsKey(field) || _crossChecks.Value[field].Contains(c)) && subDict.ContainsKey(c))
                        {
                            word.Add(c);
                            rack.Remove(c);
                            CheckWordFinish(field, subDict[c], word, original, true);
                            if (field.X <= Scrabble.BoardSize - 2)
                                RecursiveSearch(_board[field.X + 1, field.Y], rack, subDict[c], word, original, true);
                            word.RemoveAt(word.Count - 1);
                            rack.Add(c);
                        }
                    }
                    else
                    {
                        if ((!_crossChecks.Key.ContainsKey(field) || _crossChecks.Key[field].Contains(c)) && subDict.ContainsKey(c))
                        {
                            word.Add(c);
                            rack.Remove(c);
                            CheckWordFinish(field, subDict[c], word, original, false);
                            if (field.Y <= Scrabble.BoardSize - 2)
                                RecursiveSearch(_board[field.X, field.Y + 1], rack, subDict[c], word, original, false);
                            word.RemoveAt(word.Count - 1);
                            rack.Add(c);
                        }
                    }
                }
            }
        }
        
        private void CheckWordFinish(Field field, Node subDict, List<Character> word, Field original, bool horizontal)
        {
            if (subDict.Terminator)
            {
                Word w = new Word(_board, word, original, horizontal);
                if (!ContainsWord(w))
                {
                    if (w.CalculateScore(_board, word, original, horizontal) != 0)
                    {
                        if (horizontal ? original.DistH <= word.Count && (field.X == Scrabble.BoardSize - 1 || _board[field.X + 1, field.Y].Content == Character.EMPTY) : original.DistV <= word.Count && (field.Y == Scrabble.BoardSize - 1 || _board[field.X, field.Y + 1].Content == Character.EMPTY))
                        {
                            InsertWordToList(w);
                        }
                    }
                }
            }
        }

        private void InsertWordToList(Word word)
        {
            int i = 0;
            foreach (KeyValuePair<Word, int> kvp in List)
            {
                if (kvp.Key.Points <= word.Points)
                {
                    break;
                }
                i++;
            }
            List.Insert(i, new KeyValuePair<Word, int>(word, word.Points));
        }

        private bool ContainsWord(Word word)
        {
            bool result = false;
            foreach (KeyValuePair<Word, int> kvp in List)
            {
                if (kvp.Key.Start.X == word.Start.X && kvp.Key.Start.Y == word.Start.Y && kvp.Key.Length == word.Length)
                {
                    result = true;
                    for (int i = 0; result && i < kvp.Key.Length; i++) {
                        if (kvp.Key.CharList[i] != word.CharList[i])
                        {
                            result = false;
                        }
                    }
                    if (result)
                    {
                        break;
                    }
                }
            }
            return result;
        }
    }
}
