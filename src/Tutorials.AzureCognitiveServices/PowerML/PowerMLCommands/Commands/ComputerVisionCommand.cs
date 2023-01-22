using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace PowerMLCommands.Commands;

[PowerCommandDesign( description: "Run Microsoft.Azure.CognitiveServices.Vision.ComputerVision services",
                        useAsync: true,
                         example: "computervision")]
public class ComputerVisionCommand : CommandBase<PowerCommandsConfiguration>
{
    public ComputerVisionCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override async Task<RunResult> RunAsync()
    {
        var client = Authenticate(Configuration.Environment.GetValue("powercommands-computer-vision-endpoint"), Configuration.Environment.GetValue("powercommands-computer-vision"));
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