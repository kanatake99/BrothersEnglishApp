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
        // 表示用の穴埋め文を作成（例: difficult to predict -> _________ __ _______）
        public string GetQuestionText(string fullEnglish, string target)
        {
            if (string.IsNullOrEmpty(target)) return fullEnglish;

            // 1. Targetを単語ごとに分割する（スペース区切り）
            var words = target.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 2. 各単語を「文字数分のアンダーバー」に変換する
            // 例: "difficult" -> "_________" (9文字)
            var underlinedWords = words.Select(word =>
            {
                // 単語に含まれる記号（カンマやピリオド）はそのまま残すと親切だぜ
                return Regex.Replace(word, @"[a-zA-Z0-9]", "_");
            });

            // 3. 半角スペースで繋ぎ直す
            var replacement = string.Join(" ", underlinedWords);

            // 4. 全文の中の target 部分を、生成したアンダーバー群に置き換える
            // Regex.Replace を使うと大文字小文字を無視しつつ正確に置換できるぜ
            return Regex.Replace(
                fullEnglish,
                Regex.Escape(target),
                replacement,
                RegexOptions.IgnoreCase
            );
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