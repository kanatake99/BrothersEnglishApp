namespace BrothersEnglishApp.Models;

public class AppSettings
{
    public int DailyGoal { get; set; } = 10; // おぼえる単語数
    public int StudyCount { get; set; } = 10; // クイズの回数 ★追加
    public bool IsAutoSpeechEnabled { get; set; } = true;
}