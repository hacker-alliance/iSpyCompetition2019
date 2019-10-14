==== Overview ====

Students,

Our agent has uploaded a critical password that we need to decrypt a transmission, but they've hidden the password somewhere in an image of trees.
The only information we have to go on is "slope = 0.5584, intercept = 507.5". We believe there is a barcode within the image along that line, in
one of the color channels, but it's not visible to the naked eye -- we think it must be in one of the CMYK channels (either the Cyan, Magenta,
Yellow, or Black channel), instead of the typical Red, Green or Blue channels.


==== Details ====

Your job is to take the input image, scan along the given line and read in each pixel. Transform each pixel from RGB to CMYK in accordance with the
formula in the next section. Along this line of pixels, one of the CMYK channels contains a hidden barcode. By performing a simple threshold
check of the pixel value, the channel information can be transformed into either black (0) & white (255) barcode information. Here's how the thresholding
function works:

threshold(value) :=
    0   if value < 128
    255 if value >= 128

Examples:
  threshold(50) = 0
  threshold(100) = 0
  threshold(128) = 255
  threshold(200) = 255

Once this thresholded value has been written out to a new page as columns, we believe that we can run a barcode detector on the result to read the
secret password.


==== RGB to CMYK transform ====

To transform an RGB image into a CMYK image, the following is performed:
  V = Max(Red, Green, Blue)
  Cyan = 255 * (V - Red) / V
  Magenta = 255 * (V - Green) / V
  Yellow = 255 * (V - Blue) / V
  Black = 255 - V


==== Working Example ====

If the pixels read on the line are, from left to right, in RGB format:
(112, 123, 60)      (209, 83, 198)      (63, 43, 239)

The CMYK values would be:
(23, 0, 131, 132)   (0, 154, 13, 46)    (188, 209, 0, 16)

Applying the threshold function to these values, you get the following:
(0, 0, 255, 255)    (0, 255, 0, 0)      (255, 255, 0, 0)

Choosing, for example, the magenta (ie. second) channel, our results are:
0  255  255

If we created a page with these values written as columns, it might look like (an example, with a page of height 5):

(0, 0, 0)   (255, 255, 255)     (255, 255, 255)
(0, 0, 0)   (255, 255, 255)     (255, 255, 255)
(0, 0, 0)   (255, 255, 255)     (255, 255, 255)
(0, 0, 0)   (255, 255, 255)     (255, 255, 255)
(0, 0, 0)   (255, 255, 255)     (255, 255, 255)

This, we could send into a barcode analysis engine, to see if there's a hidden message.