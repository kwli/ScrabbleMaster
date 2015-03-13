using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ScrabbleMaster
{
    public static class Scrabble
    {
        public static readonly int BoardSize = 15;

        public static List<Character> Alphabet = new List<Character>(32)
        {
            Character.A,
            Character.Ą,
            Character.B,
            Character.C,
            Character.Ć,
            Character.D,
            Character.E,
            Character.Ę,
            Character.F,
            Character.G,
            Character.H,
            Character.I,
            Character.J,
            Character.K,
            Character.L,
            Character.Ł,
            Character.M,
            Character.N,
            Character.Ń,
            Character.O,
            Character.Ó,
            Character.P,
            Character.R,
            Character.S,
            Character.Ś,
            Character.T,
            Character.U,
            Character.W,
            Character.Y,
            Character.Z,
            Character.Ź,
            Character.Ż
        };

        public static Board Board;
        public static List<Character> Rack;
        public static ScrabbleDictionary Dictionary;
        public static Encoding Encoding = Encoding.GetEncoding("Windows-1250");
        public static string SaveFile = "save.txt";

        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Board = new Board(BoardSize, BoardSize);

                Rack = new List<Character>(7);

                Application.Run(new ScrabbleForm(Board));

                Rack.AddRange(Board.Rack);
            }
            catch (Exception ex)
            {
                Logger.AddLog(ex);
                if (!Board.Fresh)
                {
                    Snapshot.Save(Board.Fields, SaveFile, BoardSize);
                }
            }
        }
    }
}