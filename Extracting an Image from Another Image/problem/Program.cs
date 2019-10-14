using ImageGear.Core;
using ImageGear.Formats;
using System.IO;

namespace imagegear_stenography
{

    /**
     * Complete the function: ExtractHiddenImage()
     */
    class Program
    {
        private static string mergedImageFilepath = "merged.png";
        private static string recoveredImageFilepath = "recovered.png";

        private static void InitImageGear()
        {
            ImGearLicense.SetSolutionName("Accusoft");
            ImGearLicense.SetSolutionKey(0x00000000, 0x00000000, 0x00000000, 0x00000000);
            ImGearLicense.SetOEMLicenseKey("2.0.E6dKRyMXM7REMwJuP0JK2YUXG7RCFaiSiQUY4EiukyGEkQ2aJuFwsCRjGCU9RacudaMaMCGjHYFEMCducEFQP0GScSUjkYFuPKiuG7sw57iQc0i9k72aPjcjHXJKR7UQPCsQ20iSiEPQUYdjJKRCH9FKMyJSPYdSF9kyUKdQdKJQHYP7dYJY5udy26MwWY5CdQGQFSPY4QsuUyiS50ij2a2aGCGj5KPuHE4QUuM7GwcQJXMwJ7FukX2QMaRXdyMYP9HykKJusCkwUQc9MKME46c9HCd9GaJQPK5XRaP9M0MKPa2aJudKk9Fjd9U94EGuPEsyP9GjsXFQJXHC5QRQUSdSiy26s6cX4yijRCFEJ7JwF0suk75aRCRYGUY");

            ImGearCommonFormats.Initialize();
        }

        /**
         * Program entry point.
         */
        static void Main(string[] args)
        {
            InitImageGear();

            using (Stream mergedStream = new FileStream(mergedImageFilepath, FileMode.Open))
            using (Stream recoveredStream = new FileStream(recoveredImageFilepath, FileMode.Create))
            {
                // load merged image
                ImGearPage mergedImage = ImGearFileFormats.LoadPage(mergedStream);

                // extract hidden image
                ImGearPage recoveredImage = ExtractHiddenImage(mergedImage);

                ImGearFileFormats.SavePage(recoveredImage, recoveredStream, ImGearSavingFormats.PNG);
            }
        }
         //This function should return an ImGearPage that contains the extracted hidden image
        public static ImGearPage ExtractHiddenImage(ImGearPage mergedImage)
        {
            int bitNum = 1;
            int bitShift = mergedImage.DIB.BitsPerChannel - bitNum;
            int bitMask = 0xFF >> bitShift;

            int imageWidth = mergedImage.DIB.Width;
            int imageHeight = mergedImage.DIB.Height;

            // initialize blank ImGearRasterPage
            ImGearPage hidden = new ImGearRasterPage(imageWidth, imageHeight, new ImGearColorSpace(ImGearColorSpaceIDs.RGBA), new int[] { 8, 8, 8, 8 }, true);

            // For Each Pixel
            for (int x = 0; x < imageWidth; ++x) 
            {
                for (int y = 0; y < imageHeight; ++y)
                {
                    // Copy Pixel
                    ImGearPixel s = mergedImage.DIB.GetPixelCopy(x, y);
                    ImGearPixel p = new ImGearPixel(4, 8);
                   
                    //For Each Channel
                    for (int c = 0; c < 3; ++c)
                    {
                        // Get Low bit
                        bool bit = (s[c] & (1 << 0)) != 0;

                        // Set to Either 1 or 0 Followed by 7 0s
                        if (bit)
                        {
                            p[c] = 128; //10000000
                        }
                        else
                        {
                            p[c] = 0; //0000000
                        }
                    }
                    // Keep Alpha Channel Intact
                    p[3] = 255;
                    // Set the Pixel
                    hidden.DIB.UpdatePixelFrom(x, y, p);
                }
            }
            // Return Extracted Image
            return hidden;
        }

    }
}
