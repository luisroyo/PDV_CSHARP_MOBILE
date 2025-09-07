using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Text;

namespace Pos.Desktop.Wpf.Services
{
    public class PeripheralService
    {
        private readonly ILogger<PeripheralService> _logger;
        private SerialPort? _barcodeReader;
        private SerialPort? _scale;

        public event EventHandler<string>? BarcodeScanned;
        public event EventHandler<decimal>? WeightReceived;

        public PeripheralService(ILogger<PeripheralService> logger)
        {
            _logger = logger;
        }

        public bool InitializeBarcodeReader(string portName = "COM1", int baudRate = 9600)
        {
            try
            {
                _barcodeReader = new SerialPort(portName, baudRate)
                {
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Parity = Parity.None,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                _barcodeReader.DataReceived += OnBarcodeDataReceived;
                _barcodeReader.Open();

                _logger.LogInformation("Leitor de código de barras inicializado na porta {PortName}", portName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar leitor de código de barras na porta {PortName}", portName);
                return false;
            }
        }

        public bool InitializeScale(string portName = "COM2", int baudRate = 9600)
        {
            try
            {
                _scale = new SerialPort(portName, baudRate)
                {
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Parity = Parity.None,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                _scale.DataReceived += OnScaleDataReceived;
                _scale.Open();

                _logger.LogInformation("Balança inicializada na porta {PortName}", portName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar balança na porta {PortName}", portName);
                return false;
            }
        }

        public async Task<string?> ReadBarcodeAsync()
        {
            try
            {
                if (_barcodeReader?.IsOpen != true)
                {
                    _logger.LogWarning("Leitor de código de barras não está conectado");
                    return null;
                }

                var data = new StringBuilder();
                var timeout = DateTime.Now.AddSeconds(5);

                while (DateTime.Now < timeout)
                {
                    if (_barcodeReader.BytesToRead > 0)
                    {
                        var buffer = new byte[_barcodeReader.BytesToRead];
                        var bytesRead = _barcodeReader.Read(buffer, 0, buffer.Length);
                        data.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));

                        // Verificar se recebeu um código completo (terminado com Enter)
                        if (data.ToString().Contains('\r') || data.ToString().Contains('\n'))
                        {
                            var barcode = data.ToString().Trim('\r', '\n', ' ');
                            _logger.LogInformation("Código de barras lido: {Barcode}", barcode);
                            return barcode;
                        }
                    }

                    await Task.Delay(50);
                }

                _logger.LogWarning("Timeout ao ler código de barras");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ler código de barras");
                return null;
            }
        }

        public async Task<decimal?> ReadWeightAsync()
        {
            try
            {
                if (_scale?.IsOpen != true)
                {
                    _logger.LogWarning("Balança não está conectada");
                    return null;
                }

                var data = new StringBuilder();
                var timeout = DateTime.Now.AddSeconds(3);

                while (DateTime.Now < timeout)
                {
                    if (_scale.BytesToRead > 0)
                    {
                        var buffer = new byte[_scale.BytesToRead];
                        var bytesRead = _scale.Read(buffer, 0, buffer.Length);
                        data.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));

                        // Verificar se recebeu um peso completo
                        if (data.ToString().Contains('\r') || data.ToString().Contains('\n'))
                        {
                            var weightData = data.ToString().Trim('\r', '\n', ' ');
                            if (decimal.TryParse(weightData, out var weight))
                            {
                                _logger.LogInformation("Peso lido: {Weight} kg", weight);
                                return weight;
                            }
                        }
                    }

                    await Task.Delay(50);
                }

                _logger.LogWarning("Timeout ao ler peso da balança");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ler peso da balança");
                return null;
            }
        }

        public bool SendCommandToPrinter(string command)
        {
            try
            {
                if (_barcodeReader?.IsOpen != true)
                {
                    _logger.LogWarning("Dispositivo não está conectado");
                    return false;
                }

                var bytes = Encoding.ASCII.GetBytes(command);
                _barcodeReader.Write(bytes, 0, bytes.Length);
                
                _logger.LogInformation("Comando enviado: {Command}", command);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar comando: {Command}", command);
                return false;
            }
        }

        public List<string> GetAvailablePorts()
        {
            try
            {
                return SerialPort.GetPortNames().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar portas seriais");
                return new List<string>();
            }
        }

        public bool IsBarcodeReaderConnected => _barcodeReader?.IsOpen == true;
        public bool IsScaleConnected => _scale?.IsOpen == true;

        private void OnBarcodeDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var data = _barcodeReader?.ReadExisting();
                if (!string.IsNullOrEmpty(data))
                {
                    var barcode = data.Trim('\r', '\n', ' ');
                    if (!string.IsNullOrEmpty(barcode))
                    {
                        BarcodeScanned?.Invoke(this, barcode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar dados do leitor de código de barras");
            }
        }

        private void OnScaleDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var data = _scale?.ReadExisting();
                if (!string.IsNullOrEmpty(data))
                {
                    var weightData = data.Trim('\r', '\n', ' ');
                    if (decimal.TryParse(weightData, out var weight))
                    {
                        WeightReceived?.Invoke(this, weight);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar dados da balança");
            }
        }

        public void Dispose()
        {
            _barcodeReader?.Close();
            _barcodeReader?.Dispose();
            _scale?.Close();
            _scale?.Dispose();
        }
    }
}
