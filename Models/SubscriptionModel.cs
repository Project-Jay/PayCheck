using SQLite;

namespace SubscriptionTracker.Models
{
    // 구독 카테고리
    public enum SubscriptionCategory
    {
        OTT,        // 넷플릭스, 유튜브
        Music,      // 스포티파이, 멜론
        Game,       // 게임패스, PS Plus
        Software,   // Adobe, MS365
        Cloud,      // iCloud, 구글원
        News,       // 뉴스, 잡지
        Fitness,    // 헬스, 요가
        Other       // 기타
    }

    // 결제 주기
    public enum BillingCycle
    {
        Monthly,    // 월간
        Yearly,     // 연간
        Weekly      // 주간
    }

    // 통화
    public enum Currency
    {
        KRW,    // 원화
        USD,    // 달러
        JPY,    // 엔화
        EUR     // 유로
    }

    [Table("Subscriptions")]
    public class SubscriptionModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public double Amount { get; set; }
        public Currency Currency { get; set; } = Currency.KRW;
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
        public SubscriptionCategory Category { get; set; } = SubscriptionCategory.Other;
        public int BillingDay { get; set; } = 1;        // 결제일 (1~31)
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? LastUsedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string Memo { get; set; } = string.Empty;
        public string IconEmoji { get; set; } = "💳";

        // DB에 저장 안 하는 계산 프로퍼티
        [Ignore]
        public double MonthlyAmountKRW { get; set; }  // 원화 환산 월 금액

        [Ignore]
        public string AmountText => $"{Amount:N0} {CurrencySymbol}";

        [Ignore]
        public string CurrencySymbol => Currency switch
        {
            Currency.KRW => "원",
            Currency.USD => "$",
            Currency.JPY => "¥",
            Currency.EUR => "€",
            _ => "원"
        };

        [Ignore]
        public string BillingCycleText => BillingCycle switch
        {
            BillingCycle.Monthly => "월간",
            BillingCycle.Yearly => "연간",
            BillingCycle.Weekly => "주간",
            _ => "월간"
        };

        [Ignore]
        public string CategoryText => Category switch
        {
            SubscriptionCategory.OTT => "OTT",
            SubscriptionCategory.Music => "음악",
            SubscriptionCategory.Game => "게임",
            SubscriptionCategory.Software => "소프트웨어",
            SubscriptionCategory.Cloud => "클라우드",
            SubscriptionCategory.News => "뉴스",
            SubscriptionCategory.Fitness => "피트니스",
            _ => "기타"
        };

        [Ignore]
        public string CategoryEmoji => Category switch
        {
            SubscriptionCategory.OTT => "📺",
            SubscriptionCategory.Music => "🎵",
            SubscriptionCategory.Game => "🎮",
            SubscriptionCategory.Software => "💻",
            SubscriptionCategory.Cloud => "☁️",
            SubscriptionCategory.News => "📰",
            SubscriptionCategory.Fitness => "💪",
            _ => "💳"
        };

        [Ignore]
        public int DDay
        {
            get
            {
                var today = DateTime.Today;
                var nextBilling = new DateTime(today.Year, today.Month, Math.Min(BillingDay, DateTime.DaysInMonth(today.Year, today.Month)));
                if (nextBilling < today)
                    nextBilling = nextBilling.AddMonths(1);
                return (nextBilling - today).Days;
            }
        }

        [Ignore]
        public string DDayText => DDay == 0 ? "오늘 결제!" : $"D-{DDay}";

        [Ignore]
        public Color DDayColor => DDay <= 3
            ? Color.FromArgb("#EF4444")
            : DDay <= 7
                ? Color.FromArgb("#F59E0B")
                : Color.FromArgb("#10B981");

        [Ignore]
        public bool IsUnused => LastUsedDate.HasValue
            ? (DateTime.Now - LastUsedDate.Value).TotalDays > 90
            : (DateTime.Now - StartDate).TotalDays > 90;

        [Ignore]
        public string MonthlyAmountKRWText => $"≈ {MonthlyAmountKRW:N0}원/월";
    }

    // 환율 정보
    public class ExchangeRateModel
    {
        public string BaseCurrency { get; set; } = "KRW";
        public Dictionary<string, double> Rates { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }
}