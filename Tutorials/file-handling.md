# PowerCommands - File handling

This tutorial will guide you trough the process of the creation of and the design of a PowerCommands solution. We will creating a project that show you how you easily can integrate the file system in your PowerCommands Console application.

## Create a new PowerCommands Visual Studio solution
This requires that you have done the [Preparations](../README.md) where you downloaded the VS template and put it in your **%USERPROFILE%\Documents\Visual Studio \Templates\ProjectTemplates** directory. And that you are familiar with how to create a new PowerCommands solution as described in [The basics](the-basics.md) tutorial. This project will have the name **FileHandlingTutorial** and the Solution name is **Tutorials.FileHandlingTutorial** the names or of course not that important.

## Use case
- The user wants to find big files in the current working directory
- The user requires that it is easy to navigate through directories, with the same look and feel as the fabulous cmd prompt.
- The user wants that a default file size is set in the configuration file so that it could be optional
- As this could take a while we need it to run async

## Let´s create the first command
This time we will use the build in functionality to create new commands, startup the PowerCommand console and run this command:
```
powercommand new --command BigFiles
```
This will create the file **BigFilesCommand** in the **Commands** directory of the  **PainKiller.PowerCommands.FileHandlingTutorialCommands** project.
```
public class BigFilesCommand : CommandBase<PowerCommandsConfiguration>
{
    public BigFilesCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        //insert your code here
        return Ok();
    }
}
```
As you can se the Run method is all setup for you to start write the implementation of the code. But, before we do that, lets design the command a bit by adding this **PowerCommandDesign** attribute.
```
[PowerCommandDesign(description: "Analyze the current working directory and its subdirectories and find big files, that are either of an default size set in config or a given one by the user input using the option --megabytes.",
                        options: "megabytes",
                        example: "bigfiles --megabytes 1024",
                       useAsync: true)]
public class BigFilesCommand : CommandBase<PowerCommandsConfiguration>
```
The most of this is self explanatory I hope, if not it will all fall in to place as we move along.

## Run commands async
In the design attribute we can see that the useAsync property is set to true, but... that means that we also has to change witch run method to override from the base command class, lets override the RunAsync method instead, change the name and the signature of the existing run method like this.
```
public override async Task<RunResult> RunAsync()
{
    return Ok();
}
```
## Now we are ready to write the implementation
Please note that the implementation code traversing files and directories may be a bit sloppy! 😁
```
[PowerCommandDesign(description: "Analyze the current working directory and its subdirectories and find big files, that are either of an default size set in config or a given one by the user input using the option --megabytes.",
                        options: "megabytes",
                        example: "bigfiles --megabytes 1024",
                       useAsync: true)]
public class BigFilesCommand : CommandBase<PowerCommandsConfiguration>
{
    private readonly List<string> _bigFiles = new();
    const int OneMegabyte = 1048576;
    public BigFilesCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        //insert your code here
        return Ok();
    }

    public override async Task<RunResult> RunAsync()
    {
        var path = CdCommand.WorkingDirectory;
        var minFileSize = Input.OptionToInt("megabytes", 1);
        var rootDirectory = new DirectoryInfo(path);
        await RunIterations(rootDirectory, minFileSize);

        return Ok();
    }

    private async Task RunIterations(DirectoryInfo rootDirectory, int minFileSize)
    {
        await Task.Yield();
        TraverseDirectory(rootDirectory,minFileSize);
        OverwritePreviousLine($"Big files found over {minFileSize} MB:");
        foreach (var bigFile in _bigFiles) WriteLine(bigFile);
        Console.Write($"\nDone!\n{ConfigurationGlobals.Prompt}");
    }
    private void TraverseDirectory(DirectoryInfo startDirectory, int minFileSize)
    {
        foreach (var subDirectory in startDirectory.GetDirectories())
        {
            OverwritePreviousLine(subDirectory.Name);
            TraverseDirectory(subDirectory, minFileSize);
        }
        foreach (var fileInfo in startDirectory.GetFiles())
        {
            OverwritePreviousLine(fileInfo.Name);
            var fileSizeInMegaBytes = fileInfo.Length > OneMegabyte ? fileInfo.Length / OneMegabyte : 0;
            if (fileSizeInMegaBytes > minFileSize) _bigFiles.Add($"{startDirectory.FullName}\\{fileInfo.Name} {fileSizeInMegaBytes} Megabytes");
        }
    }
}
```
## Lets try this
Startup the PowerCommand Console and run this command:
```
bigfiles
```
This will probably go very fast and no files will be found, in my case the log file was bigger then 1 MB.
![Alt text](images/file-handling.png?raw=true "New solution")
If you run the **dir** you see that the current working directory is the bin folder of the PowerCommand console.
You can use the cd command to navigate to a folder where you suspect to find a lot of bigger files and try the 
bigfile command again.

You will see a progress where the files are printed out to the console, all on the same row that is because the command uses the helper method **OverwritePreviousLine(fileInfo.Name);**.

Later I will add just how to customize the configuration so that we can set a default file size in MB, and change the implementation code so it uses that instead of the hardcoded 1 Mb.