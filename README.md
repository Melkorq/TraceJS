# TraceJS

**TraceJS** is a simple yet powerful C# console application designed to help you identify URLs and paths within a given web page.
It analyzes both inline and external JavaScript as well as the raw HTML content, using regular expressions to extract potential endpoints, file paths, and network calls — making it a handy tool for understanding what’s happening behind the scenes of any website.

## 🚀 Getting Started

### Prerequisites

- [.NET 6.0 SDK or later](https://dotnet.microsoft.com/en-us/download)
- Internet connection

### Run the project

```bash
dotnet build
dotnet run
```
You will see a simple menu in your console:
```
1 - Show inline JavaScript code from the page  
2 - Extract URLs and paths from the page's HTML 
3 - List external JavaScript file URLs
4 - Analyze paths inside external JavaScript files 
5 - Exit  
```

## 🧠 Regex Power
This tool uses a well-known pattern inspired by [LinkFinder](https://github.com/GerbenJavado/LinkFinder) to extract:
- API endpoints
- Path references
- File requests
- Hidden resources

## 🛠 Planned Features
-  Save results to file
-  URL deduplication and filtering
-  Multi-threaded JS analysis for performance boost
