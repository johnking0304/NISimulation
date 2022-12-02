using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JK.Framework.ModbusTCP;
using JK.Framework.Utils;
using JK.Framework.Const;
using Simulation.Model;
using Newtonsoft.Json;
using Equipment.NIDAQController;
using Equipment.PickeringController;
using System.Threading;
using JK.Framework.Channels;

namespace NISimulationClient
{
    public enum EVENT_MANAGE
    {
        INIT = 0,
        UPDATE = 1,
    }


    public class NIBoxServers
    {
        public int total { get; set; }
        public List<NIBoxServer> rows { get; set; }
    }


    public class NISimulationClient : SimulationManager
    {

        private string name { get; set; }
        public string boxCode { get; set; }
        private string mainURL { get; set; }
        private NIBoxServers NIBoxServers { get; set; }
        public NIDAQmxController NIDAQmxController { get; set; }
        public PickeringController PickeringController{get;set;}

        public NIBoxServer NIBoxServer { get
            {
                if ((this.NIBoxServers != null) && (this.NIBoxServers.total == 1))
                {
                    return this.NIBoxServers.rows[0];
                }
                return (NIBoxServer)null;
            }
        }


        public NISimulationClient() : base()
        {
            this.name = "NISimulationClient";            

            this.NIDAQmxController = new NIDAQmxController();
        }
        public void Initialize()
        {
            this.LoadFromFile(Resource.ConfigFileName);
          
            if (!this.LoadConfigFromServer())
            {
                this.LoadConfigFromFile(Resource.BoxClientsDataFile);
            }
               

            foreach (FieldRelationShip relation in this.NIBoxServer.AOChannels)
            {
                relation.UpdateConfig();
            }
            
            foreach (FieldRelationShip relation in this.NIBoxServer.AIChannels)
            {
                relation.UpdateConfig();
            }

           
            this.NIDAQmxController.Initialize();
            if (this.NIBoxServer != null)
            {
                this.NIBoxServer.Initialize();
                this.NIBoxServer.AttachObserver(this.subjectObserver.Update);
                
                //this.NIBoxServer.Start();
;            }

            
         

            this.Start();

        }


        public void DOChannelsWrite()
        {
            foreach (FieldRelationShip relation in this.NIBoxServer.DOChannels)
            {
                this.NIDAQmxController.DOWirte(relation.channel.name,"", relation.channel.value == 1 ? true : false);
            }
          
        }
        public void AOChannelsWrite()
        {
            foreach (FieldRelationShip relation in this.NIBoxServer.AOChannels)
            {
                if( relation.channel.cardType == "NI")
                {
                    this.NIDAQmxController.AOWirte(relation.channel.name, "", relation.channel.value, relation.config);
                }
                else if (relation.channel.cardType == "Pickering")
               {
                    //Bus slot channel ,value
                    int bus =0 ;
                    int solt = 0;
                    int channelId = 0;
                    double data = (double)relation.channel.value / (double)relation.config.rate;

                    if (int.TryParse(relation.channel.name, out bus) &&
                        int.TryParse(relation.channel.socketCode, out solt) &&
                        int.TryParse(relation.channel.portCode, out channelId))
                    {
                        if (relation.channel.fieldType == "RTD")
                        {
                            this.PickeringController.RTDWirte(bus, solt, channelId, data);
   
                        }
                        else if (relation.channel.fieldType == "TC")
                        {
                            //Bus slot channel ,value
                            this.PickeringController.TCWirte(bus, solt, channelId, data);
                        }
                    }
               }
            }
        }

        public void AIChannelsRead()
        {
            foreach (FieldRelationShip relation in this.NIBoxServer.AIChannels)
            {
                relation.channel.value = (ushort)this.NIDAQmxController.AIRead(relation.channel.name,"", relation.config);
            }

        }

        public void DIChannelsRead()
        {
            foreach (FieldRelationShip relation in this.NIBoxServer.DIChannels)
            {
                relation.channel.value = this.NIDAQmxController.DIRead(relation.channel.name,"") ? (ushort)(1):(ushort)(0);
            }
        }

        public void Start()
        {
            this.processor = new Thread(new ThreadStart(this.Process));
            this.processor.IsBackground = true;
            this.processor.Start();
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
                        this.NIBoxServer.ReadChannelValueToNICard();
                        this.DOChannelsWrite();
                        this.AOChannelsWrite();


                        this.DIChannelsRead();
                        this.AIChannelsRead();
                        this.NIBoxServer.WriteNICardValueToChannel();                      
                    }
                }
                catch (System.Threading.ThreadInterruptedException)
                {
                }
            }
        }


         public void LoadFromFile(string fileName)
         {
            string Section = this.name;

            this.boxCode = IniFiles.GetStringValue(fileName, Section, "BoxCode", "NIB0001");
            this.mainURL = IniFiles.GetStringValue(fileName, Section, "MainURL", "http://127.0.0.1:8000/");


            if (!System.IO.File.Exists(fileName) || (!IniFiles.GetAllSectionNames(fileName).Contains(Section)))
             {
                 this.SaveToFile(fileName);
             }
         }

         public void SaveToFile(string fileName)
         {
             string Section = this.name;
             IniFiles.WriteStringValue(fileName, Section, "BoxCode", this.boxCode);
             IniFiles.WriteStringValue(fileName, Section, "MainURL", this.mainURL);
         }

        public bool LoadConfig(string configText)
        {
            try
            {
                if (configText != "")
                {
                    this.NIBoxServers = JsonConvert.DeserializeObject<NIBoxServers>(configText);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool LoadConfigFromFile(string filename)
        {
            string config = Utils.LoadFromFile(filename);
            return this.LoadConfig(config);

        }


        public bool LoadConfigFromServer()
        {
            string url = string.Format("{0}{1}", this.mainURL, URL.GetBoxListURL);
            string param = string.Format("box_code={0}",this.boxCode);

            this.Notify((int)EVENT_MANAGE.INIT, "", "", null, "开始获取配置数据(服务器)");
            string config = HttpClient.HttpGet(url, param);
            if (config != "")
            {
                this.Notify((int)EVENT_MANAGE.INIT, "", "", null, "获取配置数据(服务器)成功！");
                return this.LoadConfig(config);
            }
            else
            {
                this.Notify((int)EVENT_MANAGE.INIT, "", "", null, "获取配置数据(服务器)失败！");
                return false;
            }

        }



    }
}
