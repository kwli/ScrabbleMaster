using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ImageUtils;

namespace ScrabbleMaster
{
    public class Board
    {
        public Field[,] Fields;
        public List<Character> Rack = new List<Character>(7);

        public static readonly SolidBrush Rubber = new SolidBrush(Color.White);
        private readonly Pen _blackPen = new Pen(Color.Black, 1);
        public readonly StringFormat _stringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        public readonly Font _drawFont = new Font("Verdana", 28, FontStyle.Regular, GraphicsUnit.Point);
        private readonly Font _boldFont = new Font("Verdana", 32, FontStyle.Bold, GraphicsUnit.Point);
        private string _rackValue = "";

        public const int FIELD_SIZE = 60;
        private readonly int _boardSize = 15 * FIELD_SIZE + 2;

        public Dictionary<Bonus, SolidBrush> BoardFieldBonuses;
        public Dictionary<int, SolidBrush> LetterValues;

        public Graphics _boardGraphics;
        public Graphics _rackGraphics;
        private Bitmap _bitmap;
        private Bitmap _rackBitmap;

        public KeyValuePair<Dictionary<Field, List<Character>>, Dictionary<Field, List<Character>>> CrossChecks;
        public KeyValuePair<List<Field>, List<Field>> Anchors;

        public static ScrabbleDictionary Dictionary;

        public bool Fresh
        {
            get
            {
                for (int i = 0; i < Scrabble.BoardSize; i++)
                {
                    for (int j = 0; j < Scrabble.BoardSize; j++)
                    {
                        if (Fields[i, j].Definitive)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public Board(int x, int y)
        {
            Fields = new Field[x, y];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    Fields[i, j] = new Field(i, j);
                }
            }

            Fields[7, 7].DistH = 0;
            Fields[7, 7].DistV = 0;
            BoardFieldBonuses = InitBonusesDictionary();
            LetterValues = InitLetterValues();
        }

        #region MOVEMENTS

        public List<KeyValuePair<Word, int>> GenerateMovement()
        {
            CrossChecks = GetCrossChecks(Fields);
            CalculateDists(Fields);
            return new Movement(Fields, CrossChecks, Rack).FullSearch(ScrabbleForm.Dictionary);
        }

        public List<KeyValuePair<Word, int>> FirstMovement()
        {
            CrossChecks = new KeyValuePair<Dictionary<Field, List<Character>>, Dictionary<Field, List<Character>>>(new Dictionary<Field, List<Character>>(), new Dictionary<Field, List<Character>>());
            for (int i = 0; i <= 7; i++)
            {
                int value = 8 - i;
                Fields[i, 7].DistH = value;
                Fields[7, i].DistV = value;
            }

            return new Movement(Fields, CrossChecks, Rack).FullSearch(ScrabbleForm.Dictionary);
        }

        #endregion

        #region CROSS_CHEKS & ANCHORS
        private KeyValuePair<Dictionary<Field, List<Character>>, Dictionary<Field, List<Character>>> GetCrossChecks(Field[,] board)
        {
            Dictionary<Field, List<Character>> horizontal = new Dictionary<Field, List<Character>>();
            Dictionary<Field, List<Character>> vertical = new Dictionary<Field, List<Character>>();

            //horizontal
            // * A B C

            for (int i = 0; i < Scrabble.BoardSize; i++)
            {
                for (int j = 0; j < Scrabble.BoardSize; j++)
                {
                    Field field = board[i, j];
                    if (!field.Definitive && field.Content == Character.EMPTY)
                    {
                        int k = i - 1;
                        int n = i + 1;

                        while (k >= 0 && board[k, j].Content != Character.EMPTY)
                        {
                            k--;
                        }
                        int length1 = i - k - 1;

                        while (n < Scrabble.BoardSize && board[n, j].Content != Character.EMPTY)
                        {
                            n++;
                        }
                        int length2 = n - i - 1;

                        if (length1 == 0 && length2 == 0) continue;
                        Character[] word = new Character[length1 + length2 + 1];
                        byte[] wordBytes = new byte[length1 + length2 + 1];
                        int w = 0;
                        if (length1 > 0)
                        {
                            for (int m = k + 1; m < i; m++)
                            {
                                word[w] = board[m, j].Content;
                                wordBytes[w++] = (byte)board[m, j].Content;
                            }
                        }
                        w++;
                        if (length2 > 0)
                        {
                            for (int m = i + 1; m < n; m++)
                            {
                                word[w] = board[m, j].Content;
                                wordBytes[w++] = (byte)board[m, j].Content;
                            }
                        }

                        //field.Constrained = true;
                        foreach (Character symbol in Scrabble.Alphabet)
                        {
                            word[length1] = symbol;
                            wordBytes[length1] = (byte)symbol;
                            List<Character> fieldSymbols;
                            if (horizontal.ContainsKey(field))
                            {
                                fieldSymbols = horizontal[field];
                            }
                            else
                            {
                                fieldSymbols = new List<Character>();
                                horizontal.Add(field, fieldSymbols);
                            }

                            if (ScrabbleForm.Dictionary.SequenceExists(word))
                            {
                                fieldSymbols.Add(symbol);
                            }
                        }

                    }
                }
            }

            //vertical
            // *
            // A
            // B
            // C

            for (int i = 0; i < Scrabble.BoardSize; i++)
            {
                for (int j = 0; j < Scrabble.BoardSize; j++)
                {
                    Field field = board[i, j];
                    if (!field.Definitive && field.Content == Character.EMPTY)
                    {
                        int k = j - 1;
                        int n = j + 1;

                        while (k >= 0 && board[i, k].Content != Character.EMPTY)
                        {
                            k--;
                        }
                        int length1 = j - k - 1;

                        while (n < Scrabble.BoardSize && board[i, n].Content != Character.EMPTY)
                        {
                            n++;
                        }
                        int length2 = n - j - 1;

                        if (length1 == 0 && length2 == 0) continue;
                        {
                            Character[] word = new Character[length1 + length2 + 1];
                            byte[] wordBytes = new byte[length1 + length2 + 1];
                            int w = 0;
                            if (length1 > 0)
                            {
                                for (int m = k + 1; m < j; m++)
                                {
                                    word[w] = board[i, m].Content;
                                    wordBytes[w++] = (byte)board[i, m].Content;
                                }
                            }
                            w++;
                            if (length2 > 0)
                            {
                                for (int m = j + 1; m < n; m++)
                                {
                                    word[w] = board[i, m].Content;
                                    wordBytes[w++] = (byte)board[i, m].Content;
                                }
                            }

                            //field.Constrained = true;
                            foreach (Character symbol in Scrabble.Alphabet)
                            {
                                word[length1] = symbol;
                                wordBytes[length1] = (byte)symbol;
                                List<Character> fieldSymbols;
                                if (vertical.ContainsKey(field))
                                {
                                    fieldSymbols = vertical[field];
                                }
                                else
                                {
                                    fieldSymbols = new List<Character>();
                                    vertical.Add(field, fieldSymbols);
                                }

                                if (ScrabbleForm.Dictionary.SequenceExists(word))
                                {
                                    fieldSymbols.Add(symbol);
                                }
                            }
                        }
                    }
                }
            }

            KeyValuePair<Dictionary<Field, List<Character>>, Dictionary<Field, List<Character>>> crossChecksPair =
                new KeyValuePair<Dictionary<Field, List<Character>>, Dictionary<Field, List<Character>>>(horizontal, vertical);

            return crossChecksPair;
        }

        public void CalculateDists(Field[,] board)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= 14; i++)
            {
                for (int j = 14, k = 14 - i; j >= 14 - i && k <= 14; j--, k++)
                {
                    board[j, k].DistH = Dist(board, j, k, true);
                    board[j, k].DistV = Dist(board, j, k, false);
                }
            }

            for (int i = 0; i <= 13; i++)
            {
                for (int j = 13 - i, k = 0; j >= 0 && k <= 13 - i; j--, k++)
                {
                    board[j, k].DistH = Dist(board, j, k, true);
                    board[j, k].DistV = Dist(board, j, k, false);
                }
            }
        }

        private int Dist(Field[,] board, int x, int y, bool horizontal)
        {
            if (board[x, y].Definitive)
            {
                return 0;
            }
            if (horizontal)
            {
                if (x == 14) return -1;
                int next = board[x + 1, y].DistH;
                return next == -1 ? -1 : next + 1;
            }
            else
            {
                if (y == 14) return -1;
                int next = board[x, y + 1].DistV;
                return next == -1 ? -1 : next + 1;
            }
        }

        #endregion

        #region DRAW
        public Image Draw()
        {
            _bitmap = new Bitmap(_boardSize, _boardSize);
            _boardGraphics = Graphics.FromImage(_bitmap);

            _boardGraphics.FillRectangle(Rubber, 0, 0, _bitmap.Width, _bitmap.Height);
            for (int i = 0; i < 15; i++)
            {
                int xl = i * FIELD_SIZE;

                for (int j = 0; j < 15; j++)
                {
                    Rectangle rectangle = new Rectangle(xl, j * FIELD_SIZE, FIELD_SIZE, FIELD_SIZE);
                    if (Fields[i, j].Content != Character.EMPTY)
                    {
                        _boardGraphics.FillRectangle(LetterValues[GetPoints(Fields[i, j].Content)], rectangle);
                        _boardGraphics.DrawString(Scrabble.Encoding.GetString(new[] { (byte)Fields[i, j].Content }).ToUpper(), _drawFont, Fields[i, j].Definitive ? LetterValues[0] : Rubber, rectangle, _stringFormat);
                    }
                    else
                    {
                        _boardGraphics.FillRectangle(BoardFieldBonuses[Fields[i, j].GetBonus()], rectangle);
                    }
                }
            }

            return DrawLines();
        }

        public Image DrawLines()
        {
            for (int i = 0; i <= _boardSize; i += FIELD_SIZE)
            {
                _boardGraphics.DrawLine(_blackPen, i, 0, i, _bitmap.Height);
                _boardGraphics.DrawLine(_blackPen, 0, i, _bitmap.Width, i);
            }
            return ImageTransparency.ChangeOpacity(_bitmap, 0.3f);
        }

        public Image DrawEmptyRack()
        {
            _rackBitmap = new Bitmap(7 * FIELD_SIZE, FIELD_SIZE);
            _rackGraphics = Graphics.FromImage(_rackBitmap);
            _rackGraphics.FillRectangle(Rubber, 0, 0, _rackBitmap.Width, _rackBitmap.Height);
            return _rackBitmap;
        }

        #endregion

        #region GUI

        public Image FieldLocated(MouseEventArgs mea, ref TextBox textBox, int currentActive)
        {
            _rackValue = textBox.Text;
            int x = mea.X / Board.FIELD_SIZE;
            int y = mea.Y / Board.FIELD_SIZE;
            if (!Fields[x, y].Definitive && Fields[x, y].Content == Character.EMPTY)
            {
                Rectangle rectangle = new Rectangle(x * Board.FIELD_SIZE, Board.FIELD_SIZE * y, Board.FIELD_SIZE,
                    Board.FIELD_SIZE);
                Fields[x, y].Content =
                    (Character)
                        Enum.Parse(typeof(Character), new string(_rackValue.ElementAt(currentActive), 1).ToUpper());
                Fields[x, y].Definitive = true;
                _boardGraphics.FillRectangle(LetterValues[GetPoints(Fields[x, y].Content)], rectangle);
                _boardGraphics.DrawString(new string(_rackValue[currentActive], 1), _drawFont, LetterValues[0], rectangle, _stringFormat);

                _rackValue = String.Format("{0}{1}", _rackValue.Substring(0, currentActive),
                    _rackValue.Substring(currentActive + 1, _rackValue.Length - currentActive - 1));

                textBox.Text = _rackValue.ToUpper();
                textBox.Select(_rackValue.Length, 0);

                return DrawLines();
            }
            int tiles = _rackValue.Length;
            for (int i = 0; i < 7; i++)
            {
                Rectangle rectangle = new Rectangle(i * FIELD_SIZE, 0, FIELD_SIZE, FIELD_SIZE);
                if (i < tiles)
                {
                    string next = new string(_rackValue.ElementAt(i), 1).ToUpper();
                    try
                    {
                        Character c = (Character)Enum.Parse(typeof(Character), next);
                        Rack.Add(c);
                        _rackGraphics.FillRectangle(LetterValues[GetPoints(c)], rectangle);
                        _rackGraphics.DrawString(next, _drawFont, LetterValues[0], rectangle, _stringFormat);
                    }
                    catch (Exception ex)
                    {
                        _rackValue = _rackValue.Substring(0, tiles - 1);
                    }
                }
                else
                {
                    _rackGraphics.FillRectangle(Rubber, rectangle);
                }
            }

            textBox.Text = _rackValue.ToUpper();
            textBox.Select(_rackValue.Length, 0);
            return DrawLines();
        }

        public Image HandleTextBox(ref TextBox textBox)
        {
            Rack = new List<Character>(7);
            string rackValue = textBox.Text;
            int tiles = rackValue.Length;
            for (int i = 0; i < 7; i++)
            {
                Rectangle rectangle = new Rectangle(i * FIELD_SIZE, 0, FIELD_SIZE, FIELD_SIZE);
                if (i < tiles)
                {
                    try
                    {
                        string nextChar = new string(rackValue.ElementAt(i), 1);
                        string next = nextChar;
                        Character c;
                        SolidBrush brush;

                        if (nextChar == "*")
                        {
                           c = Character.STAR;
                           brush = new SolidBrush(Color.Gray);
                        }
                        else
                        {
                            next = nextChar.ToUpper();
                            c = (Character)Enum.Parse(typeof(Character), next);
                            brush = LetterValues[GetPoints(c)];
                        }
                        Rack.Add(c);
                        if (_rackGraphics == null)
                        {
                            Image rackImage = DrawEmptyRack();
                            _rackBitmap = (Bitmap)rackImage;
                            _rackGraphics = Graphics.FromImage(rackImage);

                        }
                        _rackGraphics.FillRectangle(brush, rectangle);
                        _rackGraphics.DrawString(next, _drawFont, LetterValues[0], rectangle, _stringFormat);
                    }
                    catch (ArgumentException ex)
                    {
                        rackValue = rackValue.Substring(0, tiles - 1);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    _rackGraphics.FillRectangle(Rubber, rectangle);
                }
            }

            textBox.Text = rackValue.ToUpper();
            textBox.Select(rackValue.Length, 0);
            return _rackBitmap;
        }

        public Image HandleRackClick(MouseEventArgs mea, TextBox textBox, ref int activeLetter)
        {
            _rackValue = textBox.Text;
            int rackIndex = mea.X / FIELD_SIZE;
            if (rackIndex < _rackValue.Length)
            {
                Rectangle rectangle = new Rectangle(rackIndex * FIELD_SIZE, 0, FIELD_SIZE, FIELD_SIZE);
                if (activeLetter == -1)
                {
                    // pierwsze kliknięcie w rack
                    _rackGraphics.FillRectangle(LetterValues[GetPoints(Rack[rackIndex])], rectangle);
                    _rackGraphics.DrawString(new string(_rackValue.ElementAt(rackIndex), 1).ToUpper(), _boldFont,
                        LetterValues[0], rectangle,
                        _stringFormat);
                    activeLetter = rackIndex;
                }
                // drugie z rzędu kliknięcie w rack: jeśli kliknięta została dwa razy ta sama litera, to odznaczamy ją jako "w drodze na stół"; jeśli inna, to odznaczamy poprzednią a zaznaczamy
                else
                {
                    _rackGraphics.FillRectangle(LetterValues[GetPoints(Rack[activeLetter])], rectangle);
                    _rackGraphics.DrawString(new string(_rackValue.ElementAt(activeLetter), 1).ToUpper(), _drawFont,
                        LetterValues[0], rectangle, _stringFormat);
                    if (activeLetter != rackIndex)
                    {

                        _rackGraphics.FillRectangle(LetterValues[GetPoints(Rack[rackIndex])], rectangle);
                        _rackGraphics.DrawString(new string(_rackValue.ElementAt(rackIndex), 1).ToUpper(), _boldFont,
                            LetterValues[0], rectangle, _stringFormat);
                        activeLetter = rackIndex;
                    }
                    else
                    {
                        activeLetter = -1;
                    }
                }
            }
            return _rackBitmap;
        }

        public byte[] Proposal(Word word, string rackValue, string fileName = null)
        {
            byte[] rackArray = Scrabble.Encoding.GetBytes(rackValue.ToLower());
            if (word == null)
            {
                return rackArray;
            }
            int x = word.Start.X;
            int y = word.Start.Y;
            for (int i = 0; i < word.Length; i++)
            {
                Character c = word.CharList[i];
                //if (!rackArray.Contains((byte)c))
                //{

                //}
                rackArray = UpdateRack(rackArray, rackArray.Contains((byte)c) ? c : Character.STAR);

                if (word.Horizontal)
                {
                    Fields[x + i, y].Content = c;
                }
                else
                {
                    Fields[x, y + i].Content = c;
                }
            }
            //int scores = word.CalculateScore(Fields, word.CharList, word.Start, word.Horizontal);
            return rackArray;
        }

        public byte[] Cancel(Word word, byte[] rackBytes)
        {
            int x = word.Start.X;
            int y = word.Start.Y;
            int wordLength = word.Length;
            int j = rackBytes.Length;
            byte[] characters = new byte[Math.Min(wordLength + j, 7)];
            Array.Copy(rackBytes, 0, characters, 0, j);
            for (int i = 0; i < wordLength; i++)
            {
                if (word.Horizontal)
                {
                    Field field = Fields[x + i, y];
                    if (!field.Definitive)
                    {
                        characters[j++] = (byte)word.CharList[i];
                        field.Content = Character.EMPTY;
                    }
                }
                else
                {
                    Field field = Fields[x, y + i];
                    if (!field.Definitive)
                    {
                        characters[j++] = (byte)word.CharList[i];
                        field.Content = Character.EMPTY;
                    }
                }
            }
            return characters;
        }

        public void Locate(Word word)
        {
            int x = word.Start.X;
            int y = word.Start.Y;
            for (int i = 0; i < word.Length; i++)
            {
                if (word.Horizontal)
                {
                    Field field = Fields[x + i, y];
                    field.Content = word.CharList[i];
                    field.Definitive = true;
                }
                else
                {
                    Field field = Fields[x, y + i];
                    field.Content = word.CharList[i];
                    field.Definitive = true;
                }
            }
        }

        public byte[] UpdateRack(byte[] oldRack, Character currentCharacter)
        {
            byte[] newRack = new byte[oldRack.Length];
            bool stopCondition = false;
            for (int j = 0; !stopCondition && j < oldRack.Length; j++)
            {
                if (oldRack[j] == (byte)currentCharacter)
                {
                    byte tmp = oldRack[0];
                    oldRack[0] = oldRack[j];
                    oldRack[j] = tmp;
                    stopCondition = true;
                }
            }
            int offset = Convert.ToInt32(stopCondition);
            int newSize = oldRack.Length - offset;
            Array.Copy(oldRack, offset, newRack, 0, newSize);
            Array.Resize(ref newRack, newSize);
            return newRack;
        }

        #endregion

        #region MISCEALLANOUS

        public static int GetPoints(Character c)
        {
            switch (c)
            {
                case Character.A:
                case Character.E:
                case Character.I:
                case Character.N:
                case Character.O:
                case Character.R:
                case Character.S:
                case Character.W:
                case Character.Z:
                    return 1;
                case Character.C:
                case Character.D:
                case Character.K:
                case Character.L:
                case Character.M:
                case Character.P:
                case Character.T:
                case Character.Y:
                    return 2;
                case Character.B:
                case Character.G:
                case Character.H:
                case Character.J:
                case Character.Ł:
                case Character.U:
                    return 3;
                case Character.Ą:
                case Character.Ć:
                case Character.Ę:
                case Character.F:
                case Character.Ń:
                case Character.Ó:
                case Character.Ś:
                case Character.Ź:
                case Character.Ż:
                    return 5;
                default:
                    return 0;
            }
        }

        #endregion

        #region INIT
        public Dictionary<int, SolidBrush> InitLetterValues()
        {
            Dictionary<int, SolidBrush> dictionary = new Dictionary<int, SolidBrush>(5);
            dictionary.Add(5, new SolidBrush(Color.Tomato));
            dictionary.Add(3, new SolidBrush(Color.Blue));
            dictionary.Add(2, new SolidBrush(Color.ForestGreen));
            dictionary.Add(1, new SolidBrush(Color.Gold));
            dictionary.Add(0, new SolidBrush(Color.Black));
            return dictionary;
        }

        public Dictionary<Bonus, SolidBrush> InitBonusesDictionary()
        {
            Dictionary<Bonus, SolidBrush> dictionary = new Dictionary<Bonus, SolidBrush>(7);
            dictionary.Add(Bonus.TRIPLE, new SolidBrush(Color.Aqua));
            dictionary.Add(Bonus.DOUBLE, new SolidBrush(Color.Tan));
            dictionary.Add(Bonus.RED, new SolidBrush(Color.Tomato));
            dictionary.Add(Bonus.BLUE, new SolidBrush(Color.Blue));
            dictionary.Add(Bonus.GREEN, new SolidBrush(Color.ForestGreen));
            dictionary.Add(Bonus.YELLOW, new SolidBrush(Color.Gold));
            dictionary.Add(Bonus.EMPTY, Rubber);
            return dictionary;
        }

        #endregion
    }
}
