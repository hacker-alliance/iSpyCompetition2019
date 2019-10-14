using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using ImageGear.Formats;
using ImageGear.Core;
using ImageGear.Evaluation;

namespace Printer_Identification_Code
{
    /// <summary>
    /// Creates a CMYK colorspace instance
    /// from an 8 bit integer RGB colorspace input
    /// </summary>
    internal class CMYKColor
    {
        public float cyan;
        public float magenta;
        public float yellow;
        public float key;

        internal CMYKColor(int red, int green, int blue)
        {
            // determine maximum rgb value
            float max = Math.Max(red, Math.Max(green, blue));

            // avoid possible division by zero
            max += float.Epsilon;

            // convert to CMYK
            key = 1 - max;
            cyan = (1 - red - key) / max;
            magenta = (1 - green - key) / max;
            yellow = (1 - blue - key) / max;
        }

    }

    /// <summary>
    /// Driver class for 
    /// the decoding solution
    /// </summary>
    internal class Program
    {
        // fetch full filepath
        private const string micInstanceFileName = "instance.png";
        private static string micInstanceRelativePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, micInstanceFileName);
        private static string micInstancePath = Path.GetFullPath(micInstanceRelativePath);

        // pattern spacings
        private const int patternOffset = 75;
        private const int encodingWidth = 100;

        // row keys
        static private readonly List<int> serialRows = new List<int>(Enumerable.Range(3, 3));
        static private readonly List<int> yearRows = new List<int>(Enumerable.Range(8, 1));
        static private readonly List<int> monthRows = new List<int>(Enumerable.Range(9, 1));
        static private readonly List<int> dayRows = new List<int>(Enumerable.Range(10, 1));
        static private readonly List<int> hourRows = new List<int>(Enumerable.Range(11, 1));
        static private readonly List<int> minuteRows = new List<int>(Enumerable.Range(14, 1));

        // answer string for console output
        private const string answerString = "Document printed by printer `{0}` on `{1}/{2}/20{3}` at `{4}:{5}`";

        /// <summary>
        /// Handles initialization of ImageGear utilities
        /// </summary>
        private static void InitializeImageGear()
        {
            ImGearEvaluationManager.Initialize();
            ImGearCommonFormats.Initialize();
        }

        /// <summary>
        /// Converts encoded row sums 
        /// corresponding to input answerRows 
        /// into concatenated answer strings
        /// </summary>
        private static string StringifyAnswer(Dictionary<int, int> rowSumDict, List<int> answerRows)
        {
            string answer = null;

            foreach (int row in answerRows)
            {
                rowSumDict.TryGetValue(row, out int rowValue);
                answer += rowValue.ToString();
            }
            return answer;
        }

        /// <summary>
        /// Handles filesystem interaction to
        /// load a filepath into an ImGearPage instance
        /// </summary>
        private static ImGearPage LoadPageFromLocalFile(string filePath)
        {
            // From a local file
            ImGearPage igPage;
            int pageIndex = 0;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // This remainder of this function will be left for contestants to implement
            //
            // Hint:
            // Set igPage variable with ImGearPage instance
            // https://help.accusoft.com/ImageGear-Net/v24.11/Windows/HTML/webframe.html#topic582.html
            //
            // contestant implementation ------------------------------------------------------------------------------
            using (FileStream localFile = new FileStream(filePath, FileMode.Open))
            {
                igPage = ImGearFileFormats.LoadPage(localFile, pageIndex);
            }
            // end contestant implementation --------------------------------------------------------------------------
            /////////////////////////////////////////////////////////////////////////////////////////////////////////

            return igPage;
        }

        /// <summary>
        /// Sets the thresholding for whether
        /// the yellow in a CMYK colorspace pixel 
        /// is considered yellow enough to be a yellow-dot
        /// </summary>
        private static bool YellowThreshold(float yellow)
        {
            double threshold;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // This remainder of this function will be left for contestants to implement
            //
            // Hint:
            // Determine and set acceptable threshold value in [0,1]
            //
            // contestant implementation ------------------------------------------------------------------------------
            threshold = 0.5;
            // end contestant implementation --------------------------------------------------------------------------
            /////////////////////////////////////////////////////////////////////////////////////////////////////////

            return yellow > threshold;
        }

        /// <summary>
        /// Determines whether a pixel
        /// instance is a yellow-dot
        /// </summary>
        private static bool IsYellowDot(ImGearDIB igDIB, int x_position, int y_position)
        {
            const int redChannel = 0;
            const int greenChannel = 1;
            const int blueChannel = 2;

            int red, green, blue;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // This remainder of this function will be left for contestants to implement
            //
            // Hint:
            // Gets a pixel from the ImGearDIB instance.
            // https://help.accusoft.com/ImageGear-Net/v24.11/Windows/HTML/webframe.html#topic3854.html
            // Sets variables red, green, and blue with their appropriate values from ImGearPixel instance.
            // https://help.accusoft.com/ImageGear-Net/v24.11/Windows/HTML/webframe.html#topic4189.html
            //
            // contestant implementation ------------------------------------------------------------------------------
            ImGearPixel p = igDIB.GetPixelCopy(x_position, y_position);
            red = p[0];
            green = p[1];
            blue = p[2];
            // end contestant implementation --------------------------------------------------------------------------
            /////////////////////////////////////////////////////////////////////////////////////////////////////////

            CMYKColor cmyk = new CMYKColor(red, green, blue);

            return YellowThreshold(cmyk.yellow);
        }

        /// <summary>
        /// Determines whether a grid coordinate
        /// is relevant to our decoding process.
        /// Coordinates defining parity or that
        /// do not correspond to valuable rows
        /// are not worth storing (updating).
        /// </summary>
        private static bool IsUpdateableCoordinate(int rowValue, int columnValue)
        {
            // define parity column
            const int parityColumn = 1;

            // combine and unroll lists
            List<int>[] rowsList = { serialRows, yearRows, monthRows, dayRows, hourRows, minuteRows };
            List<int> answerRowsList = new List<List<int>>(rowsList).SelectMany(i => i).ToList<int>();

            bool isCoordinateToUpdate = false;
            bool isParityColumn = false;
            bool isAnswerRow = false;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // This remainder of this function will be left for contestants to implement
            //
            // Hint:
            // Determine and set appropriate value for isParityColumn and isAnswerRow
            // Use values for isParityColumn and isAnswerRow to set isCoordinateToUpdate accordingly
            // https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.contains?view=netframework-4.6.1
            //
            // contestant implementation ------------------------------------------------------------------------------
            if (answerRowsList.contains(rowValue))
            {
                isCoordinateToUpdate = true;
            }

            if (columnValue == 0)
            {
                isCoordinateToUpdate = false;
            }
            // end contestant implementation --------------------------------------------------------------------------
            /////////////////////////////////////////////////////////////////////////////////////////////////////////

            return isCoordinateToUpdate;
        }

        /// <summary>
        /// Handles updates to dictionary
        /// containing current values for each row
        /// in the encoding
        /// </summary>
        private static void UpdateValueDict(ref Dictionary<int, int> valueDict,
            int rowValue, int columnValue, int totalColumns)
        {
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // This remainder of this function will be left for contestants to implement
            //
            // Hint:
            // Update valueDict with appropriate value
            // Refer to "Encoding Method" section of README.md
            // https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.trygetvalue?view=netframework-4.6.1#System_Collections_Generic_Dictionary_2_TryGetValue__0__1__
            // https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.item?view=netframework-4.6.1#System_Collections_Generic_Dictionary_2_Item__0_
            // 
            // contestant implementation ------------------------------------------------------------------------------
            int value;
            if (valueDict.TryGetValue(rowValue, out value))
            {
                valueDict.Add(rowValue, value + 2^(columnValue - 1));
            }


            // end contestant implementation --------------------------------------------------------------------------
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
        }

        /// <summary>
        /// Driver function for the
        /// decoding solution
        /// </summary>
        private static void Main()
        {
            // Initialize ImageGear.
            InitializeImageGear();

            // load image
            ImGearPage igPage = LoadPageFromLocalFile(micInstancePath);
            // get image dib
            ImGearDIB igDIB = igPage.DIB;

            // get image dimensions
            int height = igDIB.Height;
            int width = igDIB.Width;

            // get table dimensions
            int totalRows = height / encodingWidth;
            int totalColumns = width / encodingWidth;

            // storage for image's row values
            Dictionary<int, int> rowSumDict = new Dictionary<int, int>();

            // step through pixel space
            foreach (int x_position in MoreEnumerable.Sequence(patternOffset, width, encodingWidth))
            {
                foreach (int y_position in MoreEnumerable.Sequence(patternOffset, height, encodingWidth))
                {
                    if (IsYellowDot(igDIB, x_position, y_position))
                    {
                        // define table coordinates
                        int columnValue = (x_position - patternOffset) / encodingWidth + 1;
                        int rowValue = (y_position - patternOffset) / encodingWidth + 1;

                        if (IsUpdateableCoordinate(rowValue, columnValue))
                        {
                            UpdateValueDict(ref rowSumDict, rowValue, columnValue, totalColumns);
                        }
                    }
                }
            }

            // create answer strings for output
            string serialAnswer = StringifyAnswer(rowSumDict, serialRows);
            string yearAnswer = StringifyAnswer(rowSumDict, yearRows);
            string monthAnswer = StringifyAnswer(rowSumDict, monthRows);
            string dayAnswer = StringifyAnswer(rowSumDict, dayRows);
            string hourAnswer = StringifyAnswer(rowSumDict, hourRows);
            string minuteAnswer = StringifyAnswer(rowSumDict, minuteRows);

            Console.WriteLine(string.Format(answerString, serialAnswer, monthAnswer, dayAnswer, yearAnswer, hourAnswer, minuteAnswer));
            Console.ReadKey();
        }
    }
}
