namespace GnomoriaModUI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.grid_modlist = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_launchwithmods = new System.Windows.Forms.Button();
            this.btn_buildgame = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.pictureBox_loader = new System.Windows.Forms.PictureBox();
            this.btn_reloadsettings = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grid_modlist)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_loader)).BeginInit();
            this.SuspendLayout();
            // 
            // grid_modlist
            // 
            this.grid_modlist.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grid_modlist.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid_modlist.Location = new System.Drawing.Point(12, 43);
            this.grid_modlist.Name = "grid_modlist";
            this.grid_modlist.Size = new System.Drawing.Size(424, 273);
            this.grid_modlist.TabIndex = 1;
            this.grid_modlist.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_modlist_CellValueChanged);
            this.grid_modlist.CurrentCellDirtyStateChanged += new System.EventHandler(this.grid_modlist_CurrentCellDirtyStateChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(163, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "gnomoria mod blabla";
            // 
            // btn_launchwithmods
            // 
            this.btn_launchwithmods.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_launchwithmods.Location = new System.Drawing.Point(38, 365);
            this.btn_launchwithmods.Name = "btn_launchwithmods";
            this.btn_launchwithmods.Size = new System.Drawing.Size(333, 55);
            this.btn_launchwithmods.TabIndex = 3;
            this.btn_launchwithmods.Text = "launch modded game";
            this.btn_launchwithmods.UseVisualStyleBackColor = true;
            this.btn_launchwithmods.Click += new System.EventHandler(this.btn_launchwithmods_Click);
            // 
            // btn_buildgame
            // 
            this.btn_buildgame.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_buildgame.Enabled = false;
            this.btn_buildgame.Location = new System.Drawing.Point(200, 322);
            this.btn_buildgame.Name = "btn_buildgame";
            this.btn_buildgame.Size = new System.Drawing.Size(140, 23);
            this.btn_buildgame.TabIndex = 4;
            this.btn_buildgame.Text = "debug_create modded exe";
            this.btn_buildgame.UseVisualStyleBackColor = true;
            this.btn_buildgame.Click += new System.EventHandler(this.btn_build_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.AccessibleName = "";
            this.richTextBox1.Location = new System.Drawing.Point(377, 324);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(59, 98);
            this.richTextBox1.TabIndex = 5;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            this.richTextBox1.Visible = false;
            // 
            // pictureBox_loader
            // 
            this.pictureBox_loader.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox_loader.BackColor = System.Drawing.Color.White;
            this.pictureBox_loader.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox_loader.Image = global::GnomoriaModUI.Properties.Resources.loader1;
            this.pictureBox_loader.Location = new System.Drawing.Point(200, 186);
            this.pictureBox_loader.Name = "pictureBox_loader";
            this.pictureBox_loader.Size = new System.Drawing.Size(31, 31);
            this.pictureBox_loader.TabIndex = 6;
            this.pictureBox_loader.TabStop = false;
            // 
            // btn_reloadsettings
            // 
            this.btn_reloadsettings.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_reloadsettings.Location = new System.Drawing.Point(102, 322);
            this.btn_reloadsettings.Name = "btn_reloadsettings";
            this.btn_reloadsettings.Size = new System.Drawing.Size(92, 23);
            this.btn_reloadsettings.TabIndex = 7;
            this.btn_reloadsettings.Text = "cancel / rescan";
            this.btn_reloadsettings.UseVisualStyleBackColor = true;
            this.btn_reloadsettings.Click += new System.EventHandler(this.btn_reloadsettings_Click);
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(38, 322);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(45, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 432);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btn_reloadsettings);
            this.Controls.Add(this.pictureBox_loader);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btn_buildgame);
            this.Controls.Add(this.btn_launchwithmods);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.grid_modlist);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grid_modlist)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_loader)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView grid_modlist;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_launchwithmods;
        private System.Windows.Forms.Button btn_buildgame;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.PictureBox pictureBox_loader;
        private System.Windows.Forms.Button btn_reloadsettings;
        private System.Windows.Forms.Button button1;
    }
}

