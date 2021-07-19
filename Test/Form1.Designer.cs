namespace Test
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
			this.getAccountButton = new System.Windows.Forms.Button();
			this.accountNumberTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.cardNumber = new System.Windows.Forms.TextBox();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.dateFrom = new System.Windows.Forms.TextBox();
			this.dateTo = new System.Windows.Forms.TextBox();
			this.button4 = new System.Windows.Forms.Button();
			this.loanAppId = new System.Windows.Forms.TextBox();
			this.button5 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.AddContactButton = new System.Windows.Forms.Button();
			this.UpdateContactButton = new System.Windows.Forms.Button();
			this.DeleteContactButton = new System.Windows.Forms.Button();
			this.getContactButton = new System.Windows.Forms.Button();
			this.ContactIdTextBox = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.SaveUtilityPaymentButton = new System.Windows.Forms.Button();
			this.ContactAccountsDataGridView = new System.Windows.Forms.DataGridView();
			this.Id = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.AccountNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DeleteContactAccountButton = new System.Windows.Forms.Button();
			this.UpdateContactAccountButton = new System.Windows.Forms.Button();
			this.AddContactAccountbutton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.ContactId = new System.Windows.Forms.Label();
			this.DescriptionTextBox = new System.Windows.Forms.TextBox();
			this.GetContactsButton = new System.Windows.Forms.Button();
			this.button6 = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContactAccountsDataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// getAccountButton
			// 
			this.getAccountButton.Location = new System.Drawing.Point(157, 10);
			this.getAccountButton.Name = "getAccountButton";
			this.getAccountButton.Size = new System.Drawing.Size(75, 23);
			this.getAccountButton.TabIndex = 0;
			this.getAccountButton.Text = "GetAccount";
			this.getAccountButton.UseVisualStyleBackColor = true;
			this.getAccountButton.Click += new System.EventHandler(this.getAccountButton_Click);
			// 
			// accountNumberTextBox
			// 
			this.accountNumberTextBox.Location = new System.Drawing.Point(51, 12);
			this.accountNumberTextBox.Name = "accountNumberTextBox";
			this.accountNumberTextBox.Size = new System.Drawing.Size(100, 20);
			this.accountNumberTextBox.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Հաշիվ";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(10, 59);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 3;
			this.button1.Text = "GetCards";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// cardNumber
			// 
			this.cardNumber.Location = new System.Drawing.Point(10, 87);
			this.cardNumber.Name = "cardNumber";
			this.cardNumber.Size = new System.Drawing.Size(158, 20);
			this.cardNumber.TabIndex = 5;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(93, 58);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 4;
			this.button2.Text = "GetCard";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(174, 59);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(115, 23);
			this.button3.TabIndex = 6;
			this.button3.Text = "GetCardStatement";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// dateFrom
			// 
			this.dateFrom.Location = new System.Drawing.Point(174, 87);
			this.dateFrom.Name = "dateFrom";
			this.dateFrom.Size = new System.Drawing.Size(58, 20);
			this.dateFrom.TabIndex = 7;
			// 
			// dateTo
			// 
			this.dateTo.Location = new System.Drawing.Point(238, 87);
			this.dateTo.Name = "dateTo";
			this.dateTo.Size = new System.Drawing.Size(58, 20);
			this.dateTo.TabIndex = 8;
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(10, 114);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(75, 23);
			this.button4.TabIndex = 9;
			this.button4.Text = "GetLoans";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// loanAppId
			// 
			this.loanAppId.Location = new System.Drawing.Point(80, 141);
			this.loanAppId.Name = "loanAppId";
			this.loanAppId.Size = new System.Drawing.Size(100, 20);
			this.loanAppId.TabIndex = 10;
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(91, 114);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(75, 23);
			this.button5.TabIndex = 11;
			this.button5.Text = "GetLoan";
			this.button5.UseVisualStyleBackColor = true;
			this.button5.Click += new System.EventHandler(this.button5_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(10, 144);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 13);
			this.label2.TabIndex = 12;
			this.label2.Text = "Loan AppID";
			// 
			// AddContactButton
			// 
			this.AddContactButton.Location = new System.Drawing.Point(336, 69);
			this.AddContactButton.Name = "AddContactButton";
			this.AddContactButton.Size = new System.Drawing.Size(90, 23);
			this.AddContactButton.TabIndex = 13;
			this.AddContactButton.Text = "AddContact";
			this.AddContactButton.UseVisualStyleBackColor = true;
			this.AddContactButton.Click += new System.EventHandler(this.AddContactButton_Click);
			// 
			// UpdateContactButton
			// 
			this.UpdateContactButton.Location = new System.Drawing.Point(336, 98);
			this.UpdateContactButton.Name = "UpdateContactButton";
			this.UpdateContactButton.Size = new System.Drawing.Size(90, 23);
			this.UpdateContactButton.TabIndex = 13;
			this.UpdateContactButton.Text = "UpdateContact";
			this.UpdateContactButton.UseVisualStyleBackColor = true;
			this.UpdateContactButton.Click += new System.EventHandler(this.UpdateContactButton_Click);
			// 
			// DeleteContactButton
			// 
			this.DeleteContactButton.Location = new System.Drawing.Point(336, 127);
			this.DeleteContactButton.Name = "DeleteContactButton";
			this.DeleteContactButton.Size = new System.Drawing.Size(90, 23);
			this.DeleteContactButton.TabIndex = 14;
			this.DeleteContactButton.Text = "DeleteContact";
			this.DeleteContactButton.UseVisualStyleBackColor = true;
			this.DeleteContactButton.Click += new System.EventHandler(this.DeleteContactButton_Click);
			// 
			// getContactButton
			// 
			this.getContactButton.Location = new System.Drawing.Point(336, 40);
			this.getContactButton.Name = "getContactButton";
			this.getContactButton.Size = new System.Drawing.Size(90, 23);
			this.getContactButton.TabIndex = 14;
			this.getContactButton.Text = "GetContact";
			this.getContactButton.UseVisualStyleBackColor = true;
			this.getContactButton.Click += new System.EventHandler(this.getContactButton_Click);
			// 
			// ContactIdTextBox
			// 
			this.ContactIdTextBox.Location = new System.Drawing.Point(80, 22);
			this.ContactIdTextBox.Name = "ContactIdTextBox";
			this.ContactIdTextBox.Size = new System.Drawing.Size(58, 20);
			this.ContactIdTextBox.TabIndex = 15;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.button6);
			this.groupBox1.Controls.Add(this.SaveUtilityPaymentButton);
			this.groupBox1.Controls.Add(this.ContactAccountsDataGridView);
			this.groupBox1.Controls.Add(this.DeleteContactAccountButton);
			this.groupBox1.Controls.Add(this.UpdateContactAccountButton);
			this.groupBox1.Controls.Add(this.AddContactAccountbutton);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.ContactId);
			this.groupBox1.Controls.Add(this.DescriptionTextBox);
			this.groupBox1.Controls.Add(this.ContactIdTextBox);
			this.groupBox1.Controls.Add(this.GetContactsButton);
			this.groupBox1.Controls.Add(this.getContactButton);
			this.groupBox1.Controls.Add(this.DeleteContactButton);
			this.groupBox1.Controls.Add(this.AddContactButton);
			this.groupBox1.Controls.Add(this.UpdateContactButton);
			this.groupBox1.Location = new System.Drawing.Point(13, 187);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(499, 304);
			this.groupBox1.TabIndex = 16;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "groupBox1";
			// 
			// SaveUtilityPaymentButton
			// 
			this.SaveUtilityPaymentButton.Location = new System.Drawing.Point(336, 241);
			this.SaveUtilityPaymentButton.Name = "SaveUtilityPaymentButton";
			this.SaveUtilityPaymentButton.Size = new System.Drawing.Size(115, 29);
			this.SaveUtilityPaymentButton.TabIndex = 17;
			this.SaveUtilityPaymentButton.Text = "SaveUtility Payment";
			this.SaveUtilityPaymentButton.UseVisualStyleBackColor = true;
			this.SaveUtilityPaymentButton.Click += new System.EventHandler(this.SaveUtilityPaymentButton_Click);
			// 
			// ContactAccountsDataGridView
			// 
			this.ContactAccountsDataGridView.AllowUserToAddRows = false;
			this.ContactAccountsDataGridView.AllowUserToDeleteRows = false;
			this.ContactAccountsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.ContactAccountsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Id,
            this.AccountNumber,
            this.Description});
			this.ContactAccountsDataGridView.Location = new System.Drawing.Point(17, 107);
			this.ContactAccountsDataGridView.Name = "ContactAccountsDataGridView";
			this.ContactAccountsDataGridView.RowHeadersVisible = false;
			this.ContactAccountsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.ContactAccountsDataGridView.Size = new System.Drawing.Size(284, 150);
			this.ContactAccountsDataGridView.TabIndex = 18;
			// 
			// Id
			// 
			this.Id.HeaderText = "ID";
			this.Id.Name = "Id";
			this.Id.Width = 40;
			// 
			// AccountNumber
			// 
			this.AccountNumber.HeaderText = "AccountNumber";
			this.AccountNumber.Name = "AccountNumber";
			this.AccountNumber.Width = 120;
			// 
			// Description
			// 
			this.Description.HeaderText = "Description";
			this.Description.Name = "Description";
			this.Description.Width = 120;
			// 
			// DeleteContactAccountButton
			// 
			this.DeleteContactAccountButton.Location = new System.Drawing.Point(336, 214);
			this.DeleteContactAccountButton.Name = "DeleteContactAccountButton";
			this.DeleteContactAccountButton.Size = new System.Drawing.Size(129, 23);
			this.DeleteContactAccountButton.TabIndex = 17;
			this.DeleteContactAccountButton.Text = "DeleteContactAccountButton";
			this.DeleteContactAccountButton.UseVisualStyleBackColor = true;
			this.DeleteContactAccountButton.Click += new System.EventHandler(this.DeleteContactAccountButton_Click);
			// 
			// UpdateContactAccountButton
			// 
			this.UpdateContactAccountButton.Location = new System.Drawing.Point(336, 185);
			this.UpdateContactAccountButton.Name = "UpdateContactAccountButton";
			this.UpdateContactAccountButton.Size = new System.Drawing.Size(129, 23);
			this.UpdateContactAccountButton.TabIndex = 17;
			this.UpdateContactAccountButton.Text = "UpdateContactAccountButton";
			this.UpdateContactAccountButton.UseVisualStyleBackColor = true;
			this.UpdateContactAccountButton.Click += new System.EventHandler(this.UpdateContactAccountButton_Click);
			// 
			// AddContactAccountbutton
			// 
			this.AddContactAccountbutton.Location = new System.Drawing.Point(336, 156);
			this.AddContactAccountbutton.Name = "AddContactAccountbutton";
			this.AddContactAccountbutton.Size = new System.Drawing.Size(129, 23);
			this.AddContactAccountbutton.TabIndex = 17;
			this.AddContactAccountbutton.Text = "AddContactAccount";
			this.AddContactAccountbutton.UseVisualStyleBackColor = true;
			this.AddContactAccountbutton.Click += new System.EventHandler(this.AddContactAccountbutton_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(14, 56);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(60, 13);
			this.label3.TabIndex = 16;
			this.label3.Text = "Description";
			// 
			// ContactId
			// 
			this.ContactId.AutoSize = true;
			this.ContactId.Location = new System.Drawing.Point(14, 27);
			this.ContactId.Name = "ContactId";
			this.ContactId.Size = new System.Drawing.Size(18, 13);
			this.ContactId.TabIndex = 16;
			this.ContactId.Text = "ID";
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Location = new System.Drawing.Point(80, 53);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = new System.Drawing.Size(178, 20);
			this.DescriptionTextBox.TabIndex = 15;
			// 
			// GetContactsButton
			// 
			this.GetContactsButton.Location = new System.Drawing.Point(336, 11);
			this.GetContactsButton.Name = "GetContactsButton";
			this.GetContactsButton.Size = new System.Drawing.Size(90, 23);
			this.GetContactsButton.TabIndex = 14;
			this.GetContactsButton.Text = "GetContacts";
			this.GetContactsButton.UseVisualStyleBackColor = true;
			this.GetContactsButton.Click += new System.EventHandler(this.GetContactsButton_Click);
			// 
			// button6
			// 
			this.button6.Location = new System.Drawing.Point(190, 273);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(75, 23);
			this.button6.TabIndex = 19;
			this.button6.Text = "button6";
			this.button6.UseVisualStyleBackColor = true;
			this.button6.Click += new System.EventHandler(this.button6_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(524, 504);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.button5);
			this.Controls.Add(this.loanAppId);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.dateTo);
			this.Controls.Add(this.dateFrom);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.cardNumber);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.accountNumberTextBox);
			this.Controls.Add(this.getAccountButton);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContactAccountsDataGridView)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button getAccountButton;
        private System.Windows.Forms.TextBox accountNumberTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox cardNumber;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox dateFrom;
        private System.Windows.Forms.TextBox dateTo;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox loanAppId;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button AddContactButton;
		private System.Windows.Forms.Button UpdateContactButton;
		private System.Windows.Forms.Button DeleteContactButton;
		private System.Windows.Forms.Button getContactButton;
		private System.Windows.Forms.TextBox ContactIdTextBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label ContactId;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox DescriptionTextBox;
		private System.Windows.Forms.Button DeleteContactAccountButton;
		private System.Windows.Forms.Button UpdateContactAccountButton;
		private System.Windows.Forms.Button AddContactAccountbutton;
		private System.Windows.Forms.DataGridView ContactAccountsDataGridView;
		private System.Windows.Forms.DataGridViewTextBoxColumn Id;
		private System.Windows.Forms.DataGridViewTextBoxColumn AccountNumber;
		private System.Windows.Forms.DataGridViewTextBoxColumn Description;
		private System.Windows.Forms.Button GetContactsButton;
		private System.Windows.Forms.Button SaveUtilityPaymentButton;
		private System.Windows.Forms.Button button6;
	}
}

