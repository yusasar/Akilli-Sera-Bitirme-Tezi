 Yapay Zeka Destekli Otonom Sera ve İklimlendirme Sistemi

![Platform](https://img.shields.io/badge/Platform-Raspberry_Pi_5_%7C_Arduino-blue?style=for-the-badge)
![AI](https://img.shields.io/badge/Yapay_Zeka-TensorFlow_%7C_OpenCV-orange?style=for-the-badge)
![UI](https://img.shields.io/badge/Aray%C3%BCz-C%23_WinForms-purple?style=for-the-badge)
![Build](https://img.shields.io/badge/Durum-Tamamland%C4%B1-success?style=for-the-badge)
![University](https://img.shields.io/badge/YT%C3%9C-EHMB-red?style=for-the-badge)

Bu proje, Yıldız Teknik Üniversitesi Elektronik ve Haberleşme Mühendisliği Bölümü Lisans Bitirme Tezi olarak geliştirilmiş; geleneksel tarım ve sera yönetimi süreçlerini **Endüstri 4.0** ve **IoT (Nesnelerin İnterneti)** standartlarına taşıyan kapalı döngü (closed-loop) bir **Siber-Fiziksel Sistem (CPS)** mimarisidir.


 Proje Özeti ve Endüstriyel Çıktı

Projenin temel amacı; bitki hastalıklarının ve iklimsel stres faktörlerinin insan gözlemine dayalı olarak geç tespit edilmesi problemini çözmektir. Bu doğrultuda sistem, iklimlendirme aktüatörlerini (su, ısı, havalandırma) yapay zeka algoritmalarıyla otonom olarak yönetir. Optimum kaynak kullanımı sağlayarak (gerektiği kadar sulama, gerektiği kadar havalandırma) enerji verimliliğini artırır ve zirai fireleri minimize eder.

---

## ⚙️ Teknik Mimari ve Mühendislik Yaklaşımı

Sistem, işlem yükünü (processing overhead) dağıtmak ve gerçek zamanlı (real-time) kararlılığı maksimize etmek amacıyla **Master-Slave (Usta-Köle) çoklu işlemci mimarisi** üzerine inşa edilmiştir. 

### 1. Master Katmanı (Sınır Bilişim - Edge Computing)
Sistemin merkezi beyin işlevini **Raspberry Pi 5** üstlenmektedir. 
* **Lokal Yapay Zeka İşleme:** Bulut tabanlı ağ gecikmelerini (latency) ortadan kaldırmak için görüntü işleme süreçleri doğrudan donanım üzerinde koşar.
* **Derin Öğrenme (Deep Learning):** 1000'den fazla özgün yaprak görseli kullanılarak eğitilen YOLO/CNN tabanlı model, HD kamera üzerinden aldığı canlı akışı milisaniyeler içinde analiz eder. Bitkideki dokusal hastalıkları tespit ederek otonom karar mekanizmasını tetikler.

### 2. Slave Katmanı (Fiziksel Kontrol ve Sinyal İşleme)
Donanım kontrolörü olarak konumlandırılan **Arduino**, analog ve dijital sensör ağlarının okumalarını gerçekleştirir.
* **Histerezis Kontrolü:** Toprak nemi veya sıcaklık kritik eşik değerlerinin dışına çıktığında salınımı engellemek için algoritmik bir bant aralığı kullanılır.
* **Güç Elektroniği Yönetimi:** Master birimden gelen kararlar doğrultusunda röle sürücü devreleri üzerinden su pompası, soğutma fanları ve ısıtıcı rezistanslar tetiklenir.

### 3. Asenkron SCADA Haberleşmesi ve Arayüz (GUI)
* **JSON Tabanlı Telemetri:** Mikrodenetleyiciler kendi aralarında UART/Seri Haberleşme üzerinden şifrelenmiş JSON veri paketleri gönderir.
* **Thread ve Non-Blocking Mimari:** C# (WinForms) ile geliştirilen kontrol panelindeki çoklu iş parçacığı (Multi-threading) mimarisi sayesinde, donanım kesmeleri arayüzü kilitlemez. Canlı kamera akışı, sensör verileri ve 3 farklı aktüatörün çalışma durumu 7 parçalı ekranda gerçek zamanlı güncellenir.

---

## 🛠️ Kullanılan Donanımlar ve Elektronik Komponentler

| Komponent | Görevi ve Teknik Detayı |
| :--- | :--- |
| **Raspberry Pi 5** | Sistemin beyni. Görüntü işleme ve yapay zeka modelinin koşturulduğu Edge cihaz. |
| **Arduino Uno/Nano** | Sensör verilerini okuyan ve röleleri tetikleyen alt işlemci birimi (Slave). |
| **Pi Camera Module** | Yapraklardan anlık görüntü alan optik sensör. |
| **DHT11 / DHT22** | Sera içi ve dışı ortam sıcaklığı/bağıl nem okuyucusu. |
| **Kapasitif Toprak Nem Sensörü**| Toprağın iletkenlik değerini okuyarak % bazında nem ölçer. |
| **5V/12V Röle Kartları** | Düşük akımlı tetikleme sinyaliyle yüksek akımlı motorları/ısıtıcıları sürer. |
| **DC Su Pompası & Fanlar** | İklimlendirme ve sulama işlemlerini gerçekleştiren aktüatörler. |
| **I2C 20x4 LCD Ekran** | Verileri ve sistem hazır durumunu sahada gösteren panel. |

---

## 📂 Dosya Yapısı

```text
📦 Akilli-Sera-Otomasyonu
 ┣ 📂 Arduino_Codes       # greenhouse_hardware_control.ino ve kütüphaneler
 ┣ 📂 AI_Model            # model_training.py, model.h5
 ┣ 📂 Dataset             # Healthy ve Unhealthy eğitim görselleri
 ┣ 📂 SCADA_UI_Dashboard  # C# WinForms proje dosyaları ve icon klasörü
 ┣ 📂 Images              # README ve arayüz ekran görüntüleri
 ┣ 📜 README.md           # Proje dökümantasyonu
 ┗ 📜 requirements.txt    # Python bağımlılıkları listesi

