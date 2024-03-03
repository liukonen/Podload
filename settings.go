package main

import "encoding/xml"

type Settings struct {
	XMLName xml.Name  `xml:"xml"`
	Items   []XmlFeed `xml:"feed"`
}

type XmlFeed struct {
	Id       string            `xml:"id,attr"`
	Path     string            `xml:"path,attr"`
	Download []XmlFeedDownload `xml:"download"`
}

type XmlFeedDownload struct {
	Path       string `xml:"path,attr"`
	Downloaded string `xml:"downloaded,attr"`
	Uid        string `xml:"uid,attr"`
}
