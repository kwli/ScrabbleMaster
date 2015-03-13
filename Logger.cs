using System;
using System.IO;
using System.Text;

namespace ScrabbleMaster
{
    public class Logger
    {
        public static void AddLog(Exception ex)
        {
            AddLog(ex.Source);
            AddLog(ex.StackTrace);
            AddLog(ex.Message);
        }

        public static void AddLog(string pText)
        {
            try
            {
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now));
                sb.AppendLine(pText);
                File.AppendAllText(file, sb.ToString());
            }
            catch (Exception)
            {
                // something was wrong
            }
        }
    }
}
