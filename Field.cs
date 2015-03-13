namespace ScrabbleMaster
{
    public class Field
    {
        public int X;
        public int Y;
        public Character Content;
        public bool Definitive;
        public int DistH;
        public int DistV;

        public Field(int x, int y)
        {
            X = x;
            Y = y;
            Content = Character.EMPTY;
            Definitive = false;
            DistH = -1;
            DistV = -1;
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }

        public Bonus GetBonus()
        {
            if (((X == 0 || X == 14) && (Y == 2 || Y == 12)) || ((X == 2 || X == 12) && (Y == 0 || Y == 14)))
            {
                return Bonus.TRIPLE;
            }
            if (((X == 2 || X == 12) && (Y == 5 || Y == 9)) || ((X == 3 || X == 11) && (Y == 4 || Y == 10)) || ((X == 4 || X == 10) && (Y == 3 || Y == 11)) || ((X == 5 || X == 9) && (Y == 2 || Y == 12)))
            {
                return Bonus.DOUBLE;
            }
            if (((X == 0 || X == 7 || X == 14) && (Y == 0 || Y == 7 || Y == 14)) || ((X == 1 || X == 13) && (Y == 6 || Y == 8)) || ((X == 6 || X == 8) && (Y == 1 || Y == 13)))
            {
                return Bonus.RED;
            }
            if (((X == 5 || X == 9) && Y == 7) || ((X == 6 || X == 8) && (Y == 6 || Y == 8)) || (X == 7 && (Y == 5 || Y == 9)))
            {
                return Bonus.BLUE;
            }
            if (((X == 0 || X == 14) && (Y == 5 || Y == 9)) || ((X == 1 || X == 13) && (Y == 4 || Y == 10)) || ((X == 2 || X == 12) && (Y == 3 || Y == 11)) || ((X == 3 || X == 11) && (Y == 2 || Y == 12)) || ((X == 4 || X == 10) && (Y == 13 || Y == 1)) || ((X == 5 || X == 9) && (Y == 0 || Y == 14)))
            {
                return Bonus.GREEN;
            }
            if (((X == 2 || X == 12) && Y == 7) || ((X == 3 || X == 11) && (Y == 6 || Y == 8)) || ((X == 4 || X == 10) && (Y == 5 || Y == 9)) || ((X == 5 || X == 9) && (Y == 4 || Y == 10)) || ((X == 6 || X == 8) && (Y == 3 || Y == 11)) || (X == 7 && (Y == 2 || Y == 12)))
            {
                return Bonus.YELLOW;
            }
            return Bonus.EMPTY;
        }
    }
}
