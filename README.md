[Follow progress on twitter](https://twitter.com/PowerCommands) <img src="https://github.com/PowerCommands/PowerCommands2022/blob/main/Docs/images/Twitter.png?raw=true" alt="drawing" width="20"/>

# PowerCommands-Tutorials

## Preparations
Setup the VS Template to create new Solution for your PowerCommands project.
- Download the template [Visual Studio project template](https://github.com/PowerCommands/PowerCommands2022/tree/main/Templates/Artifact)
- Copy the .zip file into the user project template directory. By default, this directory is %USERPROFILE%\Documents\Visual Studio \Templates\ProjectTemplates.
- Open Visual Studio and write Power in the searchbox, you should find the PowerCommand template.
![Alt text](Tutorials/images/vs_new_solution.png?raw=true "New solution")

## Tutorials
[The basics](/Tutorials/the-basics.md)

This tutorial will guide you trough the process of the creation of and the design of a PowerCommands solution. We will creating a small and simple project that has only one Command that are compressing file from a given directory to a zip file. After this you will understand the core principles and the power of PowerCommands.

[File handling part I](/Tutorials/file-handling.md)

In this tutorial we go trough how to handle files and directories and how we can run the async method instead of the default run method. This tutorial will also show you how to add your custom configuration and use it in your Command class.

[File handling part II](/Tutorials/file-handling2.md)

We extend the tutorial from part I and add a path option to the command and show how we could make it use code completion and show the current sub directories of the current working directory, without writing any implementation code, just by using a different base class and implementing a interface.
