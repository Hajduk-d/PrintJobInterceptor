

# Implementation

### Monitoring and management
Print job monitoring is done via WMI events. I chose WMI as there is more information online about it, 
a lot of stackoverflow posts and blog posts.

There is a central service that does all the monitoring, so there is only one point in code that does the creation and 
updates of objects with WMI.
The code for monitoring is split up in a printjob monitor for all the printjobs and a printer monitor for printers.
Both of these extract data from WMI events and pass it to the service. The service, in turn, creates or updates the
corresponding objects.

printjobs and printers can pause, resume and cancel directly by calling invoking methods on the wmi events

### Document Grouping Logic
Not entirely sure how grouping of documents should work, as i could not get a single document to start 
multiple print jobs. 

Print jobs are assigned to a document first by checking for name and if the prin job is in a certain timeframe,
then by checking for the owner of the print job and document.

This was tested by printing multiple test pages in a short span of time, assuming that one document creates multiple
print jobs it would make more sense to add checks for printed pages and states of print jobs.

### Logging

As the logging framework, I chose [NLog](https://nlog-project.org/) as I'm already familiar with it and like its 
customizability.

### Installer

To create the installer, I chose [Velopack](https://docs.velopack.io/) as it is straightforward to use and supports 
features like: delta updates, auto updates, easy CI integrations


### UI

For the mvvm framework I chose [ReactiveUI](https://www.reactiveui.net/).

I am already familiar with it, it has good documentation and is part of the dotnetfoundation.
Bindings, DI, Navigation as well as Task scheduling are handled through reactive ui.

The ui design is roughly based on [Micorosoft Fluent Design 2 guidelines](https://fluent2.microsoft.design/design-principles)

# Possible Improvements

### WMI events
Using wmi events, especially when using Temporary
event consumer as opposed to [Permanent](https://learn.microsoft.com/en-us/windows/win32/wmisdk/monitoring-events#using-temporary-event-consumers) consumers,
might cause some problems with receiving every event. This happens a lot when printing via Microsoft print to PDF.

### PrintTestPage method
This method is not using WMI events but directly the printui through the run dll.
This ensures that when using the Print to PDF (printer), the file save dialog pops up.
Using WMI events, this dialog never opens, so the process cannot finish.

### Print jobs & Printers
Print jobs and Printers directly call WMI methods, this could be done through the Printerservice similarly like 
updating print jobs but the other way around. A print job sends a request to the printerservice to pause/resume/cancel
itself, and the printerservice would do the rest. This makes sure that there is only one part of the code 
that handles all the WMI interactions. 

### Navigation Service
The navigation service registers every viewmodel for each print job, printer and document. This goes against 
the best practice of using a DI for ui. The problem is that because all the viewmodels reference each other,  
this creates circular dependencies when creating one of the viewmodels. This is also why the FinishedInit signal exists 
in the PrintJob class. To ensure that all the references are set before passing it to the ui. 

### Sidebar
The sidebar implementation is very basic, for a more complex application the navbar should automatically get its items