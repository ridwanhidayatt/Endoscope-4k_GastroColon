﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CsvHelper;
using System.Windows.Forms;
using System.Text;
using System.Xml.Linq;
using PictureBox = System.Windows.Forms.PictureBox;
using System.Drawing.Drawing2D;

namespace WindowsFormsApp1.Format_4
{
    public partial class Form48Gambar : Form
    {
        public int TEClose10Gambar { get; internal set; }

        public delegate void TransfDelegate(String value);
        public event TransfDelegate TEClose48Gambar;


        string splitTahun, splitBulan, tanggal;
        string logoValue, jenisValue;

        string dirRtf = @"D:\GLEndoscope\FileRTF\";
        string dirLogo = @"D:\GLEndoscope\LogoKOP\";
        string csvFilePath = "D:\\GLEndoscope\\Database\\dataPasien\\dataDefault.csv";

        string ambilDaerah, gabung, gabung1, jam;
        string[] new_order = new string[10];


        private bool isButton2Pressed = false;
        private bool isButton4Pressed = false;
        private bool isButton6Pressed = false;
        private bool isButton8Pressed = false;

        private string selectedPrinter;

        private Dictionary<PictureBox, PictureBoxControls> pictureBoxControls = new Dictionary<PictureBox, PictureBoxControls>();

        public Form48Gambar()
        {
            InitializeComponent();

            // Tambahkan item ke ComboBox
            comboBox3.Items.Add("Gastrokopi");
            comboBox3.Items.Add("Kolonoskopi");

            // Event ketika item di ComboBox berubah
            comboBox3.SelectedIndexChanged += ComboBox3_SelectedIndexChanged;


            PopulatePrinterComboBox(); // Call to populate printers
            comboBox1.SelectedIndex = -1; // Ensure no printer is selected by default
            InitializeThumbnails();
            InitializeThumbnailsForToday();
            InitializeMainPictureBoxes();
            InitializeComboBox();
            InitializeComboBoxNow();

            comboBox1.KeyPress += new KeyPressEventHandler(ComboBox_KeyPress);
            comboBox2.KeyPress += new KeyPressEventHandler(ComboBox_KeyPress);

            //FormatRichTextBoxJustified();
        }

        //private void FormatRichTextBoxJustified()
        //{
        //    richTextBox1.Clear();
        //    //richTextBox1.Font = new Font("Consolas", 10);

        //    richTextBox1.AppendText("ESOFAGUS :\n\n");
        //    richTextBox1.AppendText("GASTER    \n");
        //    richTextBox1.AppendText("CARDIA :\n");
        //    richTextBox1.AppendText("FUNDUS :\n");
        //    richTextBox1.AppendText("CORPUS :\n");
        //    richTextBox1.AppendText("ANTRUM :\n\n");
        //    richTextBox1.AppendText("DUODENUM  \n");
        //    richTextBox1.AppendText("BULBUS :\n");
        //    richTextBox1.AppendText("PART II :\n");

        //}


        private void InitializeComboBox()
        {
            // Menambahkan placeholder item
            cbx_baru.Items.Add("-PILIH TANGGAL-");
            //MessageBox.Show("Placeholder added: " + cbx_baru.Items.Count);  // Debugging

            // Menyimpan index placeholder
            int placeholderIndex = cbx_baru.Items.Count - 1;

            // Set dropdown style untuk mencegah pengeditan
            cbx_baru.DropDownStyle = ComboBoxStyle.DropDownList;

            // Event handler untuk memastikan placeholder hilang ketika item dipilih
            cbx_baru.SelectedIndexChanged += (s, e) =>
            {
                // Jika placeholder dipilih, reset kembali ke placeholder
                if (cbx_baru.SelectedIndex == placeholderIndex)
                {
                    // Pilih item placeholder kembali
                    cbx_baru.SelectedIndex = placeholderIndex;
                }
            };

            // Set placeholder lagi jika pengguna tidak memilih item valid
            this.Load += (s, e) => EnsurePlaceholder(placeholderIndex);
        }

        // Metode terpisah untuk memastikan placeholder
        private void EnsurePlaceholder(int placeholderIndex)
        {
            if (cbx_baru.SelectedIndex == -1 || cbx_baru.SelectedIndex == placeholderIndex)
            {
                cbx_baru.SelectedIndex = placeholderIndex;
            }
        }

        public class ComboBoxItem
        {

            public string FolderPath { get; set; }
            public string DisplayText { get; set; }

            public override string ToString()
            {
                // Tampilkan hanya DisplayText
                return DisplayText;
            }
        }

        private void InitializeComboBoxNow()
        {
            cbx_now.DropDownStyle = ComboBoxStyle.DropDownList;

            // Mengurutkan item berdasarkan tanggal terbaru
            if (cbx_now.Items.Count > 1)
            {
                var sortedItems = cbx_now.Items.Cast<ComboBoxItem>()
                    .OrderByDescending(item =>
                    {
                        DateTime parsedDate;
                        // Menggunakan kultur bahasa Indonesia agar sesuai dengan format bulan (misalnya "September")
                        var culture = new CultureInfo("id-ID");

                        // Coba parsing tanggal
                        if (DateTime.TryParse(item.DisplayText, culture, DateTimeStyles.None, out parsedDate))
                        {
                            return parsedDate;
                        }
                        else
                        {
                            // Jika gagal diparsing, kembalikan DateTime.MinValue agar item tersebut berada di urutan paling bawah
                            return DateTime.MinValue;
                        }
                    })
                    .ToList();

                // Bersihkan item ComboBox dan tambahkan kembali item yang sudah diurutkan
                cbx_now.Items.Clear();

                foreach (var item in sortedItems)
                {
                    cbx_now.Items.Add(item);
                }

                // Pilih item terbaru (item pertama setelah diurutkan)
                cbx_now.SelectedIndex = 0;
            }

            // Placeholder handling
            int placeholderIndex = cbx_now.Items.Count - 1;

            cbx_now.SelectedIndexChanged += (s, e) =>
            {
                // Jika placeholder dipilih, biarkan pengguna memilih item lain
                if (cbx_now.SelectedIndex == placeholderIndex)
                {
                    // Tidak melakukan apa-apa, biarkan pengguna memilih
                }
            };

            // Memastikan placeholder dipilih jika tidak ada item valid
            this.Load += (s, e) =>
            {
                if (cbx_now.SelectedIndex == -1 || cbx_now.SelectedIndex == placeholderIndex)
                {
                    cbx_now.SelectedIndex = placeholderIndex; // Pilih placeholder jika tidak ada item valid
                }
            };
        }

        private string AddNewlinesIfTooLong(string inputText, int maxLineLength)
        {
            StringBuilder result = new StringBuilder();
            string[] words = inputText.Split(' ');  // Memecah teks menjadi kata-kata
            int currentLineLength = 0;

            foreach (string word in words)
            {
                // Jika menambahkan kata akan melebihi batas, maka pindah ke baris berikutnya
                if (currentLineLength + word.Length + 1 > maxLineLength)
                {
                    result.AppendLine();  // Menambahkan newline
                    currentLineLength = 0; // Reset panjang baris saat ini
                }

                // Menambahkan kata ke baris dan memperbarui panjang baris
                if (currentLineLength > 0)
                {
                    result.Append(" ");  // Menambahkan spasi jika bukan kata pertama di baris
                    currentLineLength++;  // Menambah 1 untuk spasi
                }

                result.Append(word);  // Menambahkan kata
                currentLineLength += word.Length;  // Menambah panjang kata ke panjang baris
            }

            return result.ToString();
        }

        private void ReadDataFromCSV(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader))
                {
                    // Baca record dari file CSV satu per satu
                    while (csv.Read())
                    {
                        // Ambil data dari record saat ini
                        var noRM = csv.GetField<string>("Rm");
                        var name = csv.GetField<string>("Nama");
                        var action = csv.GetField<string>("Jenis Pemeriksaan");
                        var gender = csv.GetField<string>("Jenis Kelamin");
                        var date = csv.GetField<string>("Tanggal Kunjungan");
                        var tanggalLahir = csv.GetField<string>("Tanggal Lahir");
                        var umur = csv.GetField<string>("Umur");
                        var alamat = csv.GetField<string>("Alamat");
                        var dokterNama = csv.GetField<string>("Dokter");
                        gabung = noRM + "-" + name;

                        DateTime today = DateTime.Now;
                        jam = today.ToString("hhmmss");
                        gabung1 = noRM + "-" + name + "-" + jam;

                        string combinedText = name;
                        textBox9.Text = AddNewlinesIfTooLong(combinedText, 30);

                        string tgl_lahir, tglKunjungan;
                        tgl_lahir = tanggalLahir;
                        textBox10.Text = tgl_lahir + " - " + umur;
                        textBox16.Text = gender;
                        textBox8.Text = noRM;
                        textBox13.Text = action;
                        tglKunjungan = date;

                        string combinedAlamat = alamat;
                        textBox11.Text = AddNewlinesIfTooLong(combinedAlamat, 40);

                        richTextBoxNRS.LoadFile(dirRtf + "RtfFile.rtf", RichTextBoxStreamType.RichText);
                        richTextBoxBE.LoadFile(dirRtf + "RtfFile1.rtf", RichTextBoxStreamType.RichText);
                        richTextBoxJalan.LoadFile(dirRtf + "RtfFile2.rtf", RichTextBoxStreamType.RichText);
                        richTextBoxEmail.LoadFile(dirRtf + "RtfFile3.rtf", RichTextBoxStreamType.RichText);
                        //richTextBox2.LoadFile(dirRtf + "RtfFile5.rtf", RichTextBoxStreamType.RichText);
                        richTextBox5.LoadFile(dirRtf + "RtfFile4.rtf", RichTextBoxStreamType.RichText);

                        ambilDaerah = richTextBox5.Text;
                        labelLokTgl.Text = ambilDaerah + ", " + tglKunjungan;

                        string namaDokter = dokterNama;
                        if (string.IsNullOrWhiteSpace(namaDokter))
                        {
                            MessageBox.Show("Data dokter tidak ditemukan. Mohon isi data dokter di form Pasien terlebih dahulu.", "Data Dokter Tidak Ada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        labelNamaDokter.Text = "(" + namaDokter + ")";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tidak ada data yang tersedia. Mohon isi data pasien terlebih dahulu.", "Informasi!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeThumbnails()
        {
            // Call the method to read data from the CSV file
            ReadDataFromCSV(csvFilePath);

            tanggal = DateTime.Now.ToString("ddMMyyyy");
            string text = DateTime.Now.ToString("Y");
            string[] arr = text.Split(' ');
            splitBulan = arr[0];
            splitTahun = arr[1];

            // Gabungkan string menjadi bagian dari folder path yang akan dicari
            string searchPattern = $@"{gabung}\Image";

            // Root folder
            string rootPath = @"D:\GLEndoscope";

            // Bersihkan ComboBox dan FlowLayoutPanel sebelum memulai
            cbx_baru.Items.Clear();
            flowLayoutPanel1.Controls.Clear();

            // Format untuk mendapatkan nama bulan
            var culture = new System.Globalization.CultureInfo("id-ID");

            // Loop untuk mencari semua subfolder di rootPath
            foreach (string yearFolder in Directory.GetDirectories(rootPath))
            {
                foreach (string monthFolder in Directory.GetDirectories(yearFolder))
                {
                    foreach (string dayFolder in Directory.GetDirectories(monthFolder))
                    {
                        foreach (string patientFolder in Directory.GetDirectories(dayFolder))
                        {
                            // Gabungkan folder dengan folder Image
                            string folderPath = Path.Combine(patientFolder, "Image");

                            // Cek apakah folder saat ini adalah folder yang sesuai
                            if (patientFolder.EndsWith(gabung) && Directory.Exists(folderPath))
                            {
                                // Pastikan folder tersebut memiliki file gambar
                                string[] imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                                    .Where(file => file.ToLower().EndsWith(".jpg") ||
                                                   file.ToLower().EndsWith(".jpeg") ||
                                                   file.ToLower().EndsWith(".png") ||
                                                   file.ToLower().EndsWith(".bmp") ||
                                                   file.ToLower().EndsWith(".gif"))
                                    .ToArray();

                                if (imageFiles.Length > 0)
                                {

                                    string day = Path.GetFileName(dayFolder);

                                    // Memisahkan hari, bulan, dan tahun
                                    string dayPart = day.Substring(0, 2);
                                    string monthPart = day.Substring(2, 2);
                                    string yearPart = day.Substring(4, 4);

                                    // Deklarasikan variabel untuk hasil parsing bulan
                                    int monthNumber;

                                    // Coba konversi bulan menjadi nomor bulan dan ambil nama bulannya
                                    if (int.TryParse(monthPart, out monthNumber) && monthNumber >= 1 && monthNumber <= 12)
                                    {
                                        // Mendapatkan nama bulan berdasarkan culture
                                        monthPart = culture.DateTimeFormat.GetMonthName(monthNumber);
                                    }

                                    // Format tanggal sesuai dengan format yang diinginkan: "dd-MM-yyyy"
                                    string formattedDate = $"{dayPart} - {monthPart} - {yearPart}";

                                    // Tambahkan formattedDate ke ComboBox
                                    cbx_baru.Items.Add(new ComboBoxItem
                                    {
                                        FolderPath = folderPath,
                                        DisplayText = formattedDate
                                    });
                                }
                            }
                        }
                    }
                }
            }

            // Event handler ketika pilihan pada ComboBox berubah
            cbx_baru.SelectedIndexChanged += (s, e) =>
            {
                // Hapus semua thumbnail yang ada di FlowLayoutPanel
                flowLayoutPanel1.Controls.Clear();

                // Dapatkan folder path dari pilihan yang dipilih
                ComboBoxItem selectedItem = cbx_baru.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    string selectedFolder = selectedItem.FolderPath;

                    // Ambil semua file gambar dari folder yang dipilih
                    string[] imageFiles = Directory.GetFiles(selectedFolder, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(file => file.ToLower().EndsWith(".jpg") ||
                                       file.ToLower().EndsWith(".jpeg") ||
                                       file.ToLower().EndsWith(".png") ||
                                       file.ToLower().EndsWith(".bmp") ||
                                       file.ToLower().EndsWith(".gif"))
                        .ToArray();

                    // Tampilkan gambar sebagai thumbnail
                    foreach (string file in imageFiles)
                    {
                        try
                        {
                            Image image = Image.FromFile(file);
                            PictureBox thumbnail = new PictureBox
                            {
                                Image = ResizeImage(image, 1058, 797),
                                SizeMode = PictureBoxSizeMode.StretchImage,
                                Size = new Size(203, 134),
                                Margin = new Padding(2),
                                Tag = file
                            };

                            // Tambahkan event handler untuk menangani klik pada thumbnail
                            thumbnail.MouseDown += Thumbnail_MouseDown;
                            flowLayoutPanel1.Controls.Add(thumbnail);  // Tambahkan thumbnail ke FlowLayoutPanel
                        }
                        catch (Exception ex)
                        {
                            // Jika ada kesalahan saat memuat gambar, tampilkan pesan error
                            MessageBox.Show($"Error loading image {file}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            // Jika tidak ada folder yang cocok
            if (cbx_baru.Items.Count == 0)
            {
                //MessageBox.Show("Tidak ditemukan folder yang sesuai dengan gabungan NORM dan Nama.", "Folder Tidak Ditemukan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }

        //private void InitializeThumbnailsForToday()
        //{
        //    // Call the method to read data from the CSV file
        //    ReadDataFromCSV(csvFilePath);

        //    tanggal = DateTime.Now.ToString("ddMMyyyy");
        //    string text = DateTime.Now.ToString("Y");
        //    string[] arr = text.Split(' ');
        //    splitBulan = arr[0];
        //    splitTahun = arr[1];

        //    // Gabungkan string menjadi bagian dari folder path yang akan dicari
        //    string searchPattern = $@"{gabung}\Image";

        //    // Root folder
        //    string rootPath = @"D:\GLEndoscope";

        //    // Bersihkan ComboBox dan FlowLayoutPanel sebelum memulai
        //    cbx_now.Items.Clear();
        //    flowLayoutPanel2.Controls.Clear();

        //    // Format untuk mendapatkan nama bulan
        //    var culture = new System.Globalization.CultureInfo("id-ID");

        //    // Loop untuk mencari semua subfolder di rootPath
        //    foreach (string yearFolder in Directory.GetDirectories(rootPath))
        //    {
        //        foreach (string monthFolder in Directory.GetDirectories(yearFolder))
        //        {
        //            foreach (string dayFolder in Directory.GetDirectories(monthFolder))
        //            {
        //                foreach (string patientFolder in Directory.GetDirectories(dayFolder))
        //                {
        //                    // Gabungkan folder dengan folder Image
        //                    string folderPath = Path.Combine(patientFolder, "Image");

        //                    // Cek apakah folder saat ini adalah folder yang sesuai
        //                    if (patientFolder.EndsWith(gabung) && Directory.Exists(folderPath))
        //                    {
        //                        // Pastikan folder tersebut memiliki file gambar
        //                        string[] imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
        //                            .Where(file => file.ToLower().EndsWith(".jpg") ||
        //                                           file.ToLower().EndsWith(".jpeg") ||
        //                                           file.ToLower().EndsWith(".png") ||
        //                                           file.ToLower().EndsWith(".bmp") ||
        //                                           file.ToLower().EndsWith(".gif"))
        //                            .ToArray();

        //                        if (imageFiles.Length > 0)
        //                        {
        //                            // Misal 'day' memiliki format seperti '30092024'
        //                            string day = Path.GetFileName(dayFolder);

        //                            // Memisahkan hari, bulan, dan tahun
        //                            string dayPart = day.Substring(0, 2);  // '30'
        //                            string monthPart = day.Substring(2, 2); // '09'
        //                            string yearPart = day.Substring(4, 4);  // '2024'

        //                            // Deklarasikan variabel untuk hasil parsing bulan
        //                            int monthNumber;

        //                            // Coba konversi bulan menjadi nomor bulan dan ambil nama bulannya
        //                            if (int.TryParse(monthPart, out monthNumber) && monthNumber >= 1 && monthNumber <= 12)
        //                            {
        //                                // Mendapatkan nama bulan berdasarkan culture
        //                                monthPart = culture.DateTimeFormat.GetMonthName(monthNumber);
        //                            }

        //                            // Format tanggal sesuai dengan format yang diinginkan: "dd-MM-yyyy"
        //                            string formattedDate = $"{dayPart} - {monthPart} - {yearPart}";

        //                            // Tambahkan formattedDate ke ComboBox
        //                            cbx_now.Items.Add(new ComboBoxItem
        //                            {
        //                                FolderPath = folderPath,
        //                                DisplayText = formattedDate
        //                            });
        //                        }


        //                    }
        //                }
        //            }
        //        }
        //    }

        //    // Event handler ketika pilihan pada ComboBox berubah
        //    cbx_now.SelectedIndexChanged += (s, e) =>
        //    {
        //        // Hapus semua thumbnail yang ada di FlowLayoutPanel
        //        flowLayoutPanel2.Controls.Clear();

        //        // Dapatkan folder path dari pilihan yang dipilih
        //        ComboBoxItem selectedItem = cbx_now.SelectedItem as ComboBoxItem;
        //        if (selectedItem != null)
        //        {
        //            string selectedFolder = selectedItem.FolderPath;

        //            // Ambil semua file gambar dari folder yang dipilih
        //            string[] imageFiles = Directory.GetFiles(selectedFolder, "*.*", SearchOption.TopDirectoryOnly)
        //                .Where(file => file.ToLower().EndsWith(".jpg") ||
        //                               file.ToLower().EndsWith(".jpeg") ||
        //                               file.ToLower().EndsWith(".png") ||
        //                               file.ToLower().EndsWith(".bmp") ||
        //                               file.ToLower().EndsWith(".gif"))
        //                .ToArray();

        //            // Tampilkan gambar sebagai thumbnail
        //            foreach (string file in imageFiles)
        //            {
        //                try
        //                {
        //                    Image image = Image.FromFile(file);
        //                    PictureBox thumbnail = new PictureBox
        //                    {
        //                        Image = ResizeImage(image, 271, 134),
        //                        SizeMode = PictureBoxSizeMode.StretchImage,
        //                        Size = new Size(255, 134),
        //                        Margin = new Padding(5),
        //                        Tag = file
        //                    };

        //                    // Tambahkan event handler untuk menangani klik pada thumbnail
        //                    thumbnail.MouseDown += Thumbnail_MouseDown;
        //                    flowLayoutPanel2.Controls.Add(thumbnail);  // Tambahkan thumbnail ke FlowLayoutPanel
        //                }
        //                catch (Exception ex)
        //                {
        //                    // Jika ada kesalahan saat memuat gambar, tampilkan pesan error
        //                    MessageBox.Show($"Error loading image {file}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                }
        //            }
        //        }
        //    };

        //    // Jika tidak ada folder yang cocok
        //    if (cbx_now.Items.Count == 0)
        //    {
        //        //MessageBox.Show("Tidak ditemukan folder yang sesuai dengan gabungan NORM dan Nama.", "Folder Tidak Ditemukan", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //}





        //doubleklik
        // Deklarasi array PictureBox untuk menyimpan 8 PictureBox
        private PictureBox[] pictureBoxes;

        private void InitializeThumbnailsForToday()
        {
            // Inisialisasi array PictureBox
            pictureBoxes = new PictureBox[] { pictureBox1, pictureBox2, pictureBox3, pictureBox4, pictureBox5, pictureBox6, pictureBox7, pictureBox8 };

            // Call the method to read data from the CSV file
            ReadDataFromCSV(csvFilePath);

            string tanggal = DateTime.Now.ToString("ddMMyyyy");
            string rootPath = @"D:\GLEndoscope";

            // Bersihkan ComboBox dan FlowLayoutPanel
            cbx_now.Items.Clear();
            flowLayoutPanel2.Controls.Clear();

            var culture = new System.Globalization.CultureInfo("id-ID");

            foreach (string yearFolder in Directory.GetDirectories(rootPath))
            {
                foreach (string monthFolder in Directory.GetDirectories(yearFolder))
                {
                    foreach (string dayFolder in Directory.GetDirectories(monthFolder))
                    {
                        foreach (string patientFolder in Directory.GetDirectories(dayFolder))
                        {
                            string folderPath = Path.Combine(patientFolder, "Image");

                            if (patientFolder.EndsWith(gabung) && Directory.Exists(folderPath))
                            {
                                string[] imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                                    .Where(file => file.ToLower().EndsWith(".jpg") ||
                                                   file.ToLower().EndsWith(".jpeg") ||
                                                   file.ToLower().EndsWith(".png") ||
                                                   file.ToLower().EndsWith(".bmp") ||
                                                   file.ToLower().EndsWith(".gif"))
                                    .ToArray();

                                if (imageFiles.Length > 0)
                                {
                                    string day = Path.GetFileName(dayFolder);
                                    string dayPart = day.Substring(0, 2);
                                    string monthPart = day.Substring(2, 2);
                                    string yearPart = day.Substring(4, 4);

                                    int monthNumber;
                                    if (int.TryParse(monthPart, out monthNumber) && monthNumber >= 1 && monthNumber <= 12)
                                    {
                                        monthPart = culture.DateTimeFormat.GetMonthName(monthNumber);
                                    }

                                    string formattedDate = $"{dayPart} - {monthPart} - {yearPart}";

                                    cbx_now.Items.Add(new ComboBoxItem
                                    {
                                        FolderPath = folderPath,
                                        DisplayText = formattedDate
                                    });
                                }
                            }
                        }
                    }
                }
            }

            cbx_now.SelectedIndexChanged += (s, e) =>
            {
                flowLayoutPanel2.Controls.Clear();

                ComboBoxItem selectedItem = cbx_now.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    string selectedFolder = selectedItem.FolderPath;
                    string[] imageFiles = Directory.GetFiles(selectedFolder, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(file => file.ToLower().EndsWith(".jpg") ||
                                       file.ToLower().EndsWith(".jpeg") ||
                                       file.ToLower().EndsWith(".png") ||
                                       file.ToLower().EndsWith(".bmp") ||
                                       file.ToLower().EndsWith(".gif"))
                        .ToArray();

                    foreach (string file in imageFiles)
                    {
                        try
                        {
                            Image image = Image.FromFile(file);
                            PictureBox thumbnail = new PictureBox
                            {
                                Image = ResizeImage(image, 1058, 797),
                                SizeMode = PictureBoxSizeMode.StretchImage,
                                Size = new Size(203, 134),
                                Margin = new Padding(2),
                                Tag = file
                            };

                            // Event handler untuk klik thumbnail
                            thumbnail.Click += (sender, args) =>
                            {
                                // Cari PictureBox yang kosong dan tampilkan gambar
                                foreach (PictureBox pb in pictureBoxes)
                                {
                                    if (pb.Image == null) // Jika PictureBox kosong
                                    {
                                        pb.Image = Image.FromFile(file); // Tampilkan gambar
                                        break; // Hentikan setelah menemukan PictureBox kosong
                                    }
                                }
                            };

                            flowLayoutPanel2.Controls.Add(thumbnail);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error loading image {file}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };


            if (cbx_now.Items.Count == 0)
            {
                //MessageBox.Show("Tidak ditemukan folder yang sesuai dengan gabungan NORM dan Nama.", "Folder Tidak Ditemukan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }



        private void Thumbnail_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox thumbnail = sender as PictureBox;
            if (thumbnail != null)
            {
                thumbnail.DoDragDrop(thumbnail.Tag, DragDropEffects.Copy);
            }
        }

        private void InitializeMainPictureBoxes()
        {
            // Daftar semua PictureBox yang akan digunakan
            PictureBox[] pictureBoxes = { pictureBox1, pictureBox2, pictureBox3, pictureBox4, pictureBox5, pictureBox6, pictureBox7, pictureBox8 };

            // Inisialisasi kontrol untuk setiap PictureBox
            for (int i = 0; i < pictureBoxes.Length; i++)
            {
                PictureBox pictureBox = pictureBoxes[i];

                // Temukan kontrol close dan add dengan nama yang sesuai
                var closeControl = this.Controls.Find($"close{i + 1}", true).FirstOrDefault();
                var addControl = this.Controls.Find($"add{i + 1}", true).FirstOrDefault();

                pictureBoxControls[pictureBox] = new PictureBoxControls
                {
                    CloseControl = closeControl,
                    AddControl = addControl
                };

                pictureBox.AllowDrop = true;
                pictureBox.DragEnter += PictureBox_DragEnter;
                pictureBox.DragDrop += PictureBox_DragDrop;
            }
        }


        private void PictureBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void PictureBox_DragDrop(object sender, DragEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            if (pictureBox != null)
            {
                if (e.Data.GetDataPresent(DataFormats.StringFormat))
                {
                    string filePath = e.Data.GetData(DataFormats.StringFormat) as string;
                    if (filePath != null && File.Exists(filePath))
                    {
                        PictureBoxControls controls;
                        if (pictureBoxControls.TryGetValue(pictureBox, out controls))
                        {
                            if (controls.CloseControl != null) controls.CloseControl.Visible = true;
                            if (controls.AddControl != null) controls.AddControl.Visible = false;

                            //// Mengatur visibilitas tombol berdasarkan PictureBox yang dipilih
                            //if (pictureBox == pictureBox3)
                            //{
                            //    button9.Visible = true;
                            //    button8.Visible = false;
                            //}
                            //else if (pictureBox == pictureBox1)
                            //{
                            //    btn_Delete.Visible = true;
                            //    buttonAdd.Visible = false;
                            //}
                            //else if (pictureBox == pictureBox5)
                            //{
                            //    button6.Visible = true;
                            //    button5.Visible = false;
                            //}
                            //else if (pictureBox == pictureBox4)
                            //{
                            //    button11.Visible = true;
                            //    button10.Visible = false;
                            //}
                            //else if (pictureBox == pictureBox2)
                            //{
                            //    button13.Visible = true;
                            //    button12.Visible = false;
                            //}
                            //else if (pictureBox == pictureBox6)
                            //{
                            //    button15.Visible = true;
                            //    button14.Visible = false;
                            //}

                            pictureBox.Image = Image.FromFile(filePath);
                        }
                    }
                }
            }
        }



        private void PopulatePrinterComboBox()
        {
            try
            {
                // Clear the existing items before populating
                comboBox1.Items.Clear();

                // Get the list of installed printers
                foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                {
                    //// Only add "HP Smart Tank 660-670 series" to the ComboBox
                    //if (printer.Contains("HP Smart Tank 660-670 series"))
                    //if (printer.Contains("Microsoft IPP Class Driver"))
                    if (printer.Contains("EPSON L3210 Series (Copy 1)"))
                        {
                        comboBox1.Items.Add(printer);
                        break; // Stop after adding the specific printer
                    }

                    //comboBox1.Items.Add(printer);
                }

                // Optionally, set default selection (e.g., -1 to not select any printer)
                comboBox1.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi kesalahan saat memuat daftar printer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public static class printer
        {
            [DllImport("winspool.drv",
              CharSet = CharSet.Auto,
              SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern Boolean SetDefaultPrinter(String name);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            if (comboBox1.SelectedIndex != -1)
            {
                selectedPrinter = comboBox1.SelectedItem.ToString();
                string Pname = comboBox1.SelectedItem.ToString();
                printer.SetDefaultPrinter(Pname);
            }
        }

        void FillListBox()
        {
            comboBox1.Items.Clear(); // Membersihkan isi comboBox terlebih dahulu
            comboBox1.Items.Add("Pilih Printer"); // Menambahkan pilihan default

            foreach (var p in PrinterSettings.InstalledPrinters)
            {
                comboBox1.Items.Add(p);
            }

            comboBox1.SelectedIndex = 0; // Mengatur pilihan default yang dipilih
        }


        private void buttonPrint_Click(object sender, EventArgs e)
        {
            // Check if the images are valid based on the button pressed
            if (!AreImagesValid())
            {
                return; // Exit early if images are invalid
            }

            // Check if ComboBoxes are correctly selected
            if (!AreComboBoxesValid())
            {
                return; // Exit early if ComboBox selections are invalid
            }

            // Check if printer is selected
            if (!IsPrinterValid())
            {
                return; // Exit early if the printer selection is invalid
            }

            // Store the selected printer if not already selected
            if (selectedPrinter == null && comboBox1.SelectedIndex != -1)
            {
                selectedPrinter = comboBox1.SelectedItem.ToString();
            }

            // Set up the print document
            PrintDocument pd = new PrintDocument
            {
                DefaultPageSettings = { PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180), Landscape = false }
            };

            if (!string.IsNullOrEmpty(selectedPrinter))
            {
                pd.PrinterSettings.PrinterName = selectedPrinter;

                if (comboBox2.Text == "Default")
                {
                    pd.PrintPage += new PrintPageEventHandler(this.printDocument1_PrintPage);
                }
                else if (comboBox2.Text == "Adjust Brightness")
                {
                    pd.PrintPage += new PrintPageEventHandler(this.printDocument2_PrintPage);
                }

                // Print the document
                pd.Print();

                // Log history and update UI
                HistoryPrintA4(comboBox2.Text);
                PopulatePrinterComboBox();
                //comboBox2.SelectedIndex = 0; // Reset profile to default

                MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Printer yang dipilih tidak valid atau tidak tersedia.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Helper method to validate image inputs based on button states
        private bool AreImagesValid()
        {
            if (isButton2Pressed && pictureBox1.Image == null && pictureBox2.Image == null)
            {
                MessageBox.Show("Gambar harus diisi semua", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (isButton4Pressed && (pictureBox1.Image == null || pictureBox2.Image == null || pictureBox3.Image == null || pictureBox4.Image == null))
            {
                MessageBox.Show("Gambar harus diisi semua", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (isButton6Pressed && (pictureBox1.Image == null || pictureBox2.Image == null || pictureBox3.Image == null || pictureBox4.Image == null || pictureBox5.Image == null || pictureBox6.Image == null))
            {
                MessageBox.Show("Gambar harus diisi semua", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (isButton8Pressed && (pictureBox1.Image == null || pictureBox2.Image == null || pictureBox3.Image == null || pictureBox4.Image == null || pictureBox5.Image == null || pictureBox6.Image == null || pictureBox7.Image == null || pictureBox8.Image == null))
            {
                MessageBox.Show("Gambar harus diisi semua", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        // Helper method to validate ComboBox selections
        private bool AreComboBoxesValid()
        {
            if (comboBox3.SelectedIndex == -1 || comboBox3.SelectedItem.ToString() == "Pilih Jenis")
            {
                MessageBox.Show("Pilih Jenis terlebih dahulu", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (comboBox2.SelectedIndex == -1 || comboBox2.SelectedItem.ToString() == "Pilih Profil")
            {
                MessageBox.Show("Pilih Profil yang dipakai", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // Helper method to validate the printer selection
        private bool IsPrinterValid()
        {
            if (comboBox1.SelectedIndex == -1 && string.IsNullOrEmpty(selectedPrinter))
            {
                MessageBox.Show("Pilih printer yang digunakan", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }


        private void AdjustPictureBoxSize(Graphics graphics, string jenis)
        {
            // Check the value of jenis and adjust the size of PictureBox controls accordingly
            if (jenis == "Persegi")
            {
                picLogo1.Size = new Size(100, 100);
                picLogo2.Size = new Size(100, 100);
            }
            else if (jenis == "Persegi Panjang")
            {
                picLogo1.Size = new Size(150, 100);
                picLogo2.Size = new Size(150, 100);
            }
            // Add more conditions as needed
        }

        private void printDocument2_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (logoValue == "1")
            {
                //AdjustPictureBoxSize(e.Graphics, "Persegi");
                //e.Graphics.DrawImage(picLogo1.Image, 30, 3, picLogo1.Width, picLogo1.Height);

                if (jenisValue == "Persegi Panjang")
                {
                    // Handle case when logoValue is "1" and jenisValue is "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, jenisValue);
                    e.Graphics.DrawImage(picLogo2.Image, 30, 10, picLogo2.Width, picLogo2.Height);
                }
                else
                {
                    // Handle case when logoValue is "1" and jenisValue is not "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, "Persegi");
                    e.Graphics.DrawImage(picLogo1.Image, 30, 10, picLogo1.Width, picLogo1.Height);
                }
            }
            else if (logoValue == "2")
            {
                AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
                e.Graphics.DrawImage(picLogo1.Image, 30, 10, picLogo1.Width, picLogo1.Height);

                // Adjust the coordinates based on jenisValue
                if (jenisValue == "Persegi")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 675, 10, picLogo2.Width, picLogo2.Height);
                }
                else if (jenisValue == "Persegi Panjang")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 625, 10, picLogo2.Width, picLogo2.Height);
                }
            }

            StringFormat SF1 = new StringFormat();
            SF1.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(richTextBoxNRS.Text, new Font("Montserrat", 16, FontStyle.Bold), Brushes.Black, 400, 18, SF1);
            e.Graphics.DrawString(richTextBoxBE.Text, new Font("Montserrat", 14, FontStyle.Bold), Brushes.Black, 400, 45, SF1);
            e.Graphics.DrawString(richTextBoxJalan.Text, new Font("Montserrat", 8, FontStyle.Bold), Brushes.Black, 400, 70, SF1);
            e.Graphics.DrawString(richTextBoxEmail.Text, new Font("Montserrat", 8, FontStyle.Bold), Brushes.Black, 400, 85, SF1);

            Color red = Color.Black;
            Pen redPen = new Pen(red);
            redPen.Width = 1;
            e.Graphics.DrawRectangle(redPen, 30, 125, 100, 21);
            e.Graphics.DrawString(textBox1.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 127);
            e.Graphics.DrawRectangle(redPen, 135, 125, 256, 21);
            e.Graphics.DrawString(textBox8.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 137, 127);

            e.Graphics.DrawRectangle(redPen, 401, 125, 100, 21);
            e.Graphics.DrawString(textBox5.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 401, 127);
            e.Graphics.DrawRectangle(redPen, 506, 125, 269, 21);
            e.Graphics.DrawString(textBox11.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 509, 127);

            e.Graphics.DrawRectangle(redPen, 30, 150, 100, 21);
            e.Graphics.DrawString(textBox3.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 152);
            e.Graphics.DrawRectangle(redPen, 135, 150, 256, 21);
            e.Graphics.DrawString(textBox9.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 137, 152);

            e.Graphics.DrawRectangle(redPen, 401, 150, 100, 21);
            e.Graphics.DrawString(textBox6.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 401, 152);
            e.Graphics.DrawRectangle(redPen, 506, 150, 269, 21);
            e.Graphics.DrawString(textBox12.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 508, 152);

            e.Graphics.DrawRectangle(redPen, 30, 175, 100, 21);
            e.Graphics.DrawString(textBox4.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 177);
            e.Graphics.DrawRectangle(redPen, 135, 175, 175, 21);
            e.Graphics.DrawString(textBox10.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 137, 177);
            e.Graphics.DrawRectangle(redPen, 313, 175, 78, 21);
            e.Graphics.DrawString(textBox16.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 315, 177);

            e.Graphics.DrawRectangle(redPen, 401, 175, 100, 21);
            e.Graphics.DrawString(textBox7.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 401, 177);
            e.Graphics.DrawRectangle(redPen, 506, 175, 269, 21);
            e.Graphics.DrawString(textBox13.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 508, 177);

            StringFormat SF2 = new StringFormat();
            SF2.Alignment = StringAlignment.Near;
            e.Graphics.DrawRectangle(redPen, 30, 200, 362, 21);
            e.Graphics.DrawString(textBox14.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 270, 200, SF2);
            e.Graphics.DrawRectangle(redPen, 401, 200, 374, 21);
            e.Graphics.DrawString(richTextBox4.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 403, 202);


            e.Graphics.DrawRectangle(redPen, 30, 225, 100, 46);
            e.Graphics.DrawString(" Obat \r\n Premedikasi", new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 227);


            e.Graphics.DrawRectangle(redPen, 135, 225, 640, 21);
            e.Graphics.DrawString(textBox18.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 137, 227);
            e.Graphics.DrawRectangle(redPen, 135, 250, 640, 21);
            e.Graphics.DrawString(textBox19.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 137, 252);

            //float contrast = 1.41f;
            //float contrast = 1.00f;
            float contrast = 0.80f;
            float gamma = 0.715f;
            //float reed = 0.56f;
            float reed = 0.86f;
            float green = 0.35f;
            //float blue = 0.28f;
            float blue = 0.14f;

            ImageAttributes ia = new ImageAttributes();
            float[][] ptsarray = {
                        new float[] { contrast+reed, 0f, 0f, 0f, 0f},
                        new float[] { 0f, contrast+green, 0f, 0f, 0f},
                        new float[] { 0f, 0f, contrast+blue, 0f, 0f},
                        new float[] { 0f, 0f,       0f, 1f, 0f},
                        new float[] {   0, 0,        0, 1f, 1f},
                };
            ia.ClearColorMatrix();
            ia.SetColorMatrix(new ColorMatrix(ptsarray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            ia.SetGamma(gamma, ColorAdjustType.Bitmap);

            //e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(326, 300, 220, 165), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
            //e.Graphics.DrawImage(pictureBox2.Image, new Rectangle(556, 300, 220, 165), 0, 0, pictureBox2.Image.Width, pictureBox2.Image.Height, GraphicsUnit.Pixel, ia);
            //e.Graphics.DrawImage(pictureBox3.Image, new Rectangle(326, 475, 220, 165), 0, 0, pictureBox3.Image.Width, pictureBox3.Image.Height, GraphicsUnit.Pixel, ia);
            //e.Graphics.DrawImage(pictureBox4.Image, new Rectangle(556, 475, 220, 165), 0, 0, pictureBox4.Image.Width, pictureBox4.Image.Height, GraphicsUnit.Pixel, ia);
            //e.Graphics.DrawImage(pictureBox5.Image, new Rectangle(326, 650, 220, 165), 0, 0, pictureBox5.Image.Width, pictureBox5.Image.Height, GraphicsUnit.Pixel, ia);
            //e.Graphics.DrawImage(pictureBox6.Image, new Rectangle(556, 650, 220, 165), 0, 0, pictureBox6.Image.Width, pictureBox6.Image.Height, GraphicsUnit.Pixel, ia);
            //e.Graphics.DrawImage(pictureBox7.Image, new Rectangle(326, 825, 220, 165), 0, 0, pictureBox7.Image.Width, pictureBox7.Image.Height, GraphicsUnit.Pixel, ia);
            //e.Graphics.DrawImage(pictureBox8.Image, new Rectangle(556, 825, 220, 165), 0, 0, pictureBox8.Image.Width, pictureBox8.Image.Height, GraphicsUnit.Pixel, ia);


            // Memeriksa apakah tombol 2 ditekan
            if (isButton2Pressed == true)
            {


                e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(355, 275, 420, 317), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox2.Image, new Rectangle(355, 594, 420, 317), 0, 0, pictureBox2.Image.Width, pictureBox2.Image.Height, GraphicsUnit.Pixel, ia);
            }

            // Memeriksa apakah tombol 4 ditekan
            if (isButton4Pressed == true)
            {
                //MessageBox.Show("Tombol 4 ditekan!");
                //e.Graphics.DrawImage(pictureBox1.Image, 419, 228, 364, 186);
                //e.Graphics.DrawImage(pictureBox2.Image, 419, 419, 364, 186);
                //e.Graphics.DrawImage(pictureBox3.Image, 419, 610, 364, 186);
                //e.Graphics.DrawImage(pictureBox4.Image, 419, 801, 364, 186);

                e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(460, 275, 209, 157), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox2.Image, new Rectangle(460, 434, 209, 157), 0, 0, pictureBox2.Image.Width, pictureBox2.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox3.Image, new Rectangle(460, 593, 209, 157), 0, 0, pictureBox3.Image.Width, pictureBox3.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox4.Image, new Rectangle(460, 752, 209, 157), 0, 0, pictureBox4.Image.Width, pictureBox4.Image.Height, GraphicsUnit.Pixel, ia);
            }

            if (isButton6Pressed)
            {
                //e.Graphics.DrawImage(pictureBox1.Image, 485, 228, 233, 122);
                //e.Graphics.DrawImage(pictureBox2.Image, 485, 355, 233, 122);
                //e.Graphics.DrawImage(pictureBox3.Image, 485, 482, 233, 122);
                //e.Graphics.DrawImage(pictureBox4.Image, 485, 609, 233, 122);
                //e.Graphics.DrawImage(pictureBox5.Image, 485, 736, 233, 122);
                //e.Graphics.DrawImage(pictureBox6.Image, 485, 863, 233, 122); 


                e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(355, 275, 209, 157), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox2.Image, new Rectangle(566, 275, 209, 157), 0, 0, pictureBox2.Image.Width, pictureBox2.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox3.Image, new Rectangle(355, 434, 209, 157), 0, 0, pictureBox3.Image.Width, pictureBox3.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox4.Image, new Rectangle(566, 434, 209, 157), 0, 0, pictureBox4.Image.Width, pictureBox4.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox5.Image, new Rectangle(355, 593, 209, 157), 0, 0, pictureBox5.Image.Width, pictureBox5.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox6.Image, new Rectangle(566, 593, 209, 157), 0, 0, pictureBox6.Image.Width, pictureBox6.Image.Height, GraphicsUnit.Pixel, ia);
            }

            if (isButton8Pressed)
            {

                //e.Graphics.DrawImage(pictureBox1.Image, 404, 228, 195, 94);
                //e.Graphics.DrawImage(pictureBox2.Image, 604, 228, 195, 94);

                //e.Graphics.DrawImage(pictureBox3.Image, 404, 327, 195, 94);
                //e.Graphics.DrawImage(pictureBox4.Image, 604, 327, 195, 94);

                //e.Graphics.DrawImage(pictureBox5.Image, 404, 426, 195, 94);
                //e.Graphics.DrawImage(pictureBox6.Image, 604, 426, 195, 94);

                //e.Graphics.DrawImage(pictureBox7.Image, 404, 525, 195, 94);
                //e.Graphics.DrawImage(pictureBox8.Image, 604, 525, 195, 94);



                //e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(404, 228, 195, 94), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
                //e.Graphics.DrawImage(pictureBox2.Image, new Rectangle(604, 228, 195, 94), 0, 0, pictureBox2.Image.Width, pictureBox2.Image.Height, GraphicsUnit.Pixel, ia);

                //e.Graphics.DrawImage(pictureBox3.Image, new Rectangle(404, 327, 195, 94), 0, 0, pictureBox3.Image.Width, pictureBox3.Image.Height, GraphicsUnit.Pixel, ia);
                //e.Graphics.DrawImage(pictureBox4.Image, new Rectangle(604, 327, 195, 94), 0, 0, pictureBox4.Image.Width, pictureBox4.Image.Height, GraphicsUnit.Pixel, ia);

                //e.Graphics.DrawImage(pictureBox5.Image, new Rectangle(404, 426, 195, 94), 0, 0, pictureBox5.Image.Width, pictureBox5.Image.Height, GraphicsUnit.Pixel, ia);
                //e.Graphics.DrawImage(pictureBox6.Image, new Rectangle(604, 426, 195, 94), 0, 0, pictureBox6.Image.Width, pictureBox6.Image.Height, GraphicsUnit.Pixel, ia);

                //e.Graphics.DrawImage(pictureBox7.Image, new Rectangle(404, 525, 195, 94), 0, 0, pictureBox7.Image.Width, pictureBox7.Image.Height, GraphicsUnit.Pixel, ia);
                //e.Graphics.DrawImage(pictureBox8.Image, new Rectangle(604, 525, 195, 94), 0, 0, pictureBox8.Image.Width, pictureBox8.Image.Height, GraphicsUnit.Pixel, ia);




                //e.Graphics.DrawImage(pictureBox1.Image, 404, 228, 195, 94);
                //e.Graphics.DrawImage(pictureBox2.Image, 604, 228, 195, 94);

                //e.Graphics.DrawImage(pictureBox3.Image, 404, 414, 195, 94);
                //e.Graphics.DrawImage(pictureBox4.Image, 604, 414, 195, 94);

                //e.Graphics.DrawImage(pictureBox5.Image, 404, 600, 195, 94);
                //e.Graphics.DrawImage(pictureBox6.Image, 604, 600, 195, 94);

                //e.Graphics.DrawImage(pictureBox7.Image, 404, 786, 195, 94);
                //e.Graphics.DrawImage(pictureBox8.Image, 604, 786, 195, 94);




                //e.Graphics.DrawImage(pictureBox1.Image, 509, 228, 184, 92);
                //e.Graphics.DrawImage(pictureBox2.Image, 509, 324, 184, 92);
                //e.Graphics.DrawImage(pictureBox3.Image, 509, 420, 184, 92);
                //e.Graphics.DrawImage(pictureBox4.Image, 509, 516, 184, 92);
                //e.Graphics.DrawImage(pictureBox5.Image, 509, 612, 184, 92);
                //e.Graphics.DrawImage(pictureBox6.Image, 509, 708, 184, 92);
                //e.Graphics.DrawImage(pictureBox7.Image, 509, 804, 184, 92);
                //e.Graphics.DrawImage(pictureBox8.Image, 509, 900, 184, 92);

                e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(355, 275, 209, 157), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox2.Image, new Rectangle(566, 275, 209, 157), 0, 0, pictureBox2.Image.Width, pictureBox2.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox3.Image, new Rectangle(355, 434, 209, 157), 0, 0, pictureBox3.Image.Width, pictureBox3.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox4.Image, new Rectangle(566, 434, 209, 157), 0, 0, pictureBox4.Image.Width, pictureBox4.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox5.Image, new Rectangle(355, 593, 209, 157), 0, 0, pictureBox5.Image.Width, pictureBox5.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox6.Image, new Rectangle(566, 593, 209, 157), 0, 0, pictureBox6.Image.Width, pictureBox6.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox7.Image, new Rectangle(355, 752, 209, 157), 0, 0, pictureBox7.Image.Width, pictureBox7.Image.Height, GraphicsUnit.Pixel, ia);
                e.Graphics.DrawImage(pictureBox8.Image, new Rectangle(566, 752, 209, 157), 0, 0, pictureBox8.Image.Width, pictureBox8.Image.Height, GraphicsUnit.Pixel, ia);


            }

            e.Graphics.DrawRectangle(redPen, 30, 275, 320, 378);
            e.Graphics.DrawString("HASIL", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 30, 276);
            //string combinedText = richTextBox1.Text;
            //string hasil = AddNewlinesIfTooLong(combinedText, 34);
            //e.Graphics.DrawString(hasil, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 313);

            // Ukuran area cetak
            float printWidth = 237;
            float printHeight = 343;

            // Inisialisasi ukuran font awal
            float fontSize = 9; // Ukuran font awal, bisa disesuaikan
            Font font = new Font("Montserrat", fontSize);
            string text = richTextBox1.Text;

            // Mengukur teks dengan ukuran font saat ini
            SizeF textSize = e.Graphics.MeasureString(text, font, (int)printWidth);

            // Mengecilkan ukuran font sampai teks sesuai dengan area cetak
            while (textSize.Height > printHeight && fontSize > 1)
            {
                fontSize -= 0.5f; // Kurangi ukuran font sedikit demi sedikit
                font = new Font("Montserrat", fontSize);
                textSize = e.Graphics.MeasureString(text, font, (int)printWidth);
            }

            // Cetak teks di area yang ditentukan dengan ukuran font yang sesuai
            e.Graphics.DrawString(text, font, Brushes.Black, new RectangleF(30, 295, printWidth, printHeight));

            e.Graphics.DrawRectangle(redPen, 30, 658, 320, 177);
            e.Graphics.DrawString("KESIMPULAN", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 30, 659);
            //string combinedText1 = richTextBox2.Text;
            //string kesimpulan = AddNewlinesIfTooLong(combinedText1, 34);
            //e.Graphics.DrawString(kesimpulan, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 504);

            // Ukuran area cetak
            float printWidthKesimpulan = 320;
            float printHeightKesimpulan = 142;

            // Inisialisasi ukuran font awal
            float fontSizeKesimpulan = 9; // Ukuran font awal, bisa disesuaikan
            Font fontKesimpulan = new Font("Montserrat", fontSizeKesimpulan);
            string textKesimpulan = richTextBox2.Text;

            // Mengukur teks dengan ukuran font saat ini
            SizeF textSizeKesimpulan = e.Graphics.MeasureString(textKesimpulan, fontKesimpulan, (int)printWidthKesimpulan);

            // Mengecilkan ukuran font sampai teks sesuai dengan area cetak
            while (textSizeKesimpulan.Height > printHeightKesimpulan && fontSizeKesimpulan > 1)
            {
                fontSizeKesimpulan -= 0.5f; // Kurangi ukuran font sedikit demi sedikit
                fontKesimpulan = new Font("Montserrat", fontSizeKesimpulan);
                textSizeKesimpulan = e.Graphics.MeasureString(textKesimpulan, fontKesimpulan, (int)printWidthKesimpulan);
            }

            // Cetak teks di area yang ditentukan dengan ukuran font yang sesuai
            e.Graphics.DrawString(textKesimpulan, fontKesimpulan, Brushes.Black, new RectangleF(30, 678, printWidthKesimpulan, printHeightKesimpulan));










            e.Graphics.DrawRectangle(redPen, 30, 840, 320, 177);
            e.Graphics.DrawString("SARAN", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 30, 841);
            //string combinedText2 = richTextBox3.Text;
            //string saran = AddNewlinesIfTooLong(combinedText2, 34);
            //e.Graphics.DrawString(saran, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 695);


            // Ukuran area cetak
            float printWidthSaran = 320;
            float printHeightSaran = 142;

            // Inisialisasi ukuran font awal
            float fontSizeSaran = 9; // Ukuran font awal, bisa disesuaikan
            Font fontSaran = new Font("Montserrat", fontSizeSaran);
            string textSaran = richTextBox3.Text;

            // Mengukur teks dengan ukuran font saat ini
            SizeF textSizeSaran = e.Graphics.MeasureString(textSaran, fontSaran, (int)printWidthSaran);

            // Mengecilkan ukuran font sampai teks sesuai dengan area cetak
            while (textSizeSaran.Height > printHeightSaran && fontSizeSaran > 1)
            {
                fontSizeSaran -= 0.5f; // Kurangi ukuran font sedikit demi sedikit
                fontSaran = new Font("Montserrat", fontSizeSaran, FontStyle.Regular);
                textSizeSaran = e.Graphics.MeasureString(textSaran, fontSaran, (int)printWidthSaran);
            }

            // Cetak teks di area yang ditentukan dengan ukuran font yang sesuai
            e.Graphics.DrawString(textSaran, fontSaran, Brushes.Black, new RectangleF(30, 860, printWidthSaran, printHeightSaran));




            if (comboBox3.SelectedItem.ToString() == "Gastrokopi")
            {
                e.Graphics.DrawImage(pictureBox9.Image, 569, 930, 206, 206);
            }
            else if (comboBox3.SelectedItem.ToString() == "Kolonoskopi")
            {
                e.Graphics.DrawImage(pictureBox9.Image, 460, 950, 206, 206);
            }







            e.Graphics.DrawString(labelLokTgl.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 150, 1021, SF1);
            e.Graphics.DrawString(label30.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 150, 1035, SF1);
            e.Graphics.DrawString(labelNamaDokter.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 150, 1125, SF1);
        }

        private void HistoryPrintA4(string profile)
        {
            string text = DateTime.Now.ToString("Y");
            string[] arr = text.Split(' ');
            string splitBulan = arr[0];
            string splitTahun = arr[1];
            string tanggal = DateTime.Now.ToString("ddMMyyy");


            if (isButton2Pressed == true)
            {
                string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\History Print\Format-4" + @"\2-Gambar\";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string existingPathName = dir;
                string notExistingFileName;

                if (profile == "Default")
                {
                    notExistingFileName = dir + gabung1 + ".pdf";
                }
                else if (profile == "Adjust Brightness")
                {
                    notExistingFileName = dir + gabung1 + "_Adjust_Brightness.pdf";
                }
                else
                {
                    // Handle the case where the profile is not recognized
                    MessageBox.Show("Profile tidak dikenali", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (Directory.Exists(existingPathName) && !File.Exists(notExistingFileName))
                {
                    PrintDocument pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                    pdoc.PrinterSettings.PrintFileName = notExistingFileName;
                    pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                    pdoc.DefaultPageSettings.Landscape = false;
                    //pdoc.PrintPage += pdoc_PrintPage;
                    pdoc.PrintPage += printDocument1_PrintPage;
                    pdoc.Print();
                }
            }

            if (isButton4Pressed == true)
            {
                string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\History Print\Format-4" + @"\4-Gambar\";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string existingPathName = dir;
                string notExistingFileName;

                if (profile == "Default")
                {
                    notExistingFileName = dir + gabung1 + ".pdf";
                }
                else if (profile == "Adjust Brightness")
                {
                    notExistingFileName = dir + gabung1 + "_Adjust_Brightness.pdf";
                }
                else
                {
                    // Handle the case where the profile is not recognized
                    MessageBox.Show("Profile tidak dikenali", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (Directory.Exists(existingPathName) && !File.Exists(notExistingFileName))
                {
                    PrintDocument pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                    pdoc.PrinterSettings.PrintFileName = notExistingFileName;
                    pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                    pdoc.DefaultPageSettings.Landscape = false;
                    //pdoc.PrintPage += pdoc_PrintPage;
                    pdoc.PrintPage += printDocument1_PrintPage;
                    pdoc.Print();
                }
            }

            if (isButton6Pressed == true)
            {
                string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\History Print\Format-4" + @"\6-Gambar\";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string existingPathName = dir;
                string notExistingFileName;

                if (profile == "Default")
                {
                    notExistingFileName = dir + gabung1 + ".pdf";
                }
                else if (profile == "Adjust Brightness")
                {
                    notExistingFileName = dir + gabung1 + "_Adjust_Brightness.pdf";
                }
                else
                {
                    // Handle the case where the profile is not recognized
                    MessageBox.Show("Profile tidak dikenali", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (Directory.Exists(existingPathName) && !File.Exists(notExistingFileName))
                {
                    PrintDocument pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                    pdoc.PrinterSettings.PrintFileName = notExistingFileName;
                    pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                    pdoc.DefaultPageSettings.Landscape = false;
                    //pdoc.PrintPage += pdoc_PrintPage;
                    pdoc.PrintPage += printDocument1_PrintPage;
                    pdoc.Print();
                }
            }

            if (isButton8Pressed == true)
            {
                string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\History Print\Format-4" + @"\8-Gambar\";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string existingPathName = dir;
                string notExistingFileName;

                if (profile == "Default")
                {
                    notExistingFileName = dir + gabung1 + ".pdf";
                }
                else if (profile == "Adjust Brightness")
                {
                    notExistingFileName = dir + gabung1 + "_Adjust_Brightness.pdf";
                }
                else
                {
                    // Handle the case where the profile is not recognized
                    MessageBox.Show("Profile tidak dikenali", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (Directory.Exists(existingPathName) && !File.Exists(notExistingFileName))
                {
                    PrintDocument pdoc = new PrintDocument();
                    pdoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                    pdoc.PrinterSettings.PrintFileName = notExistingFileName;
                    pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                    pdoc.DefaultPageSettings.Landscape = false;
                    //pdoc.PrintPage += pdoc_PrintPage;
                    pdoc.PrintPage += printDocument1_PrintPage;
                    pdoc.Print();
                }
            }


        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (logoValue == "1")
            {
                //AdjustPictureBoxSize(e.Graphics, "Persegi");
                //e.Graphics.DrawImage(picLogo1.Image, 30, 3, picLogo1.Width, picLogo1.Height);

                if (jenisValue == "Persegi Panjang")
                {
                    // Handle case when logoValue is "1" and jenisValue is "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, jenisValue);
                    e.Graphics.DrawImage(picLogo2.Image, 30, 10, picLogo2.Width, picLogo2.Height);
                }
                else
                {
                    // Handle case when logoValue is "1" and jenisValue is not "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, "Persegi");
                    e.Graphics.DrawImage(picLogo1.Image, 30, 10, picLogo1.Width, picLogo1.Height);
                }
            }
            else if (logoValue == "2")
            {
                AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
                e.Graphics.DrawImage(picLogo1.Image, 30, 10, picLogo1.Width, picLogo1.Height);

                // Adjust the coordinates based on jenisValue
                if (jenisValue == "Persegi")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 675, 10, picLogo2.Width, picLogo2.Height);
                }
                else if (jenisValue == "Persegi Panjang")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 625, 10, picLogo2.Width, picLogo2.Height);
                }
            }

            StringFormat SF1 = new StringFormat();
            SF1.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(richTextBoxNRS.Text, new Font("Montserrat", 16, FontStyle.Bold), Brushes.Black, 400, 18, SF1);
            e.Graphics.DrawString(richTextBoxBE.Text, new Font("Montserrat", 14, FontStyle.Bold), Brushes.Black, 400, 45, SF1);
            e.Graphics.DrawString(richTextBoxJalan.Text, new Font("Montserrat", 8, FontStyle.Bold), Brushes.Black, 400, 70, SF1);
            e.Graphics.DrawString(richTextBoxEmail.Text, new Font("Montserrat", 8, FontStyle.Bold), Brushes.Black, 400, 85, SF1);

            Color red = Color.Black;
            Pen redPen = new Pen(red);
            redPen.Width = 1;
            e.Graphics.DrawRectangle(redPen, 30, 125, 100, 21);
            e.Graphics.DrawString(textBox1.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 127);
            e.Graphics.DrawRectangle(redPen, 135, 125, 256, 21);
            e.Graphics.DrawString(textBox8.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 137, 127);

            e.Graphics.DrawRectangle(redPen, 401, 125, 100, 21);
            e.Graphics.DrawString(textBox5.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 401, 127);
            e.Graphics.DrawRectangle(redPen, 506, 125, 269, 21);
            e.Graphics.DrawString(textBox11.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 509, 127);

            e.Graphics.DrawRectangle(redPen, 30, 150, 100, 21);
            e.Graphics.DrawString(textBox3.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 152);
            e.Graphics.DrawRectangle(redPen, 135, 150, 256, 21);
            e.Graphics.DrawString(textBox9.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 137, 152);

            e.Graphics.DrawRectangle(redPen, 401, 150, 100, 21);
            e.Graphics.DrawString(textBox6.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 401, 152);
            e.Graphics.DrawRectangle(redPen, 506, 150, 269, 21);
            e.Graphics.DrawString(textBox12.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 508, 152);

            e.Graphics.DrawRectangle(redPen, 30, 175, 100, 21);
            e.Graphics.DrawString(textBox4.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 177);
            e.Graphics.DrawRectangle(redPen, 135, 175, 175, 21);
            e.Graphics.DrawString(textBox10.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 137, 177);
            e.Graphics.DrawRectangle(redPen, 313, 175, 78, 21);
            e.Graphics.DrawString(textBox16.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 315, 177);

            e.Graphics.DrawRectangle(redPen, 401, 175, 100, 21);
            e.Graphics.DrawString(textBox7.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 401, 177);
            e.Graphics.DrawRectangle(redPen, 506, 175, 269, 21);
            e.Graphics.DrawString(textBox13.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 508, 177);

            StringFormat SF2 = new StringFormat();
            SF2.Alignment = StringAlignment.Near;
            e.Graphics.DrawRectangle(redPen, 30, 200, 362, 21);
            e.Graphics.DrawString(textBox14.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 270, 200, SF2);
            e.Graphics.DrawRectangle(redPen, 401, 200, 374, 21);
            e.Graphics.DrawString(richTextBox4.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 403, 202);


            e.Graphics.DrawRectangle(redPen, 30, 225, 100, 46);
            e.Graphics.DrawString(" Obat \r\n Premedikasi", new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 227);

            //e.Graphics.DrawRectangle(redPen, 135, 225, 215, 21);
            e.Graphics.DrawRectangle(redPen, 135, 225, 640, 21);
            e.Graphics.DrawString(textBox18.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 137, 227);
            //e.Graphics.DrawRectangle(redPen, 135, 250, 627, 21);
            e.Graphics.DrawRectangle(redPen, 135, 250, 640, 21);
            e.Graphics.DrawString(textBox19.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 137, 252);

            //e.Graphics.DrawImage(pictureBox1.Image, 355, 224, 209, 157);
            //e.Graphics.DrawImage(pictureBox2.Image, 566, 224, 209, 157);
            //e.Graphics.DrawImage(pictureBox3.Image, 355, 383, 209, 157);
            //e.Graphics.DrawImage(pictureBox4.Image, 566, 383, 209, 157);
            //e.Graphics.DrawImage(pictureBox5.Image, 355, 542, 209, 157);
            //e.Graphics.DrawImage(pictureBox6.Image, 566, 542, 209, 157);
            //e.Graphics.DrawImage(pictureBox7.Image, 355, 701, 209, 157);
            //e.Graphics.DrawImage(pictureBox8.Image, 566, 701, 209, 157);


            // Memeriksa apakah tombol 2 ditekan
            if (isButton2Pressed == true)
            { 
                //MessageBox.Show("Tombol 2 ditekan!");
                e.Graphics.DrawImage(pictureBox1.Image, 355, 275, 420, 317);
                e.Graphics.DrawImage(pictureBox2.Image, 355, 594, 420, 317); 
               
            }

            // Memeriksa apakah tombol 4 ditekan
            if (isButton4Pressed == true)
            {
                //MessageBox.Show("Tombol 4 ditekan!");
                //e.Graphics.DrawImage(pictureBox1.Image, 419, 228, 364, 186);
                //e.Graphics.DrawImage(pictureBox2.Image, 419, 419, 364, 186);
                //e.Graphics.DrawImage(pictureBox3.Image, 419, 610, 364, 186);
                //e.Graphics.DrawImage(pictureBox4.Image, 419, 801, 364, 186);
                //isButton4Pressed = false; // Reset setelah digunakan


                e.Graphics.DrawImage(pictureBox1.Image, 460, 275, 209, 157);
                e.Graphics.DrawImage(pictureBox2.Image, 460, 434, 209, 157);
                e.Graphics.DrawImage(pictureBox3.Image, 460, 593, 209, 157);
                e.Graphics.DrawImage(pictureBox4.Image, 460, 752, 209, 157);


                //e.Graphics.DrawImage(pictureBox1.Image, 355, 275, 209, 157);
                //e.Graphics.DrawImage(pictureBox2.Image, 566, 275, 209, 157);
                //e.Graphics.DrawImage(pictureBox3.Image, 355, 434, 209, 157);
                //e.Graphics.DrawImage(pictureBox4.Image, 566, 434, 209, 157); 



            }

            if (isButton6Pressed)
            {
                //e.Graphics.DrawImage(pictureBox1.Image, 485, 275, 146, 110);
                //e.Graphics.DrawImage(pictureBox2.Image, 485, 387, 146, 110);
                //e.Graphics.DrawImage(pictureBox3.Image, 485, 499, 146, 110);
                //e.Graphics.DrawImage(pictureBox4.Image, 485, 611, 146, 110);
                //e.Graphics.DrawImage(pictureBox5.Image, 485, 723, 146, 110);
                //e.Graphics.DrawImage(pictureBox6.Image, 485, 835, 146, 110);

                //e.Graphics.DrawImage(pictureBox6.Image, 401, 888, 400, 105);
                //e.Graphics.DrawImage(pictureBox6.Image, 604, 558, 198, 430);


                e.Graphics.DrawImage(pictureBox1.Image, 355, 275, 209, 157);
                e.Graphics.DrawImage(pictureBox2.Image, 566, 275, 209, 157);
                e.Graphics.DrawImage(pictureBox3.Image, 355, 434, 209, 157);
                e.Graphics.DrawImage(pictureBox4.Image, 566, 434, 209, 157);
                e.Graphics.DrawImage(pictureBox5.Image, 355, 593, 209, 157);
                e.Graphics.DrawImage(pictureBox6.Image, 566, 593, 209, 157);


            }

            if (isButton8Pressed)
            {

                //e.Graphics.DrawImage(pictureBox1.Image, 404, 228, 195, 94);
                //e.Graphics.DrawImage(pictureBox2.Image, 604, 228, 195, 94);

                //e.Graphics.DrawImage(pictureBox3.Image, 404, 327, 195, 94);
                //e.Graphics.DrawImage(pictureBox4.Image, 604, 327, 195, 94);

                //e.Graphics.DrawImage(pictureBox5.Image, 404, 426, 195, 94);
                //e.Graphics.DrawImage(pictureBox6.Image, 604, 426, 195, 94);

                //e.Graphics.DrawImage(pictureBox7.Image, 404, 525, 195, 94);
                //e.Graphics.DrawImage(pictureBox8.Image, 604, 525, 195, 94);




                //e.Graphics.DrawImage(pictureBox1.Image, 404, 228, 195, 94);
                //e.Graphics.DrawImage(pictureBox2.Image, 604, 228, 195, 94);

                //e.Graphics.DrawImage(pictureBox3.Image, 404, 414, 195, 94);
                //e.Graphics.DrawImage(pictureBox4.Image, 604, 414, 195, 94);

                //e.Graphics.DrawImage(pictureBox5.Image, 404, 600, 195, 94);
                //e.Graphics.DrawImage(pictureBox6.Image, 604, 600, 195, 94);

                //e.Graphics.DrawImage(pictureBox7.Image, 404, 786, 195, 94);
                //e.Graphics.DrawImage(pictureBox8.Image, 604, 786, 195, 94);




                //e.Graphics.DrawImage(pictureBox1.Image, 509, 228, 184, 92);
                //e.Graphics.DrawImage(pictureBox2.Image, 509, 324, 184, 92);
                //e.Graphics.DrawImage(pictureBox3.Image, 509, 420, 184, 92);
                //e.Graphics.DrawImage(pictureBox4.Image, 509, 516, 184, 92);
                //e.Graphics.DrawImage(pictureBox5.Image, 509, 612, 184, 92);
                //e.Graphics.DrawImage(pictureBox6.Image, 509, 708, 184, 92);
                //e.Graphics.DrawImage(pictureBox7.Image, 509, 804, 184, 92);
                //e.Graphics.DrawImage(pictureBox8.Image, 509, 900, 184, 92);

                //e.Graphics.DrawImage(pictureBox1.Image, 270, 275, 252, 135);
                //e.Graphics.DrawImage(pictureBox2.Image, 524, 275, 252, 135);
                //e.Graphics.DrawImage(pictureBox3.Image, 270, 412, 252, 135);
                //e.Graphics.DrawImage(pictureBox4.Image, 524, 412, 252, 135);
                //e.Graphics.DrawImage(pictureBox5.Image, 270, 549, 252, 135);
                //e.Graphics.DrawImage(pictureBox6.Image, 524, 549, 252, 135);
                //e.Graphics.DrawImage(pictureBox7.Image, 270, 686, 252, 135);
                //e.Graphics.DrawImage(pictureBox8.Image, 524, 686, 252, 135);

                e.Graphics.DrawImage(pictureBox1.Image, 355, 275, 209, 157);
                e.Graphics.DrawImage(pictureBox2.Image, 566, 275, 209, 157);
                e.Graphics.DrawImage(pictureBox3.Image, 355, 434, 209, 157);
                e.Graphics.DrawImage(pictureBox4.Image, 566, 434, 209, 157);
                e.Graphics.DrawImage(pictureBox5.Image, 355, 593, 209, 157);
                e.Graphics.DrawImage(pictureBox6.Image, 566, 593, 209, 157);
                e.Graphics.DrawImage(pictureBox7.Image, 355, 752, 209, 157);
                e.Graphics.DrawImage(pictureBox8.Image, 566, 752, 209, 157);



            }








            //e.Graphics.DrawImage(pictureBox9.Image, 355, 860, 209, 157);
            //e.Graphics.DrawImage(pictureBox10.Image, 566, 860, 209, 157);

            e.Graphics.DrawRectangle(redPen, 30, 275, 320, 378);
            e.Graphics.DrawString("HASIL", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 30, 276);
            //string combinedText = richTextBox1.Text;
            //string hasil = AddNewlinesIfTooLong(combinedText, 34);
            //e.Graphics.DrawString(hasil, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 313);

            // Ukuran area cetak
            float printWidth = 320;
            float printHeight = 373;

            // Inisialisasi ukuran font awal
            float fontSize = 9; // Ukuran font awal, bisa disesuaikan
            Font font = new Font("Montserrat", fontSize);
            string text = richTextBox1.Text;

            // Mengukur teks dengan ukuran font saat ini
            SizeF textSize = e.Graphics.MeasureString(text, font, (int)printWidth);

            // Mengecilkan ukuran font sampai teks sesuai dengan area cetak
            while (textSize.Height > printHeight && fontSize > 1)
            {
                fontSize -= 0.5f; // Kurangi ukuran font sedikit demi sedikit
                font = new Font("Montserrat", fontSize);
                textSize = e.Graphics.MeasureString(text, font, (int)printWidth);
            }

            // Cetak teks di area yang ditentukan dengan ukuran font yang sesuai
            e.Graphics.DrawString(text, font, Brushes.Black, new RectangleF(30, 295, printWidth, printHeight));

            e.Graphics.DrawRectangle(redPen, 30, 658, 320, 177);
            e.Graphics.DrawString("KESIMPULAN", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 30, 659);
            //string combinedText1 = richTextBox2.Text;
            //string kesimpulan = AddNewlinesIfTooLong(combinedText1, 34);
            //e.Graphics.DrawString(kesimpulan, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 504);

            // Ukuran area cetak
            float printWidthKesimpulan = 320;
            float printHeightKesimpulan = 177;

            // Inisialisasi ukuran font awal
            float fontSizeKesimpulan = 9; // Ukuran font awal, bisa disesuaikan
            Font fontKesimpulan = new Font("Montserrat", fontSizeKesimpulan);
            string textKesimpulan = richTextBox2.Text;

            // Mengukur teks dengan ukuran font saat ini
            SizeF textSizeKesimpulan = e.Graphics.MeasureString(textKesimpulan, fontKesimpulan, (int)printWidthKesimpulan);

            // Mengecilkan ukuran font sampai teks sesuai dengan area cetak
            while (textSizeKesimpulan.Height > printHeightKesimpulan && fontSizeKesimpulan > 1)
            {
                fontSizeKesimpulan -= 0.5f; // Kurangi ukuran font sedikit demi sedikit
                fontKesimpulan = new Font("Montserrat", fontSizeKesimpulan);
                textSizeKesimpulan = e.Graphics.MeasureString(textKesimpulan, fontKesimpulan, (int)printWidthKesimpulan);
            }

            // Cetak teks di area yang ditentukan dengan ukuran font yang sesuai
            e.Graphics.DrawString(textKesimpulan, fontKesimpulan, Brushes.Black, new RectangleF(30, 678, printWidthKesimpulan, printHeightKesimpulan));










            e.Graphics.DrawRectangle(redPen, 30, 840, 320, 177);
            e.Graphics.DrawString("SARAN", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 30, 841);
            //string combinedText2 = richTextBox3.Text;
            //string saran = AddNewlinesIfTooLong(combinedText2, 34);
            //e.Graphics.DrawString(saran, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 30, 695);


            // Ukuran area cetak
            float printWidthSaran = 320;
            float printHeightSaran = 177;

            // Inisialisasi ukuran font awal
            float fontSizeSaran = 9; // Ukuran font awal, bisa disesuaikan
            Font fontSaran = new Font("Montserrat", fontSizeSaran);
            string textSaran = richTextBox3.Text;

            // Mengukur teks dengan ukuran font saat ini
            SizeF textSizeSaran = e.Graphics.MeasureString(textSaran, fontSaran, (int)printWidthSaran);

            // Mengecilkan ukuran font sampai teks sesuai dengan area cetak
            while (textSizeSaran.Height > printHeightSaran && fontSizeSaran > 1)
            {
                fontSizeSaran -= 0.5f; // Kurangi ukuran font sedikit demi sedikit
                fontSaran = new Font("Montserrat", fontSizeSaran, FontStyle.Regular);
                textSizeSaran = e.Graphics.MeasureString(textSaran, fontSaran, (int)printWidthSaran);
            }

            // Cetak teks di area yang ditentukan dengan ukuran font yang sesuai
            e.Graphics.DrawString(textSaran, fontSaran, Brushes.Black, new RectangleF(30, 860, printWidthSaran, printHeightSaran));




            if (comboBox3.SelectedItem.ToString() == "Gastrokopi")
            {
                e.Graphics.DrawImage(pictureBox9.Image, 569, 930, 206, 206);
            }
            else if (comboBox3.SelectedItem.ToString() == "Kolonoskopi")
            {
                e.Graphics.DrawImage(pictureBox9.Image, 460, 950, 206, 206);
            }







            e.Graphics.DrawString(labelLokTgl.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 150, 1021, SF1);
            e.Graphics.DrawString(label30.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 150, 1035, SF1);
            e.Graphics.DrawString(labelNamaDokter.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 150, 1125, SF1);
        }

        private void close1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            //close1.Visible = false;
        }




        private void ClearImages()
        {
            // Menghapus gambar dari PictureBox1 hingga PictureBox8
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null;
            pictureBox5.Image = null;
            pictureBox6.Image = null;
            pictureBox7.Image = null;
            pictureBox8.Image = null;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // Menampilkan dan mengatur lokasi serta ukuran PictureBox1
            pictureBox1.Visible = true;
            pictureBox1.Location = new Point(491, 275);
            pictureBox1.Size = new Size(339, 256);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox2
            pictureBox2.Visible = true;
            pictureBox2.Location = new Point(491, 533);
            pictureBox2.Size = new Size(339, 256);

            close1.Visible = true;
            close1.Location = new Point(798, 282);

            close2.Visible = true;
            close2.Location = new Point(798, 540);


            // Menyembunyikan PictureBox3 hingga PictureBox8
            pictureBox3.Visible = false;
            pictureBox4.Visible = false;
            pictureBox5.Visible = false;
            pictureBox6.Visible = false;
            pictureBox7.Visible = false;
            pictureBox8.Visible = false;

            close3.Visible = false;
            close4.Visible = false;
            close5.Visible = false;
            close6.Visible = false;
            close7.Visible = false;
            close8.Visible = false;

            //ShowMessageForButton(2); // Panggil metode untuk menampilkan MessageBox
            ClearImages();

            isButton2Pressed = true;
            isButton4Pressed = false;
            isButton6Pressed = false;
            isButton8Pressed = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Menampilkan dan mengatur lokasi serta ukuran PictureBox1
            pictureBox1.Visible = true;
            pictureBox1.Location = new Point(569, 276);
            pictureBox1.Size = new Size(159, 120);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox2
            pictureBox2.Visible = true;
            pictureBox2.Location = new Point(569, 401);
            pictureBox2.Size = new Size(159, 120);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox3
            pictureBox3.Visible = true;
            pictureBox3.Location = new Point(569, 526);
            pictureBox3.Size = new Size(159, 120);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox4
            pictureBox4.Visible = true;
            //pictureBox4.Location = new Point(658, 403);
            pictureBox4.Location = new Point(569, 651);
            pictureBox4.Size = new Size(159, 120);

            close1.Visible = true;
            close1.Location = new Point(697, 282);

            close2.Visible = true;
            close2.Location = new Point(697, 407);

            close3.Visible = true;
            close3.Location = new Point(697, 532);

            close4.Visible = true;
            close4.Location = new Point(697, 657);



            // Menyembunyikan PictureBox5 hingga PictureBox8
            pictureBox5.Visible = false;
            pictureBox6.Visible = false;
            pictureBox7.Visible = false;
            pictureBox8.Visible = false;

            close5.Visible = false;
            close6.Visible = false;
            close7.Visible = false;
            close8.Visible = false;

            ClearImages();

            isButton2Pressed = false;
            isButton4Pressed = true;
            isButton6Pressed = false;
            isButton8Pressed = false;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Menampilkan dan mengatur lokasi serta ukuran PictureBox1
            pictureBox1.Visible = true;
            pictureBox1.Location = new Point(478, 275);
            pictureBox1.Size = new Size(181, 130);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox2
            pictureBox2.Visible = true;
            pictureBox2.Location = new Point(660, 275);
            pictureBox2.Size = new Size(181, 130);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox3
            pictureBox3.Visible = true;
            pictureBox3.Location = new Point(478, 406);
            pictureBox3.Size = new Size(181, 130);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox4
            pictureBox4.Visible = true;
            pictureBox4.Location = new Point(660, 406);
            pictureBox4.Size = new Size(181, 130);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox5
            pictureBox5.Visible = true;
            pictureBox5.Location = new Point(478, 537);
            pictureBox5.Size = new Size(181, 130);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox6
            pictureBox6.Visible = true;
            pictureBox6.Location = new Point(660, 537);
            pictureBox6.Size = new Size(181, 130);


            close1.Visible = true;
            close1.Location = new Point(628, 281);

            close2.Visible = true;
            close2.Location = new Point(810, 281);

            close3.Visible = true;
            close3.Location = new Point(628, 412);

            close4.Visible = true;
            close4.Location = new Point(810, 412);

            close5.Visible = true;
            close5.Location = new Point(628, 543);

            close6.Visible = true;
            close6.Location = new Point(810, 543);





            // Menyembunyikan PictureBox7 dan PictureBox8
            pictureBox7.Visible = false;
            pictureBox8.Visible = false;

            close7.Visible = false;
            close8.Visible = false;

            ClearImages();

            isButton2Pressed = false;
            isButton4Pressed = false;
            isButton6Pressed = true;
            isButton8Pressed = false;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Menampilkan dan mengatur lokasi serta ukuran PictureBox1
            pictureBox1.Visible = true;
            pictureBox1.Location = new Point(478, 275);
            pictureBox1.Size = new Size(181, 130);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox2
            pictureBox2.Visible = true;
            pictureBox2.Location = new Point(660, 275);
            pictureBox2.Size = new Size(181, 130);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox3
            pictureBox3.Visible = true;
            pictureBox3.Location = new Point(478, 406);
            pictureBox3.Size = new Size(181, 130);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox4
            pictureBox4.Visible = true;
            pictureBox4.Location = new Point(660, 406);
            pictureBox4.Size = new Size(181, 130);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox5
            pictureBox5.Visible = true;
            pictureBox5.Location = new Point(478, 537);
            pictureBox5.Size = new Size(181, 130);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox6
            pictureBox6.Visible = true;
            pictureBox6.Location = new Point(660, 537);
            pictureBox6.Size = new Size(181, 130);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox7
            pictureBox7.Visible = true;
            pictureBox7.Location = new Point(478, 668);
            pictureBox7.Size = new Size(181, 130);

            // Menampilkan dan mengatur lokasi serta ukuran PictureBox8
            pictureBox8.Visible = true;
            pictureBox8.Location = new Point(660, 668);
            pictureBox8.Size = new Size(181, 130);


            close1.Visible = true;
            close1.Location = new Point(628, 281);

            close2.Visible = true;
            close2.Location = new Point(810, 281);

            close3.Visible = true;
            close3.Location = new Point(628, 412);

            close4.Visible = true;
            close4.Location = new Point(810, 412);

            close5.Visible = true;
            close5.Location = new Point(628, 543);

            close6.Visible = true;
            close6.Location = new Point(810, 543);

            close7.Visible = true;
            close7.Location = new Point(628, 674);

            close8.Visible = true;
            close8.Location = new Point(810, 674);

            ClearImages();


            isButton2Pressed = false;
            isButton4Pressed = false;
            isButton6Pressed = false;
            isButton8Pressed = true;

        }

        private void close1_Click_1(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
        }

        private void close2_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = null;
        }

        private void close3_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = null;
        }

        private void close4_Click(object sender, EventArgs e)
        {
            pictureBox4.Image = null;
        }

        private void close5_Click(object sender, EventArgs e)
        {
            pictureBox5.Image = null;
        }

        private void close6_Click(object sender, EventArgs e)
        {
            pictureBox6.Image = null;
        }

        private void close7_Click(object sender, EventArgs e)
        {
            pictureBox7.Image = null;
        }

        private void close8_Click(object sender, EventArgs e)
        {
            pictureBox8.Image = null;
        }





        // Metode untuk menampilkan MessageBox berdasarkan tombol yang ditekan
        //private void ShowMessageForButton(int buttonNumber)
        //{
        //    // Logika untuk menampilkan MessageBox sesuai dengan status tombol
        //    if (buttonNumber == 2)
        //    {
        //        if (isButton2Pressed)
        //            MessageBox.Show("Tombol 2 ditekan!"); 
        //    }
        //    else if (buttonNumber == 4)
        //    {
        //        if (isButton4Pressed)
        //            MessageBox.Show("Tombol 4 ditekan!"); 
        //    }
        //    else if (buttonNumber == 6)
        //    {
        //        if (isButton6Pressed)
        //            MessageBox.Show("Tombol 6 ditekan!"); 
        //    }
        //    else if (buttonNumber == 8)
        //    {
        //        if (isButton4Pressed)
        //            MessageBox.Show("Tombol 8 ditekan!"); 
        //    }
        //}

        public class PictureBoxControls
        {
            public Control CloseControl { get; set; }
            public Control AddControl { get; set; }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (panel1.BorderStyle == BorderStyle.FixedSingle)
            {
                int thickness = 2;//it's up to you
                int halfThickness = thickness / 2;
                using (Pen p = new Pen(Color.Black, thickness))
                {
                    e.Graphics.DrawRectangle(p, new System.Drawing.Rectangle(halfThickness,
                                                              halfThickness,
                                                              panel1.ClientSize.Width - thickness,
                                                              panel1.ClientSize.Height - thickness));
                }
            }
        }

        private void buttonExportPdf_Click(object sender, EventArgs e)
        {
            DateTime today = DateTime.Now;
            jam = today.ToString("hhmmss");

            if (comboBox3.SelectedIndex == -1 || comboBox3.SelectedItem.ToString() == "Pilih Jenis")
            {
                MessageBox.Show("Pilih Jenis terlebih dahulu", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Proses berdasarkan tombol yang ditekan
            if (isButton2Pressed)
            {
                ExportToPDF("2-Gambar");
            }
            if (isButton4Pressed)
            {
                ExportToPDF("4-Gambar");
            }
            if (isButton6Pressed)
            {
                ExportToPDF("6-Gambar");
            }
            if (isButton8Pressed)
            {
                ExportToPDF("8-Gambar");
            }
        }



        private void ExportToPDF(string folderName)
        {
            string baseDir = $@"D:\GLEndoscope\{splitTahun}\{splitBulan}\{tanggal}\{gabung}\EksporPDF\Format-4\{folderName}\";

            // Membuat direktori jika belum ada
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }

            // Tambahkan jam, menit, dan detik pada nama file
            string currentTime = DateTime.Now.ToString("HHmmss"); // Format waktu: HHmmss (24 jam)
            string filePath = baseDir + $"{gabung1}_{currentTime}.pdf";

            // Cek apakah file sudah ada
            if (File.Exists(filePath))
            {
                DialogResult result = MessageBox.Show(
                    $"File {gabung1}_{currentTime}.pdf sudah ada di folder {folderName}. Apakah Anda ingin menimpa file tersebut?",
                    "File Sudah Ada",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.No)
                {
                    return;
                }
            }

            // Proses pencetakan ke PDF
            using (PrintDocument pdoc = new PrintDocument())
            {
                try
                {
                    pdoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                    pdoc.PrinterSettings.PrintFileName = filePath;
                    pdoc.PrinterSettings.PrintToFile = true;
                    pdoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                    pdoc.DefaultPageSettings.Landscape = false;
                    pdoc.PrintPage += printDocument1_PrintPage;

                    // Cetak dokumen
                    pdoc.Print();
                    MessageBox.Show($"Export PDF ke {folderName} berhasil!\n", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Terjadi kesalahan saat mencetak PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; // Mencegah karakter yang diketik ditampilkan di ComboBox
        }


        private void ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Path gambar yang ingin Anda tampilkan
            string gastroImagePath = @"D:\GLEndoscope\LogoKOP\aset-02.png";
            string colonImagePath = @"D:\GLEndoscope\LogoKOP\aset-01.png";

            // Cek item yang dipilih
            if (comboBox3.SelectedItem.ToString() == "Gastrokopi")
            {
                pictureBox9.Image = Image.FromFile(gastroImagePath);
                pictureBox9.Location = new Point(757, 804);
            }
            else if (comboBox3.SelectedItem.ToString() == "Kolonoskopi")
            {
                pictureBox9.Image = Image.FromFile(colonImagePath);
                pictureBox9.Location = new Point(615, 804);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.icon;
            pictureBox2.Image = Properties.Resources.icon;
            pictureBox3.Image = Properties.Resources.icon;
            pictureBox4.Image = Properties.Resources.icon;
            pictureBox5.Image = Properties.Resources.icon;
            pictureBox6.Image = Properties.Resources.icon;
            pictureBox7.Image = Properties.Resources.icon;
            pictureBox8.Image = Properties.Resources.icon;

            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            pictureBox2.Image.Dispose();
            pictureBox2.Image = null;
            pictureBox3.Image.Dispose();
            pictureBox3.Image = null;
            pictureBox4.Image.Dispose();
            pictureBox4.Image = null;
            pictureBox5.Image.Dispose();
            pictureBox5.Image = null;
            pictureBox6.Image.Dispose();
            pictureBox6.Image = null;
            pictureBox7.Image.Dispose();
            pictureBox7.Image = null;
            pictureBox8.Image.Dispose();
            pictureBox8.Image = null;

            comboBox1.Items.Clear();
            comboBox1.ResetText();
            int kondisii = 21;
            TEClose48Gambar(kondisii.ToString());
            this.Close();
            //buttobDeleteFalse();
        }

        private void Form48Gambar_Load(object sender, EventArgs e)
        {

            this.ActiveControl = label1;
            string dirlogo1 = dirLogo + "logo1.png";
            //string dirlogo1 = dir + "1160358.png";
            if (!Directory.Exists(dirlogo1))
            {
                picLogo1.Image = Image.FromFile(dirLogo + "logo1.png");
                picLogo2.Image = Image.FromFile(dirLogo + "logo2.png");
            }

            LoadAndSetValues();



            textBox1.ReadOnly = true;
            textBox1.BackColor = System.Drawing.SystemColors.Window;

            textBox8.ReadOnly = true;
            textBox8.BackColor = System.Drawing.SystemColors.Window;

            textBox3.ReadOnly = true;
            textBox3.BackColor = System.Drawing.SystemColors.Window;

            textBox9.ReadOnly = true;
            textBox9.BackColor = System.Drawing.SystemColors.Window;

            textBox4.ReadOnly = true;
            textBox4.BackColor = System.Drawing.SystemColors.Window;

            textBox10.ReadOnly = true;
            textBox10.BackColor = System.Drawing.SystemColors.Window;

            textBox16.ReadOnly = true;
            textBox16.BackColor = System.Drawing.SystemColors.Window;

            textBox14.ReadOnly = true;
            textBox14.BackColor = System.Drawing.SystemColors.Window;

            textBox17.ReadOnly = true;
            textBox17.BackColor = System.Drawing.SystemColors.Window;

            textBox5.ReadOnly = true;
            textBox5.BackColor = System.Drawing.SystemColors.Window;

            textBox11.ReadOnly = true;
            textBox11.BackColor = System.Drawing.SystemColors.Window;

            textBox7.ReadOnly = true;
            textBox7.BackColor = System.Drawing.SystemColors.Window;

            textBox13.ReadOnly = true;
            textBox13.BackColor = System.Drawing.SystemColors.Window;

            //richTextBox4.LoadFile(dirRtf + "RtfFile5.rtf", RichTextBoxStreamType.RichText);
            //richTextBox4.Enabled = true;

            richTextBox4.SelectionAlignment = HorizontalAlignment.Left; // Mengatur align ke kiri


            textBox6.ReadOnly = true;
            textBox6.BackColor = System.Drawing.SystemColors.Window;

            //buttobDeleteFalse();
            pictureBox9.Image = null;

            falseAll();

            button4.PerformClick();
        }

        private void falseAll()
        {
            pictureBox1.Visible = false;
            pictureBox2.Visible = false;
            pictureBox3.Visible = false;
            pictureBox4.Visible = false;
            pictureBox5.Visible = false;
            pictureBox6.Visible = false;
            pictureBox7.Visible = false;
            pictureBox8.Visible = false;

            close1.Visible = false;
            close2.Visible = false;
            close3.Visible = false;
            close4.Visible = false;
            close5.Visible = false;
            close6.Visible = false;
            close7.Visible = false;
            close8.Visible = false;
        }

        private void LoadAndSetValues()
        {
            string filePath = @"D:\GLEndoscope\LogoKOP\logo.xml";

            try
            {
                // Check if the file exists
                if (File.Exists(filePath))
                {
                    // Load the XML document from the file
                    XDocument xd = XDocument.Load(filePath);

                    // Retrieve the logo value from the XML document
                    logoValue = xd.Element("userdata")?.Element("logo")?.Value;


                    // Set the logo value to the TextBox
                    //textBoxLogo.Text = logoValue;

                    // Retrieve the jenis value from the XML document
                    jenisValue = xd.Element("userdata")?.Element("jenis")?.Value;


                    // Check the logoValue and show/hide PictureBox controls accordingly
                    if (logoValue == "1")
                    {
                        picLogo1.Visible = true;
                        picLogo2.Visible = false;
                    }
                    else if (logoValue == "2")
                    {
                        picLogo1.Visible = true;
                        picLogo2.Visible = true;
                    }
                    else
                    {
                        // Handle other cases if needed
                        picLogo1.Visible = false;
                        picLogo2.Visible = false;
                    }

                    // Check the jenisValue and adjust the size of PictureBox controls accordingly
                    if (jenisValue == "Persegi")
                    {
                        picLogo1.Size = new Size(100, 100);
                        picLogo2.Size = new Size(100, 100);

                        richTextBoxNRS.Size = new Size(644, 20);
                        richTextBoxNRS.Location = new Point(113, 12);

                        richTextBoxBE.Size = new Size(644, 20);
                        richTextBoxBE.Location = new Point(113, 34);

                        richTextBoxJalan.Size = new Size(644, 18);
                        richTextBoxJalan.Location = new Point(113, 55);

                        richTextBoxEmail.Size = new Size(644, 18);
                        richTextBoxEmail.Location = new Point(113, 71);

                        //label1.Size = new Size(538, 23);
                        //label1.Location = new Point(164, 27);

                        //label2.Size = new Size(613, 23);
                        //label2.Location = new Point(127, 50);
                    }
                    else if (jenisValue == "Persegi Panjang")
                    {
                        picLogo1.Size = new Size(150, 100);
                        picLogo2.Size = new Size(150, 100);
                        picLogo2.Location = new Point(717, 5);

                        richTextBoxNRS.Size = new Size(544, 20);
                        richTextBoxBE.Size = new Size(544, 20);
                        richTextBoxJalan.Size = new Size(544, 18);
                        richTextBoxEmail.Size = new Size(544, 18);

                        richTextBoxNRS.Location = new Point(159, 12);
                        richTextBoxBE.Location = new Point(159, 34);
                        richTextBoxJalan.Location = new Point(159, 55);
                        richTextBoxEmail.Location = new Point(159, 71);


                        //label1.Size = new Size(343, 23);
                        //label1.Location = new Point(262, 27);

                        //label2.Size = new Size(418, 23);
                        //label2.Location = new Point(225, 50);
                    }
                }
                else
                {
                    MessageBox.Show("The XML file does not exist.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

    }
}