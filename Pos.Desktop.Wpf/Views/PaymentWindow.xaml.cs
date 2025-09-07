using System.Windows;
using Pos.Desktop.Wpf.ViewModels;

namespace Pos.Desktop.Wpf.Views
{
    public partial class PaymentWindow : Window
    {
        public PaymentWindow(decimal totalAmount, string orderNumber, int itemCount)
        {
            InitializeComponent();
            DataContext = new PaymentViewModel(totalAmount, orderNumber, itemCount);
            
            // Focar no campo de valor recebido se for pagamento em dinheiro
            Loaded += (s, e) => 
            {
                if (DataContext is PaymentViewModel viewModel && viewModel.IsCashPayment)
                {
                    ReceivedAmountBox.Focus();
                }
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
