namespace TaintAnalyzer.Models
{
    /// <summary>
    /// Primary class for Taint Analyzer work
    /// </summary>
    /// <param name="inputPath">Path to .cs file</param>
    /// <param name="outputPath">Path to save result</param>
    /// <param name="format">Output file extension</param>
    /// <param name="vulnerabilities">Vulnerability to check</param>
    public class TaintAnalyzerOptions(
        string inputPath,
        string outputPath,
        string? format,
        string[]? vulnerabilities)
    {
        /// <summary>
        /// Path to .cs file
        /// </summary>
        public string InputPath { get; set; } = inputPath;

        /// <summary>
        /// Path to save result
        /// </summary>
        public string OutputPath { get; set; } = outputPath;
        
        /// <summary>
        /// Output file extension
        /// </summary>
        public string? Format { get; set; } = format;

        /// <summary>
        /// Vulnerability to check
        /// </summary>
        public string[]? Vulnerabilities { get; set; } = vulnerabilities;
    }
}