using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace ConvertCurrencyToWords
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private static readonly Regex _regex = new Regex("[^0-9,]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
         
        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            string strNumberIn = txtboxNumber.Text;
            int numberOfDecimals = 0;
            try
            {
                bool b = double.TryParse(strNumberIn, out double NumberIn);
                if (b)
                {
                    if (strNumberIn.Contains(".") || strNumberIn.Contains("-"))//
                    {
                        //throw new System.ArgumentException("All characters must be digits. No negative numbers and no commas. Use periods to separate dollars and cents.", strNumberIn.ToString());
                        throw new System.ArgumentException($"All characters must be digits. No negative numbers and no periods. " +
                                                    $"Use commas to separate dollars and cents.", strNumberIn.ToString());
                    }
                    int decimalPlace = strNumberIn.IndexOf(",");
                    if (decimalPlace == -1)  // there is NO decimal place here
                    {
                        numberOfDecimals = 0;
                    }
                    else
                    {   // Yes we have a decimal place
                        numberOfDecimals = strNumberIn.Length - decimalPlace - 1;
                    }
                    if (numberOfDecimals < 3)
                    {
                        // Must remove any leading zeros.
                        strNumberIn = strNumberIn.TrimStart(new Char[] { '0' });
                        txtblkWords.Text = ConvertToWords(strNumberIn);
                    }
                    else
                    {
                        throw new System.ArgumentException("Maximum of two decimal places.", strNumberIn.ToString());
                    }
                }
                else
                {
                    throw new System.ArgumentException("All characters must be digits. No commas, no negative numbers, no spaces and no other punctuation such as currency symbols.", strNumberIn.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Convert Currency to Words");
            }
        }
        private static string ConvertToWords(string strNumberIn)
        {
            string strConverted = "", strWholeNumber = "", strFractionalNumber = "", strDollars = "Dollars", pointStr = "", strCents = "";
            try
            {
                int decimalPlace = strNumberIn.IndexOf(",");
                if (decimalPlace >= 0)
                {
                    strWholeNumber = strNumberIn.Substring(0, decimalPlace);

                    strFractionalNumber = strNumberIn.Substring(decimalPlace + 1);

                    strDollars = strDollars + " and";
                    strCents = "Cents" + strCents;
                    pointStr = ConvertDecimals(strFractionalNumber);
                }
                else
                {
                    // no decimal place given therefore just do whole number no cents.
                    strWholeNumber = strNumberIn;
                }
                strConverted = string.Format("{0} {1}{2} {3}",
                    ConvertWholeNumber(strWholeNumber).Trim(), strDollars, pointStr, strCents);
            }
            catch { }
            return strConverted;
        }
        private static string ConvertDecimals(string strFractionalNumber)
        {
            // strFractionalNumber is everything after the decimal point (cents)
            string engOne = "", cd = "";
            if (strFractionalNumber.Length == 0)
            {
                engOne = "Zero";
            }
            if (strFractionalNumber.Length == 1)
            {
                if (strFractionalNumber == "0")
                {   // they put "?.0"
                    engOne = "Zero";
                }
                else
                {   // they put "?.1" or "?.2" or "?.3" or and so on
                    strFractionalNumber = strFractionalNumber + "0";
                    engOne = ConvertWholeNumber(strFractionalNumber);
                }
            }
            if (strFractionalNumber.Length == 2)
            {
                if (strFractionalNumber == "00")
                {
                    engOne = "Zero";
                }
                else
                {
                    engOne = ConvertWholeNumber(strFractionalNumber);
                }
            }
            cd += " " + engOne;
            return cd;
        }
        private static string ConvertWholeNumber(string Number)
        {
            string word = "";

            try
            {
                bool isDone = false;  //test if already translated 
                //if (Number == "") Number = "0"; code smell here.

                bool b = Double.TryParse(Number, out double ddd);
                if (!b) return "Zero"; // we got empty string so zero dollars

                double dblAmt = Convert.ToDouble(Number);

                if (Number != "")
                {
                    if (dblAmt > 0)
                    {
                        int numDigits = dblAmt.ToString().Length;
                        Number = dblAmt.ToString();
                        int pos = 0;  //store digit grouping    
                        string place = "";  //digit grouping name:hundres,thousand,etc...    
                        switch (numDigits)
                        {
                            case 1://ones' range    
                                word = ones(Number); isDone = true; break;
                            case 2://tens' range    
                                word = tens(Number); isDone = true; break;
                            case 3://hundreds' range    
                                pos = (numDigits % 3) + 1; place = " Hundred "; break;
                            case 4://thousands' range    
                            case 5:
                            case 6: pos = (numDigits % 4) + 1; place = " Thousand "; break;
                            case 7://millions' range    
                            case 8:
                            case 9: pos = (numDigits % 7) + 1; place = " Million "; break;
                            case 10://Billions's range    
                            case 11:
                            case 12: pos = (numDigits % 10) + 1; place = " Billion "; break;
                            case 13: // Trillions range
                            case 14:
                            case 15: pos = (numDigits % 13) + 1; place = " Trillion "; break;
                            case 16: // Quadrillion range
                            case 17:
                            case 18: pos = (numDigits % 16) + 1; place = " Quadrillion "; break;
                            case 19: // Quintillion range
                            case 20:
                            case 21: pos = (numDigits % 19) + 1; place = " Quintillion "; break;
                            default: isDone = true; break;
                        }
                        if (!isDone)
                        {   // if transalation is not done, continue...(Recursion comes in now!!)    
                            if (Number.Substring(0, pos) != "0" && Number.Substring(pos) != "0")
                            {
                                try
                                {
                                    word = ConvertWholeNumber(Number.Substring(0, pos)) +
                                        place + ConvertWholeNumber(Number.Substring(pos));
                                }
                                catch { }
                            }
                            else
                            {
                                word = ConvertWholeNumber(Number.Substring(0, pos)) +
                                    ConvertWholeNumber(Number.Substring(pos));
                            }
                        }
                        //ignore digit grouping names    
                        if (word.Trim().Equals(place.Trim())) word = "";
                    }
                }
            }
            // 
            catch { } // Error in form here. Code smell.

            return word.Trim();
        }
        private static string ones(string Number)
        {
            int _Number = Convert.ToInt32(Number);
            string name = "";
            switch (_Number)
            {
                //case 0: name = "Zero"; break; // ??????? added
                case 1: name = "One"; break;
                case 2: name = "Two"; break;
                case 3: name = "Three"; break;
                case 4: name = "Four"; break;
                case 5: name = "Five"; break;
                case 6: name = "Six"; break;
                case 7: name = "Seven"; break;
                case 8: name = "Eight"; break;
                case 9: name = "Nine"; break;
            }
            return name;
        }
        private static string tens(string Number)
        {
            int intNumber = Convert.ToInt32(Number);
            string name = null;
            switch (intNumber)
            {
                //case 0: name = "Zero"; break; this doesn't work
                case 10: name = "Ten"; break;
                case 11: name = "Eleven"; break;
                case 12: name = "Twelve"; break;
                case 13: name = "Thirteen"; break;
                case 14: name = "Fourteen"; break;
                case 15: name = "Fifteen"; break;
                case 16: name = "Sixteen"; break;
                case 17: name = "Seventeen"; break;
                case 18: name = "Eighteen"; break;
                case 19: name = "Nineteen"; break;
                case 20: name = "Twenty"; break;
                case 30: name = "Thirty"; break;
                case 40: name = "Fourty"; break;
                case 50: name = "Fifty"; break;
                case 60: name = "Sixty"; break;
                case 70: name = "Seventy"; break;
                case 80: name = "Eighty"; break;
                case 90: name = "Ninety"; break;
                default:
                    if (intNumber > 0)
                    {
                        name = tens(Number.Substring(0, 1) + "0") + " "
                            + ones(Number.Substring(1));
                    }
                    break;
            }
            return name;
        }

        private void TxtblkWords_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

        }

        private void PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
    }
}