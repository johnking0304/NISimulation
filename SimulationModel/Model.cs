using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using JK.Framework.Utils;
using JK.Framework.Const;
using Newtonsoft.Json;

namespace Simulation.Model
{

    public class URL
    {
        public static string GetBoxListURL = "simulation/nibox/list/";
    }


    public enum DeviceModel
    { 
        NI =0,
        Pickering = 1,
    }
    public enum FieldType
    {
        AI = 0,
        DI = 1,
        AO = 2,
        DO = 3,
    }


    public class DeviceChannel
    {
        public FieldType fieldType { get; set; }
        public DeviceModel deviceModel { get; set; }
        public string deviceName { get; set; }
        public string physicalName { get; set; }

        public string LineText
           
        {
            get {
                //"序号,板卡类型,设备名称,通道类型,通道标识,最小值,最大值";
                    string title = string.Format("{0},{1},{2},{3},,", this.deviceModel.ToString(),
                        this.deviceName, this.fieldType.ToString(), this.physicalName);
                    return title;
                }
        }
    }

    public class PhysicalDevice
    {
        public string Name { get; set; }
        public List<DeviceChannel> AIChannels { get; set; }
        public List<DeviceChannel> AOChannels { get; set; }
        public List<DeviceChannel> DIChannels { get; set; }
        public List<DeviceChannel> DOChannels { get; set; }

        public PhysicalDevice(string name)
        {
            this.Name = name;
            this.AIChannels = new List<DeviceChannel> { };
            this.AOChannels = new List<DeviceChannel> { };
            this.DIChannels = new List<DeviceChannel> { };
            this.DOChannels = new List<DeviceChannel> { };
        }
    }



    public class Config
    {
        public double minValue { get; set; }
        public double maxValue { get; set; }
        public int rate { get; set; }
    }
    public class BaseModel : SuperSubject

    {
        public int id { get; set; }
        public string name { get; set; }
        public string remark { get; set; }
        public int sort { get; set; }
        public string serialCode { get; set; }
        public BaseModel()
        {
            this.id = 0;
            this.name = "";
            this.remark = "";
            this.sort = 0;
            this.serialCode = "";
        }

    }
    public class SimulationFiled : BaseModel
    {
        public string fieldType { get; set; }
        public object setToBoxValue { get; set; }  //从仿真器获取AIDI值 设置到主机箱
        public object getValueForSim { get; set; }  //从主机箱获取AODO值 设置到仿真器

        public SimulationFiled() :
            base()
        {
            this.fieldType = "";
            this.setToBoxValue = (ushort)0;
        }

    }


    public class Channel : BaseModel
    {
        public string fieldType { get; set; }
        public string cardType { get; set; }
        public string socketCode { get; set; }
        public string portCode { get; set; }
        public string lineCode { get; set; }
        public int linkBoxId { get; set; }
        public int offset { get; set; }
        public ushort value { get; set; }

        public Channel()
        {
            this.value = (ushort)0;
        }
    }


    public class FieldRelationShip
    {
        public string boxClient { get; set; }
        public Channel channel { get; set; }
        public SimulationFiled simulationField { get; set; }
        public string configText { get; set; }
        public Config config { get; set; }

        public void UpdateConfig()
        {
            this.config = (Config)JsonConvert.DeserializeObject<Config>(this.configText);
        }


    }


    public class NIBoxModel : BaseModel
    {
        public int port { get; set; }
        public string address { get; set; }
        public int socketCount { get; set; }
        public List<FieldRelationShip> AIChannels { get; set; }
        public List<FieldRelationShip> AOChannels { get; set; }
        public List<FieldRelationShip> DIChannels { get; set; }
        public List<FieldRelationShip> DOChannels { get; set; }


        public int updateInterval { get; set; }
        public DateTime updateLastDatetime { get; set; }

        public Thread processor;
        public Boolean terminated { get; set; }

        public string lastMessage { get; set; }
        public ChannelErrorCode errorCode { get; set; }

        public NIBoxModel() : base()
        {
            this.updateInterval = 200;
            this.updateLastDatetime = DateTime.Now;
        }

        public  bool UpdateTimeOut()
        {
            TimeSpan span = DateTime.Now - this.updateLastDatetime;
            if (span.TotalMilliseconds >= this.updateInterval)
            {
                this.updateLastDatetime = DateTime.Now;
                return true;
            }
            return false;
        }


        public ushort DataLength(List<FieldRelationShip> channels)
        {
            if (channels.Count > 0)
            {
                FieldRelationShip relation = channels.Last();
                return (ushort)(relation.channel.offset + 1);
            }
            else
            {
                return 0;
            }
        }
    }

    public class SimulationManager : SuperSubject
    {
        public int updateInterval { get; set; }
        public DateTime updateLastDatetime { get; set; }

        public Thread processor;
        public Boolean terminated { get; set; }


        public SimulationManager()
            : base()
        {

            this.updateInterval = 200;
            this.updateLastDatetime = DateTime.Now;
        }


        public Boolean UpdateTimeOut()
        {
            TimeSpan span = DateTime.Now - this.updateLastDatetime;
            if (span.TotalMilliseconds >= this.updateInterval)
            {
                this.updateLastDatetime = DateTime.Now;
                return true;
            }
            return false;
        }

    }
}
