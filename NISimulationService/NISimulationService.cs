using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JK.Framework.Utils;
using Newtonsoft.Json;
using System.Threading;
using NISimulationService.SimDAL;
using JK.Framework.Const;
using Simulation.Model;
using JK.Framework.Channels;


namespace NISimulationService
{

    public enum EVENT_MANAGE
    {
        INIT = 0,
        UPDATE = 1,
    }




    public class NIBoxClients
    {
        public int total { get; set; }
        public List<NIBoxClient> rows { get; set; }
    }
    public class NISimulationService: SimulationManager
    {
        private string name { get; set; }
        private string mainURL { get; set; }

        private NIBoxClients NIBoxList { get; set; }

        public SimDALClient simDALClient { get; set; }


        public List<NIBoxClient> NIBoxClients
        {
            get
            {
                if (this.NIBoxList == null)
                {
                    return new List<NIBoxClient>();
                }
                else
                {
                    return this.NIBoxList.rows;
                }

            }
        }

        public NISimulationService()
            : base()
        {
            this.name = "NISimulationService";
        }


        public bool LoadConfigFromServer()
        {
            string url = string.Format("{0}{1}",this.mainURL,URL.GetBoxListURL);
            string config  =HttpClient.HttpGet(url);
            if (config != "")
            {
                return this.LoadConfig(config);
            }
            else
            {
                return false;
            }

        }



        public void LoadFromFile(string fileName)
        {
            string Section = this.name;

            this.mainURL = IniFiles.GetStringValue(fileName, Section, "MainURL", "http://127.0.0.1:8000/");

            if (!System.IO.File.Exists(fileName) || (!IniFiles.GetAllSectionNames(fileName).Contains(Section)))
            {
                this.SaveToFile(fileName);
            }
        }

        public void SaveToFile(string fileName)
        {
            string Section = this.name;
            IniFiles.WriteStringValue(fileName, Section, "MainURL", this.mainURL);
        }

        public void Initialize()
        {
            this.updateInterval = 100;
            this.updateLastDatetime = DateTime.Now;
            this.simDALClient = new SimDALClient();
            this.simDALClient.Initialize(Resource.ConfigFileName);

            this.simDALClient.AttachObserver(this.subjectObserver.Update);


            foreach (NIBoxClient client in this.NIBoxClients)
            {
                client.AttachObserver(this.subjectObserver.Update);               
            }

            this.processor = new Thread(new ThreadStart(this.Process));
            this.processor.IsBackground = true;
            this.Start();
        }



        public void Process()
        {
            while (!this.terminated)
            {
                try
                {
                    Thread.Sleep(1);
                    if (!this.simDALClient.hasError)
                    {
                        if (this.UpdateTimeOut())
                        {
                            {
                                this.GetVPClientDatas();
                                this.SetVPClientDatas();
                            }
                        }
                    }
                    else
                    {
                        this.simDALClient.ReRunServer();
                    }
                }
                catch (System.Threading.ThreadInterruptedException)
                {
                }
            }
        }
        public void Start()
        {
            this.processor.Start();
        }

        public void GetVPClientDatas()
        {

            foreach (NIBoxClient client in this.NIBoxClients)
            {
                int index = 0;
                object[] values = this.simDALClient.GetValues(client.setToNIBoxNames);
               

                foreach (FieldRelationShip relation in client.AOChannels)
                {
                    relation.simulationField.setToBoxValue = values[index];
                    index += 1;
                }
                foreach (FieldRelationShip relation in client.DOChannels)
                {
                    relation.simulationField.setToBoxValue = values[index];
                    index += 1;
                }
            }         
        }

        public void SetVPClientDatas()
        {
            foreach (NIBoxClient client in this.NIBoxClients)
            { 
                client.UpdateSetToSimulationFieldValues();
                this.simDALClient.SetValues(client.setToSimulationNames,client.setToSimulationValues);              
            }      
        }



        public bool LoadConfigFromFile(string filename)
        {
            string config = Utils.LoadFromFile(filename);
            return this.LoadConfig(config);
        }


        public bool  LoadConfig(string configText)
        {
            try
            {
                if (configText != "")
                {
                    this.NIBoxList = JsonConvert.DeserializeObject<NIBoxClients>(configText);
                    foreach (NIBoxClient client in this.NIBoxClients)
                    {
                        client.Initialize();
                    }

                    return true;
                }
                return false;
            }
            catch(Exception)
            {
                return false;
            }

        }
    }
}
