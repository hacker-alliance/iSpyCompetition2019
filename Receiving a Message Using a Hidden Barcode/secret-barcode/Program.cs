using Accusoft.BarcodeXpressSdk;
using ImageGear.Core;
using ImageGear.Formats;
using System;
using System.IO;

namespace SecretBarcode
{
    class Program
    {
        static double BarcodeSlope = 0.5584;
        static double BarcodeYIntercept = 507.5;

        static void Main(string[] args)
        {
            ImGearLicense.SetSolutionName("Accusoft");
            ImGearLicense.SetSolutionKey(0x00000000, 0x00000000, 0x00000000, 0x00000000);
            ImGearLicense.SetOEMLicenseKey("2.0.E6dKRyMXM7REMwJuP0JK2YUXG7RCFaiSiQUY4EiukyGEkQ2aJuFwsCRjGCU9RacudaMaMCGjHYFEMCducEFQP0GScSUjkYFuPKiuG7sw57iQc0i9k72aPjcjHXJKR7UQPCsQ20iSiEPQUYdjJKRCH9FKMyJSPYdSF9kyUKdQdKJQHYP7dYJY5udy26MwWY5CdQGQFSPY4QsuUyiS50ij2a2aGCGj5KPuHE4QUuM7GwcQJXMwJ7FukX2QMaRXdyMYP9HykKJusCkwUQc9MKME46c9HCd9GaJQPK5XRaP9M0MKPa2aJudKk9Fjd9U94EGuPEsyP9GjsXFQJXHC5QRQUSdSiy26s6cX4yijRCFEJ7JwF0suk75aRCRYGUY");
            new BarcodeXpress().Licensing.SetOEMLicenseKey("2.0.ENhwvGbL56y2F6F6l3n7F7hNlNsBv7l7FLy2e2d4FpD8fByBbkFGFZeSW6yw53b3W7WLeZn3xjF8ywxjFSsGvGfZxGv4dG56f3W4s4nkfSF4fSF4lZD2FSD4hUf3bBWBdNn853bSf8nwW6DGW7dLeZn3xje3fwlZb45jvwU3xGv4DjFwd8bSs7lBUkbBb7y7y3s2D4d4yNxLbNdBeNd8dLf2f3xLv8U3f2W8FLvLF6UjeZWjeWdB7");

			
            ImGearCommonFormats.Initialize();

            // Load the image
            ImGearRasterPage inPage;
            using (var stream = new FileStream("trees.jpg", FileMode.Open))
                inPage = ImGearFileFormats.LoadPage(stream, 0) as ImGearRasterPage;

            // Create a result page for the barcode
            ImGearRasterPage outPage = new ImGearRasterPage(inPage.DIB.Width, inPage.DIB.Height, new ImGearColorSpace(ImGearColorSpaceIDs.RGB), new int[] { 8, 8, 8 }, true);

            // Transform the input image into the output barcode
            DecodePage(inPage, outPage);

            // Read the encoded barcode!
            ReadBarcodes(outPage);
        }

        static void DecodePage(ImGearRasterPage inPage, ImGearRasterPage outPage)
        {
            // Along the line y = m*x + b, where m = BarcodeSlope and b = BarcodeYIntercept,
            // Read pixels along the line from the inPage image. Transform the RGB values to CMYK.
            // Values less than 128 should be set to 0, and greater than or equal to 128 should be 255.
            // Finally, call the WriteColumn(...) function with one of the channel values.
        }

        static void WriteColumn(ImGearRasterPage outPage, int xCoordinate, int value)
        {
            var pixel = new ImGearPixel(3, 8);
            pixel[0] = pixel[1] = pixel[2] = value;

            // Write a pixel value to each y-coordinate, given an x-coordinate (ie. write a full column of the same pixel)
            // <code here>
        }

        static void ReadBarcodes(ImGearRasterPage page)
        {
            Result[] barcodeResults = new Result[] { };

            // Given a raster page, attempt to read any barcodes from this image.
            // <code here>

            // For each barcode found, print the interpreted value to the screen and pause
            foreach (var result in barcodeResults)
            {
                Console.WriteLine($"The password is: {result.BarcodeValue}");
                Console.ReadLine();
            }
        }
    }
}
