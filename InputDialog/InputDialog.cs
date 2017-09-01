using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InputDialog
{
	public partial class InputDialog : Form
	{
		public InputDialog(string title, string instructions, string[] options)
		{
			//auto generated, has to be untouched
			InitializeComponent();

			//init method params
			this.Text = title;
			this.label1.Text = instructions;
			this.comboBox1.Items.AddRange(options);
			if (options.Length > 0)
				this.comboBox1.Text = options[0];

		}

		//method to request combobox value
		public string getSelection()
		{
			return this.comboBox1.Text;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		//load dialog at mouse position for minimal mouse movement
		private void InputDialog_Load(object sender, EventArgs e)
		{
			var _point = new System.Drawing.Point(Cursor.Position.X, Cursor.Position.Y);
			Top = _point.Y;
			Left = _point.X;
		}
	}
}
