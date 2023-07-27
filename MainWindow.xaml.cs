using Microsoft.Win32;
using ServiceTool.VM;
using System.Linq;
using System.Windows;

namespace ConfigDetect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VMMain model;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = model = new VMMain();
        }

    }
}
