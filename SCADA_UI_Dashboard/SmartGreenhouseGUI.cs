/*
 * PROJE: Yapay Zeka Destekli Akıllı Sera ve İklimlendirme Sistemi
 * DOSYA: SmartGreenhouseGUI.cs (Ana Arayüz Sınıfı)
 * AÇIKLAMA: Arduino'dan gelen JSON telemetri verilerini okuyan, 
 * ekranı 7 parçalı SCADA paneline bölen ve yapay zeka kamera simülasyonunu
 * barındıran Asenkron (Thread-Safe) Windows Forms arayüz kodudur.
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Ports;
using System.Text.Json;

namespace SmartGreenhouseGUI
{
    public partial class Form1 : Form
    {
        // --- DONANIM HABERLEŞME DEĞİŞKENLERİ ---
        private SerialPort arduinoPort;
        private Timer cameraTimer;

        public Form1()
        {
            // Formu ve arayüzü başlat
            InitializeComponent();
            SetupDashboardLayout();
            InitializeHardware();
        }

        // ====================================================================
        // 1. ARAYÜZ (GUI) 7 PANELİNİN KODLA OLUŞTURULMASI VE TASARIMI
        // ====================================================================
        private void SetupDashboardLayout()
        {
            this.Text = "YTÜ - Görüntü İşleme Destekli Akıllı Sera Kontrol Paneli";
            this.Size = new Size(1280, 720);
            this.BackColor = Color.FromArgb(18, 18, 18); // Koyu Tema (Dark Mode)
            this.ForeColor = Color.White;

            // --- PANEL 1: Kamera ve Bitki Sağlığı (Sol Üst) ---
            Label lblHealthTitle = new Label { Text = "1. BİTKİ SAĞLIK DURUMU", Location = new Point(20, 20), AutoSize = true, Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.LightBlue };
            Label lblHealthStatus = new Label { Name = "lblHealthStatus", Text = "ANALİZ BEKLENİYOR...", Location = new Point(20, 50), AutoSize = true, Font = new Font("Arial", 16, FontStyle.Bold), ForeColor = Color.Yellow };
            PictureBox picCamera = new PictureBox { Name = "picCamera", Location = new Point(20, 90), Size = new Size(400, 300), BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };
            
            // --- PANEL 2: Toprak Nem Seviyesi ---
            Label lblMoisture = new Label { Name = "lblMoisture", Text = "2. Toprak Nem Seviyesi: %0", Location = new Point(450, 50), AutoSize = true, Font = new Font("Arial", 14, FontStyle.Bold) };
            ProgressBar progMoisture = new ProgressBar { Name = "progMoisture", Location = new Point(450, 90), Size = new Size(250, 30), Value = 0 };

            // --- PANEL 3: İç Sıcaklık ---
            Label lblInTempTitle = new Label { Text = "3. Sera İçi Sıcaklık:", Location = new Point(750, 50), AutoSize = true, Font = new Font("Arial", 12) };
            Label lblInTemp = new Label { Name = "lblInTemp", Text = "0 °C", Location = new Point(750, 80), AutoSize = true, Font = new Font("Arial", 22, FontStyle.Bold), ForeColor = Color.LightCoral };

            // --- PANEL 4: Dış Sıcaklık ---
            Label lblOutTempTitle = new Label { Text = "4. Dış Ortam Sıcaklığı:", Location = new Point(1000, 50), AutoSize = true, Font = new Font("Arial", 12) };
            Label lblOutTemp = new Label { Name = "lblOutTemp", Text = "0 °C", Location = new Point(1000, 80), AutoSize = true, Font = new Font("Arial", 22, FontStyle.Bold), ForeColor = Color.LightSkyBlue };

            // --- PANEL 5: Fan Durumu ---
            PictureBox picFan = new PictureBox { Name = "picFan", Location = new Point(450, 200), Size = new Size(64, 64), SizeMode = PictureBoxSizeMode.Zoom };
            Label lblFan = new Label { Name = "lblFan", Text = "5. FAN: DEAKTİF", Location = new Point(530, 220), AutoSize = true, Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.Red };

            // --- PANEL 6: Isıtıcı Durumu ---
            PictureBox picHeater = new PictureBox { Name = "picHeater", Location = new Point(750, 200), Size = new Size(64, 64), SizeMode = PictureBoxSizeMode.Zoom };
            Label lblHeater = new Label { Name = "lblHeater", Text = "6. ISITICI: DEAKTİF", Location = new Point(830, 220), AutoSize = true, Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.Red };

            // --- PANEL 7: Su Pompası Durumu ---
            PictureBox picPump = new PictureBox { Name = "picPump", Location = new Point(1000, 200), Size = new Size(64, 64), SizeMode = PictureBoxSizeMode.Zoom };
            Label lblPump = new Label { Name = "lblPump", Text = "7. POMPA: DEAKTİF", Location = new Point(1080, 220), AutoSize = true, Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.Red };

            // Tüm nesneleri ana ekrana (Form) ekle
            this.Controls.Add(lblHealthTitle); this.Controls.Add(lblHealthStatus); this.Controls.Add(picCamera);
            this.Controls.Add(lblMoisture); this.Controls.Add(progMoisture);
            this.Controls.Add(lblInTempTitle); this.Controls.Add(lblInTemp);
            this.Controls.Add(lblOutTempTitle); this.Controls.Add(lblOutTemp);
            this.Controls.Add(picFan); this.Controls.Add(lblFan);
            this.Controls.Add(picHeater); this.Controls.Add(lblHeater);
            this.Controls.Add(picPump); this.Controls.Add(lblPump);

            // Başlangıç İkonlarını Yüklemeyi Dene (İkonlar 'icons' klasöründe olmalı)
            try {
                picFan.Image = Image.FromFile(@"icons\fan_kapali.png");
                picHeater.Image = Image.FromFile(@"icons\isitici_kapali.png");
                picPump.Image = Image.FromFile(@"icons\pompa_kapali.png");
            } catch { /* İkon yoksa programın çökmesini engelle */ }
        }

        // ====================================================================
        // 2. ARDUINO VE KAMERA BAĞLANTILARININ BAŞLATILMASI
        // ====================================================================
        private void InitializeHardware()
        {
            // Arduino Seri Port (UART) Bağlantısı
            try
            {
                arduinoPort = new SerialPort("COM3", 9600); // Sistem portuna göre değiştirilebilir
                arduinoPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                arduinoPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Arduino Bağlantı Hatası: " + ex.Message, "Donanım Uyarısı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Yapay Zeka (YOLO) Kamera Simülasyonu / Tetikleyicisi
            cameraTimer = new Timer();
            cameraTimer.Interval = 100; // 100ms = 10 FPS
            cameraTimer.Tick += CameraTimer_Tick;
            cameraTimer.Start();
        }

        // ====================================================================
        // 3. KAMERA GÖRÜNTÜSÜ VE YAPAY ZEKA GÜNCELLEMESİ
        // ====================================================================
        private void CameraTimer_Tick(object sender, EventArgs e)
        {
            // Not: Gerçek projede OpenCV/EmguCV ile model predict burada çalışır.
            Label lblHealth = (Label)this.Controls["lblHealthStatus"];
            lblHealth.Text = "SAĞLIKLI (%98.5)";
            lblHealth.ForeColor = Color.LimeGreen;
        }

        // ====================================================================
        // 4. ARDUINO'DAN ASENKRON VERİ OKUMA VE JSON PARSE İŞLEMİ (INVOKE)
        // ====================================================================
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            try
            {
                string inData = sp.ReadLine().Trim();
                
                // Gelen veriyi (JSON) parse et: Örn: {"InsideTemp":25, "Soil":65, "Fan":1, "Heater":0, "Pump":1}
                using (JsonDocument doc = JsonDocument.Parse(inData))
                {
                    JsonElement root = doc.RootElement;

                    // Arayüz (UI) Thread'ine güvenli geçiş yapıyoruz (Cross-thread hatasını önlemek için)
                    this.Invoke(new MethodInvoker(delegate
                    {
                        UpdateDashboard(root);
                    }));
                }
            }
            catch { /* Hatalı seri paket geldiğinde sistem çökmesin diye yutulur */ }
        }

        // ====================================================================
        // 5. EKRAN GÖRSELLERİNİN VE YAZILARININ GÜNCELLENMESİ
        // ====================================================================
        private void UpdateDashboard(JsonElement data)
        {
            // Toprak Nem Güncellemesi
            if (data.TryGetProperty("Soil", out JsonElement soilElement)) {
                int soil = soilElement.GetInt32();
                this.Controls["lblMoisture"].Text = $"2. Toprak Nem Seviyesi: %{soil}";
                ((ProgressBar)this.Controls["progMoisture"]).Value = Math.Min(soil, 100);
            }

            // Sıcaklık Güncellemeleri
            if (data.TryGetProperty("InsideTemp", out JsonElement inTemp))
                this.Controls["lblInTemp"].Text = $"{inTemp.GetInt32()} °C";
            if (data.TryGetProperty("OutTemp", out JsonElement outTemp))
                this.Controls["lblOutTemp"].Text = $"{outTemp.GetInt32()} °C";

            // Fan Durumu ve İkon Güncellemesi
            if (data.TryGetProperty("Fan", out JsonElement fanState)) {
                Label l = (Label)this.Controls["lblFan"]; PictureBox p = (PictureBox)this.Controls["picFan"];
                if (fanState.GetInt32() == 1) { l.Text = "5. FAN: ÇALIŞIYOR"; l.ForeColor = Color.LimeGreen; SetImageSafe(p, @"icons\fan_acik.png"); }
                else { l.Text = "5. FAN: DEAKTİF"; l.ForeColor = Color.Red; SetImageSafe(p, @"icons\fan_kapali.png"); }
            }

            // Isıtıcı Durumu ve İkon Güncellemesi
            if (data.TryGetProperty("Heater", out JsonElement heaterState)) {
                Label l = (Label)this.Controls["lblHeater"]; PictureBox p = (PictureBox)this.Controls["picHeater"];
                if (heaterState.GetInt32() == 1) { l.Text = "6. ISITICI: ÇALIŞIYOR"; l.ForeColor = Color.Orange; SetImageSafe(p, @"icons\isitici_acik.png"); }
                else { l.Text = "6. ISITICI: DEAKTİF"; l.ForeColor = Color.Red; SetImageSafe(p, @"icons\isitici_kapali.png"); }
            }

            // Pompa Durumu ve İkon Güncellemesi
            if (data.TryGetProperty("Pump", out JsonElement pumpState)) {
                Label l = (Label)this.Controls["lblPump"]; PictureBox p = (PictureBox)this.Controls["picPump"];
                if (pumpState.GetInt32() == 1) { l.Text = "7. POMPA: SULAMA YAPILIYOR"; l.ForeColor = Color.DeepSkyBlue; SetImageSafe(p, @"icons\pompa_acik.png"); }
                else { l.Text = "7. POMPA: DEAKTİF"; l.ForeColor = Color.Red; SetImageSafe(p, @"icons\pompa_kapali.png"); }
            }
        }

        // İkon değiştirirken dosya bulunamazsa programın çökmesini engelleyen güvenli fonksiyon
        private void SetImageSafe(PictureBox pic, string path)
        {
            try { pic.Image = Image.FromFile(path); } catch { }
        }

        // Program kapatılırken donanım portlarını güvenle temizle
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (arduinoPort != null && arduinoPort.IsOpen)
                arduinoPort.Close();
            base.OnFormClosing(e);
        }

        // Not: InitializeComponent() metodu Visual Studio Designer tarafından Form1.Designer.cs içine otomatik yazılır.
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "Form1";
            this.ResumeLayout(false);
        }
    }
}
