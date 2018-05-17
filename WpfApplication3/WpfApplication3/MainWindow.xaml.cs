using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApplication3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string sciezkaOtwartegoPliku = null;
        private string tekstBezEdycji;

        public string SciezkaOtwartegoPliku
        {
            get
            {
                if (sciezkaOtwartegoPliku != null)
                {
                    return sciezkaOtwartegoPliku;
                }
                else
                {
                    return "Bez tytułu";
                }
            }
            set
            {
                sciezkaOtwartegoPliku = value;
                if (PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("SciezkaOtwartegoPliku"));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            txtEdit.Focus();
            List<int> pom = new List<int>();
            for(int i=8; i<=12; i++)
            {
                pom.Add(i);
            }
            for(int i=14; i<28; i+=2)
            {
                pom.Add(i);
            }
            pom.Add(36);
            pom.Add(48);
            pom.Add(72);
            cbSize.ItemsSource = pom;
            cbSize.Text = "12";
            String s = cbFonts.FontFamily.ToString();
            cbFonts.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            cbFonts.Text = s;
            skorki.ItemsSource = new List<string>() { "Domyślna", "Skórka 1", "Skórka 2", "Skórka 3", "Skórka 4"};
            skorki.SelectedIndex = 0;
            aktualnyPlik.DataContext = this;
        }

        private void txtEdit_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int row = txtEdit.GetLineIndexFromCharacterIndex(txtEdit.CaretIndex);
            int col = txtEdit.CaretIndex - txtEdit.GetCharacterIndexFromLineIndex(row);
            txtInfo.Text = "Wiersz: " + (row + 1) + " Kolumna: " + (col + 1);
        }

        private void cbFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtEdit.FontFamily = (FontFamily)(sender as ComboBox).SelectedItem;
        }

        private void zapisz()
        {
            if (sciezkaOtwartegoPliku == null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Pliki tekstowe|*.txt|Wszystkie pliki|*.*";
                sfd.ShowDialog();
                if (!sfd.FileName.Equals(String.Empty))
                {
                    using (StreamWriter sw = File.CreateText(sfd.FileName))
                    {
                        foreach (string s in podzielWgLinijek(txtEdit.Text))
                        {
                            sw.WriteLine(s);
                        }
                    }
                }
                SciezkaOtwartegoPliku = sfd.FileName;
                if (sfd.FileName.Equals(String.Empty))
                {
                    sciezkaOtwartegoPliku = null;
                    return;
                }
                tekstBezEdycji = txtEdit.Text;
            }
            else
            {
                File.WriteAllText(sciezkaOtwartegoPliku, String.Empty);
                using (StreamWriter sw = File.CreateText(SciezkaOtwartegoPliku))
                {
                    foreach (string s in podzielWgLinijek(txtEdit.Text))
                    {
                        sw.WriteLine(s);
                    }
                }
                tekstBezEdycji = txtEdit.Text;
            }
        }

        private string[] podzielWgLinijek(string napis)
        {
            string[] pom = napis.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            return pom;
        }

        private void otworz_Click(object sender, RoutedEventArgs e)
        {
            if (!txtEdit.Text.Equals(String.Empty) && !txtEdit.Text.Equals(tekstBezEdycji))
            {
                switch (MessageBox.Show("Czy chcesz zapisać zmiany do pliku "+ (SciezkaOtwartegoPliku == null ? "Bez tytułu" : 
                    System.IO.Path.GetFileName(SciezkaOtwartegoPliku)) + "?", "Notatnik", MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk))
                {
                    case MessageBoxResult.Yes:
                        zapisz();
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Pliki tekstowe|*.txt|Wszystkie pliki|*.*";
            ofd.ShowDialog();
            if (!ofd.FileName.Equals(String.Empty))
            {
                SciezkaOtwartegoPliku = ofd.FileName;
                using (StreamReader plik = new StreamReader(ofd.FileName))
                {
                    txtEdit.Text = String.Empty;
                    string pom;
                    string wynik = String.Empty;
                    while ((pom = plik.ReadLine()) != null)
                    {
                        if (!wynik.Equals(String.Empty))
                        {
                            wynik = wynik + "\n" + pom;
                        }
                        else
                        {
                            wynik = pom;
                        }
                    }
                txtEdit.Text = wynik;
                tekstBezEdycji = txtEdit.Text;
                }
            }
        }

        private ResourceDictionary rd = null;

        private void skorki_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string nazwaSkorki = null;
            switch ((sender as ComboBox).SelectedItem as string)
            {
                case "Domyślna":
                    App.Current.Resources.MergedDictionaries.Remove(rd);
                    rd = null;
                    return;
                case "Skórka 1":
                    nazwaSkorki = "Skorka1.xaml";
                    break;
                case "Skórka 2":
                    nazwaSkorki = "Skorka2.xaml";
                    break;
                case "Skórka 3":
                    nazwaSkorki = "Skorka3.xaml";
                    break;
                case "Skórka 4":
                    nazwaSkorki = "Skorka4.xaml";
                    break;
                case default(string):
                    return;
            }
            Collection<ResourceDictionary> appR;
            appR = App.Current.Resources.MergedDictionaries;
            if (rd != null)
            {
                appR.Remove(rd);
            }
            rd = (ResourceDictionary)App.LoadComponent(new Uri(nazwaSkorki, UriKind.Relative));
            appR.Add(rd);
        }

        private void b_Click(object sender, RoutedEventArgs e)
        {
            if (b.IsChecked == true)
            {
                txtEdit.FontWeight = FontWeights.Bold;
            }
            else
            {
                txtEdit.FontWeight = FontWeights.Normal;
            }
        }

        private void zapisz_Click(object sender, RoutedEventArgs e)
        {
            zapisz();
            tekstBezEdycji = txtEdit.Text;
        }

        private void iPrzycisk_Click(object sender, RoutedEventArgs e)
        {
            if (iPrzycisk.IsChecked == true)
            {
                txtEdit.FontStyle=FontStyles.Italic;
            }
            else
            {
                txtEdit.FontStyle = FontStyles.Normal;
            }
        }

        private void u_Click(object sender, RoutedEventArgs e)
        {
            if (u.IsChecked == true)
            {
                txtEdit.TextDecorations = TextDecorations.Underline;
            }
            else
            {
                txtEdit.TextDecorations = null;
            }
        }

        private void cbSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtEdit.FontSize = (int)((sender as ComboBox).SelectedItem);
        }

        private void nowy_Click(object sender, RoutedEventArgs e)
        {
            //if (!txtEdit.Text.Equals(String.Empty) && !txtEdit.Text.Equals(tekstBezEdycji))
            if ((!txtEdit.Text.Equals(String.Empty) && sciezkaOtwartegoPliku == null) || (!txtEdit.Text.Equals(tekstBezEdycji) && sciezkaOtwartegoPliku != null))
            {
                switch (MessageBox.Show("Czy chcesz zapisać zmiany do pliku " + (sciezkaOtwartegoPliku == null ? "Bez tytułu" : System.IO.Path.GetFileName(SciezkaOtwartegoPliku))
                    + "?", "Notatnik", MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk))
                {
                    case MessageBoxResult.Yes:
                        zapisz();
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }
            txtEdit.Text = String.Empty;
            SciezkaOtwartegoPliku = null;
        }

        private void zapiszJako_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Pliki tekstowe|*.txt|Wszystkie pliki|*.*";
            sfd.ShowDialog();
            if (!sfd.FileName.Equals(String.Empty))
            {
                using (StreamWriter sw = File.CreateText(sfd.FileName))
                {
                    foreach (string s in podzielWgLinijek(txtEdit.Text))
                    {
                        sw.WriteLine(s);
                    }
                }
                tekstBezEdycji = txtEdit.Text;
            }
            if (sfd.FileName.Equals(String.Empty))
            {
                return;
            }
            SciezkaOtwartegoPliku = sfd.FileName;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if ((!txtEdit.Text.Equals(String.Empty) && sciezkaOtwartegoPliku == null) || sciezkaOtwartegoPliku != null)
            {
                switch (MessageBox.Show("Czy chcesz zapisać zmiany do pliku " + (SciezkaOtwartegoPliku == null ? "Bez tytułu" : System.IO.Path.GetFileName(SciezkaOtwartegoPliku))
                    + "?", "Notatnik", MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk))
                {
                    case MessageBoxResult.Yes:
                        zapisz();
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void zLewej_Click(object sender, RoutedEventArgs e)
        {
            txtEdit.TextAlignment = TextAlignment.Left;
        }

        private void naSrodku_Click(object sender, RoutedEventArgs e)
        {
            txtEdit.TextAlignment = TextAlignment.Center;
        }

        private void zPrawej_Click(object sender, RoutedEventArgs e)
        {
            txtEdit.TextAlignment = TextAlignment.Right;
        }

        private void znajdzIZamien_Click(object sender, RoutedEventArgs e)
        {
            ZamienOkno okno = new ZamienOkno();
            if (okno.ShowDialog() == true)
            {
                txtEdit.Text = txtEdit.Text.Replace(okno.co, okno.naCo);
            }
        }
    }
}
