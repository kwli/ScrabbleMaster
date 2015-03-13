using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;

namespace ScrabbleMaster
{
    public partial class ScrabbleForm : Form
    {
        private Board _board;

        private int _currentActive = -1;

        public static ScrabbleDictionary Dictionary;
        private List<KeyValuePair<Word, int>> _words;
        private byte[] _currentWord;
        private string _originalRack;

        public ScrabbleForm(Board board)
        {
            _board = board;
            InitializeComponent();
            Dictionary = new ScrabbleDictionary("slowa-win.txt", 35924528);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs mea = (MouseEventArgs)e;
            if (mea.Button == MouseButtons.Left)
            {
                if (_currentActive != -1)
                {
                    pictureBox1.Image = _board.FieldLocated(mea, ref textBox1, _currentActive);
                    pictureBox2.Image = _board.HandleTextBox(ref textBox1);
                    _currentActive = -1;
                }
            }
            else
            {
                Form prompt = new Form() { Width = 500, Height = 150, FormBorderStyle = FormBorderStyle.FixedDialog, Text = "Wprowadź słowo", StartPosition = FormStartPosition.CenterScreen };
                
                Label textLabel = new Label() { Left = 50, Top = 20, Text = "Słowo: " };
                Label cbLabel = new Label() { Left = 150, Top = 20, Text = "Poziomo: " };
                CheckBox cb = new CheckBox() { Left = 250, Top = 20, Checked = true };
                TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
                Button confirmation = new Button() { Text = "OK", Left = 350, Width = 100, Top = 70 };
                confirmation.Click += (s, ev) => { prompt.Close(); };

                prompt.Controls.Add(cb);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(cbLabel);
                prompt.AcceptButton = confirmation;
                textBox.Focus();
                prompt.ShowDialog();

                byte[] word = Scrabble.Encoding.GetBytes(textBox.Text.ToLower());

                int x = mea.X / Board.FIELD_SIZE;
                int y = mea.Y / Board.FIELD_SIZE;

                try
                {
                    for (int i = 0; i < word.Length; i++)
                    {
                        Field field = cb.Checked ? _board.Fields[x + i, y] : _board.Fields[x, y + i];
                        field.Content = (Character)word[i];
                        field.Definitive = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.AddLog(ex);
                }
                pictureBox1.Image = _board.Draw();
                FinishWordProcessing();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            pictureBox2.Image = _board.HandleTextBox(ref textBox1);
        }

        private void pictureBox2_Click(object sender, System.EventArgs e)
        {
            pictureBox2.Image = _board.HandleRackClick((MouseEventArgs)e, textBox1, ref _currentActive);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _originalRack = textBox1.Text;
            listView1.Clear();
            listView1.Columns.Clear();
            listView1.View = View.Details;
            listView1.Columns.Add("Slowo", 170);
            listView1.Columns.Add("Punkty", 70);
            listView1.Columns.Add("Początek", 60);
            listView1.Columns.Add("Kierunek", 50);
            _words = _board.Fresh ? _board.FirstMovement() : _board.GenerateMovement();

            if (_words.Count > 1)
            {
                foreach (KeyValuePair<Word, int> kvp in _words)
                {
                    listView1.Items.Add(
                        new ListViewItem(new[]
                        {
                            kvp.Key.Text, kvp.Value + "", kvp.Key.Start.ToString(),
                            kvp.Key.Horizontal ? Encoding.UTF8.GetString(new byte[]{0xE2, 0x86, 0x92}) : Encoding.UTF8.GetString(new byte[]{0xE2, 0x86, 0x93})}));
                }

                label1.Text = String.Format("({0}):", _words.Count());
                listView1.Items[0].Selected = true;
                listView1.Select();
                if (_originalRack.IndexOf('*') == -1)
                {
                    HandleProposal(_words.First().Key);
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (_currentWord != null)
                {
                    textBox1.Text = Scrabble.Encoding.GetString(_currentWord);
                }
                HandleProposal(_words[listView1.SelectedItems[0].Index].Key);
                button1.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                _board.Locate(_words[listView1.SelectedItems[0].Index].Key);
                pictureBox1.Image = _board.Draw();
                FinishWordProcessing();
            }
        }

        private void HandleProposal(Word word)
        {
            try
            {
                _currentWord = _board.Proposal(word, textBox1.Text);
                pictureBox1.Image = _board.Draw();
                textBox1.Text = Scrabble.Encoding.GetString(_currentWord);
                pictureBox2.Image = _board.HandleTextBox(ref textBox1);
                _currentWord = _board.Cancel(word, _currentWord);
            }
            catch (Exception ex)
            {
                Logger.AddLog(ex);
            }
        }


        private void ScrabbleForm_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = _board.Draw();
            pictureBox2.Image = _board.DrawEmptyRack();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                _board = Snapshot.Load(Scrabble.SaveFile, 255, Scrabble.BoardSize);
            }
            catch (Exception ex)
            {

            }
            pictureBox1.Image = _board.Draw();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Snapshot.Save(_board.Fields, Scrabble.SaveFile, Scrabble.BoardSize);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FinishWordProcessing();
        }

        private void FinishWordProcessing()
        {
            _currentWord = null;
            listView1.Items.Clear();
            _words = null;
            button1.Enabled = true;
        }
    }
}