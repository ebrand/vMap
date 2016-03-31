namespace vMap.ControlPanel
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
			this.noiseMapPictureBox = new System.Windows.Forms.PictureBox();
			this.showBordersCheckBox = new System.Windows.Forms.CheckBox();
			this.showSiteCenterpointsCheckBox = new System.Windows.Forms.CheckBox();
			this.showDelaunayCheckBox = new System.Windows.Forms.CheckBox();
			this.showHelpCheckBox = new System.Windows.Forms.CheckBox();
			this.showAgentsCheckBox = new System.Windows.Forms.CheckBox();
			this.noiseMapLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.noiseMapPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// noiseMapPictureBox
			// 
			this.noiseMapPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.noiseMapPictureBox.Location = new System.Drawing.Point(12, 232);
			this.noiseMapPictureBox.Name = "noiseMapPictureBox";
			this.noiseMapPictureBox.Size = new System.Drawing.Size(409, 303);
			this.noiseMapPictureBox.TabIndex = 1;
			this.noiseMapPictureBox.TabStop = false;
			// 
			// showBordersCheckBox
			// 
			this.showBordersCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
			this.showBordersCheckBox.Location = new System.Drawing.Point(12, 12);
			this.showBordersCheckBox.Name = "showBordersCheckBox";
			this.showBordersCheckBox.Size = new System.Drawing.Size(77, 23);
			this.showBordersCheckBox.TabIndex = 2;
			this.showBordersCheckBox.Text = "Borders";
			this.showBordersCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.showBordersCheckBox.UseVisualStyleBackColor = true;
			// 
			// showSiteCenterpointsCheckBox
			// 
			this.showSiteCenterpointsCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
			this.showSiteCenterpointsCheckBox.Location = new System.Drawing.Point(95, 12);
			this.showSiteCenterpointsCheckBox.Name = "showSiteCenterpointsCheckBox";
			this.showSiteCenterpointsCheckBox.Size = new System.Drawing.Size(77, 23);
			this.showSiteCenterpointsCheckBox.TabIndex = 2;
			this.showSiteCenterpointsCheckBox.Text = "Site Points";
			this.showSiteCenterpointsCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.showSiteCenterpointsCheckBox.UseVisualStyleBackColor = true;
			// 
			// showDelaunayCheckBox
			// 
			this.showDelaunayCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
			this.showDelaunayCheckBox.Location = new System.Drawing.Point(178, 12);
			this.showDelaunayCheckBox.Name = "showDelaunayCheckBox";
			this.showDelaunayCheckBox.Size = new System.Drawing.Size(77, 23);
			this.showDelaunayCheckBox.TabIndex = 3;
			this.showDelaunayCheckBox.Text = "Delaunay";
			this.showDelaunayCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.showDelaunayCheckBox.UseVisualStyleBackColor = true;
			// 
			// showHelpCheckBox
			// 
			this.showHelpCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
			this.showHelpCheckBox.Location = new System.Drawing.Point(344, 12);
			this.showHelpCheckBox.Name = "showHelpCheckBox";
			this.showHelpCheckBox.Size = new System.Drawing.Size(77, 23);
			this.showHelpCheckBox.TabIndex = 3;
			this.showHelpCheckBox.Text = "Help Text";
			this.showHelpCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.showHelpCheckBox.UseVisualStyleBackColor = true;
			// 
			// showAgentsCheckBox
			// 
			this.showAgentsCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
			this.showAgentsCheckBox.Location = new System.Drawing.Point(261, 12);
			this.showAgentsCheckBox.Name = "showAgentsCheckBox";
			this.showAgentsCheckBox.Size = new System.Drawing.Size(77, 23);
			this.showAgentsCheckBox.TabIndex = 3;
			this.showAgentsCheckBox.Text = "Agents";
			this.showAgentsCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.showAgentsCheckBox.UseVisualStyleBackColor = true;
			// 
			// noiseMapLabel
			// 
			this.noiseMapLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.noiseMapLabel.AutoSize = true;
			this.noiseMapLabel.Location = new System.Drawing.Point(9, 216);
			this.noiseMapLabel.Name = "noiseMapLabel";
			this.noiseMapLabel.Size = new System.Drawing.Size(70, 13);
			this.noiseMapLabel.TabIndex = 4;
			this.noiseMapLabel.Text = "Noise Map:";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(433, 547);
			this.Controls.Add(this.noiseMapLabel);
			this.Controls.Add(this.showAgentsCheckBox);
			this.Controls.Add(this.showHelpCheckBox);
			this.Controls.Add(this.showDelaunayCheckBox);
			this.Controls.Add(this.showSiteCenterpointsCheckBox);
			this.Controls.Add(this.showBordersCheckBox);
			this.Controls.Add(this.noiseMapPictureBox);
			this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "Form1";
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.noiseMapPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox noiseMapPictureBox;
		private System.Windows.Forms.CheckBox showBordersCheckBox;
		private System.Windows.Forms.CheckBox showSiteCenterpointsCheckBox;
		private System.Windows.Forms.CheckBox showDelaunayCheckBox;
		private System.Windows.Forms.CheckBox showHelpCheckBox;
		private System.Windows.Forms.CheckBox showAgentsCheckBox;
		private System.Windows.Forms.Label noiseMapLabel;
	}
}

