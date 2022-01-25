namespace EDHM_DX
{
	partial class ShipPreviewForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShipPreviewForm));
			this.picturePreview = new DevExpress.XtraEditors.PictureEdit();
			((System.ComponentModel.ISupportInitialize)(this.picturePreview.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// picturePreview
			// 
			this.picturePreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.picturePreview.Location = new System.Drawing.Point(0, 0);
			this.picturePreview.Name = "picturePreview";
			this.picturePreview.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
			this.picturePreview.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
			this.picturePreview.Size = new System.Drawing.Size(797, 466);
			this.picturePreview.TabIndex = 8;
			// 
			// ShipPreviewForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(797, 466);
			this.Controls.Add(this.picturePreview);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ShipPreviewForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Theme Preview";
			this.Load += new System.EventHandler(this.ShipPreviewForm_Load);
			this.ResizeEnd += new System.EventHandler(this.ShipPreviewForm_ResizeEnd);
			((System.ComponentModel.ISupportInitialize)(this.picturePreview.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.PictureEdit picturePreview;
	}
}