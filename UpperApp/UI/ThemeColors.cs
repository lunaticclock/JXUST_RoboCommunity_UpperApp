using System.Drawing;

namespace UpperApp.UI
{
    internal static class ThemeColors
    {
        public static readonly Color BackgroundPrimary = Color.FromArgb(30, 30, 30);
        public static readonly Color BackgroundSecondary = Color.FromArgb(40, 40, 42);
        public static readonly Color BackgroundTertiary = Color.FromArgb(50, 50, 55);
        public static readonly Color BackgroundCard = Color.FromArgb(45, 45, 50);

        public static readonly Color Border = Color.FromArgb(70, 70, 75);
        public static readonly Color BorderActive = Color.FromArgb(88, 166, 255);

        public static readonly Color TextPrimary = Color.FromArgb(230, 237, 243);
        public static readonly Color TextSecondary = Color.FromArgb(170, 170, 175);
        public static readonly Color TextMuted = Color.FromArgb(72, 79, 88);

        public static readonly Color AccentBlue = Color.FromArgb(88, 166, 255);
        public static readonly Color AccentGreen = Color.FromArgb(63, 185, 80);
        public static readonly Color AccentOrange = Color.FromArgb(210, 153, 34);
        public static readonly Color AccentRed = Color.FromArgb(248, 81, 73);
        public static readonly Color AccentPurple = Color.FromArgb(188, 140, 255);
        public static readonly Color AccentCyan = Color.FromArgb(57, 210, 192);

        public static readonly Color GlowBlue = Color.FromArgb(22, 43, 68);
        public static readonly Color GlowGreen = Color.FromArgb(16, 46, 20);

        public static readonly Color ButtonPrimary = AccentBlue;
        public static readonly Color ButtonPrimaryHover = Color.FromArgb(121, 184, 255);
        public static readonly Color ButtonDanger = AccentRed;
        public static readonly Color ButtonGhost = Color.FromArgb(55, 55, 60);

        public static readonly Color InputBackground = BackgroundPrimary;
        public static readonly Color InputBorder = Border;
        public static readonly Color InputFocusBorder = AccentBlue;

        public static readonly Color StatusBarBg = BackgroundTertiary;

        public static readonly Color TrackBarTrack = BackgroundPrimary;
        public static readonly Color TrackBarFill = Color.FromArgb(88, 166, 255);
        public static readonly Color TrackBarThumb = AccentCyan;
    }
}
