using System;
using System.IO;
using System.Text;

namespace ScrabbleMaster
{
    static class Snapshot
    {
        public static void Save(Field[,] board, string fileName, int boardSize)
        {
            using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), FileMode.OpenOrCreate))
            {
                StringBuilder s = new StringBuilder();
                for (int i = 0; i < boardSize; i++)
                {
                    byte[] row = new byte[boardSize];
                    for (int j = 0; j < boardSize; j++)
                    {
                        Field field = board[j, i];
                        row[j] = (byte) (field.Definitive ? field.Content : Character.EMPTY);
                    }
                    s.AppendLine(Scrabble.Encoding.GetString(row));
                }
                byte[] buffer = Scrabble.Encoding.GetBytes(s.ToString());
                fs.Write(buffer, 0, buffer.Length);
            }
        }

        public static Board Load(string fileName, int fileLength, int boardSize)
        {
            Board board = new Board(boardSize, boardSize);
            using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), FileMode.Open))
            {
                byte[] buffer = new byte[fileLength];
                fs.Read(buffer, 0, fileLength);
                for (int i = 0, k = 0; i < 15; i++, k += 2)
                {
                    for (int j = 0; j < 15; j++, k++)
                    {
                        Character c = (Character)buffer[k];
                        Field field = board.Fields[j, i];
                        field.Content = c;
                        field.Definitive = field.Content != Character.EMPTY;
                    }
                }
            }
            return board;
        }
    }
}
