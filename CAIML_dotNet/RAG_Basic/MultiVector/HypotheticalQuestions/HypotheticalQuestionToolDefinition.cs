// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace MultiVector.HypotheticalQuestions;

public class HypotheticalQuestionToolDefinition
{
    public string type { get; set; } = "object";
    public Properties properties { get; set; } = new();
    public string[] required { get; set; } = ["Questions"];
}

public class Properties
{
    public QuestionProperty Questions { get; set; } = new();
}

public class QuestionProperty
{
    public string type { get; set; } = "array";
    public Item items { get; set; } = new();
}

public class Item
{
    public string type { get; set; } = "string";
}