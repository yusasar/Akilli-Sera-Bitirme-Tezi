/*
 * ============================================================================
 * PROJE ADI   : Yapay Zeka Destekli Akıllı Sera ve İklimlendirme Sistemi
 * DOSYA       : greenhouse_hardware_control.ino
 * GELİŞTİRİCİ : Hüseyin Yuşa SARI
 * BÖLÜM       : Yıldız Teknik Üniversitesi - Elektronik ve Haberleşme Müh.
 * AÇIKLAMA    : Sensör verilerini okuyan, otonom iklimlendirme aktüatörlerini 
 * yöneten ve C# SCADA arayüzüne JSON formatında telemetri 
 * gönderen donanım (Slave) kontrol yazılımı.
 * ============================================================================
 */

#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include <DHT.h>

// --- DONANIM PİN TANIMLAMALARI ---
#define DHTPIN 2                // DHT11 Sıcaklık ve Nem Sensörü Veri Pini
#define DHTTYPE DHT11           // Sensör Tipi (Gerekirse DHT22 yapılabilir)
#define SOIL_MOISTURE_PIN A0    // Kapasitif Toprak Nemi Sensörü Analog Pini
#define RELAY_PUMP_PIN 8        // Su Pompası Röle Kontrol Pini
#define RELAY_FAN_PIN 9         // Havalandırma/Soğutma Fanı Röle Kontrol Pini
#define RELAY_HEATER_PIN 10     // Isıtıcı Sistem Röle Kontrol Pini

// --- SİSTEM EŞİK DEĞERLERİ (HİSTEREZİS) ---
const int MIN_MOISTURE = 45;            // [%] Sulama başlama alt sınırı
const int MAX_MOISTURE = 75;            // [%] Sulama durma üst sınırı
const float TEMP_HOT_THRESHOLD = 28.0;  // [°C] Fan çalıştırma üst sıcaklık sınırı
const float TEMP_COLD_THRESHOLD = 18.0; // [°C] Isıtıcı çalıştırma alt sıcaklık sınırı

// --- ASENKRON ÇALIŞMA ZAMANLAYICILARI ---
unsigned long lastUpdate = 0;
const long interval = 2000;             // Sensör okuma/yazma döngü aralığı (2000ms = 2sn)

// --- DONANIM NESNELERİ ---
LiquidCrystal_I2C lcd(0x27, 20, 4);     // I2C Haberleşmeli 20x4 LCD (Adres genelde 0x27)
DHT dht(DHTPIN, DHTTYPE);

// --- GLOBAL SİSTEM DEĞİŞKENLERİ ---
int moisturePercent = 0;
float insideTemp = 0.0;
float outTemp = 0.0;
float humidity = 0.0;

// Aktüatör Durum Bayrakları (1: AKTİF, 0: DEAKTİF)
int isPumpActive = 0;
int isFanActive = 0;
int isHeaterActive = 0;

void setup() {
  // Bilgisayar / C# Arayüzü ile haberleşme baud rate ayarı
  Serial.begin(9600); 
  
  // Çevresel birimlerin başlatılması
  dht.begin();
  lcd.init();
  lcd.backlight();
  
  // Pin giriş/çıkış yönlendirmeleri
  pinMode(RELAY_PUMP_PIN, OUTPUT);
  pinMode(RELAY_FAN_PIN, OUTPUT);
  pinMode(RELAY_HEATER_PIN, OUTPUT);
  
  // Başlangıçta tüm röleleri kapalı (güvenli) konuma al
  // Not: Düşük tetiklemeli (Low-Level Trigger) röleler için HIGH gönderilir
  digitalWrite(RELAY_PUMP_PIN, HIGH);
  digitalWrite(RELAY_FAN_PIN, HIGH);
  digitalWrite(RELAY_HEATER_PIN, HIGH);

  showBootScreen();
}

void loop() {
  unsigned long currentMillis = millis();

  // Sistemin kilitlenmesini önlemek için delay() yerine millis() kullanımı
  if (currentMillis - lastUpdate >= interval) {
    lastUpdate = currentMillis;
    
    readSensors();
    controlActuators();
    updateDisplay();
    sendTelemetry();
  }
}

// ============================================================================
// 1. SENSÖR VERİLERİNİN OKUNMASI VE İŞLENMESİ
// ============================================================================
void readSensors() {
  // Toprak Nem Sensörü ADC okuması ve yüzdeye kalibrasyonu
  int rawValue = analogRead(SOIL_MOISTURE_PIN);
  moisturePercent = map(rawValue, 1023, 250, 0, 100); 
  moisturePercent = constrain(moisturePercent, 0, 100);

  // Ortam sıcaklık ve nem okuması
  insideTemp = dht.readTemperature();
  humidity = dht.readHumidity();
  
  // Dış ortam sıcaklığı için simülasyon/varsayım (Arayüz entegrasyonu)
  outTemp = insideTemp - 4.0; 

  // Nan (Not a Number) hata kontrolü (Sensör kopuksa sistemi korur)
  if (isnan(insideTemp) || isnan(humidity)) {
    return;
  }
}

// ============================================================================
// 2. OTONOM AKTÜATÖR KONTROLÜ (KARAR MEKANİZMASI)
// ============================================================================
void controlActuators() {
  // Sulama Logiği (Histerezis - Salınım Önleyici)
  if (moisturePercent < MIN_MOISTURE) {
    digitalWrite(RELAY_PUMP_PIN, LOW);   // Röle çek, Su Pompası AÇIK
    isPumpActive = 1;
  } else if (moisturePercent > MAX_MOISTURE) {
    digitalWrite(RELAY_PUMP_PIN, HIGH);  // Röle bırak, Su Pompası KAPALI
    isPumpActive = 0;
  }

  // Havalandırma / Soğutma Fanı Logiği
  if (insideTemp > TEMP_HOT_THRESHOLD) {
    digitalWrite(RELAY_FAN_PIN, LOW);    // Fan AÇIK
    isFanActive = 1;
  } else {
    digitalWrite(RELAY_FAN_PIN, HIGH);   // Fan KAPALI
    isFanActive = 0;
  }

  // Isıtıcı Sistem Logiği
  if (insideTemp < TEMP_COLD_THRESHOLD) {
    digitalWrite(RELAY_HEATER_PIN, LOW); // Isıtıcı AÇIK
    isHeaterActive = 1;
  } else {
    digitalWrite(RELAY_HEATER_PIN, HIGH); // Isıtıcı KAPALI
    isHeaterActive = 0;
  }
}

// ============================================================================
// 3. SAHA EKRANI (LCD) GÜNCELLEMESİ
// ============================================================================
void updateDisplay() {
  lcd.clear();

  // 1. Satır: Sıcaklık ve Nem
  lcd.setCursor(0, 0);
  lcd.print("Sira: ");
  lcd.print((int)insideTemp);
  lcd.print((char)223); // Derece sembolü
  lcd.print("C H: %");
  lcd.print((int)humidity);

  // 2. Satır: Toprak Nemi
  lcd.setCursor(0, 1);
  lcd.print("Toprak Nemi: %");
  lcd.print(moisturePercent);

  // 3. Satır: Pompa ve Fan Durumu
  lcd.setCursor(0, 2);
  lcd.print("Pompa:");
  lcd.print(isPumpActive ? "ACIK " : "KAPALI");
  lcd.print(" F:");
  lcd.print(isFanActive ? "ON" : "OFF");

  // 4. Satır: Isıtıcı Durumu
  lcd.setCursor(0, 3);
  lcd.print("Isitici:");
  lcd.print(isHeaterActive ? "AKTIF" : "PASIF");
}

// ============================================================================
// 4. C# ARAYÜZÜ İÇİN JSON FORMATINDA SERİ HABERLEŞME (TELEMETRY)
// ============================================================================
void sendTelemetry() {
  // DİKKAT: C# (System.Text.Json) kütüphanesinin sorunsuz okuyabilmesi için 
  // değişken isimleri mutlaka çift tırnak (") içinde gönderilmektedir.
  
  Serial.print("{\"InsideTemp\":");  Serial.print((int)insideTemp);
  Serial.print(", \"OutTemp\":");    Serial.print((int)outTemp);
  Serial.print(", \"Soil\":");       Serial.print(moisturePercent);
  Serial.print(", \"Pump\":");       Serial.print(isPumpActive);
  Serial.print(", \"Fan\":");        Serial.print(isFanActive);
  Serial.print(", \"Heater\":");     Serial.print(isHeaterActive);
  Serial.println("}");
}

// ============================================================================
// 5. BAŞLANGIÇ EKRANI ANİMASYONU
// ============================================================================
void showBootScreen() {
  lcd.setCursor(2, 1);
  lcd.print("SMART GREENHOUSE");
  lcd.setCursor(4, 2);
  lcd.print("SYSTEM READY");
  delay(2000);
  lcd.clear();
}
