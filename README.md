# 💳 PayCheck

> **구독 서비스 가계부** — .NET MAUI 기반 크로스플랫폼 앱

![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20Windows-brightgreen)
![Framework](https://img.shields.io/badge/.NET-9.0%20MAUI-512BD4)
![Language](https://img.shields.io/badge/Language-C%23-239120)

---

## 📱 소개

PayCheck는 넷플릭스, 유튜브 프리미엄 등 구독 서비스를 한 곳에서 관리하는 앱입니다.
결제일 D-Day 알림, 환율 자동 환산, 미사용 구독 감지까지 지원합니다.

---

## ✨ 주요 기능

| 기능 | 설명 |
|------|------|
| 📋 **구독 관리** | 이름, 금액, 결제일, 주기, 카테고리 등록/삭제 |
| 💰 **월간/연간 지출** | 전체 구독 월간 합계 및 연간 예상 지출 |
| ⏰ **D-Day 표시** | 결제일까지 남은 일수 실시간 표시 |
| 💱 **환율 자동 환산** | USD/JPY/EUR → KRW 자동 환산 |
| ⚠️ **미사용 구독 감지** | 90일 이상 미사용 구독 알림 |
| 🔔 **로컬 알림** | 결제 예정 Push 알림 |

---

## 🎨 디자인

따뜻한 다크 퍼플 테마 (#1A0A2E) 기반의 모던 UI

---

## 🛠️ 기술 스택

| 분류 | 기술 |
|------|------|
| Framework | .NET 9 MAUI |
| Language | C# / XAML |
| Architecture | MVVM (INotifyPropertyChanged) |
| Database | SQLite (sqlite-net-pcl) |
| 환율 API | ExchangeRate-API (무료) |
| 알림 | Plugin.LocalNotification 11.1.4 |

---

## 📁 프로젝트 구조

```
SubscriptionTracker/
├── Converters/Converters.cs
├── Helpers/Constants.cs
├── Models/SubscriptionModel.cs
├── Services/
│   ├── DatabaseService.cs
│   ├── ExchangeRateService.cs
│   └── NotificationService.cs
├── ViewModels/MainViewModel.cs
└── MainPage.xaml
```

---

## 🚀 시작하기

**1. 레포지토리 클론**
```bash
git clone https://github.com/Project-Jay/PayCheck.git
```

**2. API Key 설정** (`Helpers/Constants.cs`)
```csharp
public const string ExchangeRateApiKey = "YOUR_API_KEY";
```

**3. 빌드 & 실행**
```bash
dotnet build -f net9.0-android -c Debug
```

---

## 🔧 트러블슈팅

| 문제 | 원인 | 해결 |
|------|------|------|
| 탭 전환 안 됨 | InverseBoolConverter int 오작동 | IsListTabVisible 프로퍼티 직접 사용 |
| StrokeThickness 오류 | MAUI는 단일 값만 지원 | 숫자 하나로 변경 |
| Command ExecuteAsync 오류 | MAUI Command는 ExecuteAsync 미지원 | Execute로 변경 |

---

## 👨‍💻 개발자

**Project-Jay** · [GitHub](https://github.com/Project-Jay)
