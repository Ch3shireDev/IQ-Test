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

        private async Task StartTesting()
        {
            d1 = DateTime.Now;
            StartButton.IsEnabled = false;
            numTests++;
            if (numTests % 5 == 0)
            {
                DigitsCount.Value++;
                LabelB_Update(DigitsCount.Value);
            }
            Cyfry_TextBox.Focus();
            time = (int)TimeChange.Value;
            numDigits = (int)DigitsCount.Value;
            digitsList = TestCase(time, numDigits);
            await Task.Delay((int)TimeChange.Value * 1000);
            ListenForDigits();
        }

        private void BreakTesting()
        {
            StartButton.IsEnabled = true;
            allowDigits = false;
            numTests = 0;
        }

        private void SaveLists(List<int> listA, List<int> listB)
        {
            string path = "output.txt";
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
            var brush = Cyfry_TextBox.Foreground;
            if (x.Count == 0 && y.Count == 0)
            {
                Cyfry_TextBox.Foreground = Brushes.SpringGreen;
            }
            else
            {
                Cyfry_TextBox.Foreground = Brushes.Red;
            }
            await Task.Delay(500);
            Cyfry_TextBox.Foreground = brush;
            Cyfry_TextBox.Text = "";
            await Task.Delay(200);
            StartTesting();
        }

        private bool allowDigits = false;
        private int numCheckedDigits = 0;
        private List<int> digitsList;

        private void ListenForDigits()
        {
            Cyfry_TextBox.Text = "";
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
                Cyfry_TextBox.Text += n.ToString();
                if (givenDigits.Count < digitsList.Count)
                {
                    Cyfry_TextBox.Text += ", ";
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
            Cyfry_TextBox.Text = "";
            Cyfry_TextBox.FontSize = 48;
            List<int> Out = new List<int>();
            for (int i = 0; i < digits; i++)
            {
                Out.Add(numbersList[i]);
                Cyfry_TextBox.Text += numbersList[i];
                if (i < digits - 1) Cyfry_TextBox.Text += ", ";
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
            string s = e.Key.ToString();
            if (s.Length == 0) return;
            if (Int32.TryParse(s[s.Length - 1].ToString(), out var n))
            {
                SendDigit(n);
            }
        }
    }
}