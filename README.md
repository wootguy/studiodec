# studiodec

This is a stripped down version of [Crowbar](https://github.com/ZeqMacaw/Crowbar) which can only decompile GoldSrc models in CLI mode. It is hardcoded to work in single-file mode with the UV fix enabled.

I want to isolate the decompilation code and convert this to C++ some day so we can have a crossplatform opensource CLI decompiler without the UV shifting problem. Apparently that doesn't exist yet.

# Usage
```
studiodec path\to\model.mdl output\folder\
```

You must use back slashes. If decompiling a model in the same folder, prefix the path with ".\" as in ".\model.mdl". Otherwise you get a misleading "path too long" error.

# Building
Should work with Visual Studio 2017 communuity or later (I used 2022). Switch the solution configuration to x86 before building.
