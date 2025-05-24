# Booknix

Booknix, .NET 8.0 ile geliştirilmiş modern bir randevu planlama ve hizmet yönetim sistemidir. Temiz mimari yaklaşımıyla randevuları, hizmetleri ve lokasyonları yönetmek için kapsamlı bir çözüm sunar.

🌐 **Demo Site:** [https://booknix.ismailparlak.com](https://booknix.ismailparlak.com)

## 🚀 Özellikler

- Kullanıcı kimlik doğrulama ve yetkilendirme
- Randevu planlama sistemi
- Hizmet ve lokasyon yönetimi
- SignalR ile gerçek zamanlı bildirimler
- Kuyruk sistemi ile e-posta bildirimleri
- Değerlendirme sistemi
- Çalışan yönetimi ve planlaması
- Sektör bazlı organizasyon
- Modern tasarımlı duyarlı kullanıcı arayüzü

## 🏗️ Proje Yapısı

Çözüm, Temiz Mimari prensiplerini takip eder ve aşağıdaki projelerden oluşur:

- **Booknix.MVCUI**: ASP.NET Core MVC kullanan web uygulama katmanı
- **Booknix.Application**: Uygulama iş kuralları ve kullanım senaryoları
- **Booknix.Domain**: İş kuralları ve varlıklar
- **Booknix.Infrastructure**: Framework'ler ve harici bağımlılıklar
- **Booknix.Persistence**: Veritabanı erişimi ve repository'ler
- **Booknix.Shared**: Paylaşılan yardımcı programlar ve yapılandırmalar

## 🛠️ Teknoloji Altyapısı

- .NET 8.0
- ASP.NET Core MVC
- Entity Framework Core
- PostgreSQL
- Gerçek zamanlı özellikler için SignalR
- E-posta işlevselliği için MailKit
- Stil için Tailwind CSS
- jQuery ve modern JavaScript

## 📋 Gereksinimler

- .NET 8.0 SDK
- PostgreSQL
- E-posta işlevselliği için SMTP sunucusu

## 🔧 Ortam Kurulumu

1. Kök dizinde aşağıdaki değişkenleri içeren bir `.env` dosyası oluşturun:

```env
DB_HOST=veritabani_sunucusu
DB_PORT=veritabani_portu
DB_NAME=veritabani_adi
DB_USER=veritabani_kullanici
DB_PASSWORD=veritabani_sifresi

EMAIL_HOST=smtp_sunucusu
EMAIL_PORT=smtp_portu
EMAIL_USE_SSL=true/false
EMAIL_HOST_USER=eposta_kullanici
EMAIL_HOST_PASSWORD=eposta_sifresi
```

2. `appsettings.json` ve ortama özel ayar dosyalarını uygun değerlerle güncelleyin.

## 🚀 Başlangıç

1. Depoyu klonlayın
2. Ortam değişkenlerini ayarlayın
3. Aşağıdaki komutları çalıştırın:

```bash
dotnet restore
dotnet build
dotnet run --project Booknix.MVCUI
```

Uygulama şu adreslerde erişilebilir olacaktır:

- Geliştirme: http://localhost:5122
- Üretim: https://booknix.ismailparlak.com

## 🔐 Güvenlik Özellikleri

- BCrypt ile güvenli şifre hashleme
- Oturum doğrulama middleware'i
- Token tabanlı kimlik doğrulama
- Denetim günlüğü
- Güvenilir IP yönetimi

## 📧 E-posta Sistemi

Uygulama, aşağıdaki özelliklere sahip sağlam bir e-posta sistemi içerir:

- Daha iyi performans için e-posta kuyruğu
- E-posta kuyruğunun arka planda işlenmesi
- Geliştirme modunda e-posta yönlendirme
- HTML e-posta şablonları

## 🔄 Gerçek Zamanlı Özellikler

- SignalR ile gerçek zamanlı bildirimler
- Randevular için canlı güncellemeler
- Anlık durum değişiklikleri

## 📱 Kullanıcı Arayüzü

- Duyarlı tasarım
- Tailwind CSS ile modern arayüz
- İstemci tarafı doğrulama
- Aşamalı geliştirme
- Mobil uyumlu arayüz

## 📸 Ekran Görüntüleri

> Ekran görüntüleri yakında eklenecektir.

## 📝 Lisans

Telif Hakkı © 2025 Booknix. Tüm hakları saklıdır.

## 🤝 Katkıda Bulunma

1. Depoyu fork edin
2. Özellik dalınızı oluşturun
3. Değişikliklerinizi commit edin
4. Dalınıza push yapın
5. Yeni bir Pull Request oluşturun
