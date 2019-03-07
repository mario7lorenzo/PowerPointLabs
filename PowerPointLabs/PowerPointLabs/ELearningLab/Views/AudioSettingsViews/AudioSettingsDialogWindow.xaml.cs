﻿using System.ComponentModel;

using PowerPointLabs.ELearningLab.AudioGenerator;

namespace PowerPointLabs.ELearningLab.Views
{
    /// <summary>
    /// Interaction logic for NarrationsLabSettingsDialogBox.xaml
    /// </summary>
    public partial class AudioSettingsDialogWindow : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        public AudioSettingsPage CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentPage"));
            }
        }

        private static AudioSettingsDialogWindow instance;
        private AudioSettingsPage _currentPage { get; set; } = AudioSettingsPage.MainSettingsPage;

        public void SetCurrentPage(AudioSettingsPage page)
        {
            CurrentPage = page;
        }

        public static AudioSettingsDialogWindow GetInstance(AudioSettingsPage page = AudioSettingsPage.MainSettingsPage)
        {
            if (instance == null)
            {                
                instance = new AudioSettingsDialogWindow();
                instance.SetCurrentPage(page);
            }
            return instance;
        }

        public void SetDialogWindowHeight(int height)
        {
            Height = height;
        }
        public void Destroy()
        {
            AzureVoiceLoginPage.GetInstance().Destroy();
            AudioMainSettingsPage.GetInstance().Destroy();
            AudioPreviewPage.GetInstance().Destroy();
            instance = null;
        }
        private AudioSettingsDialogWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}