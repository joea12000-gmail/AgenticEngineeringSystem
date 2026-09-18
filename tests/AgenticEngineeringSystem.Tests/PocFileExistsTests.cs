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
        var path = Path.Combine("docs", "Agentic_Engineering_System_POC.html");
        Assert.IsTrue(File.Exists(path), $"Expected POC file at '{path}' to exist.");
    }
}
