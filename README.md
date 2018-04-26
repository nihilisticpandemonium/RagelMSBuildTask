Ragel MSBuild Task
==================

Compiles Ragel files with the C# Language Target.

As of version 1.1 Ragel 4.6.1 binaries are bundled for Windows, Linux, and Mac OS X.

Very simple task with a few properties:

  * Inputs: List of ragel files ending with .rl to compile.
  * OutputPath: Path to put the generated files in.  Default is $(BaseIntermediateOutputPath)$(Configuration)\
  * Outputs: List of generated files that can be used to include files in compilation.  Default is @(Ragel->$(BaseIntermediateOutputPath)$(Configuration)\%(Filename).cs)

In Visual Studio:

  Create or add a Ragel file.
  Set its Build Action to Ragel.

Elsewhere:
  Add:
    ```
      <ItemGroup>
        <Ragel Include="**\*.rl"/>
      </ItemGroup>
    ```
  To automatically compile all Ragel files in the solution.
