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
        private Size normalSize = new Size(50, 50); // Formun ba�lang�� boyutu
        private Size hoverSize = new Size(200, 250); // Formun a��ld���nda boyutu (y�ksekli�i art�rd�k, butonlar i�in yer a�mak i�in)
        private Point mouseOffset;
        private bool isMouseDown = false;
        private bool isExpanding = false; // Formun geni�leme/k���lme durumu
        private int currentSizeStep = 0; // Animasyon i�in ad�m
        private const int ANIMATION_STEPS = 8; // Animasyon ad�m� say�s�
        private const int ANIMATION_DELAY = 15; // Animasyon h�z� (milisaniye)

        private List<string> favorites = new List<string>(); // Favori uygulamalar/klas�rler listesi

        [DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public Form1()
        {
            InitializeComponent();
            // Performans optimizasyonlar� i�in SetStyle
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // �ift tamponlama optimize et
            this.SetStyle(ControlStyles.UserMouse, true); // Mouse olaylar�n� optimize et
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true); // Yenileme performans�n� art�r
            this.SetStyle(ControlStyles.ResizeRedraw, true); // Boyut de�i�iminde yeniden �iz
            this.UpdateStyles(); // Stil g�ncellemelerini uygula

            this.DoubleBuffered = true; // Ekstra �ift tamponlama
            SetupForm(); // Form �zelliklerini ve olaylar� manuel olarak ayarla
            MakeFormRounded(); // K��eleri yuvarlak yap
            ConfigureTimer(); // Timer�� yap�land�r
            InitializeControls(); // Buton ve di�er kontrolleri ekle

            // Formun d���na t�kland���nda kapanmas�n� sa�lamak i�in global mouse hook (WinForms i�in �zel bir yakla��m)
            this.Deactivate += new EventHandler(Form_Deactivate);

            // Formun ba�lang�� konumunu ekran�n sa� �st k��esine ayarla (y�zdelik oranlarla)
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
            // K��eleri yuvarlak yap (20 piksel yuvarlakl�k, istedi�in kadar de�i�tirebilirsin)
            IntPtr ptr = CreateRoundRectRgn(0, 0, this.Width, this.Height, 20, 20);
            this.Region = Region.FromHrgn(ptr);
        }

        private void ConfigureTimer()
        {
            if (timer1 != null)
            {
                timer1.Interval = ANIMATION_DELAY; // 15 milisaniye aral�k
                timer1.Tick += new EventHandler(timer1_Tick);
            }
            else
            {
                MessageBox.Show("timer1 bulunamad�, l�tfen Form Designer�da bir Timer ekleyin ve ad�n� 'timer1' olarak kontrol edin.");
            }
        }

        private void InitializeControls()
        {
            // "Ekle" butonu (Form Designer�da tan�mlanacak, burada sadece olay ba�lanacak)
            if (button1 != null)
            {
                button1.Click += new EventHandler(button1_Click);
                button1.MouseEnter += new EventHandler(button1_MouseEnter); // Butona hover ekle (formu a�mak i�in)
                button1.MouseLeave += new EventHandler(button1_MouseLeave); // Butondan ayr�lma (hi�bir �ey yapma)
            }
            else
            {
                MessageBox.Show("button1 bulunamad�, l�tfen Form Designer�da bir Button eklein ve ad�n� 'button1' olarak kontrol edin.");
            }

            // Favori butonlar�n� g�stermek i�in dinamik yerle�tirme (�imdilik bo�, UpdateFavoritesButtons�da doldurulacak)
            UpdateFavoritesButtons();

            // Hata ay�klama i�in Label (iste�e ba�l�, kald�r�labilir)
            Label debugLabel = new Label
            {
                Name = "debugLabel",
                Size = new Size(180, 20),
                Location = new Point(5, 225), // Formun hover boyutunda alt k�sm�na yerle�tir (200x250)
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(debugLabel);
        }

        private void SetStartPosition()
        {
            // Ekran�n y�zdelik oranlar�na g�re formun konumunu hesapla (CSS benzeri)
            Screen primaryScreen = Screen.PrimaryScreen;
            int screenWidth = primaryScreen.WorkingArea.Width;
            int screenHeight = primaryScreen.WorkingArea.Height;

            // Formun normal boyutuna g�re sa� �st k��e pozisyonu (y�zdelik oranlarla)
            // Sa� kenar i�in %100 geni�lik (ekran�n sa��ndan ba�la)
            float widthPercentage = 90f; // %100 (sa� kenar)
            float heightPercentage = 5f;  // %5 (yakla��k 20 piksel i�in, ekran y�ksekli�ine g�re)

            int formX = (int)((screenWidth * widthPercentage / 100) - normalSize.Width); // Sa�dan form geni�li�i kadar i�eride
            int formY = (int)(screenHeight * heightPercentage / 100); // Ekran y�ksekli�inin %5�i

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(formX, formY);
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            // Hover olaylar�n� devre d��� b�rak�yoruz, yaln�zca button1 �zerinden a�ma yapaca��z
        }

        private void Form1_MouseLeave(object sender, EventArgs e)
        {
            // Hover olaylar�n� devre d��� b�rak�yoruz, kapanma yaln�zca form d���na t�kland���nda olacak
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            // Mouse ile s�r�kleme i�in g�ncellendi
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                this.Location = mousePos;

                // Hata ay�klama i�in (iste�e ba�l�, kald�r�labilir)
                UpdateDebugLabel($"S�r�kleniyor - X: {mousePos.X}, Y: {mousePos.Y}, isMouseDown: {isMouseDown}");
            }
        }

        private void Form_Deactivate(object sender, EventArgs e)
        {
            // Form oda��n� kaybetti�inde (mouse formun d���nda bir yere t�kland���nda) k���lt
            if (this.Size == hoverSize) // Yaln�zca hover boyutundaysa
            {
                if (!timer1.Enabled) // Timer �al��m�yorsa
                {
                    isExpanding = false;
                    currentSizeStep = ANIMATION_STEPS - 1; // K���lme i�in ba�lang��
                    timer1.Start(); // Formu k���lt
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.SuspendLayout(); // Yenilemeyi ge�ici olarak durdur
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
                        MakeFormRounded(); // Boyut de�i�ti�inde k��eleri yine yuvarlak yap
                    }
                    else
                    {
                        timer1.Stop();
                        this.Size = hoverSize; // Tam boyuta ula�
                        MakeFormRounded();
                    }
                }
                else // K���lme
                {
                    currentSizeStep--;
                    if (currentSizeStep >= 0)
                    {
                        int newWidth = hoverSize.Width - (hoverSize.Width - normalSize.Width) * (ANIMATION_STEPS - currentSizeStep) / ANIMATION_STEPS;
                        int newHeight = hoverSize.Height - (hoverSize.Height - normalSize.Height) * (ANIMATION_STEPS - currentSizeStep) / ANIMATION_STEPS;
                        this.Size = new Size(newWidth, newHeight);
                        MakeFormRounded(); // Boyut de�i�ti�inde k��eleri yine yuvarlak yap
                    }
                    else
                    {
                        timer1.Stop();
                        this.Size = normalSize; // Orijinal boyuta d�n
                        MakeFormRounded();
                    }
                }
            }
            finally
            {
                this.ResumeLayout(); // Yenilemeyi yeniden ba�lat
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            // Mouse ile s�r�kleme i�in g�ncellendi
            if (e.Button == MouseButtons.Left)
            {
                Point clientPoint = this.PointToClient(Cursor.Position);
                if (IsPointInRegion(clientPoint)) // Yaln�zca formun yuvarlak Region alan� i�inde t�kland�ysa
                {
                    // Kontrollerin �zerine t�kland�ysa s�r�klemeyi engelle
                    bool isOverControl = false;
                    foreach (Control control in this.Controls)
                    {
                        if (control.Bounds.Contains(clientPoint))
                        {
                            isOverControl = true;
                            break;
                        }
                    }

                    if (!isOverControl) // Yaln�zca formun bo� alan�na t�kland�ysa
                    {
                        mouseOffset = new Point(-clientPoint.X, -clientPoint.Y);
                        isMouseDown = true;

                        // Hata ay�klama i�in (iste�e ba�l�, kald�r�labilir)
                        UpdateDebugLabel($"Mouse Down - X: {clientPoint.X}, Y: {clientPoint.Y}, In Region: {IsPointInRegion(clientPoint)}, Over Control: {isOverControl}");
                    }
                    else
                    {
                        // Hata ay�klama i�in (iste�e ba�l�, kald�r�labilir)
                        UpdateDebugLabel($"Mouse Down - X: {clientPoint.X}, Y: {clientPoint.Y}, Over Control: {isOverControl}");
                    }
                }
                else
                {
                    // Hata ay�klama i�in (iste�e ba�l�, kald�r�labilir)
                    UpdateDebugLabel($"Mouse Down - X: {clientPoint.X}, Y: {clientPoint.Y}, NOT In Region");
                }
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;

                // Hata ay�klama i�in (iste�e ba�l�, kald�r�labilir)
                UpdateDebugLabel("Mouse Up - S�r�kleme bitti");
            }
        }

        private bool IsPointInRegion(Point point)
        {
            // Formun yuvarlak Region alan� i�inde mi kontrol et
            if (this.Region != null)
            {
                return this.Region.IsVisible(point);
            }
            return this.ClientRectangle.Contains(point); // Region yoksa istemci alan� kontrol et
        }

        private void UpdateDebugLabel(string text)
        {
            // Hata ay�klama i�in Label g�ncelle (iste�e ba�l�, kald�r�labilir)
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
            menu.Items.Add("Klas�r Ekle", null, FolderAdd_Click);
            menu.Items.Add("��k��", null, Exit_Click); // ��k�� se�ene�ini ekledik
            menu.Show(button1, new Point(0, button1.Height));
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            if (!timer1.Enabled) // Timer �al��m�yorsa
            {
                isExpanding = true;
                currentSizeStep = 0;
                timer1.Start(); // Formu geni�let (buton �zerinden)
            }
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            // Butondan ayr�ld���nda hi�bir �ey yapma, kapanma yaln�zca form d���na t�kland���nda olacak
        }

        private void ApplicationAdd_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
                openFileDialog.Title = "Favori Uygulama Se�";
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
                folderBrowserDialog.Description = "Favori Klas�r Se�";
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
            // Uygulamay� tamamen kapat
            Application.Exit();
        }

        private void UpdateFavoritesButtons()
        {
            // Mevcut favori butonlar�n� temizle
            foreach (Control control in this.Controls)
            {
                if (control is Button favButton && control.Name.StartsWith("favButton"))
                {
                    this.Controls.Remove(favButton);
                    favButton.Dispose();
                }
            }

            // Yeni favori butonlar�n� ekle
            int buttonWidth = 50; // button1 ile ayn� geni�lik
            int buttonHeight = 40; // Art�r�lm�� y�kseklik
            int padding = 5; // Butonlar aras� bo�luk
            int x = 5; // Ba�lang�� x konumu
            int y = 55; // Butonun alt�na yerle�tir (button1�in alt�ndan ba�la)

            for (int i = 0; i < favorites.Count; i++)
            {
                string favorite = favorites[i];
                string buttonText = System.IO.Path.GetFileName(favorite) ?? favorite; // Dosya/klas�r ad�n� g�ster, uzun isimleri k�salt

                // Buton metnini 15 karakterle s�n�rla (y�kseklik art�nca daha fazla karakter s��abilir)
                if (buttonText.Length > 15)
                {
                    buttonText = buttonText.Substring(0, 15) + "...";
                }

                Button favButton = new Button
                {
                    Name = $"favButton{i}",
                    Text = buttonText,
                    Size = new Size(buttonWidth, buttonHeight), // Buton boyutu burada ayarlan�yor
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
                            favButton.ImageAlign = ContentAlignment.MiddleLeft; // �konu butonun soluna yerle�tir
                            favButton.TextAlign = ContentAlignment.MiddleRight; // Metni sa�a kayd�r
                            favButton.TextImageRelation = TextImageRelation.ImageBeforeText; // �kon metnin solunda
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"�kon y�klenemedi: {ex.Message}");
                    }
                }

                // Sa� t�klama men�s� i�in ContextMenuStrip ekle
                ContextMenuStrip contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Sil", null, (s, ev) => RemoveFavorite(favButton)); // Butonu parametre olarak g�nderiyoruz
                favButton.ContextMenuStrip = contextMenu;

                favButton.Click += (s, e) => FavoriteButton_Click(s, e, favorite);
                this.Controls.Add(favButton);

                x += buttonWidth + padding; // Yeni buton i�in x konumunu g�ncelle
                if (x + buttonWidth > hoverSize.Width - padding) // Sat�r sonuna geldiyse
                {
                    x = 5; // Yeni sat�r i�in x�i s�f�rla
                    y += buttonHeight + padding; // Yeni sat�r i�in y�i art�r
                }
            }
        }

        private void FavoriteButton_Click(object sender, EventArgs e, string favoritePath)
        {
            if (System.IO.File.Exists(favoritePath)) // E�er dosya ise (uygulama)
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
                    MessageBox.Show($"Uygulama ba�lat�lamad�: {ex.Message}");
                }
            }
            else if (System.IO.Directory.Exists(favoritePath)) // E�er klas�r ise
            {
                try
                {
                    Process.Start("explorer.exe", favoritePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Klas�r a��lamad�: {ex.Message}");
                }
            }
        }

        private void RemoveFavorite(Button favButton)
        {
            // Butonun ad�ndan index�i ��kar
            string buttonName = favButton.Name;
            if (buttonName.StartsWith("favButton") && int.TryParse(buttonName.Replace("favButton", ""), out int index))
            {
                if (index >= 0 && index < favorites.Count)
                {
                    favorites.RemoveAt(index);
                    UpdateFavoritesButtons(); // Butonlar� g�ncelle
                    MessageBox.Show("Favori ba�ar�yla silindi.");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Form y�klendi�inde ekstra bir �ey yapman� istiyorsan buraya ekleyebilirsin
        }
    }
}