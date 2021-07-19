﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExternalBanking.PensionSystemRef {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PersonData", Namespace="http://schemas.datacontract.org/2004/07/PensionSystem")]
    [System.SerializableAttribute()]
    public partial class PersonData : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime BirthDateField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FirstNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string LastNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private long PSNField;
        
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
        public System.DateTime BirthDate {
            get {
                return this.BirthDateField;
            }
            set {
                if ((this.BirthDateField.Equals(value) != true)) {
                    this.BirthDateField = value;
                    this.RaisePropertyChanged("BirthDate");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FirstName {
            get {
                return this.FirstNameField;
            }
            set {
                if ((object.ReferenceEquals(this.FirstNameField, value) != true)) {
                    this.FirstNameField = value;
                    this.RaisePropertyChanged("FirstName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LastName {
            get {
                return this.LastNameField;
            }
            set {
                if ((object.ReferenceEquals(this.LastNameField, value) != true)) {
                    this.LastNameField = value;
                    this.RaisePropertyChanged("LastName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public long PSN {
            get {
                return this.PSNField;
            }
            set {
                if ((this.PSNField.Equals(value) != true)) {
                    this.PSNField = value;
                    this.RaisePropertyChanged("PSN");
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
    [System.Runtime.Serialization.DataContractAttribute(Name="Result", Namespace="http://schemas.datacontract.org/2004/07/PensionSystem")]
    [System.SerializableAttribute()]
    public partial class Result : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DescriptionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private ExternalBanking.PensionSystemRef.ResultCode ResultCodeField;
        
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
        public string Description {
            get {
                return this.DescriptionField;
            }
            set {
                if ((object.ReferenceEquals(this.DescriptionField, value) != true)) {
                    this.DescriptionField = value;
                    this.RaisePropertyChanged("Description");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public ExternalBanking.PensionSystemRef.ResultCode ResultCode {
            get {
                return this.ResultCodeField;
            }
            set {
                if ((this.ResultCodeField.Equals(value) != true)) {
                    this.ResultCodeField = value;
                    this.RaisePropertyChanged("ResultCode");
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
    [System.Runtime.Serialization.DataContractAttribute(Name="ResultCode", Namespace="http://schemas.datacontract.org/2004/07/PensionSystem")]
    public enum ResultCode : ushort {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Succeeded = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        NoActivePersonAccount = 101,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        InvalidLastName = 102,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        InvalidFirstName = 103,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        InvalidBirthDate = 104,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        AccessDenied = 105,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        InvalidSignature = 106,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        UnspecifiedError = 300,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="PensionSystemRef.IPensionSystemService")]
    public interface IPensionSystemService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPensionSystemService/GetBalance", ReplyAction="http://tempuri.org/IPensionSystemService/GetBalanceResponse")]
        System.ValueTuple<decimal, ExternalBanking.PensionSystemRef.Result> GetBalance(ExternalBanking.PensionSystemRef.PersonData personData);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPensionSystemService/GetBalance", ReplyAction="http://tempuri.org/IPensionSystemService/GetBalanceResponse")]
        System.Threading.Tasks.Task<System.ValueTuple<decimal, ExternalBanking.PensionSystemRef.Result>> GetBalanceAsync(ExternalBanking.PensionSystemRef.PersonData personData);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IPensionSystemServiceChannel : ExternalBanking.PensionSystemRef.IPensionSystemService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class PensionSystemServiceClient : System.ServiceModel.ClientBase<ExternalBanking.PensionSystemRef.IPensionSystemService>, ExternalBanking.PensionSystemRef.IPensionSystemService {
        
        public PensionSystemServiceClient() {
        }
        
        public PensionSystemServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public PensionSystemServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PensionSystemServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PensionSystemServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.ValueTuple<decimal, ExternalBanking.PensionSystemRef.Result> GetBalance(ExternalBanking.PensionSystemRef.PersonData personData) {
            return base.Channel.GetBalance(personData);
        }
        
        public System.Threading.Tasks.Task<System.ValueTuple<decimal, ExternalBanking.PensionSystemRef.Result>> GetBalanceAsync(ExternalBanking.PensionSystemRef.PersonData personData) {
            return base.Channel.GetBalanceAsync(personData);
        }
    }
}
