using System.Net.Http;
using System.Text.Json;
using SubscriptionTracker.Helpers;
using SubscriptionTracker.Models;

namespace SubscriptionTracker.Services
{
    public class ExchangeRateService
    {
        private readonly HttpClient _httpClient = new();
        private ExchangeRateModel? _cachedRates;
        private DateTime _lastFetched = DateTime.MinValue;

        public async Task<ExchangeRateModel> GetRatesAsync()
        {
            // 1시간 캐시
            if (_cachedRates != null && (DateTime.Now - _lastFetched).TotalHours < 1)
                return _cachedRates;

            try
            {
                var url = $"{Constants.ExchangeRateApiUrl}/{Constants.ExchangeRateApiKey}/latest/KRW";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return GetDefaultRates();

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var rates = doc.RootElement.GetProperty("conversion_rates");

                _cachedRates = new ExchangeRateModel
                {
                    BaseCurrency = "KRW",
                    LastUpdated = DateTime.Now,
                    Rates = new Dictionary<string, double>
                    {
                        ["USD"] = rates.GetProperty("USD").GetDouble(),
                        ["JPY"] = rates.GetProperty("JPY").GetDouble(),
                        ["EUR"] = rates.GetProperty("EUR").GetDouble(),
                        ["KRW"] = 1.0
                    }
                };

                _lastFetched = DateTime.Now;
                return _cachedRates;
            }
            catch
            {
                return GetDefaultRates();
            }
        }

        // 원화로 변환
        public async Task<double> ToKRWAsync(double amount, Currency currency)
        {
            if (currency == Currency.KRW) return amount;

            var rates = await GetRatesAsync();
            var currencyCode = currency.ToString();

            if (rates.Rates.TryGetValue(currencyCode, out var rate) && rate > 0)
                return amount / rate;

            return amount;
        }

        // 월간 원화 금액 계산
        public async Task<double> GetMonthlyKRWAsync(SubscriptionModel sub)
        {
            var amountKRW = await ToKRWAsync(sub.Amount, sub.Currency);

            return sub.BillingCycle switch
            {
                BillingCycle.Monthly => amountKRW,
                BillingCycle.Yearly => amountKRW / 12,
                BillingCycle.Weekly => amountKRW * 4.33,
                _ => amountKRW
            };
        }

        // API 키 없을 때 기본값
        private ExchangeRateModel GetDefaultRates() => new()
        {
            BaseCurrency = "KRW",
            LastUpdated = DateTime.Now,
            Rates = new Dictionary<string, double>
            {
                ["USD"] = 0.00075,   // 1 KRW = 0.00075 USD (≈ 1340원)
                ["JPY"] = 0.107,     // 1 KRW = 0.107 JPY (≈ 9.3원)
                ["EUR"] = 0.00069,   // 1 KRW = 0.00069 EUR (≈ 1450원)
                ["KRW"] = 1.0
            }
        };
    }
}