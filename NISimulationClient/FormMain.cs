using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JK.Framework.Utils;
using JK.Framework.Const;
using Simulation.Model;

namespace NISimulationClient
{
    public partial class FormMain : Form
    {

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Program.NISimulationManager = new NISimulationClient();
            Program.NISimulationManager.AttachObserver(this.Update);

            Program.NISimulationManager.Notify((int)EVENT_MANAGE.INIT, "", "", null, "系统启动，开始初始化数据");      
            Program.NISimulationManager.Initialize();
            Program.NISimulationManager.Notify((int)EVENT_MANAGE.INIT, "OK", "", null, "系统启动，初始化数据成功");


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
            this.listViewNIBoxs.BeginUpdate();
            this.listViewNIBoxs.Items.Clear();
   
            for (int i = 0; i < Program.NISimulationManager.NIBoxServer.AIChannels.Count; i++)
            {
                FieldRelationShip relation = Program.NISimulationManager.NIBoxServer.AIChannels[i];
                this.AddListviewItem(relation);
            }
            for (int i = 0; i < Program.NISimulationManager.NIBoxServer.DIChannels.Count; i++)
            {
                FieldRelationShip relation = Program.NISimulationManager.NIBoxServer.DIChannels[i];
                this.AddListviewItem(relation);
            }
            for (int i = 0; i < Program.NISimulationManager.NIBoxServer.AOChannels.Count; i++)
            {
                FieldRelationShip relation = Program.NISimulationManager.NIBoxServer.AOChannels[i];
                this.AddListviewItem(relation);
            }
            for (int i = 0; i < Program.NISimulationManager.NIBoxServer.DOChannels.Count; i++)
            {
                FieldRelationShip relation = Program.NISimulationManager.NIBoxServer.DOChannels[i];
                this.AddListviewItem(relation);
            }
            this.listViewNIBoxs.EndUpdate();
        }

        private void AddListviewItem(FieldRelationShip relation)
        {
            ListViewItem listViewItem = new ListViewItem();
            listViewItem.Text = relation.simulationField.name;
            listViewItem.SubItems.Add(relation.simulationField.fieldType);
            listViewItem.SubItems.Add(relation.channel.socketCode);
            listViewItem.SubItems.Add(relation.channel.name);
            listViewItem.SubItems.Add(relation.channel.fieldType);
            listViewItem.SubItems.Add(relation.channel.value.ToString());
            listViewItem.Tag = relation;
            FieldType type = (FieldType)Enum.Parse(typeof(FieldType), relation.simulationField.fieldType);
            listViewItem.Group = this.listViewNIBoxs.Groups[(int)type];
            this.listViewNIBoxs.Items.Add(listViewItem);
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
                            this.timerRefresh.Enabled = true;
                        }
                        break;

                    }
            }

            this.AppendLog(message);

        }

        private void AppendLog(string message)
        {
            string log = string.Format("{0}:{1}\n", DateTime.Now.ToString(), message);
            this.richTextBoxLog.AppendText(log);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void toolStripButtonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            this.listViewNIBoxs.BeginUpdate();
            foreach (ListViewItem item in this.listViewNIBoxs.Items)
            {
                FieldRelationShip relation = (FieldRelationShip)item.Tag;
                item.SubItems[5].Text = relation.channel.value.ToString();
            }
            this.listViewNIBoxs.EndUpdate();
        }
    }
}
