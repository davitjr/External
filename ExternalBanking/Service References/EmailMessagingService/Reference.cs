﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExternalBanking.EmailMessagingService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="EmailNotification", Namespace="http://schemas.datacontract.org/2004/07/MessagingLibrary.Services.EmailMessagingS" +
        "ervice.Model")]
    [System.SerializableAttribute()]
    public partial class EmailNotification : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Collections.Generic.List<ExternalBanking.EmailMessagingService.notification> notificationListField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.Generic.List<ExternalBanking.EmailMessagingService.notification> notificationList {
            get {
                return this.notificationListField;
            }
            set {
                if ((object.ReferenceEquals(this.notificationListField, value) != true)) {
                    this.notificationListField = value;
                    this.RaisePropertyChanged("notificationList");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="notification", Namespace="http://schemas.datacontract.org/2004/07/MessagingLibrary.Services.EmailMessagingS" +
        "ervice.Model")]
    [System.SerializableAttribute()]
    public partial class notification : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Collections.Generic.List<ExternalBanking.EmailMessagingService.parameter> parametersField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private ExternalBanking.EmailMessagingService.properties propertiesField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.Generic.List<ExternalBanking.EmailMessagingService.parameter> parameters {
            get {
                return this.parametersField;
            }
            set {
                if ((object.ReferenceEquals(this.parametersField, value) != true)) {
                    this.parametersField = value;
                    this.RaisePropertyChanged("parameters");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public ExternalBanking.EmailMessagingService.properties properties {
            get {
                return this.propertiesField;
            }
            set {
                if ((object.ReferenceEquals(this.propertiesField, value) != true)) {
                    this.propertiesField = value;
                    this.RaisePropertyChanged("properties");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="properties", Namespace="http://schemas.datacontract.org/2004/07/MessagingLibrary.Services.EmailMessagingS" +
        "ervice.Model")]
    [System.SerializableAttribute()]
    public partial class properties : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string emailAddressField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string idField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string recipientUserIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string referenceIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string templateIDField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string emailAddress {
            get {
                return this.emailAddressField;
            }
            set {
                if ((object.ReferenceEquals(this.emailAddressField, value) != true)) {
                    this.emailAddressField = value;
                    this.RaisePropertyChanged("emailAddress");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                if ((object.ReferenceEquals(this.idField, value) != true)) {
                    this.idField = value;
                    this.RaisePropertyChanged("id");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string recipientUserID {
            get {
                return this.recipientUserIDField;
            }
            set {
                if ((object.ReferenceEquals(this.recipientUserIDField, value) != true)) {
                    this.recipientUserIDField = value;
                    this.RaisePropertyChanged("recipientUserID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string referenceID {
            get {
                return this.referenceIDField;
            }
            set {
                if ((object.ReferenceEquals(this.referenceIDField, value) != true)) {
                    this.referenceIDField = value;
                    this.RaisePropertyChanged("referenceID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string templateID {
            get {
                return this.templateIDField;
            }
            set {
                if ((object.ReferenceEquals(this.templateIDField, value) != true)) {
                    this.templateIDField = value;
                    this.RaisePropertyChanged("templateID");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="parameter", Namespace="http://schemas.datacontract.org/2004/07/MessagingLibrary.Services.EmailMessagingS" +
        "ervice.Model")]
    [System.SerializableAttribute()]
    public partial class parameter : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string keyField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string locationField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string valueField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string key {
            get {
                return this.keyField;
            }
            set {
                if ((object.ReferenceEquals(this.keyField, value) != true)) {
                    this.keyField = value;
                    this.RaisePropertyChanged("key");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string location {
            get {
                return this.locationField;
            }
            set {
                if ((object.ReferenceEquals(this.locationField, value) != true)) {
                    this.locationField = value;
                    this.RaisePropertyChanged("location");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string value {
            get {
                return this.valueField;
            }
            set {
                if ((object.ReferenceEquals(this.valueField, value) != true)) {
                    this.valueField = value;
                    this.RaisePropertyChanged("value");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ActionResult", Namespace="http://schemas.datacontract.org/2004/07/MessagingService.Common")]
    [System.SerializableAttribute()]
    public partial class ActionResult : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private ExternalBanking.EmailMessagingService.EnumerationsResultCode ActionResultCodeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Collections.Generic.List<string> DescriptionsField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public ExternalBanking.EmailMessagingService.EnumerationsResultCode ActionResultCode {
            get {
                return this.ActionResultCodeField;
            }
            set {
                if ((this.ActionResultCodeField.Equals(value) != true)) {
                    this.ActionResultCodeField = value;
                    this.RaisePropertyChanged("ActionResultCode");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.Generic.List<string> Descriptions {
            get {
                return this.DescriptionsField;
            }
            set {
                if ((object.ReferenceEquals(this.DescriptionsField, value) != true)) {
                    this.DescriptionsField = value;
                    this.RaisePropertyChanged("Descriptions");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Enumerations.ResultCode", Namespace="http://schemas.datacontract.org/2004/07/MessagingService.Common")]
    public enum EnumerationsResultCode : short {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Normal = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Failed = 2,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Email", Namespace="http://schemas.datacontract.org/2004/07/MessagingLibrary.Services.EmailMessagingS" +
        "ervice.Model")]
    [System.SerializableAttribute()]
    public partial class Email : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Collections.Generic.List<ExternalBanking.EmailMessagingService.MailAttachment> AttachmentsField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ContentField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int FromField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string SubjectField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ToField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.Generic.List<ExternalBanking.EmailMessagingService.MailAttachment> Attachments {
            get {
                return this.AttachmentsField;
            }
            set {
                if ((object.ReferenceEquals(this.AttachmentsField, value) != true)) {
                    this.AttachmentsField = value;
                    this.RaisePropertyChanged("Attachments");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Content {
            get {
                return this.ContentField;
            }
            set {
                if ((object.ReferenceEquals(this.ContentField, value) != true)) {
                    this.ContentField = value;
                    this.RaisePropertyChanged("Content");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int From {
            get {
                return this.FromField;
            }
            set {
                if ((this.FromField.Equals(value) != true)) {
                    this.FromField = value;
                    this.RaisePropertyChanged("From");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Subject {
            get {
                return this.SubjectField;
            }
            set {
                if ((object.ReferenceEquals(this.SubjectField, value) != true)) {
                    this.SubjectField = value;
                    this.RaisePropertyChanged("Subject");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string To {
            get {
                return this.ToField;
            }
            set {
                if ((object.ReferenceEquals(this.ToField, value) != true)) {
                    this.ToField = value;
                    this.RaisePropertyChanged("To");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="MailAttachment", Namespace="http://schemas.datacontract.org/2004/07/MessagingLibrary.Services.EmailMessagingS" +
        "ervice.Model")]
    [System.SerializableAttribute()]
    public partial class MailAttachment : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private byte[] DataBinaryField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FileExtensionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FileNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public byte[] DataBinary {
            get {
                return this.DataBinaryField;
            }
            set {
                if ((object.ReferenceEquals(this.DataBinaryField, value) != true)) {
                    this.DataBinaryField = value;
                    this.RaisePropertyChanged("DataBinary");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FileExtension {
            get {
                return this.FileExtensionField;
            }
            set {
                if ((object.ReferenceEquals(this.FileExtensionField, value) != true)) {
                    this.FileExtensionField = value;
                    this.RaisePropertyChanged("FileExtension");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FileName {
            get {
                return this.FileNameField;
            }
            set {
                if ((object.ReferenceEquals(this.FileNameField, value) != true)) {
                    this.FileNameField = value;
                    this.RaisePropertyChanged("FileName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="EmailMessagingService.IEmailMessagingService")]
    public interface IEmailMessagingService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IEmailMessagingService/SendEmailNotificationByTemplate", ReplyAction="http://tempuri.org/IEmailMessagingService/SendEmailNotificationByTemplateResponse" +
            "")]
        ExternalBanking.EmailMessagingService.ActionResult SendEmailNotificationByTemplate(ExternalBanking.EmailMessagingService.EmailNotification emailNotification, int senderProfileId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IEmailMessagingService/SendEmailNotificationByTemplate", ReplyAction="http://tempuri.org/IEmailMessagingService/SendEmailNotificationByTemplateResponse" +
            "")]
        System.Threading.Tasks.Task<ExternalBanking.EmailMessagingService.ActionResult> SendEmailNotificationByTemplateAsync(ExternalBanking.EmailMessagingService.EmailNotification emailNotification, int senderProfileId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IEmailMessagingService/SendEmailNotification", ReplyAction="http://tempuri.org/IEmailMessagingService/SendEmailNotificationResponse")]
        ExternalBanking.EmailMessagingService.ActionResult SendEmailNotification(ExternalBanking.EmailMessagingService.Email email);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IEmailMessagingService/SendEmailNotification", ReplyAction="http://tempuri.org/IEmailMessagingService/SendEmailNotificationResponse")]
        System.Threading.Tasks.Task<ExternalBanking.EmailMessagingService.ActionResult> SendEmailNotificationAsync(ExternalBanking.EmailMessagingService.Email email);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IEmailMessagingServiceChannel : ExternalBanking.EmailMessagingService.IEmailMessagingService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class EmailMessagingServiceClient : System.ServiceModel.ClientBase<ExternalBanking.EmailMessagingService.IEmailMessagingService>, ExternalBanking.EmailMessagingService.IEmailMessagingService {
        
        public EmailMessagingServiceClient() {
        }
        
        public EmailMessagingServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public EmailMessagingServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public EmailMessagingServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public EmailMessagingServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public ExternalBanking.EmailMessagingService.ActionResult SendEmailNotificationByTemplate(ExternalBanking.EmailMessagingService.EmailNotification emailNotification, int senderProfileId) {
            return base.Channel.SendEmailNotificationByTemplate(emailNotification, senderProfileId);
        }
        
        public System.Threading.Tasks.Task<ExternalBanking.EmailMessagingService.ActionResult> SendEmailNotificationByTemplateAsync(ExternalBanking.EmailMessagingService.EmailNotification emailNotification, int senderProfileId) {
            return base.Channel.SendEmailNotificationByTemplateAsync(emailNotification, senderProfileId);
        }
        
        public ExternalBanking.EmailMessagingService.ActionResult SendEmailNotification(ExternalBanking.EmailMessagingService.Email email) {
            return base.Channel.SendEmailNotification(email);
        }
        
        public System.Threading.Tasks.Task<ExternalBanking.EmailMessagingService.ActionResult> SendEmailNotificationAsync(ExternalBanking.EmailMessagingService.Email email) {
            return base.Channel.SendEmailNotificationAsync(email);
        }
    }
}