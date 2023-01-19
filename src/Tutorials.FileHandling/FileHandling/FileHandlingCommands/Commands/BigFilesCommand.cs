using PainKiller.PowerCommands.Core.Commands;

namespace FileHandlingCommands.Commands;

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
        var defaultFileSize = Configuration.DefaultMegabytesFileSize;
        var minFileSize = Input.OptionToInt("megabytes", defaultFileSize);
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