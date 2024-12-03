# FFLSharp

![image](https://github.com/user-attachments/assets/40ea3cb7-ec55-465f-90a1-aa839bc1d299)

C# bindings for FFL, based on [my fork of Abood's decompilation](https://github.com/ariankordi/ffl).
FFL is the the official library for rendering Mii heads and managing Mii data on Wii U.

In this repo, you will find:

* FFLSharp.Veldrid, which includes routines and classes involved with drawing Mii heads using Veldrid which should work for all of its supported APIs.
* FFLSharp.Interop, which are the actual bindings to FFL itself. 
  - These are autogenerated by ClangSharp using the GenerateFFL.rsp recipe.
* FFLSharp.BasicTests - ignore this, they are so basic they don't even count.
* FFLSharp.VeldridBasicShaderSample, pictured above. It renders a Mii head spinning, similar to [FFL-Testing](https://github.com/ariankordi/FFL-Testing), and it blinks every few seconds.
* (No sample using the FFLShader currently.)

I'm hoping that the Veldrid stuff would help if anyone wanted to use Miis in other graphics APIs/game engines, including anything using OpenGL which would be really simple
**especially if you use my FFL raylib samples to guide you: todo todo I reeeally have to publish these**

## Building
For brevity, I'm going to skip the C# portion since it should be easy enough to build. The real challenge is
* Building libffl.
* Getting it functional and acquiring the resource file.

**TODO TODO NOT DONE!!!!!!!!!! NEEEEEEEEED TO MAKE CMAKE PROJECT!!! if you are lURKING you should hack [the FFL-Testing makefile](https://github.com/ariankordi/FFL-Testing/blob/renderer-server-prototype/Makefile) to build a library tho it neeeds latest rio changes too hh**

# Acknowledgements
* [aboood40091/AboodXD](https://github.com/aboood40091) for the [FFL decompilation and port to RIO](https://github.com/aboood40091/ffl/tree/nsmbu-win-port).
* [ClangSharp](https://github.com/dotnet/ClangSharp) for autogenerated bindings.
* [Eric Mellino](https://github.com/mellinoe) for [Veldrid](https://github.com/veldrid/veldrid).
* Nintendo for making FFL.