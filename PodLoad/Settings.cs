using System.Xml.Serialization;
using System.Collections.Generic;

[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "xml")]
public partial class Settings
{

    private List<xmlFeed> itemsField;
    [System.Xml.Serialization.XmlElementAttribute("feed", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public List<xmlFeed> Items { get { return itemsField; } set { itemsField = value; } }
}




[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class xmlFeed
{
    private List<xmlFeedDownload> downloadField;
    private string idField;
    private string pathField;
    //private string xpathField;

    [System.Xml.Serialization.XmlElementAttribute("download", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public List<xmlFeedDownload> download { get { return this.downloadField; } set { this.downloadField = value; } }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string id { get { return this.idField; } set { this.idField = value; } }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string path { get { return this.pathField; } set { this.pathField = value; } }

}

[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class xmlFeedDownload
{
    private string pathField;
    private string downloadedField;
    private string uidField;

    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string path { get { return this.pathField; } set { this.pathField = value; } }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string downloaded { get { return this.downloadedField; } set { this.downloadedField = value; } }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string uid { get { return this.uidField; } set { this.uidField = value; } }
}
