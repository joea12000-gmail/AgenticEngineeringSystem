using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgenticEngineeringSystem.Tests;

[TestClass]
public class PocFileExistsTests
{
    [TestMethod]
    public void Agentic_POC_Html_File_Exists()
    {
        // Path relative to repo root when tests run from solution folder
        var fileName = "Agentic_Engineering_System_POC.html";

        // Search upward from the test assembly base directory for a docs folder containing the file.
        var dir = new DirectoryInfo(AppContext.BaseDirectory!);
        bool found = false;
        string? foundPath = null;
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "docs", fileName);
            if (File.Exists(candidate))
            {
                found = true;
                foundPath = candidate;
                break;
            }
            dir = dir.Parent;
        }

        Assert.IsTrue(found, $"Expected POC file at 'docs/{fileName}' to exist (searched up from {AppContext.BaseDirectory}). Found at: {foundPath}");
    }
}
