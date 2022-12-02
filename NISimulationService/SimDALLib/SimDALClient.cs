using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimDALLib;
using JK.Framework.Utils;
using JK.Framework.Const;


namespace NISimulationService.SimDAL
{
    public enum EVENT_VPCLIENT
    {
        INIT = 3000,
        WRITE = 3001,
        READ = 3003,
    }

    public class SimDALClient:SuperSubject
    {
        public string name { get; set; }
        public VPClient client { get; set; }
        public string host { get; set; }
        public string shop { get; set; }
        public string userName { get; set; }
        public string password { get; set; }

        public string lastMessage { get; set; }
        public ChannelErrorCode lastErrorCode { get; set; }

        public DateTime lastRunDatetime { get; set; }


        public Boolean active{
            get{
                if (this.client == null)
                {
                    return false;
                }
                else
                {
                    return this.client.Online;
                }
            }
        }
        public Boolean hasError
        {
            get
            {
                return this.lastErrorCode != ChannelErrorCode.Ok;
            }
        }

        public SimDALClient()
        {
            this.name = "SimuVPClient";           
            this.lastMessage = "";
            this.lastErrorCode = ChannelErrorCode.None;
            this.lastRunDatetime = DateTime.Now;
            this.client = new VPClient();
        }

        public override void ProcessResponse(int notifyEvent, string flag, string content, object result, string message, object sender)
        {
            this.Notify(notifyEvent, flag, content, result, message);
        }
        public void Initialize(string fileName)
        {
            this.LoadFromFile(fileName);         
        }

        public Boolean ConnectTimeOut()
        {
            TimeSpan span = DateTime.Now - this.lastRunDatetime;
            if (span.TotalSeconds >= 5)
            {
                this.lastRunDatetime = DateTime.Now;
                return true;
            }
            return false;        
            
        }

        public Boolean ReRunServer()
        {
            if (this.lastErrorCode != ChannelErrorCode.Ok && this.ConnectTimeOut())
            {
                return this.ConnectAndRunServer();
            }
            return false;
        }

        public void LoadFromFile(string fileName)
        {
            string Section = this.name;

            this.host = IniFiles.GetStringValue(fileName, Section, "Host", "127.0.0.1");
            this.shop = IniFiles.GetStringValue(fileName, Section, "Shop","HWTest");
            this.userName = IniFiles.GetStringValue(fileName, Section, "UserName", "");
            this.password = IniFiles.GetStringValue(fileName, Section, "Password", "");

            if (!System.IO.File.Exists(fileName) || (!IniFiles.GetAllSectionNames(fileName).Contains(Section)))
            {
                this.SaveToFile(fileName);
            }
        }

        public void SaveToFile(string fileName)
        {
            string Section = this.name;

            IniFiles.WriteStringValue(fileName, Section, "Host", this.host);
            IniFiles.WriteStringValue(fileName, Section, "Shop", this.shop);
            IniFiles.WriteStringValue(fileName, Section, "UserName", this.userName);
            IniFiles.WriteStringValue(fileName, Section, "Password", this.password);
        }

        public bool ConnectAndRunServer()
        {
            this.Open();
            if (this.active)
            {
                this.Run();
            }

            return this.lastErrorCode == ChannelErrorCode.Ok;
        }

        private Boolean Run()
        {
            try
            {
                this.client.Run();
                this.lastMessage = "启动仿真服务器成功！";
                this.lastErrorCode = ChannelErrorCode.Ok;
                this.Notify((int)EVENT_VPCLIENT.INIT, ChannelErrorCode.Ok.ToString(), "", this.client, this.lastMessage);
                return true;
            }
            catch
            {
                this.lastMessage = "启动仿真服务器失败！";
                this.lastErrorCode = ChannelErrorCode.InitError;
                this.Notify((int)EVENT_VPCLIENT.INIT, ChannelErrorCode.InitError.ToString(), "", this.client, this.lastMessage);
                return false;
            }
            
        }
        public Boolean Open()
        {
            try
            {
                this.lastMessage = "正在尝试连接到仿真服务器.";
                this.lastErrorCode = ChannelErrorCode.None;
                this.Notify((int)EVENT_VPCLIENT.INIT, ChannelErrorCode.None.ToString(), "", this.client, this.lastMessage);

                if (this.userName == "")
                {
                    this.client.Connect(this.host, this.shop, null, null);

                }
                else
                {
                    this.client.Connect(this.host, this.shop, this.userName, this.password);
                }
                this.lastMessage = "连接到仿真服务器成功！";
                this.lastErrorCode = ChannelErrorCode.Ok;
                this.Notify((int)EVENT_VPCLIENT.INIT, ChannelErrorCode.Ok.ToString(), "", this.client, this.lastMessage);
                return true;
            }
            catch
            {
                this.lastMessage = "无法连接到仿真服务器！";
                this.lastErrorCode = ChannelErrorCode.InitError;
                this.Notify((int)EVENT_VPCLIENT.INIT, ChannelErrorCode.InitError.ToString(), "", this.client, this.lastMessage);
                return false;
            }
                 
        }

        public Boolean Close()
        {
            try
            {
                if (this.client.Online)
                {
                    this.client.Freeze();
                }
                this.lastMessage = "Freeze成功！";
                this.lastErrorCode = ChannelErrorCode.Ok;
                this.Notify((int)EVENT_VPCLIENT.INIT, ChannelErrorCode.Ok.ToString(), "", this.client, this.lastMessage);
                return true;
            }
            catch
            {
                this.lastMessage = "Freeze失败！";
                this.lastErrorCode = ChannelErrorCode.InitError;
                this.Notify((int)EVENT_VPCLIENT.INIT, ChannelErrorCode.InitError.ToString(), "", this.client, this.lastMessage);
                return false;
            }
        
        }

        public void SetValues(object names, object values)
        {
            if (client.Online)
            {
                this.client.SetValues(names, values);
            }
        }
        public object[] GetValues(object names)
        {
            if (client.Online)
            {
                object value = this.client.GetValues(names);
                return (object[])(value);
            }
            return new object[0];
        }



    }
}
