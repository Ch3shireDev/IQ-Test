using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace IQ_Test_App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private int time = 0;
        private int numDigits = 0;

        private int numTests = 0;
        private DateTime d1;

        private void Undo()
        {
            if (givenDigits == null) return;
            if (!allowDigits) return;
            if (givenDigits.Count > 0)
            {
                givenDigits.RemoveAt(givenDigits.Count - 1);
            }
            Digits_TextBox.Text = "";
            for (int i = 0; i < givenDigits.Count; i++)
            {
                Digits_TextBox.Text += givenDigits[i].ToString();
                Digits_TextBox.Text += ", ";
            }
        }

        private async Task StartTesting()
        {
            time = (int)TimeChange.Value;
            numDigits = (int)DigitsCount.Value;
            System.Diagnostics.Debug.WriteLine(numDigits);
            d1 = DateTime.Now;
            StartButton.IsEnabled = false;
            if (progressive_CheckBox.IsChecked is true)
            {
                if (numTests % 5 == 0 && numTests != 0)
                {
                    DigitsCount.Value++;
                    LabelB_Update(DigitsCount.Value);
                    numDigits = (int)DigitsCount.Value;
                }
                if (numDigits < 8)
                {
                    Digits_TextBox.FontSize = 48;
                }
                else if (numDigits < 12)
                {
                    Digits_TextBox.FontSize = 36;
                }

            }
            Digits_TextBox.Focus();

            digitsList = TestCase(time, numDigits);
            await Task.Delay((int)TimeChange.Value * 1000);
            ListenForDigits();
        }

        private void BreakTesting()
        {
            StartButton.IsEnabled = true;
            allowDigits = false;
            numTests = 0;
            Digits_TextBox.Text = "";
        }

        private void SaveLists(List<int> listA, List<int> listB)
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Appdata\\Local\\IQ";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string path = dir + "\\output.txt";
            StreamWriter output = new StreamWriter(path, true);
            output.Write(DateTime.Now + " " + time + " " + numDigits + " [");
            foreach (var x in listA)
            {
                output.Write(x.ToString() + " ");
            }
            output.Write("] [");
            foreach (var x in listB)
            {
                output.Write(x.ToString() + " ");
            }
            output.WriteLine("] " + interval);
            output.Close();
        }

        private double interval = 0;

        private async Task EndTesting()
        {
            DateTime d2 = DateTime.Now;
            interval = d2.Subtract(d1).TotalMilliseconds;
            SaveLists(digitsList, givenDigits);
            allowDigits = false;
            var x = digitsList.Except(givenDigits).ToList();
            var y = givenDigits.Except(digitsList).ToList();
            var brush = Digits_TextBox.Foreground;
            if (x.Count == 0 && y.Count == 0)
            {
                Digits_TextBox.Foreground = Brushes.SpringGreen;
                numTests++;
            }
            else
            {
                Digits_TextBox.Foreground = Brushes.Red;
            }
            await Task.Delay(500);
            Digits_TextBox.Foreground = brush;
            Digits_TextBox.Text = "";
            await Task.Delay(200);
            StartTesting();
        }

        private bool allowDigits = false;
        private int numCheckedDigits = 0;
        private List<int> digitsList;

        private void ListenForDigits()
        {
            Digits_TextBox.Text = "";
            allowDigits = true;
            givenDigits = new List<int>();
            numCheckedDigits = 0;
        }

        private List<int> givenDigits;

        private void SendDigit(int n)
        {
            if (!allowDigits) return;
            if (givenDigits.Count < digitsList.Count)
            {
                givenDigits.Add(n);
                Digits_TextBox.Text += n.ToString();
                if (givenDigits.Count < digitsList.Count)
                {
                    Digits_TextBox.Text += ", ";
                }
                else
                {
                    EndTesting();
                }
            }
        }

        private void TimeChange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LabelA_Update(TimeChange.Value);
        }

        private void Numbers_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LabelB_Update(DigitsCount.Value);
        }

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            StartTesting();
        }

        private string secondsCountLabel;
        private string digitsCountLabel;

        private void LabelA_Update(double value)
        {
            if (secondsCountLabel == null)
            {
                secondsCountLabel = LabelA.Content.ToString();
            }
            LabelA.Content = secondsCountLabel + " " + value + " s";
        }

        private void LabelB_Update(double value)
        {
            if (digitsCountLabel == null)
            {
                digitsCountLabel = DigitLabel.Content.ToString();
            }
            DigitLabel.Content = digitsCountLabel + " " + value;
        }

        private List<int> LastNumbers = new List<int>();

        private bool CheckForDifference(List<int> before, List<int> after, int num)
        {
            for (int i = 1; i < num; i++)
            {
                if (Math.Abs(after[i] - after[i - 1]) < 2) return false;
            }

            HashSet<int> Set = new HashSet<int>();
            for (int i = 0; i < num; i++) Set.Add(after[i]);
            int count = Set.Count;
            if (num < 8 && num - count > 0) return false;

            if (LastNumbers.Count < num) return true;
            for (int i = 0; i < num; i++)
            {
                if (before[i] == after[i]) return false;
            }
            return true;
        }

        private List<int> TestCase(int seconds, int digits)
        {
            List<int> numbersList = new List<int>();
            for (int i = 0; i < 20; i++) numbersList.Add(i % 10);
            do numbersList = numbersList.OrderBy(a => Guid.NewGuid()).ToList();
            while (!CheckForDifference(LastNumbers, numbersList, digits));
            Digits_TextBox.Text = "";
            List<int> Out = new List<int>();
            for (int i = 0; i < digits; i++)
            {
                Out.Add(numbersList[i]);
                Digits_TextBox.Text += numbersList[i];
                if (i < digits - 1) Digits_TextBox.Text += ", ";
            }
            LastNumbers = new List<int>(numbersList);
            return Out;
        }

        private void GetDigit(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                BreakTesting();
            }
            if (e.Key == Key.Back)
            {
                System.Diagnostics.Debug.WriteLine("aaa");
                Undo();
            }
            string s = e.Key.ToString();
            if (s.Length == 0) return;
            if (Int32.TryParse(s[s.Length - 1].ToString(), out var n))
            {
                SendDigit(n);
            }
        }
    }
}