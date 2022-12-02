using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JK.Framework.Utils;
using NationalInstruments.DAQmx;
using Simulation.Model;
using JK.Framework.Const;


namespace Equipment.NIDAQController
{
    

    public class DeviceList
    {
        public List<PhysicalDevice> Devices { get; set; }
        public DeviceList()
        {
            this.Devices = new List<PhysicalDevice>();
        }

        public void RefreshDevices()
        {
            this.Devices.Clear();
            try
            {
                foreach (string name in DaqSystem.Local.Devices)
                {
                    PhysicalDevice device = new PhysicalDevice(name);
                    string[] channels = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);
                    foreach (string channel in channels)
                    {
                        if (channel.Contains(name))
                        {
                            DeviceChannel aChannel = new DeviceChannel()
                            {
                                deviceName = name,
                                deviceModel = DeviceModel.NI,
                                physicalName = channel,
                                fieldType = FieldType.AI,
                            };
                            device.AIChannels.Add(aChannel);
                        }
                    }
                    channels = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AO, PhysicalChannelAccess.External);
                    foreach (string channel in channels)
                    {
                        if (channel.Contains(name))
                        {
                            DeviceChannel aChannel = new DeviceChannel()
                            {
                                deviceName = name,
                                deviceModel = DeviceModel.NI,
                                physicalName = channel,
                                fieldType = FieldType.AO,
                            };
                            device.AOChannels.Add(aChannel);
                        }
                    }
                    channels = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.DILine, PhysicalChannelAccess.External);
                    foreach (string channel in channels)
                    {
                        if (channel.Contains(name))
                        {
                            DeviceChannel aChannel = new DeviceChannel()
                            {
                                deviceName = name,
                                deviceModel = DeviceModel.NI,
                                physicalName = channel,
                                fieldType = FieldType.DI,
                            };
                            device.DIChannels.Add(aChannel);
                        }
                    }
                    channels = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.DOLine, PhysicalChannelAccess.External);
                    foreach (string channel in channels)
                    {
                        if (channel.Contains(name))
                        {
                            DeviceChannel aChannel = new DeviceChannel()
                            {
                                deviceName = name,
                                deviceModel = DeviceModel.NI,
                                physicalName = channel,
                                fieldType = FieldType.DO,
                            };
                            device.DOChannels.Add(aChannel);
                        }
                    }
                    this.Devices.Add(device);
                }
            }
            catch 
            { 
                
            }

        }

        public void SaveToFile(string name)
        {
            string title = "序号,板卡类型,设备名称,通道类型,通道标识,最小值,最大值";
            List<string> lines = new List<string>();
            lines.Add(title);
            int index = 1;
            foreach (PhysicalDevice device in this.Devices)
            {
                foreach (DeviceChannel channel in device.AIChannels)
                {
                    lines.Add(string.Format("{0},{1}",index,channel.LineText));
                    index += 1;
                }
            }

            Utils.WriteListToFile(lines, name);
        }
    }


    public class NIDAQmxController : SuperSubject
    {

        public const string SaveFileName = "NIHW.csv";
        public DeviceList DaqDeviceList { get; set; }


        public NIDAQmxController():base()
        {
            this.DaqDeviceList = new DeviceList();
            this.DaqDeviceList.RefreshDevices();
        }

        public void Initialize()
        {
            this.DaqDeviceList.SaveToFile(System.IO.Path.Combine(Resource.AppPath, SaveFileName));
        }


        public void RefreshPhysicalCardInfo()
        {
           string[] channels = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.All, PhysicalChannelAccess.Internal);


        }

        public void AOWirte(string physicalName, string name, ushort value, Config config)
        {

            double data = (double)value / (double)config.rate;
            Task channelTask = new Task();
            try
            {
                channelTask.AOChannels.CreateCurrentChannel(physicalName, name, config.minValue, config.maxValue, AOCurrentUnits.Amps);
                AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(channelTask.Stream);
                writer.WriteSingleSample(true, data);
                channelTask.Dispose();
            }
            catch (Exception)
            {
                channelTask.Dispose();
            }
        }

        public ushort AIRead(string physicalName, string name, Config config)
        {
            Task channelTask = new Task();
            try
            {             
                channelTask.AIChannels.CreateCurrentChannel(
                        physicalName,
                        name,
                        (AITerminalConfiguration)(-1),
                        config.minValue,
                        config.maxValue,
                        AICurrentUnits.Amps
                        );

                channelTask.Control(TaskAction.Verify);
                AnalogMultiChannelReader AIAnalogReader = new AnalogMultiChannelReader(channelTask.Stream);

                double[] Data = AIAnalogReader.ReadSingleSample();

                channelTask.Dispose();
                return (ushort)(Data[0] * config.rate);
            }
            catch (Exception)
            {
                channelTask.Dispose();
                return 0; 
            }

            
        }


        public bool DIRead(string physicalName, string name)
        {
   
            string message = "";
            Task channelTask = new Task();
            try
            {
                channelTask.DIChannels.CreateChannel(physicalName, name, ChannelLineGrouping.OneChannelForEachLine);
                DigitalSingleChannelReader DigitalReader = new DigitalSingleChannelReader(channelTask.Stream);
                bool readData = DigitalReader.ReadSingleSampleSingleLine();
                return readData;
            }
            catch (DaqException exception)
            {
                message = String.Format("DI输出通道[{0}]读取失败({1})！", physicalName, exception.ToString());
                channelTask.Dispose();
                return false;
            }

        }
        public void DOWirte(string physicalName, string name, bool value)
        {           
            string message = "";
            Task channelTask = new Task();
            try
            {                
                channelTask.DOChannels.CreateChannel(physicalName, name, ChannelLineGrouping.OneChannelForEachLine);
                DigitalSingleChannelWriter DigitalWriter = new DigitalSingleChannelWriter(channelTask.Stream);
                DigitalWriter.WriteSingleSampleSingleLine(true, value);
                channelTask.Dispose();
            }
            catch (DaqException exception)
            {
                message = String.Format("DO输出通道[{0}]操作失败({1})！", physicalName, exception.ToString());
                channelTask.Dispose();
            }
        }

    }
}
