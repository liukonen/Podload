package main

import (
	"fmt"
	"io"
	"net/http"
	"os"
	"strings"
	"sync"

	"github.com/antchfx/xmlquery"
	"golang.org/x/net/html/charset"
)

// GetLatest downloads latest episodes
func GetLatest(settings *Settings) *Settings {
	for i, feed := range settings.Items {
		latest := getLatestItem(&feed)
		settings.Items[i] = *latest
	}
	return settings

}

func getLatestItem(feed *XmlFeed) *XmlFeed {
	fmt.Println("fetching values from ", feed.Path)
	doc, err := xmlquery.LoadURL(feed.Path)
	if err != nil {
		fmt.Printf("Error loading %s: %v", feed.Path, err)
		return feed
	}

	nodes := xmlquery.Find(doc, "//item/enclosure/@url")

	//use map to increase lookup speed from O(n) to O(1)
	existing := makeMap(feed.Download)

	for _, node := range nodes {
		url := node.InnerText()

		download := XmlFeedDownload{url, "False", generateID(url)}
		if IsDistinct(existing, download.Uid) {
			feed.Download = append(feed.Download, download)
		}
	}
	return feed
}

func makeMap(existing []XmlFeedDownload) map[string]struct{} {
	uidMap := make(map[string]struct{})

	for _, x := range existing {
		uidMap[strings.ToLower(x.Uid)] = struct{}{}
	}
	return uidMap

}

func IsDistinct(existing map[string]struct{}, testObject string) bool {
	_, exists := existing[strings.ToLower(testObject)]
	return !exists
}

func generateID(request string) string {
	lastIndex := strings.LastIndex(request, "/") + 1
	substring := request[lastIndex:]
	return strings.ReplaceAll(substring, "%", " ")
}

// Download downloads episodes (limited to 3 concurrent downloads using a channel)
func Download(settings *Settings) *Settings {
	client := &http.Client{}

	// Create a channel to control the number of concurrent downloads
	concurrency := make(chan struct{}, 3)
	var mutex sync.Mutex // Mutex for synchronization

	for i := range settings.Items {
		for j := range settings.Items[i].Download {
			d := &settings.Items[i].Download[j]
			if d.Downloaded == "True" {
				continue // Skip if already downloaded
			}
			// Start a new goroutine for each download
			concurrency <- struct{}{} // Add to the channel to limit concurrency
			go func(d *XmlFeedDownload) {
				defer func() { <-concurrency }() // Remove from the channel when done

				// download file
				fmt.Println("downloading ", d.Path)
				resp, err := client.Get(d.Path)
				if err != nil {
					fmt.Printf("Error downloading %s: What %v", d.Path, err)
					return
				}
				defer resp.Body.Close()
				if resp.StatusCode != 200 {
					fmt.Printf("Error downloading %s: What %v", d.Path, resp.StatusCode)
				}

				// detect charset
				reader, _ := charset.NewReader(resp.Body, resp.Header.Get("Content-Type"))

				// save file with unique name
				out, err := os.Create(d.Uid)
				if err != nil {
					fmt.Printf("Error creating %s: %v", d.Uid, err)
					fmt.Println("")
					return
				}

				// write downloaded file
				_, err = io.Copy(out, reader)
				fmt.Println("saving ", d.Uid)
				if err != nil {
					fmt.Printf("Error writing %s: %v", d.Uid, err)
				}

				out.Close()

				// Use mutex to protect concurrent access to the settings slice
				mutex.Lock()
				d.Downloaded = "True" 
				mutex.Unlock()
			}(d) 
		}
	}

	// Wait for all downloads to finish
	for i := 0; i < cap(concurrency); i++ {
		concurrency <- struct{}{}
	}

	return settings
}