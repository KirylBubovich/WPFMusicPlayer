﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AchifalMusic
{
    public partial class MainWindow : Window
    {
        public bool IsPlaying;
        public bool IsFinish;
        public bool HaveContact;
        public double BarMaxRelative = 755.4;
        public List<string> songs;
        public List<string> formats = new List<string>() { "mp3" };
        public int cur;
        public MainWindow()
        {
            HaveContact = false;
            InitializeComponent();
            StartTimer();
            string initialMusic = Environment.GetCommandLineArgs()[1]/*"D:\\Music\\03_ronan_keating_breathe_myzuka.fm.mp3"*/;
            FormMusicList(initialMusic);
            StartMusic(initialMusic);
        }

        private void FormMusicList(string initialMusic)
        {
            int _ = initialMusic.LastIndexOf('\\');
            string folder = initialMusic.Substring(0, _+1);
            songs = Directory.EnumerateFiles(folder).ToList();
            songs = (from item in songs
                    where formats.Contains(item.Substring(item.LastIndexOf('.') + 1)) 
                    select item).ToList();
            cur = songs.IndexOf(initialMusic);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            HaveContact = false;
            string fileName = (string)((DataObject)e.Data).GetFileDropList()[0];
            StartMusic(fileName);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaying)
            {
                Media.Pause();
            }
            else
            {
                Media.Play();
            }
            IsPlaying ^= true;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (HaveContact && IsPlaying && !IsFinish)
            {
                IsFinish = Media.Position >= Media.NaturalDuration.TimeSpan;
                int minutes, seconds;
                if (IsFinish)
                {
                    minutes = (int)Media.NaturalDuration.TimeSpan.TotalMinutes;
                    seconds = (int)Media.NaturalDuration.TimeSpan.TotalSeconds - minutes * 60;
                }
                else
                {
                    minutes = (int)Media.Position.TotalMinutes;
                    seconds = (int)Media.Position.TotalSeconds - minutes * 60;
                }
                LeftLabel.Content = $"{minutes}:{seconds.ToString("d2")}";
                Bar.Value = IsFinish ? 100 : Media.Position / Media.NaturalDuration.TimeSpan * 100;
            }
            else
            {
                if (IsFinish)
                {
                    HaveContact = false;
                    cur = (cur + 1) % songs.Count;
                    StartMusic(songs[cur]);
                }
            }
                
        }

        private void Media_MediaOpened(object sender, RoutedEventArgs e)
        {
            int minutes = (int)Media.NaturalDuration.TimeSpan.TotalMinutes;
            int seconds = (int)Media.NaturalDuration.TimeSpan.TotalSeconds - minutes * 60;
            RightLabel.Content = $"{minutes}:{seconds.ToString("d2")}";
            HaveContact = true;
        }

        private void StartMusic(string path)
        {
            Media.Source = new Uri(path);
            Media.LoadedBehavior = MediaState.Manual;
            Media.UnloadedBehavior = MediaState.Manual;
            Media.Play();
            IsPlaying = true;
            IsFinish = false;
        }

        private void StartTimer()
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dispatcherTimer.Start();
        }

        private void Bar_MouseEnter(object sender, MouseEventArgs e)
        {
            Bar.Opacity = 1;
        }

        private void Bar_MouseLeave(object sender, MouseEventArgs e)
        {
            Bar.Opacity = 0.5;
        }

        private void Bar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double newValue = e.GetPosition(Bar).X / BarMaxRelative;
            Bar.Value = newValue * 100;
            Media.Position = Media.NaturalDuration.TimeSpan * newValue;
            IsFinish = false;
        }

        private void LeftSwap_Click(object sender, RoutedEventArgs e)
        {
            HaveContact = false;
            cur = (cur - 1) % songs.Count;
            StartMusic(songs[cur]);
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            HaveContact = false;
            cur = (cur + 1) % songs.Count;
            StartMusic(songs[cur]);
        }
    }
}
