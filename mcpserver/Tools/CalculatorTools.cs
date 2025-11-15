using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCPServerTest.Tools;

[McpServerToolType]
internal class CalculatorTools
{
    [McpServerTool(Name = "add_two_numbers", Title = "Adds two numbers and returns the sum"),
     Description("Calculates the sum of two numbers")]
    public static int CalculateSum(int num1, int num2)
    {
        int result = num1 + num2;
        return result;
    }
}