![](extruder_title.png) 
================
Extrude and pad tilesets.  No more seams!

## How to use
  Having trouble with seams in your tilemap rendering?  TileExtruder can help, taking the edge pixels of all tiles in a tileset, reversing the edge pixels in a "bidi" fashion, and extruding them outwards by the value you specify.  In addition, TileExtruder can space tiles a certain distance apart to prevent tiles from bleeding into each other.
  
  Once you have your tile extrusion, you can load it into your favorite map editor (provided they support tile spacing and margins).  Tiled TMX format requires specifying the spacing at the combined amount of spacing and padding which is specified to TileExtruder, and the margin as the amount specified as padding to TileExtruder.
  
### Commandline arguments
TileExtruder inputfile [args] [outputfile]
See the built-in help ("TileExtruder -h") for more details.
* pad:  How many pixels to extrude in each direction.
* space:  The amount of space to pad each tile in each direction, after extrusion.
* size:  Tile size, in pixels.
* diag:  Diagonal extrusion mode (see built-in help for more details)

All arguments can specify 1 or more parameters.  If an argument requires 2 parameters and only one is specified, the 2nd value is assumed to be the same as the first.

## Current Issues
* Setting a pad value greater than the tile's dimensions can result in an incorrect extrusion.
* Output is automatically converted to 32bpp.

## Donate
If you like this project, please consider giving a small donation to support further development.

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=RHZMPB4RL3T82&lc=US&item_name=Nobu%27s%20Monkey%2dX%20projects&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHosted)

