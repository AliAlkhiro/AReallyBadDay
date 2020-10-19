using System.Collections.Generic;

[System.Serializable]
public class Answer {
    public string Label;
    public int NextQuestionId;
    public Dictionary<string, bool> RequireFlag;
    public Dictionary<string, bool> ChangeFlag;
}