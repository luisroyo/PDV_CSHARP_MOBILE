using System.Text;
using Microsoft.Extensions.Logging;
using Pos.Desktop.Wpf.Models;

namespace Pos.Desktop.Wpf.Services
{
    public interface IReceiptPrinterService
    {
        Task PrintReceiptAsync(ReceiptDto receipt);
        Task PrintTestPageAsync();
        Task<bool> IsPrinterAvailableAsync();
    }

    public class ReceiptPrinterService : IReceiptPrinterService
    {
        private readonly ILogger<ReceiptPrinterService> _logger;
        private readonly string _printerName;

        public ReceiptPrinterService(ILogger<ReceiptPrinterService> logger, string printerName = "Microsoft Print to PDF")
        {
            _logger = logger;
            _printerName = printerName;
        }

        public async Task PrintReceiptAsync(ReceiptDto receipt)
        {
            try
            {
                _logger.LogInformation("Iniciando impressão do cupom {OrderNumber}", receipt.OrderNumber);

                var receiptContent = GenerateReceiptContent(receipt);
                
                // Simular impressão ESC/POS
                await SimulateEscPosPrinting(receiptContent);

                _logger.LogInformation("Cupom {OrderNumber} impresso com sucesso", receipt.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao imprimir cupom {OrderNumber}", receipt.OrderNumber);
                throw;
            }
        }

        public async Task PrintTestPageAsync()
        {
            try
            {
                _logger.LogInformation("Imprimindo página de teste");

                var testContent = GenerateTestPageContent();
                await SimulateEscPosPrinting(testContent);

                _logger.LogInformation("Página de teste impressa com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao imprimir página de teste");
                throw;
            }
        }

        public async Task<bool> IsPrinterAvailableAsync()
        {
            try
            {
                // Simular verificação de impressora
                await Task.Delay(100);
                return true; // Em produção, verificar se a impressora está disponível
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar disponibilidade da impressora");
                return false;
            }
        }

        private string GenerateReceiptContent(ReceiptDto receipt)
        {
            var sb = new StringBuilder();

            // ESC/POS Commands
            sb.AppendLine("\x1B\x40"); // Initialize printer
            sb.AppendLine("\x1B\x61\x01"); // Center alignment

            // Header
            sb.AppendLine("==========================================");
            sb.AppendLine("           PDV MULTI-VERTICAL");
            sb.AppendLine("==========================================");
            sb.AppendLine($"Pedido: {receipt.OrderNumber}");
            sb.AppendLine($"Data: {receipt.DateTime:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Operador: {receipt.Cashier}");
            sb.AppendLine("------------------------------------------");

            // Items
            sb.AppendLine("\x1B\x61\x00"); // Left alignment
            sb.AppendLine("ITEM                    QTD    VALOR");
            sb.AppendLine("------------------------------------------");

            foreach (var item in receipt.Items)
            {
                var itemName = TruncateString(item.ProductName, 20);
                var qty = item.Qty.ToString("F2");
                var price = item.UnitPrice.ToString("C");
                var subtotal = item.Subtotal.ToString("C");

                sb.AppendLine($"{itemName,-20} {qty,5} {price,8}");
                if (!string.IsNullOrEmpty(item.Notes))
                {
                    sb.AppendLine($"  Obs: {item.Notes}");
                }
            }

            // Totals
            sb.AppendLine("------------------------------------------");
            sb.AppendLine($"Subtotal: {receipt.Subtotal:C}");
            if (receipt.Tax > 0)
            {
                sb.AppendLine($"Impostos: {receipt.Tax:C}");
            }
            sb.AppendLine($"TOTAL: {receipt.Total:C}");
            sb.AppendLine("------------------------------------------");

            // Payment
            sb.AppendLine($"Pagamento: {receipt.PaymentMethod}");
            sb.AppendLine("");

            // Footer
            sb.AppendLine("\x1B\x61\x01"); // Center alignment
            sb.AppendLine("Obrigado pela preferência!");
            sb.AppendLine("Volte sempre!");
            sb.AppendLine("");
            sb.AppendLine("==========================================");

            // Cut paper
            sb.AppendLine("\x1D\x56\x00"); // Full cut

            return sb.ToString();
        }

        private string GenerateTestPageContent()
        {
            var sb = new StringBuilder();

            sb.AppendLine("\x1B\x40"); // Initialize printer
            sb.AppendLine("\x1B\x61\x01"); // Center alignment

            sb.AppendLine("==========================================");
            sb.AppendLine("         TESTE DE IMPRESSÃO");
            sb.AppendLine("==========================================");
            sb.AppendLine($"Data: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine("==========================================");
            sb.AppendLine("");
            sb.AppendLine("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            sb.AppendLine("abcdefghijklmnopqrstuvwxyz");
            sb.AppendLine("0123456789");
            sb.AppendLine("!@#$%^&*()_+-=[]{}|;':\",./<>?");
            sb.AppendLine("");
            sb.AppendLine("==========================================");
            sb.AppendLine("Teste concluído com sucesso!");
            sb.AppendLine("==========================================");

            sb.AppendLine("\x1D\x56\x00"); // Full cut

            return sb.ToString();
        }

        private async Task SimulateEscPosPrinting(string content)
        {
            // Simular delay de impressão
            await Task.Delay(2000);

            // Em produção, enviaria os comandos ESC/POS para a impressora
            _logger.LogInformation("Conteúdo do cupom:\n{Content}", content);
        }

        private string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
                return input;

            return input.Substring(0, maxLength - 3) + "...";
        }
    }
}
