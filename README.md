HTTP log monitoring console program
Create a simple console program that monitors HTTP traffic on your machine:

Consume an actively written-to w3c-formatted HTTP access log (https://en.wikipedia.org/wiki/Common_Log_Format)
Every 10s, display in the console the sections of the web site with the most hits (a section is defined as being what's before the second '/' in a URL. i.e. the section for "http://my.site.com/pages/create' is "http://my.site.com/pages"), as well as interesting summary statistics on the traffic as a whole.
Make sure a user can keep the console app running and monitor traffic on their machine
Whenever total traffic for the past 2 minutes exceeds a certain number on average, add a message saying that “High traffic generated an alert - hits = {value}, triggered at {time}”
Whenever the total traffic drops again below that value on average for the past 2 minutes, add another message detailing when the alert recovered
Make sure all messages showing when alerting thresholds are crossed remain visible on the page for historical reasons.
Write a test for the alerting logic
Explain how you’d improve on this application design

The program will create a fake log at C:/Log.txt and add new random entries every 3 seconds. It'll display how many hits each section receives as well as the section with the most overall hits. It'll also record all alert messages whenever there are more than 38 hits on the website in the last two minutes, configured for testing purposes, and once the alert is triggered, it will also display a message saying that the alert has stopped. These messages are all logged and will always be displayed.

The application could be improved by allowing the user to specify where the log lives as well as whether or not the program should be in testing mode and whether or not it should be creating random messages. In addition, the alert time and threshold should be configurable and whether or not alert messages should be logged or how long they should be logged for.
