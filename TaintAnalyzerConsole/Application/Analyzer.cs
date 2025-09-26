using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.Data.SqlClient;

using TaintAnalyzer.Models;

namespace TaintAnalyzerConsole.Application
{
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
                var filesToAnalyze = GetFilesToAnalyze(_options.InputPath);

                foreach (var filePath in filesToAnalyze)
                {
                    Console.WriteLine($"Analyzing file: {filePath}");
                    AnalyzeFile(filePath);
                }

                Console.WriteLine("Analysis completed successfully!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Analyze error: {ex.Message}");
                Console.ResetColor();
            }
        }

        private List<string> GetFilesToAnalyze(string inputPath)
        {
            var files = new List<string>();
            if (File.Exists(inputPath))
            {
                files.Add(inputPath);
            }
            else if (Directory.Exists(inputPath))
            {
                files.AddRange(Directory.GetFiles(inputPath, "*.cs", SearchOption.AllDirectories));
            }
            else
            {
                throw new FileNotFoundException($"Input path '{inputPath}' does not exist.");
            }
            return files;
        }

        private void AnalyzeFile(string filePath)
        {
            string code = File.ReadAllText(filePath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var sqlClient = MetadataReference.CreateFromFile(typeof(SqlCommand).Assembly.Location);

            var compilation = CSharpCompilation.Create(Path.GetFileName(filePath))
                .AddReferences(mscorlib, sqlClient)
                .AddSyntaxTrees(tree);

            var semanticModel = compilation.GetSemanticModel(tree);

            var methods = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                var cfg = ControlFlowGraph.Create(method, semanticModel);

                if (cfg == null) continue;

                var dataFlowAnalysis = semanticModel.AnalyzeDataFlow(method.Body);

                if (!dataFlowAnalysis.Succeeded) continue;

                var taintSources = FindTaintSources(method, semanticModel);
                var taintSinks = FindTaintSinks(method, semanticModel);

                var taintedSymbols = new HashSet<ISymbol>();

                foreach (var source in taintSources)
                {
                    if (source.Parent is AssignmentExpressionSyntax assignment)
                    {
                        var leftSymbol = semanticModel.GetSymbolInfo(assignment.Left).Symbol;
                        if (leftSymbol != null)
                        {
                            taintedSymbols.Add(leftSymbol);
                        }
                    }
                }

                PropagateTaintThroughDataFlow(dataFlowAnalysis, taintedSymbols);

                CheckSinksForTaint(taintSinks, taintedSymbols, semanticModel, filePath);
            }
        }

        private List<InvocationExpressionSyntax> FindTaintSources(SyntaxNode node, SemanticModel semanticModel)
        {
            var sources = new List<InvocationExpressionSyntax>();
            var invocations = node.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var invocation in invocations)
            {
                var symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                if (symbol != null)
                {
                    if (symbol.ContainingType.Name == "Console" && symbol.Name == "ReadLine")
                    {
                        sources.Add(invocation);
                    }
                    else if (symbol.ContainingType.Name == "Environment" && symbol.Name == "GetEnvironmentVariable")
                    {
                        sources.Add(invocation);
                    }
                }
            }
            return sources;
        }

        private List<InvocationExpressionSyntax> FindTaintSinks(SyntaxNode node, SemanticModel semanticModel)
        {
            var sinks = new List<InvocationExpressionSyntax>();
            var invocations = node.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var invocation in invocations)
            {
                var symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                if (symbol != null)
                {
                    if (symbol.ContainingType.Name == "Console" && symbol.Name == "WriteLine")
                    {
                        sinks.Add(invocation);
                    }
                    else if (symbol.ContainingType.Name == "SqlCommand" && (symbol.Name == "ExecuteNonQuery" || symbol.Name == "ExecuteReader"))
                    {
                        sinks.Add(invocation);
                    }
                }
            }
            return sinks;
        }

        private void PropagateTaintThroughDataFlow(DataFlowAnalysis dataFlowAnalysis, HashSet<ISymbol> taintedSymbols)
        {
            foreach (var symbol in dataFlowAnalysis.WrittenInside.Union(dataFlowAnalysis.ReadInside))
            {
                if (taintedSymbols.Contains(symbol))
                {
                }
            }

            foreach (var symbol in dataFlowAnalysis.AlwaysAssigned)
            {
                if (dataFlowAnalysis.DataFlowsIn.Contains(symbol) && taintedSymbols.Any(t => dataFlowAnalysis.VariablesDeclared.Contains(t)))
                {
                    taintedSymbols.Add(symbol);
                }
            }
        }

        private void CheckSinksForTaint(List<InvocationExpressionSyntax> sinks, HashSet<ISymbol> taintedSymbols, SemanticModel semanticModel, string filePath)
        {
            foreach (var sink in sinks)
            {
                foreach (var arg in sink.ArgumentList.Arguments)
                {
                    var argSymbol = semanticModel.GetSymbolInfo(arg.Expression).Symbol;
                    if (argSymbol != null && taintedSymbols.Contains(argSymbol))
                    {
                        var line = sink.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[!] Taint flow detected in {filePath}: Tainted data in sink at line {line}: {sink.ToString()}");
                        Console.ResetColor();

                        SaveToSarif(filePath, line, sink.ToString());
                    }
                }
            }
        }

        //TODO: Microsoft.CodeAnalysis.Sarif
        private void SaveToSarif(string filePath, int line, string message)
        {
            var sarifPath = Path.Combine(_options.OutputPath, "results.sarif");
            File.AppendAllText(sarifPath, $"{{ \"file\": \"{filePath}\", \"line\": {line}, \"message\": \"{message}\" }}\n");
        }
    }
}