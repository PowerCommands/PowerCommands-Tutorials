# Use ComputerVision with PowerCommands
## Before you start
You need an Azure account, you need to add a new Cognitive Service of the type Computer Vision here:

https://portal.azure.com/#view/Microsoft_Azure_ProjectOxford/CognitiveServicesHub/~/overview

Setup two environment variables for the endpoint and the key, look in the PowerCommandsConfiguration.yaml file and there you se the details.

```
environment:
    variables:
    - name: powercommands-computer-vision-endpoint
      environmentVariableTarget: User
    - name: powercommands-computer-vision
      environmentVariableTarget: User
```
## Computer vision command
So the first PowerCommand I added is the ```computervision``` command named **ComputerVisionCommand**, just run it and of course check out the code.

```
ComputerVision
```

Read more about it here: https://learn.microsoft.com/en-us/azure/cognitive-services/computer-vision/quickstarts-sdk/image-analysis-client-library?tabs=visual-studio%2C3-2&pivots=programming-language-csharp

## The Code
```
[PowerCommandDesign( description: "Run Microsoft.Azure.CognitiveServices.Vision.ComputerVision services",
                        useAsync: true,
                         example: "computervision")]
public class ComputerVisionCommand : CommandBase<PowerCommandsConfiguration>
{
    public ComputerVisionCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override async Task<RunResult> RunAsync()
    {
        var client = Authenticate(Configuration.Environment.GetValue("powercommands-computer-vision-endpoint"),
            Configuration.Environment.GetValue("powercommands-computer-vision"));
        await AnalyzeImageUrl(client, "https://moderatorsampleimages.blob.core.windows.net/samples/sample16.png");
        return Ok();
    }
    public static ComputerVisionClient Authenticate(string endpoint, string key) => new(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
    public async Task AnalyzeImageUrl(ComputerVisionClient client, string imageUrl)
    {
        WriteLine("----------------------------------------------------------");
        WriteLine("ANALYZE IMAGE - URL");
        Console.WriteLine();

        // Creating a list that defines the features to be extracted from the image. 
        var features = new List<VisualFeatureTypes?> { VisualFeatureTypes.Tags };

        WriteLine($"Analyzing the image {Path.GetFileName(imageUrl)}...");
        Console.WriteLine();
        // Analyze the URL image 
        var results = await client.AnalyzeImageAsync(imageUrl, visualFeatures: features);

        // Image tags and their confidence score
        WriteLine("Tags:");
        foreach (var tag in results.Tags)
        {
            WriteLine($"{tag.Name} {tag.Confidence}");
        }
        Console.WriteLine();
    }
}
```
Note that I am using an hardcoded URL to the image in this example.

You can look at the source code [here](/src/Tutorials.AzureCognitiveServices/).