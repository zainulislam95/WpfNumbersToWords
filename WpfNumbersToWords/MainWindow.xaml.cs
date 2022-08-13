using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ConvertCurrencyToWords
{
    public partial class MainWindow : Window
    {
        #region On Load

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Check Valid Currency

        //RegularExpression that accepts number and commas only
        private static readonly Regex _regex = new Regex("[^0-9,]+");
         
        // Summary:
        //     Checks if the input value matches provideds RegularExpression. 
        //
        // Parameters:
        //   text:
        //     A string containing a number to match with RegularExpression.
        //  
        // Returns:
        //     true if text when input value doesn't match; otherwise, false.
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        #endregion

        private void BtnConvert_Click(object sender, RoutedEventArgs args)
        {
            string strNumberIn = txtboxNumber.Text;
            int numberOfDecimals = 0;
            try
            {
                bool isDouble = double.TryParse(strNumberIn, out double NumberIn);
                if (isDouble)
                {
                    if (strNumberIn.Contains(".") || strNumberIn.Contains("-"))//
                    { 
                        throw new ArgumentException($"All characters must be digits. No negative numbers and no periods. " +
                                                    $"Use commas to separate dollars and cents.", strNumberIn.ToString());
                    }

                    int decimalPlace = strNumberIn.IndexOf(",");
                    
                    // input value contians NO decimal value
                    if (decimalPlace == -1)  
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
                        throw new ArgumentException("Maximum of two decimal places.", strNumberIn.ToString());
                    }
                }
                else
                {
                    throw new ArgumentException($"All characters must be digits. No commas, no negative numbers, " +
                        $"no spaces and no other punctuation such as currency symbols.", strNumberIn.ToString());
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return strConverted;
        }

        // strFractionalNumber is everything after the decimal point (cents) 
        private static string ConvertDecimals(string strFractionalNumber)
        {
            try
            {
                string engOne = "", convertDecimal = "";
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
                convertDecimal += " " + engOne;
                return convertDecimal;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            } 
        }
        
        private static string ConvertWholeNumber(string number)
        {
            try
            {
                string word = ""; 
                bool isDone = false;  //test if already translated   
                bool isDouble = double.TryParse(number, out double ddd);
                if (!isDouble) return "Zero"; // we got empty string so zero dollars 
                double doubleAmount = Convert.ToDouble(number);

                if (number != "")
                {
                    if (doubleAmount > 0)
                    {
                        int numDigits = doubleAmount.ToString().Length;
                        number = doubleAmount.ToString();
                        int position = 0; //store digit grouping    
                        string place = ""; //digit grouping name:hundres,thousand,etc...    
                        switch (numDigits)
                        {
                            case 1://ones' range    
                                word = Ones(number); isDone = true; break;
                            case 2://tens' range    
                                word = Tens(number); isDone = true; break;
                            case 3://hundreds' range    
                                position = (numDigits % 3) + 1; place = " Hundred "; break;
                            case 4://thousands' range    
                            case 5:
                            case 6: position = (numDigits % 4) + 1; place = " Thousand "; break;
                            case 7://millions' range    
                            case 8:
                            case 9: position = (numDigits % 7) + 1; place = " Million "; break;
                            case 10://Billions's range    
                            case 11:
                            case 12: position = (numDigits % 10) + 1; place = " Billion "; break;
                            case 13: // Trillions range
                            case 14:
                            case 15: position = (numDigits % 13) + 1; place = " Trillion "; break;
                            case 16: // Quadrillion range
                            case 17:
                            case 18: position = (numDigits % 16) + 1; place = " Quadrillion "; break;
                            case 19: // Quintillion range
                            case 20:
                            case 21: position = (numDigits % 19) + 1; place = " Quintillion "; break;
                            default: isDone = true; break;
                        }
                        if (!isDone)
                        {   
                            // if transalation is not done, continue...(Recursion comes in now!!)    
                            if (number.Substring(0, position) != "0" && number.Substring(position) != "0")
                            {
                                word = ConvertWholeNumber(number.Substring(0, position)) +
                                        place + ConvertWholeNumber(number.Substring(position));
                            }
                            else
                            {
                                word = ConvertWholeNumber(number.Substring(0, position)) +
                                    ConvertWholeNumber(number.Substring(position));
                            }
                        }
                        //ignore digit grouping names    
                        if (word.Trim().Equals(place.Trim())) word = "";
                    }
                }
                return word.Trim(); 
            } 
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            } 
        }
        
        private static string Ones(string number)
        {
            try
            {
                int _Number = Convert.ToInt32(number);
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            } 
        }
         
        private static string Tens(string number)
        {
            try
            {
                int intNumber = Convert.ToInt32(number);
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
                            name = Tens(number.Substring(0, 1) + "0") + " "
                                + Ones(number.Substring(1));
                        }
                        break;
                }
                return name;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            } 
        } 

        private void PreviewTextInput(object sender, TextCompositionEventArgs args)
        {
            try
            {
                args.Handled = !IsTextAllowed(args.Text); 
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}