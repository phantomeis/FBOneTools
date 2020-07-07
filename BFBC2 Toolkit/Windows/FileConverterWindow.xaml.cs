﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;
using MahApps.Metro.Controls;
using BFBC2_Toolkit.Functions;
using BFBC2_Toolkit.Data;
using BFBC2_Toolkit.Tools;

namespace BFBC2_Toolkit.Windows
{
    public partial class FileConverterWindow : MetroWindow
    {
        private int skippedFiles = 0;

        public FileConverterWindow()
        {
            InitializeComponent();
        }

        private void TxtBoxDragAndDrop_PreviewDragOver(object sender, DragEventArgs e)
        {
            //Allow drag and drop handler of the textbox to handle all file formats
            e.Handled = true;
        }

        private async void TxtBoxDragAndDrop_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                await ConvertFiles(files);
            }
        }

        private async void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "All|*.dds;*.itexture;*.ps3texture;*.xenontexture;*.terrainheightfield;*.dbx;*.xml;*.binkmemory;*.bik|Texture|*.itexture;*.ps3texture;*.xenontexture;*.dds|Heightmap|*.terrainheightfield|Text|*.dbx;*.xml|Video|*.binkmemory;*.bik";
            ofd.Title = "Open a file...";
            ofd.Multiselect = true;
         
            if (ofd.ShowDialog() == true)
            {
                await ConvertFiles(ofd.FileNames);
            }
        }

        private async Task ConvertFiles(string[] files)
        {
            int filesCountA = 1,
                filesCountB = 0;

            filesCountB = await Task.Run(() => CountFiles(files));

            foreach (string file in files)
            {
                lblMain.Content = "Converting file " + filesCountA + " of " + filesCountB + "...";

                await ConvertFile(file);

                filesCountA++;
            }

            lblMain.Content = "Done!";

            MessageBox.Show("Done! Converted: " + (filesCountB - skippedFiles) + " Skipped: " + skippedFiles + " (Not supported yet)", "Result");

            filesCountA = 1;
            skippedFiles = 0;
        }

        private async Task ConvertFile(string filePath)
        {          
            try
            {
                if (filePath.EndsWith(".dbx") || filePath.EndsWith(".xml"))
                {
                    var process = Process.Start(Dirs.scriptDBX, "\"" + filePath);
                    await Task.Run(() => process.WaitForExit());
                }
                else if (filePath.EndsWith(".itexture") || filePath.EndsWith(".ps3texture") || filePath.EndsWith(".xenontexture") || filePath.EndsWith(".dds") || filePath.EndsWith(".terrainheightfield"))
                {
                    string[] file = { filePath };

                    await Task.Run(() => TextureConverter.ConvertFile(file, false));
                }
                else if (filePath.EndsWith(".binkmemory"))
                {
                    string fileName = Path.GetFileName(filePath);

                    await Task.Run(() => FileSystem.RenameFile(filePath, fileName.Replace(".binkmemory", ".bik")));
                }
                else if (filePath.EndsWith(".bik"))
                {
                    string fileName = Path.GetFileName(filePath);

                    await Task.Run(() => FileSystem.RenameFile(filePath, fileName.Replace(".bik", ".binkmemory")));
                }
                else
                {
                    skippedFiles++;
                }
            }
            catch (Exception ex)
            {
                skippedFiles++;

                lblMain.Content = "Unable to convert file! See error.log";
                Write.ToErrorLog(ex);               
            }
        }

        private int CountFiles(string[] files)
        {
            int filesCount = 0;

            foreach (string file in files)
                filesCount++;

            return filesCount;
        }

        private void BtnInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("BFBC2 File Converter lets you convert several Frostbite 1 files.\n\nSupported File Formats:\ndbx, xml, itexture, ps3texture, xenontexture, dds, terrainheightfield,\nbinkmemory & bik", "Info (Placeholder)");
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
