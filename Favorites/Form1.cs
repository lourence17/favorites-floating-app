using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Favorites
{
    public partial class Form1 : Form
    {
        private Size normalSize = new Size(50, 50); // Formun baþlangýç boyutu
        private Size hoverSize = new Size(200, 250); // Formun açýldýðýnda boyutu (yüksekliði artýrdýk, butonlar için yer açmak için)
        private Point mouseOffset;
        private bool isMouseDown = false;
        private bool isExpanding = false; // Formun geniþleme/küçülme durumu
        private int currentSizeStep = 0; // Animasyon için adým
        private const int ANIMATION_STEPS = 8; // Animasyon adýmý sayýsý
        private const int ANIMATION_DELAY = 15; // Animasyon hýzý (milisaniye)

        private List<string> favorites = new List<string>(); // Favori uygulamalar/klasörler listesi

        [DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public Form1()
        {
            InitializeComponent();
            // Performans optimizasyonlarý için SetStyle
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // Çift tamponlama optimize et
            this.SetStyle(ControlStyles.UserMouse, true); // Mouse olaylarýný optimize et
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true); // Yenileme performansýný artýr
            this.SetStyle(ControlStyles.ResizeRedraw, true); // Boyut deðiþiminde yeniden çiz
            this.UpdateStyles(); // Stil güncellemelerini uygula

            this.DoubleBuffered = true; // Ekstra çift tamponlama
            SetupForm(); // Form özelliklerini ve olaylarý manuel olarak ayarla
            MakeFormRounded(); // Köþeleri yuvarlak yap
            ConfigureTimer(); // Timer’ý yapýlandýr
            InitializeControls(); // Buton ve diðer kontrolleri ekle

            // Formun dýþýna týklandýðýnda kapanmasýný saðlamak için global mouse hook (WinForms için özel bir yaklaþým)
            this.Deactivate += new EventHandler(Form_Deactivate);

            // Formun baþlangýç konumunu ekranýn sað üst köþesine ayarla (yüzdelik oranlarla)
            SetStartPosition();
        }

        private void SetupForm()
        {
            this.Size = normalSize;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.BackColor = Color.LightBlue;
        }

        private void MakeFormRounded()
        {
            // Köþeleri yuvarlak yap (20 piksel yuvarlaklýk, istediðin kadar deðiþtirebilirsin)
            IntPtr ptr = CreateRoundRectRgn(0, 0, this.Width, this.Height, 20, 20);
            this.Region = Region.FromHrgn(ptr);
        }

        private void ConfigureTimer()
        {
            if (timer1 != null)
            {
                timer1.Interval = ANIMATION_DELAY; // 15 milisaniye aralýk
                timer1.Tick += new EventHandler(timer1_Tick);
            }
            else
            {
                MessageBox.Show("timer1 bulunamadý, lütfen Form Designer’da bir Timer ekleyin ve adýný 'timer1' olarak kontrol edin.");
            }
        }

        private void InitializeControls()
        {
            // "Ekle" butonu (Form Designer’da tanýmlanacak, burada sadece olay baðlanacak)
            if (button1 != null)
            {
                button1.Click += new EventHandler(button1_Click);
                button1.MouseEnter += new EventHandler(button1_MouseEnter); // Butona hover ekle (formu açmak için)
                button1.MouseLeave += new EventHandler(button1_MouseLeave); // Butondan ayrýlma (hiçbir þey yapma)
            }
            else
            {
                MessageBox.Show("button1 bulunamadý, lütfen Form Designer’da bir Button eklein ve adýný 'button1' olarak kontrol edin.");
            }

            // Favori butonlarýný göstermek için dinamik yerleþtirme (þimdilik boþ, UpdateFavoritesButtons’da doldurulacak)
            UpdateFavoritesButtons();

            // Hata ayýklama için Label (isteðe baðlý, kaldýrýlabilir)
            Label debugLabel = new Label
            {
                Name = "debugLabel",
                Size = new Size(180, 20),
                Location = new Point(5, 225), // Formun hover boyutunda alt kýsmýna yerleþtir (200x250)
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(debugLabel);
        }

        private void SetStartPosition()
        {
            // Ekranýn yüzdelik oranlarýna göre formun konumunu hesapla (CSS benzeri)
            Screen primaryScreen = Screen.PrimaryScreen;
            int screenWidth = primaryScreen.WorkingArea.Width;
            int screenHeight = primaryScreen.WorkingArea.Height;

            // Formun normal boyutuna göre sað üst köþe pozisyonu (yüzdelik oranlarla)
            // Sað kenar için %100 geniþlik (ekranýn saðýndan baþla)
            float widthPercentage = 90f; // %100 (sað kenar)
            float heightPercentage = 5f;  // %5 (yaklaþýk 20 piksel için, ekran yüksekliðine göre)

            int formX = (int)((screenWidth * widthPercentage / 100) - normalSize.Width); // Saðdan form geniþliði kadar içeride
            int formY = (int)(screenHeight * heightPercentage / 100); // Ekran yüksekliðinin %5’i

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(formX, formY);
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            // Hover olaylarýný devre dýþý býrakýyoruz, yalnýzca button1 üzerinden açma yapacaðýz
        }

        private void Form1_MouseLeave(object sender, EventArgs e)
        {
            // Hover olaylarýný devre dýþý býrakýyoruz, kapanma yalnýzca form dýþýna týklandýðýnda olacak
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            // Mouse ile sürükleme için güncellendi
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                this.Location = mousePos;

                // Hata ayýklama için (isteðe baðlý, kaldýrýlabilir)
                UpdateDebugLabel($"Sürükleniyor - X: {mousePos.X}, Y: {mousePos.Y}, isMouseDown: {isMouseDown}");
            }
        }

        private void Form_Deactivate(object sender, EventArgs e)
        {
            // Form odaðýný kaybettiðinde (mouse formun dýþýnda bir yere týklandýðýnda) küçült
            if (this.Size == hoverSize) // Yalnýzca hover boyutundaysa
            {
                if (!timer1.Enabled) // Timer çalýþmýyorsa
                {
                    isExpanding = false;
                    currentSizeStep = ANIMATION_STEPS - 1; // Küçülme için baþlangýç
                    timer1.Start(); // Formu küçült
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.SuspendLayout(); // Yenilemeyi geçici olarak durdur
            try
            {
                if (isExpanding)
                {
                    currentSizeStep++;
                    if (currentSizeStep <= ANIMATION_STEPS)
                    {
                        int newWidth = normalSize.Width + (hoverSize.Width - normalSize.Width) * currentSizeStep / ANIMATION_STEPS;
                        int newHeight = normalSize.Height + (hoverSize.Height - normalSize.Height) * currentSizeStep / ANIMATION_STEPS;
                        this.Size = new Size(newWidth, newHeight);
                        MakeFormRounded(); // Boyut deðiþtiðinde köþeleri yine yuvarlak yap
                    }
                    else
                    {
                        timer1.Stop();
                        this.Size = hoverSize; // Tam boyuta ulaþ
                        MakeFormRounded();
                    }
                }
                else // Küçülme
                {
                    currentSizeStep--;
                    if (currentSizeStep >= 0)
                    {
                        int newWidth = hoverSize.Width - (hoverSize.Width - normalSize.Width) * (ANIMATION_STEPS - currentSizeStep) / ANIMATION_STEPS;
                        int newHeight = hoverSize.Height - (hoverSize.Height - normalSize.Height) * (ANIMATION_STEPS - currentSizeStep) / ANIMATION_STEPS;
                        this.Size = new Size(newWidth, newHeight);
                        MakeFormRounded(); // Boyut deðiþtiðinde köþeleri yine yuvarlak yap
                    }
                    else
                    {
                        timer1.Stop();
                        this.Size = normalSize; // Orijinal boyuta dön
                        MakeFormRounded();
                    }
                }
            }
            finally
            {
                this.ResumeLayout(); // Yenilemeyi yeniden baþlat
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            // Mouse ile sürükleme için güncellendi
            if (e.Button == MouseButtons.Left)
            {
                Point clientPoint = this.PointToClient(Cursor.Position);
                if (IsPointInRegion(clientPoint)) // Yalnýzca formun yuvarlak Region alaný içinde týklandýysa
                {
                    // Kontrollerin üzerine týklandýysa sürüklemeyi engelle
                    bool isOverControl = false;
                    foreach (Control control in this.Controls)
                    {
                        if (control.Bounds.Contains(clientPoint))
                        {
                            isOverControl = true;
                            break;
                        }
                    }

                    if (!isOverControl) // Yalnýzca formun boþ alanýna týklandýysa
                    {
                        mouseOffset = new Point(-clientPoint.X, -clientPoint.Y);
                        isMouseDown = true;

                        // Hata ayýklama için (isteðe baðlý, kaldýrýlabilir)
                        UpdateDebugLabel($"Mouse Down - X: {clientPoint.X}, Y: {clientPoint.Y}, In Region: {IsPointInRegion(clientPoint)}, Over Control: {isOverControl}");
                    }
                    else
                    {
                        // Hata ayýklama için (isteðe baðlý, kaldýrýlabilir)
                        UpdateDebugLabel($"Mouse Down - X: {clientPoint.X}, Y: {clientPoint.Y}, Over Control: {isOverControl}");
                    }
                }
                else
                {
                    // Hata ayýklama için (isteðe baðlý, kaldýrýlabilir)
                    UpdateDebugLabel($"Mouse Down - X: {clientPoint.X}, Y: {clientPoint.Y}, NOT In Region");
                }
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;

                // Hata ayýklama için (isteðe baðlý, kaldýrýlabilir)
                UpdateDebugLabel("Mouse Up - Sürükleme bitti");
            }
        }

        private bool IsPointInRegion(Point point)
        {
            // Formun yuvarlak Region alaný içinde mi kontrol et
            if (this.Region != null)
            {
                return this.Region.IsVisible(point);
            }
            return this.ClientRectangle.Contains(point); // Region yoksa istemci alaný kontrol et
        }

        private void UpdateDebugLabel(string text)
        {
            // Hata ayýklama için Label güncelle (isteðe baðlý, kaldýrýlabilir)
            foreach (Control control in this.Controls)
            {
                if (control is Label label && label.Name == "debugLabel")
                {
                    label.Text = text;
                    break;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Uygulama Ekle", null, ApplicationAdd_Click);
            menu.Items.Add("Klasör Ekle", null, FolderAdd_Click);
            menu.Items.Add("Çýkýþ", null, Exit_Click); // Çýkýþ seçeneðini ekledik
            menu.Show(button1, new Point(0, button1.Height));
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            if (!timer1.Enabled) // Timer çalýþmýyorsa
            {
                isExpanding = true;
                currentSizeStep = 0;
                timer1.Start(); // Formu geniþlet (buton üzerinden)
            }
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            // Butondan ayrýldýðýnda hiçbir þey yapma, kapanma yalnýzca form dýþýna týklandýðýnda olacak
        }

        private void ApplicationAdd_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
                openFileDialog.Title = "Favori Uygulama Seç";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    favorites.Add(filePath);
                    UpdateFavoritesButtons();
                }
            }
        }

        private void FolderAdd_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Favori Klasör Seç";
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = folderBrowserDialog.SelectedPath;
                    favorites.Add(folderPath);
                    UpdateFavoritesButtons();
                }
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            // Uygulamayý tamamen kapat
            Application.Exit();
        }

        private void UpdateFavoritesButtons()
        {
            // Mevcut favori butonlarýný temizle
            foreach (Control control in this.Controls)
            {
                if (control is Button favButton && control.Name.StartsWith("favButton"))
                {
                    this.Controls.Remove(favButton);
                    favButton.Dispose();
                }
            }

            // Yeni favori butonlarýný ekle
            int buttonWidth = 50; // button1 ile ayný geniþlik
            int buttonHeight = 40; // Artýrýlmýþ yükseklik
            int padding = 5; // Butonlar arasý boþluk
            int x = 5; // Baþlangýç x konumu
            int y = 55; // Butonun altýna yerleþtir (button1’in altýndan baþla)

            for (int i = 0; i < favorites.Count; i++)
            {
                string favorite = favorites[i];
                string buttonText = System.IO.Path.GetFileName(favorite) ?? favorite; // Dosya/klasör adýný göster, uzun isimleri kýsalt

                // Buton metnini 15 karakterle sýnýrla (yükseklik artýnca daha fazla karakter sýðabilir)
                if (buttonText.Length > 15)
                {
                    buttonText = buttonText.Substring(0, 15) + "...";
                }

                Button favButton = new Button
                {
                    Name = $"favButton{i}",
                    Text = buttonText,
                    Size = new Size(buttonWidth, buttonHeight), // Buton boyutu burada ayarlanýyor
                    Location = new Point(x, y),
                    BackColor = Color.LightGray
                };

                // Uygulama ise ikon ekle
                if (System.IO.File.Exists(favorite) && Path.GetExtension(favorite).ToLower() == ".exe")
                {
                    try
                    {
                        Icon icon = Icon.ExtractAssociatedIcon(favorite);
                        if (icon != null)
                        {
                            favButton.Image = icon.ToBitmap().GetThumbnailImage(16, 16, null, IntPtr.Zero); // 16x16 piksel ikon
                            favButton.ImageAlign = ContentAlignment.MiddleLeft; // Ýkonu butonun soluna yerleþtir
                            favButton.TextAlign = ContentAlignment.MiddleRight; // Metni saða kaydýr
                            favButton.TextImageRelation = TextImageRelation.ImageBeforeText; // Ýkon metnin solunda
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ýkon yüklenemedi: {ex.Message}");
                    }
                }

                // Sað týklama menüsü için ContextMenuStrip ekle
                ContextMenuStrip contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Sil", null, (s, ev) => RemoveFavorite(favButton)); // Butonu parametre olarak gönderiyoruz
                favButton.ContextMenuStrip = contextMenu;

                favButton.Click += (s, e) => FavoriteButton_Click(s, e, favorite);
                this.Controls.Add(favButton);

                x += buttonWidth + padding; // Yeni buton için x konumunu güncelle
                if (x + buttonWidth > hoverSize.Width - padding) // Satýr sonuna geldiyse
                {
                    x = 5; // Yeni satýr için x’i sýfýrla
                    y += buttonHeight + padding; // Yeni satýr için y’i artýr
                }
            }
        }

        private void FavoriteButton_Click(object sender, EventArgs e, string favoritePath)
        {
            if (System.IO.File.Exists(favoritePath)) // Eðer dosya ise (uygulama)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = favoritePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Uygulama baþlatýlamadý: {ex.Message}");
                }
            }
            else if (System.IO.Directory.Exists(favoritePath)) // Eðer klasör ise
            {
                try
                {
                    Process.Start("explorer.exe", favoritePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Klasör açýlamadý: {ex.Message}");
                }
            }
        }

        private void RemoveFavorite(Button favButton)
        {
            // Butonun adýndan index’i çýkar
            string buttonName = favButton.Name;
            if (buttonName.StartsWith("favButton") && int.TryParse(buttonName.Replace("favButton", ""), out int index))
            {
                if (index >= 0 && index < favorites.Count)
                {
                    favorites.RemoveAt(index);
                    UpdateFavoritesButtons(); // Butonlarý güncelle
                    MessageBox.Show("Favori baþarýyla silindi.");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Form yüklendiðinde ekstra bir þey yapmaný istiyorsan buraya ekleyebilirsin
        }
    }
}