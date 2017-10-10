# Podload
This is a command line based tool, that pulls from a compressed configuration RSS feed info used to download podcasts. 
Works with both Mp3 and video based feeds.
This project dates back to 2008 or 2009 in Visual basic, with a rewrite for performance and smaller EXE size in 2010 in C#. 
Since then small changes such as extra tools from new frameworks, (Linq, Tasks, parallel) has been added.
This was created in a need to have my media center autodownload RSS based feeds such as revision 3 videos and podcasts.
It uses a "ZIN" file, which is nothing more then a compressed XML file. 
(should have named it a GZ file, or something else, but whatever)  
My personal use for this app involves having a directory in my "My Videos" folder, and having a scheduled task once a week 
execute the app. 
The app pulls the xml feed, extracts the URL's to download, and then downloads the files in feed directories.   
I wanted however to implement some kind of version system, which is why I added this to <strike>codeplex</strike> Github. 
I'm hoping this app can either help someone else with the task of autodownloading podcasts, or with the help of the Open 
source community, it can become something greater. 

I have confirmed as well that this application, when compiled using mono, works well on Linux systems. I have deployed it
on my Linux mint machine, and using a Cron job, I have it scheduled to continue downloading my rss feeds since my move off
windows for my home server. There are a few changes that need to be made, in order to make it work, which I will one day (soon)
move up into my source control (once I find them :/)
