Taint Analyzer
.NET Console application for static taint analysis of C# source files. Detects potential security vulnerabilities by tracking untrusted data flows.

📋 Overview
Taint Analyzer is a static analysis tool that scans pre-prepared C# source files to identify potential security vulnerabilities caused by improper handling of untrusted data (taint analysis).

🚀 Features
Static Taint Analysis: Identifies data flows from sources to sensitive sinks

C# Source Code Scanning: Analyzes .cs files for potential vulnerabilities

Configurable Rules: Customizable source/sink definitions

Console Interface: Easy-to-use command-line tool

Detailed Reporting: Comprehensive vulnerability reports

📥 Installation
Prerequisites
.NET 9.0 SDK or later

Download
bash
# Clone the repository
git clone https://github.com/OBJLAKO/Taint-Analyzer.git
cd taint-analyzer
Build from Source
bash
dotnet restore
dotnet build --configuration Release
🛠️ Usage
Basic Syntax
bash
TaintAnalyzer --input <path> --output <path> [options]
Examples
Scan a single file:

bash
TaintAnalyzer --input "SourceFile.cs" --output "report.SARIF "
Scan a directory:

bash
TaintAnalyzer --input "src/" --output "reports/" --format SARIF 
With custom rules:

bash
TaintAnalyzer --input "src/" --output "report.html" --rules "custom-rules.SARIF" --verbose
Scan with specific vulnerability types:

bash
TaintAnalyzer --input "src/" --output "report.SARIF" --vulnerabilities "sql-injection,xss,path-traversal"
Command Line Options
Option	Short	Description	Default
--input	-i	Input file or directory path	Required
--output	-o	Output file or directory path	Required