using System.Net.Http.Json;
using BrothersEnglishApp.Models;

namespace BrothersEnglishApp.Services
{
    public class SentenceRepository
    {
        private readonly HttpClient _http;
        private List<SentenceItem>? _cache;

        public SentenceRepository(HttpClient http) => _http = http;

        public async Task<List<SentenceItem>> GetSentencesAsync()
        {
            if (_cache != null) return _cache;

            // ファイル名も master_sentences.json で固定だ
            _cache = await _http.GetFromJsonAsync<List<SentenceItem>>("data/master_sentences.json");
            return _cache ?? new List<SentenceItem>();
        }
    }
}