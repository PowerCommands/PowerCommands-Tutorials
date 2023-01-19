# PowerCommands - File handling part II - Use custom configuration

This tutorial will continue where the [Files and directories part I](file-handling.md) ended, we will add the default file size to the configuration instead of hardcoded in the command.

## Configuration
Let´s start with the configuration part, the Power Commands project has already prepared for you to add stuff to your PowerCommands configuration, in the same project as you put your commands there is a directory named **Configuration**.

![Alt text](images/configuration_class.png?raw=true "Configuration")

You can also see the main configuration file **PowerCommandsConfiguration.yaml** in the project root. But first things first, we start to add a new property to the **PowerCommandsConfiguration.cs** like this.

```
public class PowerCommandsConfiguration : CommandsConfiguration
{
    //Here is the placeholder for your custom configuration, you need to add the change to the PowerCommandsConfiguration.yaml file as well
    public int DefaultMegabytesFileSize { get; set; } = 1;
    public string DefaultGitRepositoryPath { get; set; } = "C:\\repos";
}
```
As you can see, we have added the **DefaultMegabytesFileSize** property and set it to 1 as a default value if the configuration file is missing a value for this property.

You may wonder what the custom property **DefaultGitRepositoryPath** is doing there? It is used by a custom GitCommand that I always use, this will be explained in a future tutorial.

## Edit the **PowerCommandsConfiguration.yaml** file
Next step is to edit the configuration file, and add this new element and value like this:
```
configuration:
  showDiagnosticInformation: false
  defaultCommand: commands
  codeEditor: C:\Users\%USERNAME%\AppData\Local\Programs\Microsoft VS Code\Code.exe
  repository: https://github.com/PowerCommands/PowerCommands2022
  backupPath: ..\..\..\..\Core\PainKiller.PowerCommands.Core\  
  defaultGitRepositoryPath: ..\..\..\..\..\..\..\
  defaultMegabytesFileSize: 10
```
This is not the whole content of the file, but it as much as you need to see to understand where you add simple element like a string or a integer. This is a standard yaml file you could add more complex element if you want but that beyond the scope of this tutorial.

**Don´t forget to rebuild the project after you edit the PowerCommandsConfiguration.yaml file to the new file overwrites the old one in the bin directory.**
## Update the implementation code ##
Last step is to update the implementation code from the previous tutorial so that this default size from the configuration file will be used if omitted by the user running the ```bigfile``` command

```
public override async Task<RunResult> RunAsync()
{
    var path = CdCommand.WorkingDirectory;
    var defaultFileSize = Configuration.DefaultMegabytesFileSize;
    var minFileSize = Input.OptionToInt("megabytes", defaultFileSize);
    var rootDirectory = new DirectoryInfo(path);
    await RunIterations(rootDirectory, minFileSize);

    return Ok();
}
```
The new line is this 

```var defaultFileSize = Configuration.DefaultMegabytesFileSize;```

The default configuration file size is now fully implemented.