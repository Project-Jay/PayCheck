using Plugin.LocalNotification;
using SubscriptionTracker.Models;

namespace SubscriptionTracker.Services
{
    public class NotificationService
    {
        // 결제 예정 알림 등록
        public async Task SchedulePaymentNotificationsAsync(List<SubscriptionModel> subscriptions)
        {
            // 기존 알림 취소
            LocalNotificationCenter.Current.CancelAll();

            foreach (var sub in subscriptions)
            {
                if (sub.DDay <= 7 && sub.DDay >= 0)
                {
                    var notification = new NotificationRequest
                    {
                        NotificationId = sub.Id,
                        Title = $"💳 {sub.Name} 결제 예정",
                        Description = sub.DDay == 0
                            ? $"오늘 {sub.AmountText} 결제됩니다!"
                            : $"{sub.DDay}일 후 {sub.AmountText} 결제 예정",
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = DateTime.Now.AddSeconds(5) // 테스트용
                        }
                    };

                    await LocalNotificationCenter.Current.Show(notification);
                }
            }
        }

        // 미사용 구독 알림
        public async Task NotifyUnusedSubscriptionsAsync(List<SubscriptionModel> unused)
        {
            if (!unused.Any()) return;

            var names = string.Join(", ", unused.Take(3).Select(s => s.Name));
            var notification = new NotificationRequest
            {
                NotificationId = 9999,
                Title = "⚠️ 미사용 구독 감지",
                Description = $"{names} 등 {unused.Count}개의 구독이 90일 이상 미사용 중입니다.",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddSeconds(3)
                }
            };

            await LocalNotificationCenter.Current.Show(notification);
        }
    }
}