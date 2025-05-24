# Booknix

## 📑 İçindekiler

- [Demo Site](#demo-site)
- [Özellikler](#özellikler)
- [Proje Yapısı](#proje-yapısı)
- [Teknoloji Altyapısı](#teknoloji-altyapısı)
- [Gereksinimler](#gereksinimler)
- [Ortam Kurulumu](#ortam-kurulumu)
- [Başlangıç](#başlangıç)
- [Güvenlik Özellikleri](#güvenlik-özellikleri)
- [E-posta Sistemi](#e-posta-sistemi)
- [Gerçek Zamanlı Özellikler](#gerçek-zamanlı-özellikler)
- [Kullanıcı Arayüzü](#kullanıcı-arayüzü)
- [Ekran Görüntüleri](#ekran-görüntüleri)
- [Lisans](#lisans)
- [Katkıda Bulunma](#katkıda-bulunma)

Booknix, .NET 8.0 ile geliştirilmiş modern bir randevu planlama ve hizmet yönetim sistemi. Temiz mimari yaklaşımıyla randevuları, hizmetleri ve lokasyonları yönetmek için kapsamlı bir çözüm sunar.

## Demo Site

🌐 **Demo Site:** [https://booknix.ismailparlak.com](https://booknix.ismailparlak.com)

## Özellikler

- Kullanıcı kimlik doğrulama ve yetkilendirme
- Randevu planlama sistemi
- Hizmet ve lokasyon yönetimi
- SignalR ile gerçek zamanlı bildirimler
- Kuyruk sistemi ile e-posta bildirimleri
- Değerlendirme sistemi
- Çalışan yönetimi ve planlaması
- Sektör bazlı organizasyon
- Modern tasarımlı duyarlı kullanıcı arayüzü

## Proje Yapısı

Çözüm, Temiz Mimari prensiplerini takip eder ve aşağıdaki projelerden oluşur:

- **Booknix.MVCUI**: ASP.NET Core MVC kullanan web uygulama katmanı
- **Booknix.Application**: Uygulama iş kuralları ve kullanım senaryoları
- **Booknix.Domain**: İş kuralları ve varlıklar
- **Booknix.Infrastructure**: Framework'ler ve harici bağımlılıklar
- **Booknix.Persistence**: Veritabanı erişimi ve repository'ler
- **Booknix.Shared**: Paylaşılan yardımcı programlar ve yapılandırmalar

## Teknoloji Altyapısı

- .NET 8.0
- ASP.NET Core MVC
- Entity Framework Core
- PostgreSQL
- Gerçek zamanlı özellikler için SignalR
- E-posta işlevselliği için MailKit
- Stil için Tailwind CSS
- jQuery ve modern JavaScript

## Gereksinimler

- .NET 8.0 SDK
- PostgreSQL
- E-posta işlevselliği için SMTP sunucusu

## Ortam Kurulumu

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

## Başlangıç

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

## Güvenlik Özellikleri

- BCrypt ile güvenli şifre hashleme
- Oturum doğrulama middleware'i
- Token tabanlı kimlik doğrulama
- Denetim günlüğü
- Güvenilir IP yönetimi

## E-posta Sistemi

Uygulama, aşağıdaki özelliklere sahip sağlam bir e-posta sistemi içerir:

- Daha iyi performans için e-posta kuyruğu
- E-posta kuyruğunun arka planda işlenmesi
- Geliştirme modunda e-posta yönlendirme
- HTML e-posta şablonları

## Gerçek Zamanlı Özellikler

- SignalR ile gerçek zamanlı bildirimler
- Randevular için canlı güncellemeler
- Anlık durum değişiklikleri

## Kullanıcı Arayüzü

- Duyarlı tasarım
- Tailwind CSS ile modern arayüz
- İstemci tarafı doğrulama
- Aşamalı geliştirme
- Mobil uyumlu arayüz

## Ekran Görüntüleri

### Kullanıcı Arayüzü

#### Ana Sayfa

- **Kategoriler**  
  ![Kategoriler](https://github.com/user-attachments/assets/02f145a4-3bd7-47f6-8997-7ae3b7f0a6dc)
- **Popüler Hizmetler**  
  ![Popüler Hizmetler](https://github.com/user-attachments/assets/4c74c018-9878-4d70-8979-dd4466116c19)

#### Lokasyon ve Hizmetler

- **Kuaför Lokasyonları**  
  ![Kuaför Lokasyonları](https://github.com/user-attachments/assets/2794ef54-8ab7-4f91-843d-0363b525c2f1)
- **Kuaför Şubesi – Hizmetler**  
  ![Kuaför Şubesi](https://github.com/user-attachments/assets/f654b9ed-131e-4ca6-8fe4-51f3690ef1db)

#### Kullanıcı İşlemleri

- **Giriş Ekranı**  
  ![Giriş](https://github.com/user-attachments/assets/cdd8bd84-eeb5-4dcd-8475-ab089e18905e)
- **Saç Kesimi Detayı – Çalışanlar**  
  ![Çalışanlar](https://github.com/user-attachments/assets/c30ab9cc-f64e-4ef0-9d4e-a0987da87481)
- **Randevu Slot Seçimi**  
  ![Randevu](https://github.com/user-attachments/assets/627bccc7-c224-472a-89e2-1205dcdff7d7)
- **Randevularım Sayfası**  
  ![Randevularım](https://github.com/user-attachments/assets/31a14670-ee40-4cfd-8f15-ee28d4895694)
- **Güvenlik Kaydı**  
  ![Güvenlik](https://github.com/user-attachments/assets/7764c563-b6bf-4e35-9617-9031d2dacec2)
- **Profil Ayarları**  
  ![Profil](https://github.com/user-attachments/assets/d9010450-ff54-4f7f-a41c-747daf3301ab)

### Yönetim Panelleri

#### Admin Paneli

- **Ana Sayfa**  
  ![Admin Ana Sayfa](https://github.com/user-attachments/assets/af274370-cb10-42fa-a3e0-58ca4e8e13a7)
- **Kullanıcı Yönetimi**  
  ![Kullanıcı Yönetimi](https://github.com/user-attachments/assets/44c1ef4c-e714-427f-86f2-cbc393dca760)
- **E-Posta Kuyruğu**  
  ![E-Posta Kuyruğu](https://github.com/user-attachments/assets/c83db227-942f-4139-b88c-1b587e642b11)
- **Lokasyon Detayları**  
  ![Lokasyon Detayları](https://github.com/user-attachments/assets/4e284e60-6411-4a46-b7f7-18d10e54ed41)
- **Çalışanlar**  
  ![Çalışanlar](https://github.com/user-attachments/assets/d1c4bdf5-b238-48c5-aa75-b1f7681fb1b1)
- **Çalışma Saatleri**  
  ![Çalışma Saatleri](https://github.com/user-attachments/assets/ba9131b3-a114-46bf-a183-2fb3d82e7c56)
- **Randevular & Yorumlar**  
  ![Randevular](https://github.com/user-attachments/assets/3fed9444-4100-47c7-b815-74ff7ecc2fd3)

#### Çalışan Paneli

- **Ana Sayfa**  
  ![Çalışan Paneli](https://github.com/user-attachments/assets/80c067e9-d3cd-4cee-aac1-bd40abfbed74)

## 📝 Lisans

Telif Hakkı © 2025 Booknix. Tüm hakları saklıdır.

## 🤝 Katkıda Bulunma

1. Depoyu fork edin
2. Özellik dalınızı oluşturun
3. Değişikliklerinizi commit edin
4. Dalınıza push yapın
5. Yeni bir Pull Request oluşturun
