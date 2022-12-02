using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using JK.Framework.Utils;
using JK.Framework.Const;
using JK.Framework.ModbusTCP;
using Simulation.Model;




namespace NISimulationService
{

    public enum EVENT_NIBOX
    {
        INIT = 1000,
        WRITE = 1001,
        READ = 1003,
    }

    

    public class NIBoxClient: NIBoxModel
    {
        public ModbusTCPClient modbusChannel { get; set; }

        public object[] setToNIBoxNames { get; set; }
        public object[] setToSimulationNames { get; set; }
        public object[] setToSimulationValues { get; set; }

        public const int BASE_AI_ADDRESS = 0;
        public const int BASE_AO_ADDRESS = 0;
        public const int BASE_DI_ADDRESS = 0;
        public const int BASE_DO_ADDRESS = 0;



        public NIBoxClient():base()
        {

        }

        public void Initialize()
        {
            this.modbusChannel = new ModbusTCPClient();
            this.modbusChannel.Ip = this.address;
            this.modbusChannel.port = this.port;
            this.modbusChannel.slaveAddress = 1;
            this.modbusChannel.AttachObserver(this.subjectObserver.Update);

            this.setToSimulationNames = new object[this.AIChannels.Count + this.DIChannels.Count];
            this.setToSimulationValues = new object[this.AIChannels.Count + this.DIChannels.Count];

            this.setToNIBoxNames = new object[this.AOChannels.Count + this.DOChannels.Count];


            this.UpdateSetToBoxFieldNames();
            this.UpdateSetToSimulationFieldNames();

            this.processor = new Thread(new ThreadStart(this.Process));
            this.processor.IsBackground = true;

            this.Start();
        }

        public override void ProcessResponse(int notifyEvent, string flag, string content, object result, string message, object sender)
        {
            switch (notifyEvent)
            {
                case (int)EVENT_MODBUS.MODBUS_INIT:
                    {
                        this.errorCode = (ChannelErrorCode)result;
                        this.lastMessage = message;
                        this.Notify((int)EVENT_NIBOX.INIT, flag, content, result, message);
                        break;
                    }
            
            }
            
        }
        public void Start()
        {
            this.modbusChannel.ConnectToTCPServer();
            this.processor.Start();
        }


        private void UpdateSetToBoxFieldNames()
        {
            int index = 0;
            foreach (FieldRelationShip relation in this.AOChannels)
            {
                relation.UpdateConfig();
                this.setToNIBoxNames[index] = relation.simulationField.name;
                index += 1;
            }
            foreach (FieldRelationShip relation in this.DOChannels)
            {
                this.setToNIBoxNames[index] = relation.simulationField.name;
                index += 1;
            }
        
        }
        public void UpdateSetToSimulationFieldValues()
        {
            int index = 0;
            foreach (FieldRelationShip relation in this.AIChannels)
            {
                
                this.setToSimulationValues[index] = relation.simulationField.getValueForSim.ToString();
                index += 1;
            }
            foreach (FieldRelationShip relation in this.DIChannels)
            {
                this.setToSimulationValues[index] = relation.simulationField.getValueForSim.ToString();
                index += 1;
            }

        }


        private void UpdateSetToSimulationFieldNames()
        {
            int index=0;
            foreach (FieldRelationShip relation in this.AIChannels)
            {
                relation.UpdateConfig();
                this.setToSimulationNames[index] = relation.simulationField.name;

                index += 1;
            }
            foreach (FieldRelationShip relation in this.DIChannels)
            {
                this.setToSimulationNames[index] = relation.simulationField.name;
                index += 1;
            }

        }

        public void Process()
        {
            while (!this.terminated)
            {
                try
                {
                    Thread.Sleep(1);
                    if (!this.modbusChannel.hasError)
                    {
                        if (this.UpdateTimeOut())
                        {                           
                            {
                                this.UpdateDatas();
                                this.SendClientData();
                            }
                        }
                    }
                    else
                    {
                        this.modbusChannel.ReConnectTCPServer();
                    }                  
                }
                catch (System.Threading.ThreadInterruptedException)
                {
                }
            }
            
        }

        private void  UpdateDatas()
        {
            ushort length =this.DataLength(this.AIChannels);
            if (length > 0)
            {
                ushort[] datas = this.modbusChannel.ReadInputRegisters(BASE_AI_ADDRESS, length);
                //获取AI的值并根据AI进行处理   根据对应字段设置到仿真器 
                if (this.modbusChannel.lastErrorCode == ChannelErrorCode.Ok)
                {
                    foreach (FieldRelationShip relation in this.AIChannels)
                    {
                        ushort data = datas[relation.channel.offset];
                        relation.simulationField.getValueForSim =(double) data / (double)relation.config.rate;
                    }             
                }
            }

            length = this.DataLength(this.DIChannels);
            if (length > 0)
            {
                bool[] datas = this.modbusChannel.ReadInputs(BASE_DI_ADDRESS, length);
                //获取DI的值并根据DI进行处理   根据对应字段设置到仿真器 
                if (this.modbusChannel.lastErrorCode == ChannelErrorCode.Ok)
                {
                    foreach (FieldRelationShip relation in this.DIChannels)
                    {
                        bool data = datas[relation.channel.offset];
                        relation.simulationField.getValueForSim = data ? (ushort)1: (ushort)0 ;
                    }
                }
            }   

        }

        private void SendClientData()
        { 
            ushort length =this.DataLength(this.AOChannels);
            if (length >0)
            {
                ushort[] datas = new ushort[length];
                //得到仿真器字段的值
                //FIXME：根据AI对应进行设置到主机modbus地址
                foreach (FieldRelationShip relation in this.AOChannels)
                { 
                    object value= relation.simulationField.setToBoxValue;

                    ushort data = 0;
                    if (value != null)
                    {
                        data = (ushort)(double.Parse(value.ToString()) * relation.config.rate);
                    }
                    datas[relation.channel.offset] = data;          
                }
                this.modbusChannel.WriteMultipleRegisters(BASE_AO_ADDRESS, datas);
            }

            length = this.DataLength(this.DOChannels);
            if (length > 0)
            {
                bool[] datas = new bool[length];
                //得到仿真器字段的值
                //FIXME：根据DI对应进行设置到主机modbus地址
                foreach (FieldRelationShip relation in this.DOChannels)
                {
                    object value = relation.simulationField.setToBoxValue;
                    bool data = false;
                    if (value != null)
                    {
                        data = int.Parse(value.ToString()) > 0;
                        datas[relation.channel.offset] = data;
                    }
                }
                this.modbusChannel.WriteMultipleCoils(BASE_DO_ADDRESS, datas);
            }
        
        }


    
    }


}
