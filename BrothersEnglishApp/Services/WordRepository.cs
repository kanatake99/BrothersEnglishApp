// Services/WordRepository.cs
using System.Net.Http.Json;
using BrothersEnglishApp.Models;

namespace BrothersEnglishApp.Services;

public class WordRepository(HttpClient http)
{
    private List<EnglishWord>? _cachedWords;
    private List<SentenceItem>? _cachedSentences; // 追加：文章のキャッシュ

    public async Task<List<EnglishWord>> GetWordsAsync()
    {
        if (_cachedWords != null) return _cachedWords;
        _cachedWords = await http.GetFromJsonAsync<List<EnglishWord>>("data/master_words.json");
        return _cachedWords ?? new List<EnglishWord>();
    }

    /// <summary>
    /// 全文章データを取得するぜ。
    /// data/master_sentences.json が存在することを前提としている。
    /// </summary>
    public async Task<List<SentenceItem>> GetSentencesAsync()
    {
        if (_cachedSentences != null) return _cachedSentences;
        _cachedSentences = await http.GetFromJsonAsync<List<SentenceItem>>("data/master_sentences.json");
        return _cachedSentences ?? new List<SentenceItem>();
    }
}