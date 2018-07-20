using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Folder_Scanner
{
    public partial class Form1 : Form
    {
        /* 窗体可见部分宽度 ( Width_E=Width-16 ) ,高度 ( Height_E=Height-39 )  */
        TreeNode Last_Node;
        private bool Adjust_Size = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 默认listView1按项目大小排序
            listView1.ListViewItemSorter = new ListViewSort();
            listView1.Sort();
            // 初始化panel1上的控件的参数
            panel1.Location = new Point(0, 0);
            panel1.Size = new Size(Width - 16, Height - 39);
            groupBox1.Size = new Size(panel1.Width / 2, 52);
            groupBox1.Location = new Point((panel1.Width - groupBox1.Width) / 2, (panel1.Height - groupBox1.Height) / 2);
            button2.Location = new Point((panel1.Width - button2.Width) / 2, (3 * panel1.Height - 2 * groupBox1.Height) / 4);
            groupBox1.Visible = button2.Visible = button3.Visible = true;
            // 初始化窗体中的部分控件的toolTip
            ToolTip toolTip = new ToolTip();
            toolTip.ShowAlways = true;
            toolTip.SetToolTip(button1, "重置扫描的文件夹");
            toolStripStatusLabel1.Text = "0 个项目";
            toolStripStatusLabel2.Text = "已选择 0 个项目";
        }
        // 用于打开重置扫描路径界面
        private void timer1_Tick(object sender, EventArgs e)
        {
            double Ratio = (panel1.Left + 0.0) / button1.Left;
            panel1.Location = new Point(3 * panel1.Left / 4, 7 * panel1.Top / 8);
            panel1.Width = Width - 16 - (int)(Ratio * 6) - panel1.Left;
            panel1.Height = Height - 39 - (int)(Ratio * (Height - 69)) - panel1.Top;
            // 退出动画
            if (panel1.Left <= 0)
            {
                // 关闭定时器
                timer1.Enabled = false;
                //调整panel1的位置和大小
                panel1.Location = new Point(0, 0);
                panel1.Size = new Size(Width - 16, Height - 39);
                // 确定groupBox1大小、位置
                groupBox1.Size = new Size(panel1.Width / 2, 52);
                groupBox1.Location = new Point((panel1.Width - groupBox1.Width) / 2, (panel1.Height - groupBox1.Height) / 2);
                // 确定button2位置
                button2.Location = new Point((panel1.Width - button2.Width) / 2, (3 * panel1.Height - 2 * groupBox1.Height) / 4);
                // 确定panel1的子控件的可见性
                groupBox1.Visible = button2.Visible = button3.Visible = true;
            }
        }
        // 用于关闭重置扫描路径界面
        private void timer2_Tick(object sender, EventArgs e)
        {
            // Ratio为动画速度
            double Ratio = panel1.Width / 4.0 / (Width - 16);
            panel1.Width = 3 * panel1.Width / 4;
            panel1.Left = (int)(panel1.Left + Ratio * (Width + 37 - button1.Width));//37为调整动画值
            panel1.Height = 3 * panel1.Height / 4;
            panel1.Top = (int)(panel1.Top + Ratio * 6);
            // 退出动画
            if (panel1.Height <= button1.Height)
            {
                timer2.Enabled = false;
                panel1.Visible = false;
            }
        }
        // 用于监视扫描路径的任务是否完成,若完成则调整panel1内控件参数
        private void timer3_Tick(object sender, EventArgs e)
        {
            if (button2.Visible && button2.Text == "完成")
            {
                timer3.Enabled = false;
                groupBox1.Size = new Size(panel1.Width - 12, panel1.Height - 160);
                groupBox1.Location = new Point(6, (panel1.Height - groupBox1.Height) / 2);
                button2.Location = new Point((panel1.Width - button2.Width) / 2, panel1.Height - 60);
            }
        }
        // 重置
        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Location = button1.Location;
            panel1.Size = button1.Size;
            textBox2.Text = textBox1.Text;
            groupBox1.Visible = button2.Visible = button3.Visible = false;
            panel1.Visible = true;
            timer1.Enabled = true;
        }
        // 开始扫描/完成
        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "开始扫描")
            {
                //先检查是否为有效路径(是否是一个文件夹)
                if (!Directory.Exists(textBox2.Text))
                {
                    MessageBox.Show("该路径不存在!");
                    return;
                }
                // 扫描前设置控件参数
                button2.Visible = button3.Visible = false;
                button1.Focus();
                string The_Path = textBox2.Text;
                Text = "正在扫描中...";
                groupBox1.Text = "已忽略目录";
                textBox2.Text = "无";
                // groupBox1动画
                while (true)
                {
                    groupBox1.Width = (int)(groupBox1.Width * 3 / 2.0);
                    groupBox1.Height = panel1.Height * groupBox1.Width / panel1.Width;
                    groupBox1.Location = new Point((panel1.Width - groupBox1.Width) / 2, (panel1.Height - 6 - groupBox1.Height) / 2);
                    if (groupBox1.Top <= 6)
                        break;
                    Thread.Sleep(3);
                }
                groupBox1.Location = new Point(0, 6);
                groupBox1.Size = new Size(panel1.Width, panel1.Height - 6);
                // 打开扫描
                Folder_Scan newScanner = new Folder_Scan(treeView1, button2, textBox2, this);
                treeView1.Nodes.Clear();
                listView1.Items.Clear();
                ThreadPool.QueueUserWorkItem(newScanner.Get_Folder_Size, The_Path);
                // 打开对扫描的监视
                timer3.Enabled = true;
            }
            else if (button2.Text == "完成")
            {
                foreach (TreeNode node in treeView1.Nodes)
                    Last_Node = node;
                Text = "文件夹扫描";
                groupBox1.Text = "点击开始扫描下面的路径";
                button2.Text = "开始扫描";
                button3_Click(sender, e);
            }
        }
        // 返回
        private void button3_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = button2.Visible = button3.Visible = false;
            timer2.Enabled = true;
        }
        // 把treeView1的信息转到listView1
        private void treeView_To_listView(TreeNode _Node)
        {
            long Folder_Num = 0, File_Num = 0;
            DirectoryInfo dire;
            /* 显示返回父目录项 */
            listView1.Items.Clear();
            ListViewItem item = new ListViewItem();
            item.Text = "..";
            item.ImageIndex = 0;
            item.SubItems.Add(""); item.SubItems.Add(""); item.SubItems.Add("");
            item.ToolTipText = "返回到上一层目录";
            listView1.Items.Add(item);
            long total_size = long.Parse(_Node.Tag.ToString());
            /* 显示文件夹信息 */
            foreach (TreeNode child_node in _Node.Nodes)
            {
                /* 名称 */
                item = new ListViewItem(child_node.Text);
                /* 图标 */
                item.ImageIndex = 0;
                /* 大小 */
                long size = long.Parse(child_node.Tag.ToString());
                item.SubItems.Add(translate_size(size) + "  (" + child_node.Tag.ToString() + ")");
                /* 占比 */
                double ratio = 100.0 * size / total_size;
                item.SubItems.Add(Math.Round(ratio, 2).ToString() + "%");
                /* 修改时间 */
                dire = new DirectoryInfo(_Node.ToolTipText + "\\" + child_node.Text);
                item.SubItems.Add(dire.CreationTime.ToString());
                /* ToolTipText提示 */
                item.ToolTipText = "名称:" + child_node.Text + "\r\n类型:文件夹\r\n大小:" + translate_size(size) + "\r\n修改时间:" + dire.CreationTime.ToString();
                /* 添加 */
                listView1.Items.Add(item);
                Folder_Num++;
            }
            /* 显示文件信息 */
            dire = new DirectoryInfo(_Node.ToolTipText);
            try
            {
                foreach (FileInfo file in dire.GetFiles())
                {
                    /* 名称 */
                    item = new ListViewItem(file.Name);
                    /* 图标 */
                    item.ImageIndex = 1;
                    /* 大小 */
                    item.SubItems.Add(translate_size(file.Length) + "  (" + file.Length.ToString() + ")");
                    /* 占比 */
                    double ratio = 100.0 * file.Length / total_size;
                    item.SubItems.Add(Math.Round(ratio, 2).ToString() + "%");
                    /* 修改时间 */
                    item.SubItems.Add(file.LastWriteTime.ToString());
                    /* ToolTipText提示 */
                    item.ToolTipText = "名称:" + file.Name + "\r\n类型:文件\r\n大小:" + translate_size(file.Length) + "\r\n修改时间:" + dire.CreationTime.ToString();
                    /* 添加 */
                    listView1.Items.Add(item);
                    File_Num++;
                }
                textBox1.Text = _Node.ToolTipText;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("访问被拒绝! (" + _Node.ToolTipText + ")");
            }
            if (Folder_Num == 0 && File_Num == 0)
                toolStripStatusLabel1.Text = "0 个项目";
            else
                toolStripStatusLabel1.Text = (Folder_Num + File_Num).ToString() + " 个项目  " + Folder_Num.ToString() + " 个文件夹  " + File_Num.ToString() + " 个文件";
            toolStripStatusLabel2.Text = "已选择 0 个项目";
        }
        // treeView1点击事件
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeView_To_listView(e.Node);
            Last_Node = treeView1.SelectedNode;
        }
        // 字节转其他单位
        private string translate_size(long size)
        {
            int rank = 0;
            string[] units = { " B", " K", " M", " G", " T" };
            double _size = size;
            while (_size >= 1024)
            {
                _size /= 1024;
                rank++;
            }
            return Math.Round(_size, 2).ToString() + units[rank];
        }
        // listView1的Column点击事件
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            listView1.ListViewItemSorter = new ListViewSort();
            listView1.Sort();
        }
        // listView1点击事件
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ListViewItem thisItem = listView1.GetItemAt(e.X, e.Y);
                Open_This_Item(thisItem);
            }
        }
        // 控制panel1上的控件参数
        private void panel1_SizeChanged(object sender, EventArgs e)
        {
            if (panel1.Visible == true)
            {
                if (button2.Visible)
                {
                    if (button2.Text == "开始扫描") // 未开始扫描控件参数
                    {
                        groupBox1.Width = panel1.Width / 2;
                        groupBox1.Location = new Point((panel1.Width - groupBox1.Width) / 2, (panel1.Height - groupBox1.Height) / 2);
                        button2.Location = new Point((panel1.Width - button2.Width) / 2, (3 * panel1.Height - 2 * groupBox1.Height) / 4);
                    }
                    else if (button2.Text == "完成") // 扫描完成控件参数
                    {
                        groupBox1.Size = new Size(panel1.Width - 12, panel1.Height - 160);
                        groupBox1.Location = new Point(6, (panel1.Height - groupBox1.Height) / 2);
                        button2.Location = new Point((panel1.Width - button2.Width) / 2, panel1.Height - 60);
                    }
                }
                else // 正在扫描中控件参数
                {
                    groupBox1.Location = new Point(0, 6);
                    groupBox1.Size = new Size(panel1.Width, panel1.Height - 6);
                }
            }
        }
        // 打开选中项目
        private void Open_This_Item(ListViewItem thisItem)
        {
            if (thisItem == null)
            {
                MessageBox.Show("好像打开失败了,请点击重置重新扫描路径后再次查看");
                return;
            }
            if (thisItem.ImageIndex == 1) // 如果是文件则打开文件
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.Arguments = "/c " + Last_Node.ToolTipText + "\\" + thisItem.Text; // 或者textBox1.Text+"\\"+thisItem.Text
                process.Start();
                process.Close();
                return;
            }
            if (thisItem.ToolTipText == "返回到上一层目录")
            {
                if (Last_Node.Parent != null)
                {
                    Last_Node = Last_Node.Parent;
                }
            }
            else // 如果是文件夹则打开文件夹
            {
                Last_Node = Last_Node.FirstNode;
                while (Last_Node != null)
                {
                    if (Last_Node.Text == thisItem.Text)
                    {
                        break;
                    }
                    Last_Node = Last_Node.NextNode;
                }
            }
            treeView1.Focus();
            treeView1.SelectedNode = Last_Node;
            treeView_To_listView(Last_Node);
        }
        // 打开选中的第一项目
        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem thisItem = listView1.SelectedItems[0];
            Open_This_Item(thisItem);
        }
        // 打开文件管理器后只能选中一项
        private void 用文件管理器打开此处ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem thisItem = listView1.SelectedItems[0];
            Process process = new Process();
            process.StartInfo.FileName = "explorer.exe";
            if (thisItem.Text == "..")
                process.StartInfo.Arguments = @"/select," + Last_Node.ToolTipText;
            else
                process.StartInfo.Arguments = @"/select," + Last_Node.ToolTipText + "\\" + thisItem.Text;
            process.Start();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            foreach(ListViewItem item in listView1.SelectedItems)
            {
                if (item.Text == "..")
                {
                    toolStripStatusLabel2.Text = "已选择 " + (listView1.SelectedItems.Count - 1).ToString() + " 个项目";
                    return;
                }
            }
            toolStripStatusLabel2.Text = "已选择 " + listView1.SelectedItems.Count.ToString() + " 个项目";
        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 newForm = new Form2();
            newForm.Set_Position(Left, Top, Width, Height);
            newForm.ShowDialog();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("反馈邮箱:1575375168@qq.com");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("确定要退出吗?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (Adjust_Size)
            {
                Point formPoint = PointToClient(MousePosition);
                if (5 < formPoint.X && formPoint.X < (treeView1.Width + 5) || (treeView1.Width + 11) < formPoint.X && formPoint.X < (treeView1.Width + 11 + listView1.Width))
                {
                    treeView1.Width = formPoint.X - 8;
                    listView1.Left = formPoint.X + 3;
                    listView1.Width = Width - treeView1.Width - 32;
                    panel2.Left = formPoint.X - 3;
                }
            }
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            Adjust_Size = true;
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            Adjust_Size = false;
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox2.Text = textBox2.Text.Substring(0, textBox2.Text.Length - 2);
                button2_Click(sender, e);
                textBox1.Focus();
            }
        }

        private void 帮助ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("在多选时点击打开，只打开第一项");
        }
    }
    public class Folder_Scan
    {
        private DirectoryInfo Dir;
        private TreeView treeView;//存放文件夹信息的树
        private Button button;
        private TextBox textBox;//用于存放被拒绝访问的文件夹
        private Form form;

        public Folder_Scan(TreeView treeView, Button button, TextBox textBox, Form form)
        {
            this.treeView = treeView;
            this.button = button;
            this.textBox = textBox;
            this.form = form;
        }

        public void Get_Folder_Size(object obj)
        {
            string path = obj.ToString();
            // Text:文件夹名
            TreeNode newtNode = new TreeNode(path);
            // ToolTipText:绝对路径
            newtNode.ToolTipText = path;
            treeView.Invoke(new Action(() => { treeView.Nodes.Add(newtNode); }));
            // Tag:文件夹大小(开始扫描)
            newtNode.Tag = Get_Size(newtNode, path);
            // 扫描完成后调整控件参数
            After_Scanning();


            //newtNode.Expand();
            treeView.Invoke(new Action(() => { newtNode.Expand(); }));
            /* 调整提示界面 */
            treeView.Invoke(new Action(() => { button.Text = "完成"; }));
        }

        private long Get_Size(TreeNode Father_Node, string path)
        {
            long path_size = 0;
            Dir = new DirectoryInfo(path);
            /* 计算path文件夹的大小 */
            try
            {
                foreach (FileInfo file in Dir.GetFiles())
                {
                    path_size += file.Length;
                }
                foreach (DirectoryInfo child_dir in Dir.GetDirectories())
                {
                    //child_dir.
                    /* 添加新节点 */
                    TreeNode newtNode = new TreeNode(child_dir.Name);
                    newtNode.ToolTipText = path + "\\" + child_dir.Name;
                    long size_temp = Get_Size(newtNode, path + "\\" + child_dir.Name);
                    newtNode.Tag = size_temp;
                    treeView.Invoke(new Action(() => { Father_Node.Nodes.Add(newtNode); }));
                    path_size += size_temp;
                }
            }
            catch (UnauthorizedAccessException)
            {
                textBox.Invoke(new Action(() =>
                {
                    if (textBox.Text == "无")
                        textBox.Text = path + "\r\n";
                    else
                        textBox.Text += path + "\r\n";
                }));
            }
            /* 返回path文件夹的大小 */
            return path_size;
        }

        private void After_Scanning()
        {
            form.Invoke(new Action(() => { form.Text = "扫描完成"; }));
            button.Invoke(new Action(() =>
            {
                button.Text = "完成";
                button.Visible = true;
            }));
        }
    }
    public class ListViewSort : IComparer
    {
        private int col;

        public ListViewSort()
        {
            col = 1;
        }
        public int Compare(object x, object y)
        {
            if (((ListViewItem)x).SubItems[col].Text == "" || ((ListViewItem)y).SubItems[col].Text == "")
                return 0;
            string s1 = ((ListViewItem)x).SubItems[col].Text,
                   s2 = ((ListViewItem)y).SubItems[col].Text;
            int p = s1.IndexOf('(');
            s1 = s1.Substring(p + 1, s1.Length - p - 2);
            p = s2.IndexOf('(');
            s2 = s2.Substring(p + 1, s2.Length - p - 2);
            long num1 = long.Parse(s1);
            long num2 = long.Parse(s2);
            if (num1 < num2)
                return 1;
            else
                return -1;
        }
    }
}
