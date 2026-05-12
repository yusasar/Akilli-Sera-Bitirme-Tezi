# ⚙️ Donanım Spesifikasyonları ve Teknik Özellikler

Bu belgede, "Akıllı Sera ve İklimlendirme Sistemi" projesinde kullanılan temel bileşenlerin teknik özellikleri, güç gereksinimleri (Gerilim/Akım) ve sistemdeki görevleri listelenmiştir. Sistem, güç dağıtımını stabilize etmek için **12V Ana Güç Hattı** ve **5V Mantık (Logic) Hattı** olmak üzere iki ayrı güç segmentine ayrılmıştır.

## 1. Ana Kontrolcüler ve Sınır Bilişim (Edge Devices)

| Komponent | Çalışma Gerilimi | Akım / Güç Gereksinimi | Teknik İşlev |
| :--- | :--- | :--- | :--- |
| **Raspberry Pi 4** | 5V DC | ~3.0A (Maks) | Sistemin ana beyni (Master). Kamera üzerinden gelen görselleri AlexNet/CNN ile işler, YOLO koşturur. |
| **Arduino Uno / Mega** | 5V DC (Mantık) / 7-12V (Giriş) | ~50mA (Kartın kendisi) | Slave mikrodenetleyici. Sensör okumalarını yapar, histerezis logiğini işletir ve röleleri tetikler. |

## 2. Sensör Ağları

Sensörler, düşük güç tüketimi ile 7/24 kesintisiz veri akışı sağlamak üzere seçilmiştir.

| Komponent | Çalışma Gerilimi | Akım | Ölçüm Aralığı / Hassasiyet |
| :--- | :--- | :--- | :--- |
| **DHT22 Isı ve Nem Sensörü** | 3.3V - 5V DC | ~2.5mA (Ölçüm sırasında) | Sıcaklık: -40°C ~ 80°C (±0.5°C), Nem: %0-100 (±%2-5). |
| **YL-69 Toprak Nem Sensörü** | 3.3V - 5V DC | ~35mA | Toprağın iletkenlik direncini ölçerek 0-1023 arası analog değer üretir. |
| **Raspberry Kamera Modülü** | 3.3V DC (Pi üzerinden) | ~250mA | Yaprakların HD çözünürlükte görüntülerini alıp işlemciye aktarır. |

## 3. Aktüatörler (Mekanik ve Isıl Sistemler)

Sera içerisindeki fiziksel tepkileri oluşturan donanımlardır. Yüksek akım çektikleri için röle kartları ile sürülmektedirler.

| Komponent | Çalışma Gerilimi | Akım / Güç Gereksinimi | Teknik İşlev |
| :--- | :--- | :--- | :--- |
| **80x80x25 mm Fan (x4)** | 12V DC | ~0.15A - 0.2A (Adet başı) | Sera içi hava sirkülasyonu ve sıcaklık tahliyesi. Toplam ~0.8A çeker. |
| **PTC Isıtıcı / Mini Rezistans** | 12V DC | ~1.5A - 2A | Soğuk havalarda bitkiyi korumak için sera içi ısıtma. |
| **Mini Su Pompası** | 6V DC | ~200mA - 300mA | Toprak nemi kritik eşiğin altına (%45) düştüğünde sulama yapar. |
| **5V Mikroservo (x2)** | 5V DC | ~400mA (Yükte) | Havalandırma kapaklarının veya yönlendiricilerin fiziksel açılıp kapanması. |

## 4. Güç Elektroniği ve Anahtarlama

| Komponent | Çalışma Gerilimi | Teknik Çıktı / İletim | İşlev |
| :--- | :--- | :--- | :--- |
| **12V 25W Güç Kaynağı (SMPS)** | 220V AC (Giriş) -> 12V DC (Çıkış) | ~2.08A (Maksimum Akım) | Sistemin ana enerji kaynağıdır. Fanları ve ısıtıcıyı besler. |
| **Voltaj Düşürücü Regülatör** | 12V Giriş -> 5V / 6V Çıkış | ~2A - 3A | 12V ana hattı, Arduino ve su pompası (6V) için uygun seviyelere düşürür (Step-Down/Buck Converter). |
| **Röle Modülleri (4, 2 ve Tek Kanallı)** | 5V (Tetikleme) | 10A (Maksimum Anahtarlama Akımı) | Arduino'dan gelen zayıf 5V sinyali ile yüksek akım çeken fan, pompa ve ısıtıcıyı güvenle açıp kapatır. |
| **L293D Motor Sürücü** | 5V (Mantık) / 4.5V - 36V (Motor) | Kanal başı 600mA | Çift yönlü motor kontrolü ve servo/DC motor sürme işlemleri için kullanılır. |

---
**Önemli Mühendislik Notu:**
Sistemin kilitlenmemesi ve kararlı çalışması için aktüatörlerin (motor ve fanlar) oluşturabileceği ters elektromotor kuvvetini (Back-EMF) engellemek adına röle kartlarında optokuplör izolasyonu ve koruma diyotları (Flyback Diode) kullanılmıştır. Arduino'nun beslemesi, güç dalgalanmalarından etkilenmemesi için regülatör üzerinden izole bir hattan sağlanmaktadır.
