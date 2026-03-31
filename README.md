# GoldSrc MDL decompiler

This is a stripped down version of [Crowbar](https://github.com/ZeqMacaw/Crowbar) which can only decompile GoldSrc models in CLI mode. It is hardcoded to work in single-file mode with the UV-fix enabled.

I want to isolate the decompilation code and convert this to C++ some day so we can have a crossplatform opensource CLI decompiler without the UV problem. Apparently that doesn't exist yet.

# Usage
```
Crowbar.exe path\to\model.mdl output\folder\
```

You must use back slashes. If decompiling a model in the same folder, prefix the path with ".\" as in ".\model.mdl". Otherwise you get a misleading "path too long" error.

# Building
Using a modern version of Visual Studio, switch the solution configuration to x86. The original project required SteamWorks.NET, but this one shouldn't.