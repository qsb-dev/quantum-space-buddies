using System;

namespace QSB.Patches;

[Flags]
public enum PatchVendor 
{ 
	None = 0,
	Epic = 1,
	Steam = 2,
	Gamepass = 4
}

