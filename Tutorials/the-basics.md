# PowerCommands - The basics

This tutorial will guide you trough the process of the creation of and the design of a PowerCommands solution. We will creating a small and simple project that has only one Command that are compressing file from a given directory to a zip file. After this you will understand the core principles and the power of PowerCommands.

## Create a new PowerCommands Visual Studio solution
This requires that you have done the [Preparations](../README.md) where you downloaded the VS template and put it in your **%USERPROFILE%\Documents\Visual Studio \Templates\ProjectTemplates** directory.

Start your Visual Studio -> [Create new project] -> [PowerCommands Solution]

You can choose any name you want, for this tutorial the project name is **Basic** and the solution is named Tutorials.Basic.

![Alt text](images/vs_new_basic_tutorial.png?raw=true "New solution")

## Startup
Set the **PainKiller.PowerCommands.PowerCommandsConsole** as the startup project and start the solution and then run this command.
```
demo
```
This will give you a "Hello world" result and some instructions, you could also use the ```demo``` command to test your input. Therefore you may want to keep this command for a while, otherwise you could delete it from the project.

## Create the Zip command
In this tutorial we use the manually way, create a new class file named **ZipCommand** in the **Commands** directory in the **PainKiller.PowerCommands.BasicTutorialCommands** project.

The class inherits the **CommandBase<CommandsConfiguration>** base class that will give your Command a lot of helper methods that will be explained in this and other tutorials.
![Alt text](images/basic_power_command.png?raw=true "New solution")
This is actually enough to be a valid command, if you start PowerCommands again you will notice that the text of your input will turn blue if you type zip.
![Alt text](images/basic_power_command_zip.png?raw=true "New solution")

## Adding the run method
The PowerCommand console now finds your command, if you hit enter after zip you will se that the command missing an implementation to run the command, this could be solved by either override the **Run** function or the **RunAsync** function. In this tutorial we are overriding the **Run** function like this.
```
public override RunResult Run()
{
    return base.Run();
}
```
## Design the ZipCommand with a design attribute
Before rushing in to the implementation code as you normally do, with PowerCommands it´s better to give the design of your command some thoughts. 

What is the use case here?
- The user wants to zip files in a given directory
- The user may use a filter to select files that matches a certain file extension like *.txt for all text files.

What do we need? The ZipCommand needs the path for the directory to be zipped, and a option to add a filter. Lets start with adding a **PowerCommandsDesign** attribute like this.

```
[PowerCommandDesign(description: "Zip files of a given path, filter could be use to select only certain files that matches the filter",
                      arguments: "<directory>",
                         quotes: "<directory>",
                        options: "!filter",
                  example: "zip \"c:\\temp\"|zip \"c:\\temp\" --filter *.txt")]                  
```
With this attribute added to the Zip class we will get some benefits like:
- Help, the user could always you the -- help flag on this command and se the **description** and more to understand howe to use the command. Run this command and se for your self.
```
zip --help
```
But that is not all, on the options some validation will be triggered, the use of ! before the name of the option will do that if the filter option is provided they must also have a value. This means that you do not have to write code to validate this your self. In this tutorial we will not dig deeper than this when it comes to the **PowerCommandsDesign** attribute.

## Implementation code sample
We wil use the built in ZipService that is used to archive log files to actually zip the files so we do not need to write that code. But we need some implementation code to provide the ZipService with some parameters. The ZipService needs the Path to a valid directory to Zip the files in that directory. We could also use a output directory parameter if we want.

### Path to the directory
The user input must contain a path to the directory that is suppose to be zipped. We let the user use this two patterns to provide it.

```
zip C:\Program Files (x86)
```
Or
```
zip "C:\Program Files (x86)"
```
Lets start to play with this input just so that you se how a PowerCommand works. Write this code in the **Run** function.
```
public override RunResult Run()
{
    WriteLine(Input.SingleArgument);
    WriteLine(Input.SingleQuote);
    WriteLine(Input.Path);
    return Ok();
}
```
Notice that I have changed the last line to ```return Ok()``` with is a helper method in the base class to return a value that tells the runtime that the method has executed as expected.
Try different inputs with a path surrounded by " or not, try an invalid path. Noticed that if you start to type a valid path, for example C:\ you could use [tab] for code completion and cycle through all valid directories.

Notice that the **Input.Path** property always writes out a valid path no matter witch pattern you use, but the **Input.SingleArgument** does not work well on paths that has white spaces.

![Alt text](images/basic_power_command_path.png?raw=true "New solution")

So for paths it is safest to use either an argument or the path, you could also use the option parameters in case you need more then one path parameter as input, but for this tutorial we use Input.Path as we are only using one path.
```
var zipResult = ZipService.Service.ArchiveFilesInDirectory(Input.Path, "example", useTimestampSuffix: true, filter: string.IsNullOrEmpty(filter) ? "*" : filter);
```
### But what about the filter option?
Yes you are right, the filter is missing in the code line above, and how do we handle the fact that the use may choose to not input a Path at all? Let´s add some more to the **Run** method.
```
if (string.IsNullOrEmpty(Input.Path)) return BadParameterError("A valid path must be provided as argument");

var filter = GetOptionValue("filter");
WriteHeadLine($"Zipping files in directory: {Input.SingleQuote}...");
        
var zipResult = ZipService.Service.ArchiveFilesInDirectory(Input.Path, "example", useTimestampSuffix: true, filter: string.IsNullOrEmpty(filter) ? "*" : filter);
```
When we choose to use the **Input.Path** property we must validate that, that is a drawback you may say but that is not true in this case. If the **Input.Path** property has a value then we not that the user have typed one (ore more) valid UNC path so we do not need to validate that. 

### Using options
```
var filter = GetOptionValue("filter");
```
To fetch the filter parameter from the user we are using an option, witch means a value on the input command line that starts with **--** like this.
```
zip C:\Repos\Tutorials --filter *.md
```
It is valid to surround the value with " like this.
```
zip C:\Repos\Tutorials --filter "*.md"
```
The value of filter will be *.md either way.
### Print out stuff in the console
```
WriteHeadLine($"Zipping files in directory: {Input.SingleQuote}...");
```
The base class provides a lot of helper methods to write out stuff in the console, you should always use them instead of using the **Console.Write** or **Console.WriteLine** methods except when you there is a lot of output that you consider is not that important it should be added to the log file. Yes, the PowerCommands runtime always log every command you run and every thing you print out to the console if you use the helper methods.

One other **BIG** exception is of course if you are printing out sensitive value like password or access tokens, then you **SHOULD** use Console.Write methods.

## Wrap everything up
```
[PowerCommandDesign(description: "Zip files of a given path, filter could be use to select only certain files that matches the filter",
                      arguments: "<directory>",
                         quotes: "<directory>",
                        options: "!filter",
                  example: "zip \"c:\\temp\"|zip \"c:\\temp\" --filter *.txt")]
public class ZipCommand : CommandBase<CommandsConfiguration>
{
    public ZipCommand(string identifier, CommandsConfiguration configuration) : base(identifier, configuration) { }
    public override RunResult Run()
    {
        if (string.IsNullOrEmpty(Input.Path)) return BadParameterError("A valid path must be provided as argument");

        var filter = GetOptionValue("filter");
        WriteHeadLine($"Zipping files in directory: {Input.Path}...");
        
        var zipResult = ZipService.Service.ArchiveFilesInDirectory(Input.Path, "example", useTimestampSuffix: true, filter: string.IsNullOrEmpty(filter) ? "*" : filter);
        Console.WriteLine();
        
        WriteHeadLine("Result");
        WriteCodeExample(nameof(zipResult.Path), $"{zipResult.Path}");
        WriteCodeExample(nameof(zipResult.FileCount), $"{zipResult.FileCount}");
        WriteCodeExample(nameof(zipResult.FileSizeUncompressedInKb), $"{zipResult.FileSizeUncompressedInKb}");
        WriteCodeExample(nameof(zipResult.FileSizeCompressedInKb), $"{zipResult.FileSizeUncompressedInKb}");
        WriteCodeExample(nameof(zipResult.Checksum), $"{zipResult.Checksum}");

        if(zipResult.HasException) WriteError(zipResult.ExceptionMessage);
        return Ok();
    }
}
```
### Handling errors
```
if(zipResult.HasException) WriteError(zipResult.ExceptionMessage);
```
As you could see there is some alternative ways to handle errors, in the beginning we validate if the user has provided a valid file, if not we returned a BadParameter result using a helper method of the base class.

But at the end of the run method we checked the zipResult if it has any exception, if so we just print out that to the Console, there is no right or wrong here.

The whole **Run** function is encapsulated in a try/catch block if a unhandled error occurs that will be handled by the runtime. The error description will be printed out in the console and the Exception will be logged in the log file.

**Now this tutorial is complete!**
The sample code for this tutorial is located [here](../src/Tutorials.BasicTutorial/)

Next step [here](../src/Tutorials.BasicTutorial/)