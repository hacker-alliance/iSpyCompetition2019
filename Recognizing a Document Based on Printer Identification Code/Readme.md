# Recognizing a Document Based on Printer Identification Code

This problem is intended to decode the steganographic message added to printed documents by modern printers.

## Setup

Modern printers add steganographic messages to printed documents which identify attributes like what printer was used to print the document, and the time of printing.
This printing is often encoded as yellow-dots in a grid pattern. An encoded pattern can be seen in the image `instance.png`.
This pattern was lifted from the printed document `demands.png` sent by the enemy, which you can barely visible at the top of the document.

We must decode the encoded yellow-dot pattern in `instance.png` in order to gather information about the enemy's strategies and tactics.

## Encoding Method

We are aware of the encoding method of this pattern of yellow-dots:

There are 15 rows in this encoding pattern. Each row has a corresponding number encoded by the dots in that row.
Additionally, each row represents an identifying attribute about the printed document. Notably, rows 3, 4, and 5
determine the serial number of the printer, and rows 8, 9, 10, 11, and 14 determine the time (to the minute) that
the document was printed.

There are 8 columns in this encoding pattern. The first column represents the parity of the number of dots in the remainder of the row.
The other columns represent a binary digit, with a dot representing a one for that digit, and the absence of a dot meaning zero.
Note that the remaining columns' representative binary digit are in reverse numerical order, so column 2 represents digit 7 and
column 8 represents digit 1.

## Solution

We must decode the binary numbers associated with each row in this grid and convert them to decimal notation so that we know
when and where this document was printed. This will help us to determine a strategy for combatting the enemy, as we cannot
comply with their ridiculous demands!
