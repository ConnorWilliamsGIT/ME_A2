using System.Threading.Tasks;
using System.Threading;
using System;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Xamarin.Forms;
using MECHENG_313_A2.Serial;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Diagnostics;

[assembly: Dependency(typeof(SerialPortImplementation))]

public class SerialPortImplementation : ISerialPort
{
    private SerialDevice _serialDevice;
    private DataWriter _dataWriter;
    private DataReader _dataReader;
    private CancellationTokenSource _readCancellationTokenSource;
    private string _dataBuffer = "";

    public event EventHandler<string> DataReceived;

    public async Task<string[]> GetPortNames()
    {
        var aqs = SerialDevice.GetDeviceSelector();
        var deviceInfos = await DeviceInformation.FindAllAsync(aqs);

        return deviceInfos.Select(info => info.Name).ToArray();
    }

    public async Task<bool> Open(string portName, int baudRate)
    {
        var aqs = SerialDevice.GetDeviceSelector();
        var devices = await DeviceInformation.FindAllAsync(aqs);
        var deviceInfo = devices.FirstOrDefault(info => info.Name == portName);

        if (deviceInfo == null)
        {
            return false;
        }

        _serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id);

        if (_serialDevice == null)
        {
            return false;
        }

        _serialDevice.BaudRate = (uint)baudRate;
        _serialDevice.DataBits = 8;
        _serialDevice.Parity = SerialParity.None;
        _serialDevice.StopBits = SerialStopBitCount.One;

        _dataWriter = new DataWriter(_serialDevice.OutputStream);
        _dataReader = new DataReader(_serialDevice.InputStream);
        _readCancellationTokenSource = new CancellationTokenSource();

        Task.Run(ReadSerialAsync);

        return true;
    }

    public bool Close()
    {
        if (_serialDevice == null)
        {
            return false;
        }

        _readCancellationTokenSource.Cancel();
        _dataWriter?.Dispose();
        _dataReader?.Dispose();
        _serialDevice.Dispose();
        _serialDevice = null;

        return true;
    }

    public async Task<bool> Write(string data)
    {
        if (_serialDevice == null)
        {
            return false;
        }

        _dataWriter.WriteString(data);
        await _dataWriter.StoreAsync();

        return true;
    }

    private async Task ReadSerialAsync()
    {
        while (!_readCancellationTokenSource.IsCancellationRequested)
        {
            await _dataReader.LoadAsync(1).AsTask(_readCancellationTokenSource.Token);

            if(_dataReader.UnconsumedBufferLength > 0)
            {
                var data = _dataReader?.ReadString(1);
                _dataBuffer += data;

                if (string.Equals("\n", data))
                {
                    DataReceived?.Invoke(this, _dataBuffer);
                    _dataBuffer = "";
                }
            }
        }
    }
}
