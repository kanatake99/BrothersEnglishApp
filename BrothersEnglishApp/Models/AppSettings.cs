namespace BrothersEnglishApp.Models;

public class AppSettings
{
    public int DailyGoal { get; set; } = 10;
    // 追加：自動再生の設定（デフォルトはオン）
    public bool IsAutoSpeechEnabled { get; set; } = true;
}