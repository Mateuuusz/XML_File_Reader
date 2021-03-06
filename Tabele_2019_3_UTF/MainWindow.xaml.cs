﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.IO;
using System.Xml.Serialization;
using Tabele_2019_3_UTF.a.Klasy;
using System;
using Tabele_2019_3_UTF.SerializacjaViews;
using Tabele_2019_3_UTF.SerializacjaProcedurIFunkcji;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tabele_2019_3_UTF.Folder_z_Nazwą.Okna;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Tabele_2019_3_UTF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static FileInfo[]? XmlFiles { get; set; }
        public static string XmlTarget;

        public MainWindow()
        {
            InitializeComponent();

        }

        private void FilePathButton_Click(object sender, RoutedEventArgs e) // Przycisk do zaznaczenia folderu z plikami xml
        {
            var dlg = new CommonOpenFileDialog
            {
                ShowHiddenItems = true,
                AllowNonFileSystemItems = true,
                IsFolderPicker = true,
                AllowPropertyEditing = true,
                AddToMostRecentlyUsedList = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                filePathBox.Content = dlg.FileName;
                CheckIfFileExist(dlg.FileName);
            }



        }

        public void CheckIfFileExist(string SelectedPath)           //Funkcja sprawdza czy w zaznaczony folderze są jakieś pliki xml
        {
            DirectoryInfo di = new DirectoryInfo(SelectedPath);
            var XMLFiles = di.GetFiles("*.xml");
            XmlFiles = XMLFiles;
            if (XMLFiles.Length == 0)
            {
                MessageBox.Show("Brak plików XML w tym folderze");
                MiejsceZapisuNampis.Visibility = Visibility.Hidden;
                PrzyciskMiejscaZapisu.Visibility = Visibility.Hidden;
                SciezkaZapisu.Visibility = Visibility.Hidden;
            }
            else
            {
                MiejsceZapisuNampis.Visibility = Visibility.Visible;
                PrzyciskMiejscaZapisu.Visibility = Visibility.Visible;
                SciezkaZapisu.Visibility = Visibility.Visible;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)   //Przycisk do zaznaczenia pustego folder 
        {                                                             //gdzie będą przechowywane serializowane pliki              
            var dlg = new CommonOpenFileDialog
            {
                ShowHiddenItems = true,
                AllowNonFileSystemItems = true,
                IsFolderPicker = true,
                AllowPropertyEditing = true,
                AddToMostRecentlyUsedList = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SciezkaZapisu.Content = dlg.FileName;
                CheckIfFolderEmpty(dlg.FileName);
            }

        }

        public void CheckIfFolderEmpty(string FileName)         
        {
            DirectoryInfo di = new DirectoryInfo(FileName);
            var XMLFiles = di.GetFiles();
            var Folders = di.GetDirectories();            
            if (XMLFiles.Length == 0 && Folders.Length == 0)
            {
                NextWindow.Visibility = Visibility.Visible;
                XmlTarget = FileName;
            }
            else
            {
                MessageBox.Show("Zaznacz Pusty Folder");
                NextWindow.Visibility = Visibility.Hidden;
            }
        }

        private void Button_NextWindow(object sender, RoutedEventArgs e)
        {
            OptionsWindow optionsWindow = new OptionsWindow();
            optionsWindow.Show();
            this.Hide();

            SerializacjaTabeli serializacjaTabeli = new SerializacjaTabeli(XmlFiles);
            foreach (var list in serializacjaTabeli.ErrorLog)
            {
                MessageBox.Show(list, "Błąd w pliku");                                       // Komunikat o ewentualnym błędzie w pliku //
            }
            Thread t1 = new Thread(() => SaveToJSONTabele(serializacjaTabeli.xmlZawarotsc, XmlTarget));
            t1.Start();

            SerializacjaWidokow serializacjaWidokow = new SerializacjaWidokow(XmlFiles);
            foreach (var list in serializacjaWidokow.ErrorLog)
            {
                MessageBox.Show(list, "Błąd w pliku");
            }
            Thread t2 = new Thread(() => SaveToJSONWidoki(serializacjaWidokow.xmlZawarotscViewdef, XmlTarget));
            t2.Start();

            SerializacjaProcedurFunkcji serializacjaProcedurFunkcji = new SerializacjaProcedurFunkcji(XmlFiles);
            foreach (var list in serializacjaProcedurFunkcji.ErrorLog)
            {
                MessageBox.Show(list, "Błąd w pliku");
            }
            Thread t3 = new Thread(() => SaveToJSONScriptdef(serializacjaWidokow.xmlZawarotscScriptdef, XmlTarget));
            t3.Start();
            Thread t4 = new Thread(() => SaveToJSONProcedurFunkcji(serializacjaProcedurFunkcji.xmlZawarotscProcedurFunkcji, XmlTarget));
            t4.Start();

            while (true)
            {
                if (t1.IsAlive == false && t2.IsAlive == false && t3.IsAlive == false && t4.IsAlive == false)
                {
                    optionsWindow.IsDone = true;
                    optionsWindow.lad.Content = "Zrobione";
                    break;
                }
            }       
        }

                private void SaveToJSONProcedurFunkcji(List<SerializacjaProcedurIFunkcji.ProceduryIFunkcji> ProceduryIFunkcje, string FolderName)
                {
                    int i = 0;
                    string filePath = FolderName + @"\ProceduryIFunkcje";
                    // If directory does not exist, create it
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    foreach (var ProFunk in ProceduryIFunkcje)
                    {
                        filePath = FolderName + @$"\ProceduryIFunkcje\ProceduraIFunkcja{i}.json";
                        JsonSerializer jsonSerializer = new JsonSerializer();
                        if (File.Exists(filePath)) File.Delete(filePath);
                        File.WriteAllText(filePath, JsonConvert.SerializeObject(ProFunk, Formatting.Indented));
                        i++;
                    }
                }

                private void SaveToJSONScriptdef(List<SerializacjaViews.Scriptdef> Scriptdef, string FolderName)
                {
                    int i = 0;
                    string filePath = FolderName + @"\Scriptdefy";
                    // If directory does not exist, create it
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

            
                        foreach (var Scrd in Scriptdef)
                        {
                            filePath = FolderName + @$"\Scriptdefy\Scripdef{i}.json";
                        JsonSerializer jsonSerializer = new JsonSerializer();
                        if (File.Exists(filePath)) File.Delete(filePath);
                        File.WriteAllText(filePath, JsonConvert.SerializeObject(Scrd, Formatting.Indented));

                        i++;
                    }
                }

                private void SaveToJSONWidoki(List<SerializacjaViews.Viewdef> Widoki, string FolderName)
                {
                    int i = 0;
                    string filePath = FolderName + @"\Widoki";
                    // If directory does not exist, create it
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }


                    foreach (var Wid in Widoki)
                    {
                        filePath = FolderName + @$"\Widoki\Widok{i}.json";
                        JsonSerializer jsonSerializer = new JsonSerializer();
                        if (File.Exists(filePath)) File.Delete(filePath);
                        File.WriteAllText(filePath, JsonConvert.SerializeObject(Wid, Formatting.Indented));

                        i++;
                    }
                }

                public static void SaveToJSONTabele(List<Tabele_2019_3_UTF.a.Klasy.Table> Tabele, string FolderName)
                {
                    int i = 0;
                    string filePath = FolderName + @"\Tabele";
                    // If directory does not exist, create it
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    foreach (var Tab in Tabele)
                    {
                        filePath = FolderName + @$"\Tabele\Tabela{i}.json";
                        JsonSerializer jsonSerializer = new JsonSerializer();
                        if (File.Exists(filePath)) File.Delete(filePath);
                        File.WriteAllText(filePath, JsonConvert.SerializeObject(Tab, Formatting.Indented));

                        i++;
                    }
            
                }
    }
}
