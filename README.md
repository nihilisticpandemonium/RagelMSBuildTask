Ragel MSBuild Task
==================

Compiles Ragel files with the C# Language Target.

Very simple task with a few properties:

  * ToolName: Name of the tool, default ragel.exe on Windows and ragel on Linux.
  * ToolPath: Path to the tool, default C:\Ragel on Windows and /usr/bin on Linux.
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