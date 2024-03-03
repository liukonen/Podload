package main

import (
	"compress/flate"
	"encoding/xml"
	"fmt"
	"os"
)

// LoadXML loads settings from XML file
func LoadXML(fileName string) *Settings {
	f, err := os.Open(fileName)
	if err != nil {
		fmt.Print((err))
		return nil
	}
	defer f.Close()

	var settings Settings
	if err := xml.NewDecoder(f).Decode(&settings); err != nil {
		fmt.Print((err))
		return nil
	}
	return &settings
}

// LoadGzip loads compressed settings file
func LoadGzip(fileName string) *Settings {
	f, err := os.Open(fileName)
	if err != nil {
		fmt.Print(err)
		return nil
	}
	defer f.Close()

	gz := flate.NewReader(f)
    defer gz.Close()

	var settings Settings
	if err := xml.NewDecoder(gz).Decode(&settings); err != nil {
		fmt.Print(err)
		return nil
	}

	return &settings
}

// SaveXML saves settings as XML
func SaveXML(settings *Settings, fileName string) error {
	f, err := os.Create(fileName)
	if err != nil {
		return err
	}
	defer f.Close()

	return xml.NewEncoder(f).Encode(settings)
}

// SaveGzip saves compressed settings
func SaveGzip(settings *Settings, fileName string) error {
	f, err := os.Create(fileName)
	if err != nil {
		return err
	}
	defer f.Close()

	gz, _ := flate.NewWriter(f, 9)
	defer gz.Close()
	return xml.NewEncoder(gz).Encode(settings)
}
