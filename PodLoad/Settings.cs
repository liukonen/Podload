using System.Xml.Serialization;
using System.Collections.Generic;

[System.Serializable()]
[System.Diagnostics.DebuggerStepThrough()]
[System.ComponentModel.DesignerCategory("code")]
[XmlType(AnonymousType = true)]
[XmlRoot(Namespace = "", IsNullable = false, ElementName = "xml")]
public partial class Settings
{
    [XmlElement("feed", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public List<XmlFeed> Items { get; set; }
}




[System.Serializable()]
[System.Diagnostics.DebuggerStepThrough()]
[System.ComponentModel.DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public partial class XmlFeed
{
    [XmlElement("download", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public List<XmlFeedDownload> Download { get; set; }

    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlAttribute("path")]
    public string Path { get; set; }

}

[System.Serializable()]
[System.Diagnostics.DebuggerStepThrough()]
[System.ComponentModel.DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public partial class XmlFeedDownload
{
    [XmlAttribute("path")]
    public string Path { get; set; }

    [XmlAttribute("downloaded")]
    public string Downloaded { get; set; }

    [XmlAttribute("uid")]
    public string Uid { get; set; }
}
