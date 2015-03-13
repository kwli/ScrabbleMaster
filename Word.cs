using System.Collections.Generic;

namespace ScrabbleMaster
{
    public class Word
    {
        public string Text;
        public List<Character> CharList;
        public int Length;
        public Field Start;
        public bool Horizontal;
        public int Points;

        public Word(Field[,] board, List<Character> text, Field start, bool horizontal)
        {
            Length = text.Count;
            CharList = new List<Character>(Length);
            byte[] b = new byte[Length];
            int i = 0;
            foreach (Character c in text)
            {
                CharList.Add(c);
                b[i++] = (byte)c;
            }
            Text = Scrabble.Encoding.GetString(b);
            Start = start;
            Horizontal = horizontal;
            Points = CalculateScore(board, text, start, horizontal);
        }

        public int CalculateScore(Field[,] board, List<Character> text, Field original, bool horizontal)
        {
            int x = original.X, y = original.Y;
            int points = 0;
            int neighboursPoints = 0;
            int multiplier = 1;
            int fromRack = 0;

            foreach (Character c in text)
            {
                int letter = Board.GetPoints(c);

                Field field = board[x, y];
                if (!field.Definitive)
                {
                    Bonus bonus = field.GetBonus();
                    int letterPoints = 0;
                    switch (bonus)
                    {
                        case Bonus.TRIPLE:
                            multiplier *= 3;
                            letterPoints += letter;
                            break;
                        case Bonus.DOUBLE:
                            multiplier *= 2;
                            letterPoints += letter;
                            break;
                        case Bonus.RED:
                            letterPoints += letter == 5 ? 15 : letter;
                            break;
                        case Bonus.BLUE:
                            letterPoints += letter == 3 ? 9 : letter;
                            break;
                        case Bonus.GREEN:
                            letterPoints += letter == 2 ? 6 : letter;
                            break;
                        case Bonus.YELLOW:
                            letterPoints += letter == 1 ? 3 : letter;
                            break;
                        default:
                            letterPoints += letter;
                            break;
                    }
                    points += letterPoints;
                    int neighbourWord = GetWord(board, x, y, horizontal);
                    if (neighbourWord != 0)
                    {
                        neighboursPoints += neighbourWord + letterPoints;
                    } 
                    fromRack++;
                }
                else
                {
                    if (field.Content != c)
                    {
                        return 0;
                    }
                    points += letter;
                }
                if (horizontal)
                {
                    if (++x == 15)
                    {
                        break;
                    }
                }
                else
                {
                    if (++y == 15)
                    {
                        break;
                    }
                }
            }
            points *= multiplier;
            points += neighboursPoints;
            return fromRack == 7 ? points + 50 : points;
        }

        public int GetWord(Field[,] board, int x, int y, bool horizontal)
        {
            int points = 0;

            if (horizontal)
            {
                int yi = y;
                while (yi > 0)
                {
                    Field neighbour = board[x, --yi];
                    Character neighbourLetter = neighbour.Content;
                    if (neighbourLetter != Character.EMPTY && neighbour.Definitive)
                    {
                        points += Board.GetPoints(neighbourLetter);
                    }
                    else
                    {
                        yi = -1;
                    }
                }
                yi = y;
                while (yi < 14)
                {
                    Field neighbour = board[x, ++yi];
                    Character neighbourLetter = neighbour.Content;
                    if (neighbourLetter != Character.EMPTY && neighbour.Definitive)
                    {
                        points += Board.GetPoints(neighbourLetter);
                    }
                    else
                    {
                        yi = 15;
                    }
                }
            }
            else
            {
                int xi = x;
                while (xi > 0)
                {
                    Field neighbour = board[--xi, y];
                    Character neighbourLetter = neighbour.Content;
                    if (neighbourLetter != Character.EMPTY && neighbour.Definitive)
                    {
                        points += Board.GetPoints(neighbourLetter);
                    }
                    else
                    {
                        xi = -1;
                    }
                }
                xi = x;
                while (xi < 14)
                {
                    Field neighbour = board[++xi, y];
                    Character neighbourLetter = neighbour.Content;
                    if (neighbourLetter != Character.EMPTY && neighbour.Definitive)
                    {
                        points += Board.GetPoints(neighbourLetter);
                    }
                    else
                    {
                        xi = 15;
                    }
                }
            }

            return points;
        }

        public override string ToString()
        {
            return Text;
        }

        public override bool Equals(object word)
        {
            Word other = (Word)word;
            return other.Start.X == Start.X && other.Start.Y == Start.Y && other.CharList.Equals(CharList);
        }
    }
}
