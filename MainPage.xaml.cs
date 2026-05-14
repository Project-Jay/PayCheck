using SubscriptionTracker.Models;
using SubscriptionTracker.ViewModels;

namespace SubscriptionTracker
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel _viewModel => (MainViewModel)BindingContext;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnListTabTapped(object sender, TappedEventArgs e)
            => _viewModel.SelectedTabIndex = 0;

        private void OnAddTabTapped(object sender, TappedEventArgs e)
            => _viewModel.SelectedTabIndex = 1;

        private void OnAddTapped(object sender, TappedEventArgs e)
            => _viewModel.AddCommand.Execute(null);

        private async void OnDeleteTapped(object sender, TappedEventArgs e)
        {
            if (sender is Border border && border.BindingContext is SubscriptionModel sub)
            {
                var confirm = await DisplayAlert(
                    "삭제 확인",
                    $"{sub.Name} 구독을 삭제할까요?",
                    "삭제", "취소");

                if (confirm)
                    _viewModel.DeleteCommand.Execute(sub);
            }
        }

        private void OnRefreshExchangeTapped(object sender, TappedEventArgs e)
            => _viewModel.RefreshExchangeCommand.Execute(null);

        private void OnCheckUnusedTapped(object sender, TappedEventArgs e)
            => _viewModel.CheckUnusedCommand.Execute(null);

        private void OnCurrencyChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker)
                _viewModel.NewCurrency = (Currency)picker.SelectedIndex;
        }

        private void OnBillingCycleChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker)
                _viewModel.NewBillingCycle = (BillingCycle)picker.SelectedIndex;
        }

        private void OnCategoryChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker)
                _viewModel.NewCategory = (SubscriptionCategory)picker.SelectedIndex;
        }
    }
}