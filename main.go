package main

import (
	"fmt"
	"os"
)

func main() {
	xmlfile := "podload.xml"
	zinfile := "podload.zin"

	args := os.Args[1:]
	if len(args) == 0 {
		run(zinfile)
	} else {
		checkConditions(zinfile, xmlfile, args)
	}
}

func checkConditions(zinfilename, xmlfilename string, args []string) {
	setting := LoadGzip(zinfilename)
	switch args[0] {
	case "export":
		SaveXML(LoadGzip(zinfilename), xmlfilename)
	case "import":
		SaveGzip(LoadXML(xmlfilename), zinfilename)
	case "add":
		addFeed(setting, args[1], args[2], zinfilename)
	case "remove":
		removeFeed(setting, args[1], zinfilename)
	case "list":
		displayList(setting)
	default:
		displayHelp()
	}
}

func run(file string) {
	setting := LoadGzip(file)
	setting = GetLatest(setting)
	SaveGzip(setting, file)
	setting = Download(setting)
	SaveGzip(setting, file)
}

func addFeed(setting *Settings, id, path, file string) {
	newfeed := XmlFeed{Download: []XmlFeedDownload{}, Id: id, Path: path}
	setting.Items = append(setting.Items, newfeed)
	SaveGzip(setting, file)
}

func removeFeed(setting *Settings, feedid, file string) {
	for i, feed := range setting.Items {
		if feed.Id == feedid {
			setting.Items = append(setting.Items[:i], setting.Items[i+1:]...)
			break
		}
	}
	SaveGzip(setting, file)
}

func displayList(setting *Settings) {
	for _, feed := range setting.Items {
		fmt.Printf("%s %s\n", feed.Id, feed.Path)
	}
}

func displayHelp() {
	// Display help information
	fmt.Println(`
______________________________________________________________________________________________
Welcome to Podload Help:

Here is a list of valid commands you can execute under podload...
- export : takes the zin file and converts it to xml
- import : takes the xml file and converts it to a zin file
- add    : Adds a new feed to the list, for example ' add NewFeedName http://Newfeed/atom.xml'
- remove : Removes a feed from the list, example 'remove NewFeedName'
- list   : displays the names and urls of the current feeds
	`)
}

