using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MECHENG_313_A2.Serial
{
    public interface ISerialPort
    {
        Task<string[]> GetPortNames();
        Task<bool> Open(string portName, int baudRate);
        bool Close();
        Task<bool> Write(string data);
        event EventHandler<string> DataReceived;
    }
}
