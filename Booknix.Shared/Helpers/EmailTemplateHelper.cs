namespace Booknix.Shared.Helpers;

public static class EmailTemplateHelper
{
    public static string LoadTemplate(string templateName, Dictionary<string, string> replacements)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", $"{templateName}.html");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Email template not found: {templateName}");

        var content = File.ReadAllText(path);

        foreach (var kvp in replacements)
        {
            content = content.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }

        return content;
    }
}
