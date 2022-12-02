using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Simulation.Model;
using JK.Framework.ModbusTCP;
using JK.Framework.Const;

namespace NISimulationClient
{
    public class NIBoxServer : NIBoxModel
    {
        public ModbusTCPService ModbusService { get; set; }
        public NIBoxServer() : base()
        {
            this.ModbusService = new ModbusTCPService();
            this.ModbusService.AttachObserver(this.subjectObserver.Update);
        }
        public void Initialize()
        {
            this.ModbusService.Initialize(Resource.ConfigFileName);

        }

        //读取modbus server 数据(AO Channel DO Channel) 更新到channel，准备发送到NI Card
         public void ReadChannelValueToNICard()
         {
            ushort length = this.DataLength(this.AOChannels);
            if (length > 0)
            {
                ushort[] Data = this.ModbusService.GetHoldingRegisterDatas(0, length);
                foreach (FieldRelationShip relation in this.AOChannels)
                {
                    relation.channel.value = Data[relation.channel.offset];
                }
            }
            length = this.DataLength(this.DOChannels);
            if (length > 0)
            {
                bool[] DOData = this.ModbusService.GetCoilDiscreteDatas(0, length);
                foreach (FieldRelationShip relation in this.DOChannels)
                {
                    relation.channel.value = DOData[relation.channel.offset] ? (ushort)1 : (ushort)0;
                }
            }
        }

        //根据NI Card数据，发送到对应通道Channel，更新到modbus server(AI Channel DI Channel) 
        public void WriteNICardValueToChannel()
        {
            ushort length = this.DataLength(this.AIChannels);
            if (length > 0)
            {
                ushort[] data = new ushort[length];

                foreach (FieldRelationShip relation in this.AIChannels)
                {
                    data[relation.channel.offset] = relation.channel.value;
                }
                this.ModbusService.SetInputRegisterDatas(data, 0, length);
            }

            length = this.DataLength(this.DIChannels);
            if (length > 0)
            {
                bool[] data = new bool[length];

                foreach (FieldRelationShip relation in this.DIChannels)
                {
                    data[relation.channel.offset] = relation.channel.value==(ushort)1?true:false;
                }
                this.ModbusService.SetInputDiscreteDatas(data, 0, length);
            }
        }



        public void Process()
        {
            while (!this.terminated)
            {
                Thread.Sleep(1);
                try
                {
                    if (UpdateTimeOut())
                    {
                       // this.ReadChannelValueToNICard();
                      //  this.WriteNICardValueToChannel();                       
                    }
                }
                catch (System.Threading.ThreadInterruptedException)
                {
                }
            }
        }
        public void Start()
        {
            this.processor = new Thread(new ThreadStart(this.Process));
            this.processor.IsBackground = true;
            this.processor.Start();
        }

    }
}
