using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace ClientSide
{
    /// <summary>
    /// Interaction logic for SelectChangeRoom.xaml
    /// </summary>
    public partial class SelectChangeRoom : Window
    {
        private ObservableCollection<string> selectionItems = new ObservableCollection<string>();

        public SelectChangeRoom()
        {
            InitializeComponent();

            
        }
    }
}
