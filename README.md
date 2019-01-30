# Ragel MSBuild Task

Compiles Ragel files with the C# Language Target.

As of version 1.1 Ragel 4.6.1 binaries are bundled for Windows, Linux, and Mac OS X.

Very simple task with a few properties:

- Inputs: List of ragel files ending with .rl to compile.
- OutputPath: Path to put the generated files in. Default is \$(IntermediateOutputPath)\
- Outputs: List of generated files that can be used to include files in compilation. Default is \$(IntermediateOutputPath)\FileName.cs)

Since version 1.1.4, you no longer need to modify the project file
directly. Simply add the RagelMSBuildTask nuget package, create or
add an existing ragel source, and they will build.

If you do not wish to use the default globs, you can use the following property to disable them:

  <PropertyGroup>
    <EnableDefaultRagelItems>False</EnableDefaultRagelItems>
  </PropertyGroup>
