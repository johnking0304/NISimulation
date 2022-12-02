using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JK.Framework.Const;
using JK.Framework.Utils;
using NISimulationService.SimDAL;


namespace NISimulationService
{

    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            //初始化 总控 Manager
            
            Program.NISimulationManager = new NISimulationService();
            Program.NISimulationManager.AttachObserver(this.Update);

            Program.NISimulationManager.Notify((int)EVENT_MANAGE.INIT, "", "", null, "系统启动，初始化数据中台");
            Program.NISimulationManager.LoadFromFile(Resource.ConfigFileName);
           //载入点位配置表`

           //Boolean result = Program.NISimulationManager.LoadConfigFromFile(Resource.BoxClientsDataFile);
           Boolean result =  Program.NISimulationManager.LoadConfigFromServer();

            if (!result)
            {
                Program.NISimulationManager.Notify((int)EVENT_MANAGE.INIT, "FAIL", "", null, "初始化主机列表信息失败！");
            }
            else
            {
                Program.NISimulationManager.Notify((int)EVENT_MANAGE.INIT, "OK", "", null, "初始化主机列表信息成功！"); 
            }

            Program.NISimulationManager.Initialize();

        }

        public void Update(int notifyEvent, string flag, string content, object result, string message, object sender)
        {
            SubjectObserver.FormInvoke update = new SubjectObserver.FormInvoke(this.ShowStatus);
            try
            {
                this.Invoke(update, notifyEvent, flag, content, result, message, sender);
            }
            catch (System.InvalidOperationException)
            {
            }
            catch (System.ComponentModel.InvalidAsynchronousStateException)
            {

            }
        }


        private void RefreshListViewer()
        {
            this.listViewNIBoxs.Items.Clear();
            for (int i = 0; i < Program.NISimulationManager.NIBoxClients.Count; i++)
            {
                NIBoxClient client = Program.NISimulationManager.NIBoxClients[i];
                this.AddListviewItem(client);
            }
        }

        private void AddListviewItem(NIBoxClient client)
        {
            ListViewItem listViewItem = new ListViewItem();
            listViewItem.Text = (listViewNIBoxs.Items.Count+1).ToString();
            listViewItem.SubItems.Add(client.name);
            listViewItem.SubItems.Add(string.Format("{0}:{1}",client.modbusChannel.Ip,client.modbusChannel.port));
            listViewItem.SubItems.Add(client.lastMessage);
            listViewItem.Tag = client;
            this.listViewNIBoxs.Items.Add(listViewItem);
        }

        private void AppendLog(string message)
        {
            string log = string.Format("{0}:{1}\n", DateTime.Now.ToString(), message);
            this.richTextBoxLog.AppendText(log);
        }

        private void ShowStatus(int Event, string flag, string content, object result, string message, object sender)
        {
            switch (Event)
            {
                case (int)EVENT_MANAGE.INIT:
                    {
                        if (flag == "OK")
                        {
                            this.RefreshListViewer();
                        }
                        break;
                    }
                case (int)EVENT_NIBOX.INIT:       
                    {
 
                        break;
                    }
                case  (int)EVENT_VPCLIENT.INIT:
                    {
                        ChannelErrorCode code =(ChannelErrorCode)Enum.Parse(typeof(ChannelErrorCode), flag);
                        this.UpdateVPCLientStatus(code,message);

                        break;
                    }
            }

            this.AppendLog(message);

        }


        private void UpdateVPCLientStatus(ChannelErrorCode code,string message)
        {
            switch (code)
            {
                case ChannelErrorCode.None:
                    { 
                        this.toolStripStatusServer.Image = Properties.Resources.normal;
                        break;
                    }
                case ChannelErrorCode.InitError:
                case ChannelErrorCode.ReadError:
                case ChannelErrorCode.WriteError:
                {
                        this.toolStripStatusServer.Image = Properties.Resources.error;
                        break;
                    }

                case ChannelErrorCode.Ok:
                    {
                        this.toolStripStatusServer.Image = Properties.Resources.ok;
                        break;
                    }
            }
            this.toolStripStatusMessage.Text = message;
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            
        }

        private void toolStripButtonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
