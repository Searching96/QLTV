using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly Brush AdminPriColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x03, 0xA9, 0xF4));
        public static readonly Brush UserPriColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x57, 0x22));
        public static readonly Brush BgColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xD3, 0xD3, 0xD3));
    }
}