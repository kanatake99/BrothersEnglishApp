using System.Text.RegularExpressions;
using BrothersEnglishApp.Models;

namespace BrothersEnglishApp.Services
{
    public class SentenceEngine
    {
        // 正規化ロジック：記号を除去して小文字にする
        public string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            var normalized = text.ToLower().Trim();
            normalized = Regex.Replace(normalized, @"[.,!?;:]", "");
            return normalized;
        }

        // 判定処理：Targetと入力が一致するかチェック
        public bool CheckAnswer(string input, string target)
        {
            return Normalize(input) == Normalize(target);
        }

        // 表示用の穴埋め文を作成（例: with regard to -> ________）
        public string GetQuestionText(string fullEnglish, string target)
        {
            if (string.IsNullOrEmpty(target)) return fullEnglish;

            // 下線（アンダースコア）の数はTargetの長さに合わせてもいいが、
            // 固定の長さ（________）の方が見た目がスッキリするぜ
            return fullEnglish.Replace(target, "________", StringComparison.OrdinalIgnoreCase);
        }

        // Study（新規学習）用の問題を抽出
        public List<SentenceItem> GetStudyQuestions(List<SentenceItem> allSentences, UserProgress progress)
        {
            var goal = progress.Settings.SentenceStudyGoal;

            // まだ SentenceStatuses に存在しない、または Status が 0 のものから抽出
            return allSentences
                .Where(s => !progress.SentenceStatuses.ContainsKey(s.Id) || progress.SentenceStatuses[s.Id].Status == 0)
                .Take(goal)
                .ToList();
        }

        // Training（復習）用の問題を抽出
        public List<SentenceItem> GetTrainingQuestions(List<SentenceItem> allSentences, UserProgress progress)
        {
            var goal = progress.Settings.SentenceTrainingGoal;

            // 学習済み（Status == 1）の中から、古い順（LastReviewedが古い順）に抽出
            return allSentences
                .Where(s => progress.SentenceStatuses.ContainsKey(s.Id) && progress.SentenceStatuses[s.Id].Status == 1)
                .OrderBy(s => progress.SentenceStatuses[s.Id].LastReviewed)
                .Take(goal)
                .ToList();
        }

        // 学習結果を反映
        public void UpdateProgress(UserProgress progress, string sentenceId, bool isStudy)
        {
            if (!progress.SentenceStatuses.ContainsKey(sentenceId))
            {
                progress.SentenceStatuses[sentenceId] = new SentenceStatus { Id = sentenceId };
            }

            var status = progress.SentenceStatuses[sentenceId];
            status.Status = 1; // 学習済みフラグ
            status.LastReviewed = DateTime.UtcNow;

            // 今日の活動記録を更新
            var todayKey = progress.GetTodayKey();
            if (!progress.ActivityLog.ContainsKey(todayKey))
            {
                progress.ActivityLog[todayKey] = new DayActivity();
            }

            if (isStudy) progress.ActivityLog[todayKey].SentenceStudyDone = true;
            else progress.ActivityLog[todayKey].SentenceTrainingDone = true;
        }
    }
}