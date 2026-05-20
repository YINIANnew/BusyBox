using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace BusyBox.Themes
{
    public class ThemeManager
    {
        private static ThemeManager _instance;
        private Theme _currentTheme;
        private List<Theme> _themes;
        private SoundPlayer _startPlayer;
        private SoundPlayer _endPlayer;

        public static ThemeManager Instance => _instance ?? (_instance = new ThemeManager());

        public Theme CurrentTheme => _currentTheme;

        public List<Theme> Themes => _themes;

        private ThemeManager()
        {
            _themes = new List<Theme>();
            CreateDefaultThemes();
        }

        private void CreateDefaultThemes()
        {
            _themes.Clear();
            var theme1 = new Theme("theme1", "");
            theme1.BackgroundColor = Color.FromArgb(30, 30, 30);
            theme1.TextColor = Color.White;
            _themes.Add(theme1);

            var theme2 = new Theme("theme2", "");
            theme2.BackgroundColor = Color.White;
            theme2.TextColor = Color.Black;
            _themes.Add(theme2);

            if (_currentTheme == null && _themes.Count > 0)
            {
                _currentTheme = _themes[0];
            }
        }

        public void SetTheme(string themeName)
        {
            StopAllSounds();
            LoadThemes();
            var theme = _themes.Find(t => t.Name.Equals(themeName, StringComparison.OrdinalIgnoreCase));
            if (theme != null)
            {
                _currentTheme = theme;
            }
            else if (_themes.Count > 0)
            {
                _currentTheme = _themes[0];
            }
        }

        public void LoadThemes()
        {
            CreateDefaultThemes();
        }

        public void StopAllSounds()
        {
            if (_startPlayer != null)
            {
                _startPlayer.Stop();
                _startPlayer.Dispose();
                _startPlayer = null;
            }
            if (_endPlayer != null)
            {
                _endPlayer.Stop();
                _endPlayer.Dispose();
                _endPlayer = null;
            }
        }

        public void AddTheme(Theme theme)
        {
            if (!_themes.Exists(t => t.Name.Equals(theme.Name, StringComparison.OrdinalIgnoreCase)))
            {
                _themes.Add(theme);
            }
        }

        public void RemoveTheme(string themeName)
        {
            var theme = _themes.Find(t => t.Name.Equals(themeName, StringComparison.OrdinalIgnoreCase));
            if (theme != null)
            {
                _themes.Remove(theme);
            }
        }

        public void PlayStartSound()
        {
            try
            {
                SystemSounds.Beep.Play();
            }
            catch
            {
                SystemSounds.Beep.Play();
            }
        }

        public void PlayEndSound()
        {
            try
            {
                SystemSounds.Beep.Play();
                SystemSounds.Beep.Play();
                SystemSounds.Beep.Play();
            }
            catch
            {
                SystemSounds.Beep.Play();
                SystemSounds.Beep.Play();
                SystemSounds.Beep.Play();
            }
        }
    }

    public class Theme
    {
        public string Name { get; }
        public string ThemePath { get; }
        public byte[] LandscapeBackgroundData { get; set; }
        public byte[] PortraitBackgroundData { get; set; }
        public Dictionary<int, byte[]> NumbersData { get; } = new Dictionary<int, byte[]>();
        public string StartSoundPath { get; set; }
        public string EndSoundPath { get; set; }
        public Color BackgroundColor { get; set; } = Color.FromArgb(30, 30, 30);
        public Color TextColor { get; set; } = Color.White;

        public Theme(string name, string path)
        {
            Name = name;
            ThemePath = path;
            for (int i = 0; i <= 9; i++)
            {
                NumbersData[i] = null;
            }
        }

        public Image GetBackground()
        {
            Image landscapeBg = GetLandscapeBackground();
            Image portraitBg = GetPortraitBackground();

            try
            {
                bool isLandscape = Screen.PrimaryScreen.Bounds.Width > Screen.PrimaryScreen.Bounds.Height;
                return isLandscape ? landscapeBg : portraitBg;
            }
            catch
            {
                return landscapeBg ?? portraitBg;
            }
        }

        public Image GetLandscapeBackground()
        {
            return LandscapeBackgroundData != null ? ByteArrayToImage(LandscapeBackgroundData) : null;
        }

        public Image GetPortraitBackground()
        {
            return PortraitBackgroundData != null ? ByteArrayToImage(PortraitBackgroundData) : null;
        }

        public Image GetNumberImage(int number)
        {
            if (NumbersData.TryGetValue(number, out var data) && data != null)
            {
                return ByteArrayToImage(data);
            }
            return null;
        }

        public Image GetPreviewImage()
        {
            return GetLandscapeBackground() ?? GetPortraitBackground();
        }

        private Image ByteArrayToImage(byte[] data)
        {
            try
            {
                using (var stream = new System.IO.MemoryStream(data))
                {
                    return Image.FromStream(stream);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
