using Microsoft.Extensions.Configuration;
using TaintAnalyzer.Models;
using TaintAnalyzerConsole.Application;

#region customFeatures

void StartBanner()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(@"");
    Console.WriteLine(@"╔══════════════════════════════════════════════╗");
    Console.WriteLine(@"║                TAINT ANALYZER                ║");
    Console.WriteLine(@"║           Static Security Scanner            ║");
    Console.WriteLine(@"║                                              ║");
    Console.WriteLine(@"║  Version: 1.0.0       Author: OBJLAKO        ║");
    Console.WriteLine(@"║  GitHub: github.com/OBJLAKO/Taint-Analyzer   ║");
    Console.WriteLine(@"║  Started: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "                   ║");
    Console.WriteLine(@"╚══════════════════════════════════════════════╝");
    Console.WriteLine(@"");
    Console.ResetColor();
}
void ShowHelp()
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(@"");
    Console.WriteLine(@"  ████████╗ █████╗ ██╗███╗   ██╗████████╗  ██╗  ██╗███████╗██╗    ██████╗ ");
    Console.WriteLine(@"  ╚══██╔══╝██╔══██╗██║████╗  ██║╚══██╔══╝  ██║  ██║██╔════╝██║    ██╔══██╗");
    Console.WriteLine(@"     ██║   ███████║██║██╔██╗ ██║   ██║     ███████║█████╗  ██║    ██████╔╝");
    Console.WriteLine(@"     ██║   ██╔══██║██║██║╚██╗██║   ██║     ██╔══██║██╔══╝  ██║    ██╔═══╝ ");
    Console.WriteLine(@"     ██║   ██║  ██║██║██║ ╚████║   ██║     ██║  ██║███████╗██████╗██║     ");
    Console.WriteLine(@"     ╚═╝   ╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝   ╚═╝     ╚═╝  ╚═╝╚══════╝╚═════╝╚═╝     ");
    Console.WriteLine(@"");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Usage:");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("--input <path> --output <path> [options]");
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Options:");
    Console.WriteLine("  -i, --input <path>      Path to .sln file or directory (required)");
    Console.WriteLine("  -o, --output <path>     Path to save results (required)");
    Console.WriteLine("  -f, --format <format>   Output format: SARIF, JSON, HTML (default: SARIF)");
    Console.WriteLine("  -v, --vulnerabilities   Comma-separated vulnerabilities to check");
    Console.WriteLine("  --help                  Show this help message");
    Console.ResetColor();
}

void ShowError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"{message}");
    Console.ResetColor();
}

#endregion

if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
{
    ShowHelp();
    return 0;
}

try
{
    IConfiguration config = new ConfigurationBuilder()
        .AddCommandLine(args)
        .Build();

    var inputPath = config["input"] ?? config["i"];
    var outputPath = config["output"] ?? config["o"];

    if (string.IsNullOrWhiteSpace(inputPath))
    {
        ShowError("Input path is required. Use --input <path> or use --help");
        return 1;
    }

    if (string.IsNullOrWhiteSpace(outputPath))
    {
        ShowError("Output path is required. Use --output <path> or use --help");
        return 1;
    }

    var options = new TaintAnalyzerOptions(
        inputPath,
        outputPath,
        config["format"] ?? config["f"] ?? "SARIF",
        config["vulnerabilities"]?.Split(',', StringSplitOptions.RemoveEmptyEntries)
    );

    StartBanner();

    var analyzer = new Analyzer(options);
    analyzer.RunAnalysis();
    return 0;
}
catch (Exception ex)
{
    ShowError($"Unexpected error: {ex.Message}");
    return 1;
}
