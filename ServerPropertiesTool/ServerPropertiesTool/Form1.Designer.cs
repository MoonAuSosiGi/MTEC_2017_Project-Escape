namespace ServerPropertiesTool
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.button_excelSelect = new System.Windows.Forms.Button();
            this.button_JsonConvert = new System.Windows.Forms.Button();
            this.button_helpButton = new System.Windows.Forms.Button();
            this.label_status = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(162, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "InGame Excel -> Json 변환";
            // 
            // button_excelSelect
            // 
            this.button_excelSelect.Location = new System.Drawing.Point(14, 56);
            this.button_excelSelect.Name = "button_excelSelect";
            this.button_excelSelect.Size = new System.Drawing.Size(128, 23);
            this.button_excelSelect.TabIndex = 0;
            this.button_excelSelect.Text = "Excel 파일 선택";
            this.button_excelSelect.UseVisualStyleBackColor = true;
            this.button_excelSelect.Click += new System.EventHandler(this.button_excelSelect_Click);
            // 
            // button_JsonConvert
            // 
            this.button_JsonConvert.Location = new System.Drawing.Point(14, 109);
            this.button_JsonConvert.Name = "button_JsonConvert";
            this.button_JsonConvert.Size = new System.Drawing.Size(128, 23);
            this.button_JsonConvert.TabIndex = 1;
            this.button_JsonConvert.Text = "Json 변환";
            this.button_JsonConvert.UseVisualStyleBackColor = true;
            this.button_JsonConvert.Click += new System.EventHandler(this.button_JsonConvert_Click);
            // 
            // button_helpButton
            // 
            this.button_helpButton.Location = new System.Drawing.Point(192, 56);
            this.button_helpButton.Name = "button_helpButton";
            this.button_helpButton.Size = new System.Drawing.Size(125, 76);
            this.button_helpButton.TabIndex = 2;
            this.button_helpButton.Text = "도움말";
            this.button_helpButton.UseVisualStyleBackColor = true;
            this.button_helpButton.Click += new System.EventHandler(this.button_helpButton_Click);
            // 
            // label_status
            // 
            this.label_status.AutoSize = true;
            this.label_status.ForeColor = System.Drawing.Color.Red;
            this.label_status.Location = new System.Drawing.Point(190, 22);
            this.label_status.Name = "label_status";
            this.label_status.Size = new System.Drawing.Size(97, 12);
            this.label_status.TabIndex = 3;
            this.label_status.Text = "엑셀 파일 미로드";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 146);
            this.Controls.Add(this.label_status);
            this.Controls.Add(this.button_helpButton);
            this.Controls.Add(this.button_JsonConvert);
            this.Controls.Add(this.button_excelSelect);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Made by MoonAuSosiGi";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_excelSelect;
        private System.Windows.Forms.Button button_JsonConvert;
        private System.Windows.Forms.Button button_helpButton;
        private System.Windows.Forms.Label label_status;
    }
}

