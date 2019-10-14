using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using ImageGear.Formats;
using ImageGear.Evaluation;
using ImageGear.Formats.PDF;

namespace PdfMessageFilter
{
    class Program
    {
        /// <summary>Encode the binary digits to a string of Unicode
        /// characters (Zero-Width Joiner for 1 and Zero-Width Non-Joiner for 0).
        /// </summary>
        /// <param name="text">Any UTF-16 string.</param>
        /// <returns>The encoded message.</returns>
        private static String EncodeMessage(String text)
        {
            StringBuilder sb = new StringBuilder();
            // Convert String to Bytes - Little Endian
            byte[] data = Encoding.Unicode.GetBytes(text);

            foreach (byte b in data)
            {
                for (int i = 0; i < 8; ++i)
                {
                    bool bit = (b & (1 << i - 1)) != 0;
                    if (bit)
                    {
                        sb.Append('\u200D');
                    }
                    else
                    {
                        sb.Append('\u200C');
                    }
                }
            }
            return sb.ToString();
            //return "TOP SECRET";
        }


        /// <summary>
        /// Adds the Unicode string as the last text element on the first page of the PDF.
        /// </summary>
        /// <param name="pdfDocument">An opened PDF document.</param>
        /// <param name="text">Unicode text to inject.</param>
        private static void InjectMessage(ImGearPDFDocument pdfDocument, string text)
        {
            // Hint: https://help.accusoft.com/ImageGear-Net/v24.11/Windows/HTML/webframe.html#topic6633.html

            // Initialize current page attributes.
            const int pageWidth = (int)(8.5 * 300);
            const int pageHeight = 11 * 300;
            ImGearPDFFixedRect usLetterRectangle = new ImGearPDFFixedRect(0, 0, ImGearPDF.IntToFixed(pageWidth), ImGearPDF.IntToFixed(pageHeight));

            // Create new PDF page in document.
            //ImGearPDFPage igPdfPage = pdfDocument.CreateNewPage(-1, usLetterRectangle);

            // Get First Page of Document
            ImGearPDFPage igPdfPage = (ImGearPDFPage) pdfDocument.Pages[0];

            // This is current text position on the page.
            int xPosition = 20, yPosition = 40;

            // Type of encoding that will be used.
            const string encodingName = "Identity-H";
            ImGearPDFSysEncoding encoding = new ImGearPDFSysEncoding(new ImGearPDFAtom(encodingName));

            // Enumerate all system fonts to find one that can represent our text. Add to the page.
            ImGearPDFSysFont.Enumerate(new ImGearPDFSysFont.ImGearPDFSysFontEnumerate(
                delegate (ImGearPDFSysFont systemFont, object userData)
                {
            // Check the possibility to create font by system font with given encoding.
            ImGearPDEFontCreateFlags flags = systemFont.GetCreateFlags(encoding);
                    if (flags == ImGearPDEFontCreateFlags.CREATE_NOT_ALLOWED)
                    {
                        return true;
                    }

            // Check the possibility to create font for unicode text, embedding, and embedding only a subset.
            if ((flags & ImGearPDEFontCreateFlags.CREATE_TO_UNICODE) == 0 ||
                        (flags & ImGearPDEFontCreateFlags.CREATE_EMBEDDED) == 0 ||
                        (flags & ImGearPDEFontCreateFlags.CREATE_SUBSET) == 0)
                    {
                        return true;
                    }

            // Create the font by system font.
            using (ImGearPDEFont font = new ImGearPDEFont(systemFont, encoding, new ImGearPDFAtom(systemFont.Name.String), flags))
                    {
                // Check this font is able to represent given string.
                //if (font.IsTextRepresentable(text))
                        //{
                    // Get content that be able to fix the changes.
                    ImGearPDEContent pdfContent = igPdfPage.GetContent();

                            // Integrate text to the current page.
                            // ImGearPDEText textElement = CreateTextElement(text, font, 30, xPosition, pageHeight - yPosition, pageWidth - 40);
                            ImGearPDEText textElement = CreateTextElement(text, font, 30, 0, 0, pageWidth);
                            if (textElement != null)
                            {
                                Console.Write("Creating Text Element\n");
                                pdfContent.AddElement((int)ImGearPDEInsertElement.AFTER_LAST, textElement);
                                //textElement.Dispose();
                            }
                            else
                            {
                                Console.Write("Text Element Null\n");
                            }

                    // Fix the changes on the page and release content.
                    igPdfPage.SetContent();
                    igPdfPage.ReleaseContent();

                    // Integrate font to the document.
                    if (textElement != null)
                            {
                                font.CreateToUnicodeNow(pdfDocument);
                                font.EmbedNow(pdfDocument);
                                font.SubsetNow(pdfDocument);
                                return false;
                            }
                            textElement.Dispose();
                        }
                    //}
                    
                    return true;
                }), null);

            
            // Release current PDF page.
            igPdfPage.Dispose();

        }

        static ImGearPDEText CreateTextElement(string Text, ImGearPDEFont Font, Int32 FontSize, int x, int y, int w)
        {
            ImGearPDEText textElement = null;

            ImGearPDEColorSpace colorSpace =
                new ImGearPDEColorSpace(new ImGearPDFAtom("DeviceGray"));

            ImGearPDEGraphicState gState = new ImGearPDEGraphicState();
            gState.StrokeColorSpec.Space = colorSpace;
            gState.FillColorSpec.Space = colorSpace;
            gState.MiterLimit = (int)ImGearPDFFixedValues.TEN;
            gState.Flatness = (int)ImGearPDFFixedValues.ONE;
            gState.LineWidth = (int)ImGearPDFFixedValues.ONE;

            ImGearPDFFixedMatrix textMatrix = new ImGearPDFFixedMatrix();
            textMatrix.A = ImGearPDF.IntToFixed(FontSize);
            textMatrix.D = ImGearPDF.IntToFixed(FontSize);
            textMatrix.H = ImGearPDF.IntToFixed(x);
            textMatrix.V = ImGearPDF.IntToFixed(y);

            textElement = new ImGearPDEText();
            textElement.Add(ImGearPDETextFlags.RUN, 0, Text, Font, gState, null, textMatrix);

            colorSpace.Dispose();
            gState.Dispose();

            return textElement;
        }

        #region Command Line Processing


        private static void ShowUsage()
        {
            System.Console.WriteLine(@"
Usage: EncodePdfMessage [-h] [--version]
       EncodePdfMessage encode -i INPUT_PATH -o OUTPUT_PATH -m MESSAGE

Save a PDF containing a secret message.

Arguments:
  -h, --help            Show this help message and exit.
  --version             Show program's version number and exit.
  -i INPUT_PATH, --input-path INPUT_PATH
                        A path to a PDF file to use as input.
  -o INPUT_PATH, --output-path OUTPUT_PATH
                        A path to save the modified PDF file.
  -m MESSAGE, --message MESSAGE
                        The message to encode and embed.

Encoder Example:
    EncodePdfMessage.exe encode -i ""Dragon Boat 101.pdf"" -o ""Dragon Boat 101_armed.pdf"" -m ""Здраво Свете""

");
        }


        private static void ShowVersion()
        {
            Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
            String copyright = String.Empty;
            try
            {
                copyright = ", " + (assembly.GetCustomAttributes(
                    typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute).Copyright;
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine(ex.ToString());
            }
            String version = String.Format("{0} {1}.{2}.{3}{4}", new object[] {
                assembly.GetName().Name, assembly.GetName().Version.Major, assembly.GetName().Version.Minor,
                assembly.GetName().Version.Build, copyright});
            System.Console.WriteLine(version);
        }


        private enum ExitCode
        {
            CompletedSuccessfully = 0, ErrorEncountered = 1
        }


        private const int firstPage = 0;
        private static volatile ExitCode exitCode = ExitCode.ErrorEncountered;
        private static String inputPath = "";
        private static String outputPath = "";
        private static String message = "";
        private static String command = "";


        public static void Encode(String inputPdfPath, String outputPdfPath, String message)
        {
            if (message.Length == 0)
                throw new InvalidOperationException("The message must be at least one character.");

            String encodedMessage = EncodeMessage(message);

            // Open file for reading.
            using (FileStream pdfData = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                // Read PDF document to memory.
                using (ImGearPDFDocument igPdfDocument = (ImGearPDFDocument)ImGearFileFormats.LoadDocument(
                    pdfData, firstPage, (int)ImGearPDFPageRange.ALL_PAGES))
                {
                    // Package secret message.
                    InjectMessage(igPdfDocument, encodedMessage);

                    // Save PDF file.
                    igPdfDocument.Save(outputPath, ImGearSavingFormats.PDF, firstPage, firstPage,
                        igPdfDocument.Pages.Count, ImGearSavingModes.OVERWRITE);

                    exitCode = ExitCode.CompletedSuccessfully;
                }
            }
        }


        public static int Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            try
            {
                // Decode command line options.
                for (int i = 0; i < args.Length; i++)
                {
                    if (i == 0 && new List<String>() { "encode" }.Contains(args[i]))
                    {
                        command = args[i];
                    }
                    else if (args[i].Equals("-h") || args[i].Equals("--help"))
                    {
                        ShowUsage();
                        return (int)exitCode;
                    }
                    else if (args[i].Equals("--version"))
                    {
                        ShowVersion();
                        return (int)exitCode;
                    }
                    else if (args[i].Equals("-i") || args[i].Equals("--input-path"))
                    {
                        inputPath = args[i + 1];
                        i++;
                    }
                    else if (args[i].Equals("-o") || args[i].Equals("--output-path"))
                    {
                        outputPath = args[i + 1];
                        i++;
                    }
                    else if (args[i].Equals("-m") || args[i].Equals("--message"))
                    {
                        message = args[i + 1];
                        i++;
                    }
                    else
                    {
                        ShowUsage();
                        return (int)exitCode;
                    }
                }

                try
                {
                    // Initialize ImageGear once per process.
                    InitializeImageGear();

                    // Initialize ImageGear PDF once per thread.
                    InitializeImageGearPDF();

                    if (command == "encode")
                        Encode(inputPath, outputPath, message);
                    else
                        ShowUsage();
                }
                finally
                {
                    // Terminate ImageGear PDF once per thread.
                    TerminateImageGearPDF();
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine(ex.ToString());
                exitCode = ExitCode.ErrorEncountered;
            }

            return (int)exitCode;
        }


        private static void InitializeImageGear()
        {
            // Initialize IG.NET once per process.
            ImGearEvaluationManager.Initialize();
            ImGearEvaluationManager.Mode = ImGearEvaluationMode.Watermark;
            ImGearCommonFormats.Initialize();
            ImGearFileFormats.Filters.Insert(0, ImGearPDF.CreatePDFFormat());
        }


        private static void InitializeImageGearPDF()
        {
            ImGearPDF.Initialize();
            IImGearFormat pdfFormat = ImGearFileFormats.Filters.Get(ImGearFormats.PDF);
            pdfFormat.Parameters.GetByName("XFAAllowed").Value = true;
        }


        private static void TerminateImageGearPDF()
        {
            ImGearPDF.Terminate();
        }


        #endregion
    }
}
