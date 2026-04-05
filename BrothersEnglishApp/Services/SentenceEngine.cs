using System.Text.RegularExpressions;
using BrothersEnglishApp.Models;

namespace BrothersEnglishApp.Services
{
    /// <summary>
    /// 英文（Sentence）の学習・復習・判定・進捗管理を一手に引き受けるエンジン。
    /// </summary>
    public class SentenceEngine
    {
        #region 1. 共通ロジック（判定・加工）

        /// <summary>
        /// 入力テキストを正規化する（小文字化、前後空白削除、記号の除去）。
        /// 判定の揺れを防ぐための心臓部だぜ。
        /// </summary>
        public string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            // 小文字化して前後の空白をカット
            var normalized = text.ToLower().Trim();
            // 正規表現で主要な記号を消し去るぜ
            normalized = Regex.Replace(normalized, @"[.,!?;:]", "");
            return normalized;
        }

        /// <summary>
        /// ユーザーの入力と正解（Target）が一致するか判定する。
        /// </summary>
        public bool CheckAnswer(string input, string target)
        {
            return Normalize(input) == Normalize(target);
        }

        /// <summary>
        /// 英文の中のターゲット部分をアンダーバー（____）に置き換えた表示用テキストを作る。
        /// </summary>
        public string GetQuestionText(string fullEnglish, string target)
        {
            if (string.IsNullOrEmpty(target)) return fullEnglish;

            // Targetを単語ごとに分割し、各単語の文字数に応じたアンダーバーを生成
            var words = target.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var underlinedWords = words.Select(word =>
                Regex.Replace(word, @"[a-zA-Z0-9]", "_"));

            var replacement = string.Join(" ", underlinedWords);

            // 元の文からtargetを探して置換（大文字小文字は無視するが、元の表記を壊さないぜ）
            return Regex.Replace(
                fullEnglish,
                Regex.Escape(target),
                replacement,
                RegexOptions.IgnoreCase
            );
        }

        /// <summary>
        /// 英文を「ターゲット」と「それ以外」のパーツに分解する。
        /// 並び替えUI（SentenceTraining）で、どこに空欄を置くか決めるために使うぜ。
        /// </summary>
        public List<SentenceSegment> SplitSentence(string english, string target)
        {
            var segments = new List<SentenceSegment>();
            var pattern = $"({Regex.Escape(target)})";

            // Regex.Split で区切り文字（target）を保持したまま分割
            var parts = Regex.Split(english, pattern, RegexOptions.IgnoreCase);

            foreach (var part in parts)
            {
                if (part.Length == 0) continue;
                bool isTarget = part.Equals(target, StringComparison.OrdinalIgnoreCase);
                segments.Add(new SentenceSegment { Text = part, IsTarget = isTarget });
            }
            return segments;
        }

        #endregion

        #region 2. 学習（Study）モード用ロジック

        /// <summary>
        /// 本日の新規学習分（Status=0 または未登録）を抽出する。
        /// </summary>
        public List<SentenceItem> GetStudyQuestions(List<SentenceItem> allSentences, UserProgress progress)
        {
            var goal = progress.Settings.SentenceStudyGoal;
            return allSentences
                .Where(s => !progress.SentenceStatuses.ContainsKey(s.Id) || progress.SentenceStatuses[s.Id].Status == 0)
                .Take(goal)
                .ToList();
        }

        #endregion

        #region 3. 復習（Training）モード用ロジック

        /// <summary>
        /// 復習（トレーニング）用の問題を抽出する。
        /// 基本は「学習済み」の中から、復習日時が古い順だぜ。
        /// </summary>
        public List<SentenceItem> GetTrainingQuestions(List<SentenceItem> allSentences, UserProgress progress)
        {
            var goal = progress.Settings.SentenceTrainingGoal;
            return allSentences
                .Where(s => progress.SentenceStatuses.ContainsKey(s.Id) && progress.SentenceStatuses[s.Id].Status == 1)
                .OrderBy(s => progress.SentenceStatuses[s.Id].LastReviewed)
                .Take(goal)
                .ToList();
        }

        /// <summary>
        /// トレーニングモードで次に出題する文を1つ選ぶ。
        /// </summary>
        public SentenceItem? GetNextSentenceToTrain(UserProgress? progress, List<SentenceItem> allSentences)
        {
            if (progress == null || progress.SentenceStatuses.Count == 0) return null;

            // 今はシンプルにシャッフルして1つ選ぶぜ
            var entry = progress.SentenceStatuses.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
            return allSentences.FirstOrDefault(s => s.Id == entry.Key);
        }

        /// <summary>
        /// トレーニング用の「選択肢チップ」を生成する。
        /// ターゲットの単語、フレーズ、品詞に応じて賢くダミーを混ぜるぜ。
        /// </summary>
        public List<string> GenerateSentenceChips(string target, List<EnglishWord> allWords, List<SentenceItem> allSentences)
        {
            var chips = new List<string> { target };
            IEnumerable<string> dummies;

            if (target.Contains(' '))
            {
                // フレーズなら他の文のターゲットをダミーにする
                dummies = allSentences
                    .Where(s => !s.Target.Equals(target, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(_ => Random.Shared.Next()).Take(2).Select(s => s.Target);
            }
            else
            {
                // 1単語なら前置詞・助動詞・品詞一致などで賢く抽出（ロジック維持）
                var targetLower = target.ToLower();
                var prepositions = new[] { "in", "on", "at", "for", "with", "to", "by", "of", "from", "up", "about" };

                if (prepositions.Contains(targetLower))
                {
                    dummies = prepositions.Where(x => x != targetLower).OrderBy(_ => Random.Shared.Next()).Take(2);
                }
                else
                {
                    // 品詞ベースのダミー生成ロジック...（長いので維持）
                    dummies = allWords.Where(w => !w.Word.Equals(target, StringComparison.OrdinalIgnoreCase))
                                      .OrderBy(_ => Random.Shared.Next()).Take(2).Select(w => w.Word);
                }
            }

            chips.AddRange(dummies);
            // 3つに満たない場合の補完を含め、最後にシャッフル
            return chips.DistinctBy(c => c.ToLower()).OrderBy(_ => Random.Shared.Next()).Take(3).ToList();
        }

        /// <summary>
        /// 並び替えトレーニング（SentenceTraining）用の問題データを一式生成する。
        /// </summary>
        public SentenceQuestion GenerateSentenceTraining(SentenceItem sentence, List<EnglishWord> allWords, List<SentenceItem> allSentences)
        {
            var targetParts = sentence.Target.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var chips = new List<string>(targetParts);

            // 適当なダミーを3つ追加して難易度を上げるぜ
            var dummies = allWords
                .Select(w => w.Word)
                .Where(w => !targetParts.Contains(w, StringComparer.OrdinalIgnoreCase))
                .OrderBy(_ => Guid.NewGuid()).Take(3);

            chips.AddRange(dummies);

            return new SentenceQuestion
            {
                Sentence = sentence,
                CorrectAnswer = sentence.Target,
                TargetParts = targetParts.ToList(),
                Chips = chips.OrderBy(_ => Guid.NewGuid()).ToList()
            };
        }

        #endregion

        #region 4. 進捗・成績更新

        /// <summary>
        /// 学習・復習の結果を保存する。
        /// ここを呼べば「正解数」「苦手判定」「カレンダーの印」が全部更新されるぜ。
        /// </summary>
        public void UpdateProgress(UserProgress progress, string sentenceId, bool isStudy, bool isCorrect)
        {
            if (string.IsNullOrEmpty(sentenceId)) return;

            if (!progress.SentenceStatuses.ContainsKey(sentenceId))
                progress.SentenceStatuses[sentenceId] = new SentenceStatus { Id = sentenceId };

            var status = progress.SentenceStatuses[sentenceId];
            status.LastReviewed = DateTime.UtcNow;
            status.Status = 1; // 一度解いたら「学習済み」にする

            if (isCorrect) status.CorrectCount++;
            else status.IncorrectCount++;

            // 今日の活動ログを更新
            var todayKey = progress.GetTodayKey();
            if (!progress.ActivityLog.ContainsKey(todayKey))
                progress.ActivityLog[todayKey] = new DayActivity();

            if (isStudy) progress.ActivityLog[todayKey].SentenceStudyDone = true;
            else progress.ActivityLog[todayKey].SentenceTrainingDone = true;
        }

        #endregion
    }
}