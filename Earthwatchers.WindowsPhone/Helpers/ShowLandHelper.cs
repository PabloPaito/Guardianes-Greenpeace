﻿//------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by a tool.
// Runtime Version:2.0.50727.3082
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System.Xml.Serialization;

//
// This source code was auto-generated by xsd, Version=2.0.50727.3038.
//

namespace Earthwatchers.WindowsPhone.Helpers
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    //[System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Land
    {
        private LandEntityKey entityKeyField;

        private int idField;

        private string geohexKeyField;

        private int landTypeField;

        private string earthwatcherGuidField;

        private int landThreatField;

        /// <remarks/>
        public LandEntityKey EntityKey
        {
            get
            {
                return this.entityKeyField;
            }
            set
            {
                this.entityKeyField = value;
            }
        }

        /// <remarks/>
        public int Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public string GeohexKey
        {
            get
            {
                return this.geohexKeyField;
            }
            set
            {
                this.geohexKeyField = value;
            }
        }

        /// <remarks/>
        public int LandType
        {
            get
            {
                return this.landTypeField;
            }
            set
            {
                this.landTypeField = value;
            }
        }

        /// <remarks/>
        public string EarthwatcherGuid
        {
            get
            {
                return this.earthwatcherGuidField;
            }
            set
            {
                this.earthwatcherGuidField = value;
            }
        }

        /// <remarks/>
        public int LandThreat
        {
            get
            {
                return this.landThreatField;
            }
            set
            {
                this.landThreatField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    //[System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class LandEntityKey
    {

        private string entitySetNameField;

        private string entityContainerNameField;

        private LandEntityKeyEntityKeyValues entityKeyValuesField;

        /// <remarks/>
        public string EntitySetName
        {
            get
            {
                return this.entitySetNameField;
            }
            set
            {
                this.entitySetNameField = value;
            }
        }

        /// <remarks/>
        public string EntityContainerName
        {
            get
            {
                return this.entityContainerNameField;
            }
            set
            {
                this.entityContainerNameField = value;
            }
        }

        /// <remarks/>
        public LandEntityKeyEntityKeyValues EntityKeyValues
        {
            get
            {
                return this.entityKeyValuesField;
            }
            set
            {
                this.entityKeyValuesField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    //[System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class LandEntityKeyEntityKeyValues
    {

        private LandEntityKeyEntityKeyValuesEntityKeyMember entityKeyMemberField;

        /// <remarks/>
        public LandEntityKeyEntityKeyValuesEntityKeyMember EntityKeyMember
        {
            get
            {
                return this.entityKeyMemberField;
            }
            set
            {
                this.entityKeyMemberField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    //[System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class LandEntityKeyEntityKeyValuesEntityKeyMember
    {

        private string keyField;

        private int valueField;

        /// <remarks/>
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        public int Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
}