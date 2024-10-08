# Lunar Fonts

Font loader and rasterizer for TrueType (.ttf) and OpenType (.otf) format in pure C#.

## Installation

Available as a Nuget package.

```
    PM> Install-Package LunarLabs.Fonts
```

# Sample code

At the core, this library allows loading a font from a file and from there you can rasterize a single character at a specified scale (and also query info about characters).
Here's a simple example of how to do it:

```c#
	using LunarLabs.Fonts;
	//...
	var bytes = File.ReadAllBytes(fontFileName);

	var font = new Font(bytes);

	// convert font size to pixels
	var scale = font.ScaleInPixels(64);	

	var result = font.RenderGlyph('A', scale);
	// result.Image now contains the pixels, width and height of the rasterized glyph
	// it also contains other fields with useful info like xAdvance which lets you know how many pixels to move forward if rendering multiple characters sequentially
```

Note that the pixels array stores the pixels as single-channel greyscale data. What you do with this is up to you, you can blit it to the screen, to a bitmap or to an OpenGL texture etc.

You can check the included sample for a more advanced program that shows how to output a full string and save the result as a image file.

Kerning is also supported via method GetKerning(charA, charB, scale), which returns a int containing amount of pixels to subtract from xAdvance when calculating position of next character based on previous.

# Font Viewer

Include is also the source code for a font viewer that uses this library.

It allows browsing of any folder that contains .ttf files and quickly preview custom text using each font.

<p align="center">
  <img src="/viewer.png">
</p>

# Credits

- [Relfos](https://github.com/Relfos) - C# port of the [Delphi version](https://github.com/Relfos/TERRA-Engine/blob/master/Engine/Image/TERRA_TTF.pas).
- [Sean Barrett](http://nothings.org/) - The [original C version](https://github.com/nothings/stb/blob/master/stb_truetype.h)
- [Ben Baker](https://github.com/benbaker76) - Bug fixes, improvements and additional features (Eg. OpenType-SVG support, foreground and background colors).

# Contact

Let me know if you find bugs or if you have suggestions to improve the code.

And maybe follow me [@onihunters](https://twitter.com/onihunters) :)