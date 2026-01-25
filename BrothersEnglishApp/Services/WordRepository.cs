// Services/WordRepository.cs
using System.Net.Http.Json;
using BrothersEnglishApp.Models;

namespace BrothersEnglishApp.Services;

public class WordRepository(HttpClient http)
{
    private List<EnglishWord>? _cachedWords;

    public async Task<List<EnglishWord>> GetWordsAsync()
    {
        // すでに読み込み済みなら、それを返す（何度も通信しない）
        if (_cachedWords != null) return _cachedWords;

        // 通信して JSON を取得
        _cachedWords = await http.GetFromJsonAsync<List<EnglishWord>>("data/master_words.json");

        return _cachedWords ?? [];
    }
}