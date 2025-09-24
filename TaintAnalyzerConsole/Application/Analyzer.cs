using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TaintAnalyzer.Models;

namespace TaintAnalyzerConsole.Application;

internal class Analyzer
{
    private readonly TaintAnalyzerOptions _options;

    public Analyzer(TaintAnalyzerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public void RunAnalysis()
    {
        try
        {
            string code = File.ReadAllText(_options.InputPath);

            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            SyntaxNode root = tree.GetRoot();

            Console.WriteLine("Syntax tree:");
            PrintNodes(root, 0);
        }
        catch (FileNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fail: File '{_options.InputPath}' not found.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Analyze error: {ex.Message}");
            Console.ResetColor();
        }
    }

    private void PrintNodes(SyntaxNode node, int indent)
    {
        Console.WriteLine(new string(' ', indent * 2) + node.Kind() + (node is SyntaxToken ? $" (Token: {node})" : ""));

        foreach (var child in node.ChildNodesAndTokens())
        {
            if (child.IsNode)
            {
                PrintNodes(child.AsNode(), indent + 1);
            }
            else
            {
                var token = child.AsToken();
                Console.WriteLine(new string(' ', (indent + 1) * 2) + $"Token: {token.Kind()} (Value: {token.Text})");
            }
        }
    }
}
