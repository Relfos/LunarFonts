# Lunar Fonts
Font loader / rasterizer for TTF format in pure C#.

This code is a direct port from the [Delphi version](https://github.com/Relfos/TERRA-Engine/blob/master/Engine/Image/TERRA_TTF.pas) which I ported some years ago from C. 
The [original C version](https://github.com/nothings/stb/blob/master/stb_truetype.h) was written by [Sean Barrett](http://nothings.org/)

## Installation

    PM> Install-Package LunarLabs.Fonts

Since this is a .NET standard package, to use with .NET framework projects please set the target to .NET Framework 4.5 or higher, otherwise Nuget will give you installation errors.

# Sample code

At the core, this library allows loading a font from a file and from there you can rasterize a single character at a specified scale (and also query info about characters).
Here's a simple example of how to do it:

```c#
	using LunarLabs.Fonts;
	//...
	var bytes = File.ReadAllBytes(fontFileName);
	var font = new Font(bytes);
	var scale = font.ScaleInPixels(64);
	var result = font.RenderGlyph('A', scale);
	// result.Image now contains the pixels, width and height of the rasterized glyph	
```

Note that the pixels array stores the pixels as single-channel greyscale data. What you do with this is up to you, you can blit it to the screen, to a bitmap or to an OpenGL texture etc.

You can check the included sample for a more advanced program that shows how to output a full string and save the result as a image file.

# Font Viewer

Include is also the source code for a font viewer that uses this library.

It allows browsing of any folder that contains .ttf files and quickly preview custom text using each font.

<p align="center">
  <img src="/viewer.png">
</p>


# Contact

Let me know if you find bugs or if you have suggestions to improve the code.

And maybe follow me [@onihunters](https://twitter.com/onihunters) :)