using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using JK.Framework.Utils;
using JK.Framework.Const;

namespace JK.Framework.ModbusTCP
{
    public class ModbusTCPService : Subject
    {
        public static ushort DATA_LENGTH = 2000;
        public string name = "ModbusTCPService";
        public ModbusSlave modbusSlave { get; set; }
        public int listenPort { get; set; }
        public byte slaveId { get; set; }
        public string lastMessage { get; set; }
        public ChannelErrorCode lastErrorCode { get; set; }


        public ModbusTCPService()
        {
            this.name = "ModbusTCPService";
            this.lastMessage = "";
            this.listenPort = 502;
            this.slaveId = 1;
            this.lastErrorCode = ChannelErrorCode.Ok;
        }
        public override void ProcessResponse(int notifyEvent, string flag, string content, object result, string message, object sender)
        {
            this.Notify(notifyEvent, flag, content, result, message);
        }

        public void Initialize(string configFileName)
        {
            this.LoadFromFile(configFileName);
            try
            {
                IPAddress address = new IPAddress(new byte[] { 0, 0, 0, 0 });

                TcpListener slaveTcpListener = new TcpListener(address, this.listenPort);
                slaveTcpListener.Start();

                this.modbusSlave = ModbusTcpSlave.CreateTcp(this.slaveId, slaveTcpListener);
                this.modbusSlave.DataStore = DataStoreFactory.CreateDefaultDataStore(DATA_LENGTH, DATA_LENGTH, DATA_LENGTH, DATA_LENGTH);


                this.modbusSlave.Listen();
                this.lastErrorCode = ChannelErrorCode.Ok;
                this.lastMessage = "Modbus TCP Service 启动成功";

            }
            catch (Exception)
            {
                this.lastErrorCode = ChannelErrorCode.InitError;
                this.lastMessage = "Modbus TCP Service 启动失败";

            }
            this.Notify((int)EVENT_MODBUS.MODBUS_INIT, this.lastErrorCode.ToString(), "", this.lastErrorCode, this.lastMessage);
        }

        public void LoadFromFile(string fileName)
        {
            string Section = this.name;
            this.listenPort = IniFiles.GetIntValue(fileName, Section, "ListenPort", 502);
            this.slaveId = (byte)IniFiles.GetIntValue(fileName, Section, "SlaveId", 1);

            string[] list = IniFiles.GetAllSectionNames(fileName);
            if (!list.Contains(Section))
            {
                this.SaveToFile(fileName);
            }
        }
        public void SaveToFile(string fileName)
        {
            string Section = this.name;
            IniFiles.WriteIntValue(fileName, Section, "ListenPort", this.listenPort);
            IniFiles.WriteIntValue(fileName, Section, "SlaveId", this.slaveId);
        }


        public Boolean hasError
        {
            get
            {
                return this.lastErrorCode != ChannelErrorCode.Ok;
            }
        }


        public ushort[] GetHoldingRegisterDatas(int startIndex, int length)
        {
            ushort[] datas = new ushort[length];
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                datas[i - startIndex] = this.modbusSlave.DataStore.HoldingRegisters[i+1];
            }
            return datas;
        }

        public ushort[] GetInputRegisterDatas(int startIndex, int length)
        {
            ushort[] datas = new ushort[length];
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                datas[i - startIndex] = this.modbusSlave.DataStore.InputRegisters[i+1];
            }
            return datas;
        }

        public bool[] GetCoilDiscreteDatas(int startIndex, int length)
        {
            bool[] datas = new bool[length];
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                datas[i - startIndex] = this.modbusSlave.DataStore.CoilDiscretes[i+1];
            }
            return datas;
        }


        public bool[] GetInputDiscreteDatas(int startIndex, int length)
        {
            bool[] datas = new bool[length];
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                datas[i - startIndex] = this.modbusSlave.DataStore.InputDiscretes[i+1];
            }
            return datas;
        }



        public void SetHoldingRegisterDatas(ushort[] datas, int startIndex, int length)
        {
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                this.modbusSlave.DataStore.HoldingRegisters[i+1] = datas[i - startIndex];
            }
        }


        public void SetInputRegisterDatas(ushort[] datas, int startIndex, int length)
        {
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                this.modbusSlave.DataStore.InputRegisters[i+1] = datas[i - startIndex];
            }
        }





        public void SetInputDiscreteDatas(bool[] datas, int startIndex, int length)
        {

            for (int i = startIndex; i < (startIndex + length); i++)
            {
                this.modbusSlave.DataStore.InputDiscretes[i+1] = datas[i - startIndex];
            }
        }

        public void SetCoilDiscreteDatas(bool[] datas, int startIndex, int length)
        {

            for (int i = startIndex; i < (startIndex + length); i++)
            {
                this.modbusSlave.DataStore.CoilDiscretes[i+1] = datas[i - startIndex];
            }
        }


        public void ClearHoldingRegisters()
        {
            this.ClearData(ModbusDataType.HoldingRegister, 1, DATA_LENGTH);
        }

        public void ClearInputRegisters()
        {
            this.ClearData(ModbusDataType.InputRegister, 1, DATA_LENGTH);
        }


        public void ClearCoilDiscretes()
        {
            this.ClearData(ModbusDataType.Coil, 1, DATA_LENGTH);
        }

        public void ClearInputDiscretes()
        {
            this.ClearData(ModbusDataType.Input, 1, DATA_LENGTH);
        }


        public void ClearData(ModbusDataType dataType, int startIndex, int length)
        {
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                switch (dataType)
                {
                    case ModbusDataType.Coil:
                        {
                            this.modbusSlave.DataStore.CoilDiscretes[i] = false;
                            break;
                        }
                    case ModbusDataType.HoldingRegister:
                        {
                            this.modbusSlave.DataStore.HoldingRegisters[i] = 0;
                            break;
                        }
                    case ModbusDataType.InputRegister:
                        {
                            this.modbusSlave.DataStore.InputRegisters[i] = 0;
                            break;
                        }
                    case ModbusDataType.Input:
                        {
                            this.modbusSlave.DataStore.InputDiscretes[i] = false;
                            break;
                        }

                }
            }
        }



    }

    public class ModbusTCPClient : Subject
    {
        public string name = "ModbusTCPClient";
        public static int DEFAULT_PORT = 502;

        public const int REGISTER_BLOCK_LEN = 123;
        public const int COIL_BLOCK_LEN = 2000;


        public ChannelErrorCode lastErrorCode { get; set; }
        public short baseIndex { get; set; }
        public ModbusMaster modbusClient { get; set; }

        private TcpClient tcpClient;
        public byte slaveAddress { get; set; }
        public string Ip { get; set; }
        public int port { get; set; }
        private DateTime lastConnectedDatetime { get; set; }

        public string lastMessage { get; set; }
        public void LoadFromFile(string fileName)
        {
            string Section = this.name;

            this.Ip = IniFiles.GetStringValue(fileName, Section, "IPAddress", "127.0.0.1");
            this.port = IniFiles.GetIntValue(fileName, Section, "Port", DEFAULT_PORT);
            this.slaveAddress = (byte)IniFiles.GetIntValue(fileName, Section, "SlaveAddress", 1);
            this.baseIndex = (short)IniFiles.GetIntValue(fileName, Section, "BaseIndex", 0);

            string[] list = IniFiles.GetAllSectionNames(fileName);
            if (!list.Contains(Section))
            {
                this.SaveToFile(fileName);
            }
        }
        public void SaveToFile(string fileName)
        {
            string Section = this.name;
            IniFiles.WriteStringValue(fileName, Section, "IPAddress", this.Ip);
            IniFiles.WriteIntValue(fileName, Section, "Port", this.port);
            IniFiles.WriteIntValue(fileName, Section, "SlaveAddress", this.slaveAddress);
            IniFiles.WriteIntValue(fileName, Section, "BaseIndex", this.baseIndex);
        }
        public ModbusTCPClient()
        {
            this.name = "ModbusTCPClient";
            this.lastMessage = "";
            this.lastErrorCode = ChannelErrorCode.Ok;
            this.slaveAddress = 1;
        }


        public Boolean hasError
        {
            get
            {
                return this.lastErrorCode != ChannelErrorCode.Ok;
            }
        }


        public Boolean ReConnectTCPServer()
        {
            if (this.lastErrorCode != ChannelErrorCode.Ok && this.ConnectTimeOut())
            {
                return this.ConnectToTCPServer();
            }
            return true;
        }

        public Boolean ConnectTimeOut()
        {
            TimeSpan span = DateTime.Now - this.lastConnectedDatetime;
            if (span.TotalSeconds >= 5)
            {
                this.lastConnectedDatetime = DateTime.Now;
                return true;
            }
            return false;
        }
        public Boolean ConnectToTCPServer()
        {
            try
            {
                this.tcpClient = new TcpClient();
                this.modbusClient = ModbusIpMaster.CreateIp(this.tcpClient);

                
                IAsyncResult connResult = this.tcpClient.BeginConnect(this.Ip, this.port, null, null);
                connResult.AsyncWaitHandle.WaitOne(1500, true);
                if (!connResult.IsCompleted)
                {
                    this.tcpClient.Close();
                    this.lastMessage = "无法连接到 Modbus TCP 服务器";
                    this.lastErrorCode = ChannelErrorCode.InitError;
                    this.Notify((int)EVENT_MODBUS.MODBUS_INIT, ChannelErrorCode.InitError.ToString(), "", this.lastErrorCode, this.lastMessage);
                    return false;
                }
                else if (this.tcpClient != null && this.tcpClient.Connected)
                {
                    this.lastMessage = "连接到 Modbus TCP服务器成功!";
                    this.lastConnectedDatetime = DateTime.Now;
                    this.lastErrorCode = ChannelErrorCode.Ok;
                    this.Notify((int)EVENT_MODBUS.MODBUS_INIT, ChannelErrorCode.Ok.ToString(), "", this.lastErrorCode, this.lastMessage);
                    return true;
                }
                else
                {
                    this.tcpClient.Close();
                    this.lastMessage = "无法连接到 Modbus TCP 服务器";
                    this.lastErrorCode = ChannelErrorCode.InitError;
                    this.Notify((int)EVENT_MODBUS.MODBUS_INIT, ChannelErrorCode.InitError.ToString(), "", this.lastErrorCode, this.lastMessage);
                    return false;
                }
            }
            catch (Exception msg)
            {
                this.tcpClient.Close();
                this.lastMessage = "无法连接到 Modbus TCP 服务器";
                this.lastErrorCode = ChannelErrorCode.InitError;
                this.Notify((int)EVENT_MODBUS.MODBUS_INIT, ChannelErrorCode.InitError.ToString(), "", this.lastErrorCode, this.lastMessage + ":" + msg.ToString());
                return false;
            }

        }
        public void Initialize(string fileName)
        {
            this.LoadFromFile(fileName);
            this.ConnectToTCPServer();
        }

        public bool[] ReadInputs(ushort startAddress, ushort numberOfPoints)
        {
            ushort len = numberOfPoints;
            ushort remainder = (ushort)(len % COIL_BLOCK_LEN);
            int count = len / COIL_BLOCK_LEN;
            ushort newAddress = startAddress;
            int index = 0;

            bool[] data = new bool[numberOfPoints];

            try
            {
                for (int i = 0; i < count; i++)
                {
                    bool[] readDatas = this.modbusClient.ReadInputs(this.slaveAddress, newAddress, REGISTER_BLOCK_LEN);
                    Array.Copy(readDatas, 0, data, index, COIL_BLOCK_LEN);
                    newAddress += COIL_BLOCK_LEN;
                    index += COIL_BLOCK_LEN;
                }
                if (remainder != 0)
                {
                    bool[] readDatas = this.modbusClient.ReadInputs(this.slaveAddress, newAddress, remainder);
                    Array.Copy(readDatas, 0, data, index, remainder);
                }

                this.lastErrorCode = ChannelErrorCode.Ok;
                this.lastMessage = "读取Modbus数据(Inputs)成功!";
                this.Notify((int)EVENT_MODBUS.MODBUS_READ, ChannelErrorCode.Ok.ToString(), "", this.lastErrorCode, this.lastMessage);
                return data;
            }
            catch (Exception e)
            {
                this.lastErrorCode = ChannelErrorCode.ReadError;
                this.lastMessage = string.Format("无法读取Modbus数据(Inputs):{0}", e.ToString());
                this.Notify((int)EVENT_MODBUS.MODBUS_READ, ChannelErrorCode.ReadError.ToString(), "", this.lastErrorCode, this.lastMessage);
                return new bool[0];
            }
        }

        public bool[] ReadCoils(ushort startAddress, ushort numberOfPoints)
        {
            ushort len = numberOfPoints;
            ushort remainder = (ushort)(len % COIL_BLOCK_LEN);
            int count = len / COIL_BLOCK_LEN;
            ushort newAddress = startAddress;
            int index = 0;

            bool[] data = new bool[numberOfPoints];

            try
            {
                for (int i = 0; i < count; i++)
                {
                    bool[] readDatas = this.modbusClient.ReadCoils(this.slaveAddress, newAddress, REGISTER_BLOCK_LEN);
                    Array.Copy(readDatas, 0, data, index, COIL_BLOCK_LEN);
                    newAddress += COIL_BLOCK_LEN;
                    index += COIL_BLOCK_LEN;
                }
                if (remainder != 0)
                {
                    bool[] readDatas = this.modbusClient.ReadCoils(this.slaveAddress, newAddress, remainder);
                    Array.Copy(readDatas, 0, data, index, remainder);
                }

                this.lastErrorCode = ChannelErrorCode.Ok;
                this.lastMessage = "读取Modbus数据(Coils)成功!";
                this.Notify((int)EVENT_MODBUS.MODBUS_READ, ChannelErrorCode.Ok.ToString(), "", this.lastErrorCode, this.lastMessage);
                return data;
            }
            catch (Exception e)
            {
                this.lastErrorCode = ChannelErrorCode.ReadError;
                this.lastMessage = string.Format("无法读取Modbus数据(Coils):{0}", e.ToString());
                this.Notify((int)EVENT_MODBUS.MODBUS_READ, ChannelErrorCode.ReadError.ToString(), "", this.lastErrorCode, this.lastMessage);
                return new bool[0];
            }
        }


        public  ushort[] ReadInputRegisters(ushort startAddress, ushort numberOfPoints)
        {
            ushort len = numberOfPoints;
            ushort remainder = (ushort)(len % REGISTER_BLOCK_LEN);
            int count = len / REGISTER_BLOCK_LEN;
            ushort newAddress = startAddress;
            int index = 0;

            ushort[] data = new ushort[numberOfPoints];

            try
            {
                for (int i = 0; i < count; i++)
                {
                    ushort[] readDatas = this.modbusClient.ReadInputRegisters(this.slaveAddress, newAddress, REGISTER_BLOCK_LEN);
                    Array.Copy(readDatas, 0, data, index, REGISTER_BLOCK_LEN);
                    newAddress += REGISTER_BLOCK_LEN;
                    index += REGISTER_BLOCK_LEN;
                }
                if (remainder != 0)
                {
                    ushort[] readDatas = this.modbusClient.ReadInputRegisters(this.slaveAddress, newAddress, remainder);
                    Array.Copy(readDatas, 0, data, index, remainder);
                }

                this.lastErrorCode = ChannelErrorCode.Ok;
                this.lastMessage = "读取Modbus数据(Input Registers)成功!";
                this.Notify((int)EVENT_MODBUS.MODBUS_READ, ChannelErrorCode.Ok.ToString(), "", this.lastErrorCode, this.lastMessage);
                
                return data;
            }
            catch (Exception e)
            {
                this.lastErrorCode = ChannelErrorCode.ReadError;
                this.lastMessage = string.Format("无法读取Modbus数据(Input Registers):{0}", e.ToString());
                this.Notify((int)EVENT_MODBUS.MODBUS_READ, ChannelErrorCode.ReadError.ToString(), "", this.lastErrorCode, this.lastMessage);
                return new ushort[0];
            }
        }

        public ushort[] ReadHoldingRegisters(ushort startAddress, ushort numberOfPoints)
        {
            ushort len = numberOfPoints;
            ushort remainder = (ushort)(len % REGISTER_BLOCK_LEN);
            int count = len / REGISTER_BLOCK_LEN;
            ushort newAddress = startAddress;
            int index = 0;

            ushort[] datas = new ushort[numberOfPoints];

            try
            {
                for (int i = 0; i < count; i++)
                {
                    ushort[] readDatas = this.modbusClient.ReadHoldingRegisters(this.slaveAddress, newAddress, REGISTER_BLOCK_LEN);
                    Array.Copy(readDatas, 0, datas, index, REGISTER_BLOCK_LEN);
                    newAddress += REGISTER_BLOCK_LEN;
                    index += REGISTER_BLOCK_LEN;
                }
                if (remainder != 0)
                {
                    ushort[] readDatas = this.modbusClient.ReadHoldingRegisters(this.slaveAddress, newAddress, remainder);
                    Array.Copy(readDatas, 0, datas, index, remainder);
                }
                this.lastMessage = "读取 Modbus TCP 数据成功";
                this.lastErrorCode = ChannelErrorCode.Ok;
                this.Notify((int)EVENT_MODBUS.MODBUS_READ, ChannelErrorCode.Ok.ToString(), "", this.lastErrorCode, this.lastMessage );
                return datas;
            }
            catch (System.InvalidOperationException)
            {
                this.lastErrorCode = ChannelErrorCode.ReadError;
                this.lastMessage = "无法读取 Modbus TCP 数据";
                this.Notify((int)EVENT_MODBUS.MODBUS_READ, ChannelErrorCode.ReadError.ToString(), "", this.lastErrorCode, this.lastMessage);
                return datas;
            }
            catch (Exception)
            {
                this.lastErrorCode = ChannelErrorCode.ReadError;
                this.lastMessage = "无法读取 Modbus TCP 数据";
                this.Notify((int)EVENT_MODBUS.MODBUS_READ, ChannelErrorCode.ReadError.ToString(), "", this.lastErrorCode, this.lastMessage);
                return datas;
            }
        }



        public override void ProcessResponse(int notifyEvent, string flag, string content, object result, string message, object sender)
        {
            this.Notify(notifyEvent, flag, content, result, message);
        }

        public void WriteMultipleRegisters(ushort startAddress, ushort[] data)
        {
            try
            {
                ushort len = (ushort)data.Length;
                ushort remainder = (ushort)(len % REGISTER_BLOCK_LEN);
                int count = len / REGISTER_BLOCK_LEN;
                ushort newAddress = startAddress;
                int index = 0;

                ushort[] datas = new ushort[REGISTER_BLOCK_LEN];
                for (int i = 0; i < count; i++)
                {
                    Array.Copy(data, index, datas, 0, REGISTER_BLOCK_LEN);
                    this.modbusClient.WriteMultipleRegisters(this.slaveAddress, newAddress, datas);
                    newAddress += REGISTER_BLOCK_LEN;
                    index += REGISTER_BLOCK_LEN;
                }
                if (remainder != 0)
                {
                    datas = new ushort[remainder];
                    Array.Copy(data, index, datas, 0, remainder);
                    this.modbusClient.WriteMultipleRegisters(this.slaveAddress, newAddress, datas);
                }
                this.lastErrorCode = ChannelErrorCode.Ok;
                this.lastMessage = "写入 Modbus TCP 数据成功";
                this.Notify((int)EVENT_MODBUS.MODBUS_WRITE, ChannelErrorCode.Ok.ToString(), "", this.lastErrorCode, this.lastMessage );
            }
            catch (System.InvalidOperationException)
            {
                this.lastErrorCode = ChannelErrorCode.WriteError;
                this.lastMessage = "无法写入 Modbus TCP  数据";
                this.Notify((int)EVENT_MODBUS.MODBUS_WRITE, ChannelErrorCode.WriteError.ToString(), "", this.lastErrorCode, this.lastMessage);
            }
            catch (Exception)
            {
                this.lastErrorCode = ChannelErrorCode.WriteError;
                this.lastMessage = "无法写入 Modbus TCP数据";
                this.Notify((int)EVENT_MODBUS.MODBUS_WRITE, ChannelErrorCode.ReadError.ToString(), "", this.lastErrorCode, this.lastMessage);
            }

        }

        public int WriteMultipleCoils(ushort startAddress, bool[] data)
        {
            try
            {
                ushort len = (ushort)data.Length;
                ushort remainder = (ushort)(len % COIL_BLOCK_LEN);
                int count = len / COIL_BLOCK_LEN;
                ushort newAddress = startAddress;
                int index = 0;

                bool[] datas = new bool[COIL_BLOCK_LEN];
                for (int i = 0; i < count; i++)
                {
                    Array.Copy(data, index, datas, 0, COIL_BLOCK_LEN);
                    this.modbusClient.WriteMultipleCoils(this.slaveAddress, newAddress, datas);
                    newAddress += COIL_BLOCK_LEN;
                    index += COIL_BLOCK_LEN;
                }
                if (remainder != 0)
                {
                    datas = new bool[remainder];
                    Array.Copy(data, index, datas, 0, remainder);
                    this.modbusClient.WriteMultipleCoils(this.slaveAddress, newAddress, datas);
                }
                this.lastErrorCode = ChannelErrorCode.Ok;
                this.lastMessage = "写入Modbus数据(Coils)成功!";
                this.Notify((int)EVENT_MODBUS.MODBUS_WRITE, ChannelErrorCode.Ok.ToString(), "", this.lastErrorCode, this.lastMessage);
                return len;
            }
            catch (Exception e)
            {
                this.lastErrorCode = ChannelErrorCode.WriteError;
                this.lastMessage = string.Format("无法写入Modbus数据(Coils):{0}", e.ToString());
                this.Notify((int)EVENT_MODBUS.MODBUS_WRITE, ChannelErrorCode.WriteError.ToString(), "", this.lastErrorCode, this.lastMessage);
                return 0;
            }

        }

    }

}
