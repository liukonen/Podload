using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace PodLoad
{

    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "xml")]
    public partial class Settings
    {
        [XmlElement("feed", Form = XmlSchemaForm.Unqualified)]
        public List<XmlFeed> Items { get; set; }
    }

    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class XmlFeed
    {
        [XmlElement("download", Form = XmlSchemaForm.Unqualified)]
        public List<XmlFeedDownload> Download { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("path")]
        public string Path { get; set; }

    }

    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class XmlFeedDownload
    {
        [XmlAttribute("path")]
        public string Path { get; set; }

        [XmlAttribute("downloaded")]
        public CustomBoolean Downloaded { get; set; }

        [XmlAttribute("uid")]
        public string Uid { get; set; }
    }

    [DataContract]
    public enum CustomBoolean
    {
        [EnumMember]
        False = 0,

        [EnumMember]
        True = 1
    }
}