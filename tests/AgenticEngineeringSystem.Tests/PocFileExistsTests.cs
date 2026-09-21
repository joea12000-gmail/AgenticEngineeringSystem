using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgenticEngineeringSystem.Tests;

[TestClass]
public class PocFileExistsTests
{
    [TestMethod]
    public void Agentic_POC_Html_File_Exists()
    {
        // Try to locate the docs file by walking up the directory tree from the test's working directory
        var fileName = Path.Combine("docs", "Agentic_Engineering_System_POC.html");
        var dir = Directory.GetCurrentDirectory();
        var found = false;
        for (var i = 0; i < 6 && dir is not null; i++)
        {
            var candidate = Path.Combine(dir, fileName);
            if (File.Exists(candidate))
            {
                found = true;
                break;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        Assert.IsTrue(found, $"Expected POC file at '{fileName}' to exist in repository root or parent directories.");
    }
}
