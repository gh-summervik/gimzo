using Gimzo.AppServices.Backtests;
using System.Text;

namespace Gimzo.Admin.Cli;

internal partial class CliInteractive
{
    private bool CheckForQuit(string? choice) =>
        choice?.Equals("Q", StringComparison.OrdinalIgnoreCase) ?? false;

    private static void WriteDashes(int number = 40, bool blankLineBefore = false, bool blankLineAfter = false)
    {
        var num = Math.Max(0, Math.Min(100, number));

        StringBuilder sb = new();
        if (blankLineBefore)
            sb.Append(Environment.NewLine);
        sb.Append(new string('-', num));
        if (blankLineAfter)
            sb.Append(Environment.NewLine);

        Console.WriteLine(sb.ToString());
    }

    public static bool ConfirmActionWithUser(string message, string? defaultAnswer = "n")
    {
        string[] answerOptions = ["y", "n"];

        StringBuilder sb = new(message.Trim());

        if (answerOptions.Length == 0)
            return false;

        for (int i = 0; i < answerOptions.Length; i++)
        {
            if (answerOptions[i].Equals(defaultAnswer, StringComparison.OrdinalIgnoreCase))
                answerOptions[i] = answerOptions[i].ToUpper();
            else
                answerOptions[i] = answerOptions[i].ToLower();
        }

        sb.Append($" ({string.Join('/', answerOptions)}) ");

        Console.Write(sb.ToString());

        var answer = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(answer))
            answer = defaultAnswer;

        return answer?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private void Quit()
    {
        Console.WriteLine($"Interactive session complete.{Environment.NewLine}");
        Environment.Exit(0);
    }
    private void CancellationMessage() => Console.WriteLine("Action cancelled by user.");
}
