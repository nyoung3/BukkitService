using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

namespace ConsoleClient {
    /// <summary>
    /// Interaction logic for CertificateInfo.xaml
    /// </summary>
    public partial class CertificateInfo : Window {
        internal static String certificate;

        public CertificateInfo() {
            InitializeComponent();
            rtb.AppendText(certificate.Replace("\n", ""));
        }
    }
}
