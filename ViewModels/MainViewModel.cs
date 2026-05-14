using SubscriptionTracker.Models;
using SubscriptionTracker.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SubscriptionTracker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private readonly ExchangeRateService _exchangeService;
        private readonly NotificationService _notificationService;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            _exchangeService = new ExchangeRateService();
            _notificationService = new NotificationService();
            Subscriptions = new ObservableCollection<SubscriptionModel>();

            LoadCommand = new Command(async () => await LoadAsync());
            AddCommand = new Command(async () => await AddAsync());
            DeleteCommand = new Command<SubscriptionModel>(async (sub) => await DeleteAsync(sub));
            CheckUnusedCommand = new Command(async () => await CheckUnusedAsync());
            RefreshExchangeCommand = new Command(async () => await RefreshExchangeAsync());

            _ = LoadAsync();
        }

        public ObservableCollection<SubscriptionModel> Subscriptions { get; }

        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CheckUnusedCommand { get; }
        public ICommand RefreshExchangeCommand { get; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private double _totalMonthlyKRW;
        public double TotalMonthlyKRW
        {
            get => _totalMonthlyKRW;
            set { _totalMonthlyKRW = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalMonthlyText)); OnPropertyChanged(nameof(TotalYearlyText)); }
        }

        public string TotalMonthlyText => $"{TotalMonthlyKRW:N0}원";
        public string TotalYearlyText => $"{TotalMonthlyKRW * 12:N0}원";

        private string _exchangeRateText = string.Empty;
        public string ExchangeRateText
        {
            get => _exchangeRateText;
            set { _exchangeRateText = value; OnPropertyChanged(); }
        }
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsListTabVisible));
                OnPropertyChanged(nameof(IsAddTabVisible));
            }
        }

        public bool IsListTabVisible => SelectedTabIndex == 0;
        public bool IsAddTabVisible => SelectedTabIndex == 1;

        // 추가 폼 프로퍼티
        private string _newName = string.Empty;
        public string NewName
        {
            get => _newName;
            set { _newName = value; OnPropertyChanged(); }
        }

        private string _newAmount = string.Empty;
        public string NewAmount
        {
            get => _newAmount;
            set { _newAmount = value; OnPropertyChanged(); }
        }

        private int _newBillingDay = 1;
        public int NewBillingDay
        {
            get => _newBillingDay;
            set { _newBillingDay = value; OnPropertyChanged(); }
        }

        private Currency _newCurrency = Currency.KRW;
        public Currency NewCurrency
        {
            get => _newCurrency;
            set { _newCurrency = value; OnPropertyChanged(); }
        }

        private BillingCycle _newBillingCycle = BillingCycle.Monthly;
        public BillingCycle NewBillingCycle
        {
            get => _newBillingCycle;
            set { _newBillingCycle = value; OnPropertyChanged(); }
        }

        private SubscriptionCategory _newCategory = SubscriptionCategory.Other;
        public SubscriptionCategory NewCategory
        {
            get => _newCategory;
            set { _newCategory = value; OnPropertyChanged(); }
        }

        private string _newMemo = string.Empty;
        public string NewMemo
        {
            get => _newMemo;
            set { _newMemo = value; OnPropertyChanged(); }
        }

        // Picker용 리스트
        public List<string> CurrencyList => Enum.GetNames(typeof(Currency)).ToList();
        public List<string> BillingCycleList => new() { "월간", "연간", "주간" };
        public List<string> CategoryList => new()
        {
            "OTT", "음악", "게임", "소프트웨어", "클라우드", "뉴스", "피트니스", "기타"
        };

        private async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var list = await _dbService.GetAllAsync();
                Subscriptions.Clear();

                double total = 0;
                foreach (var sub in list)
                {
                    sub.MonthlyAmountKRW = await _exchangeService.GetMonthlyKRWAsync(sub);
                    total += sub.MonthlyAmountKRW;
                    Subscriptions.Add(sub);
                }

                TotalMonthlyKRW = total;

                // 결제 알림 등록
                await _notificationService.SchedulePaymentNotificationsAsync(list);

                // 환율 정보 표시
                var rates = await _exchangeService.GetRatesAsync();
                ExchangeRateText = $"1$ = {1 / rates.Rates["USD"]:N0}원  |  100¥ = {100 / rates.Rates["JPY"]:N0}원";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddAsync()
        {
            if (string.IsNullOrWhiteSpace(NewName) ||
                string.IsNullOrWhiteSpace(NewAmount) ||
                !double.TryParse(NewAmount, out var amount))
                return;

            var sub = new SubscriptionModel
            {
                Name = NewName,
                Amount = amount,
                Currency = NewCurrency,
                BillingCycle = NewBillingCycle,
                Category = NewCategory,
                BillingDay = NewBillingDay,
                Memo = NewMemo,
                IconEmoji = NewCategory switch
                {
                    SubscriptionCategory.OTT => "📺",
                    SubscriptionCategory.Music => "🎵",
                    SubscriptionCategory.Game => "🎮",
                    SubscriptionCategory.Software => "💻",
                    SubscriptionCategory.Cloud => "☁️",
                    SubscriptionCategory.News => "📰",
                    SubscriptionCategory.Fitness => "💪",
                    _ => "💳"
                }
            };

            await _dbService.AddAsync(sub);

            // 폼 초기화
            NewName = string.Empty;
            NewAmount = string.Empty;
            NewBillingDay = 1;
            NewMemo = string.Empty;

            await LoadAsync();
            SelectedTabIndex = 0;
        }

        private async Task DeleteAsync(SubscriptionModel sub)
        {
            await _dbService.DeleteAsync(sub);
            await LoadAsync();
        }

        private async Task CheckUnusedAsync()
        {
            var unused = await _dbService.GetUnusedAsync();
            if (unused.Any())
                await _notificationService.NotifyUnusedSubscriptionsAsync(unused);
            else
                await Application.Current!.MainPage!.DisplayAlert(
                    "미사용 구독 없음", "모든 구독이 최근 90일 이내에 사용되었습니다.", "확인");
        }

        private async Task RefreshExchangeAsync()
        {
            var rates = await _exchangeService.GetRatesAsync();
            ExchangeRateText = $"1$ = {1 / rates.Rates["USD"]:N0}원  |  100¥ = {100 / rates.Rates["JPY"]:N0}원";
            await LoadAsync();
        }
    }
}