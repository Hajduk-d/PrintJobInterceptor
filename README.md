# PrintInterceptor - Technical Documentation

- **Framework**: .NET 9
- **UI Framework**: [ReactiveUI](https://www.reactiveui.net/docs/handbook/dependency-inversion/index.html) for MVVM pattern
- **Logging**: [NLog](https://nlog-project.org/) for configurable logging
- **Deployment**: [Velopack](https://docs.velopack.io/) for installation and updates
- **Monitoring**: Windows WMI events
- **Design**: [Microsoft Fluent Design 2 guidelines](https://fluent2.microsoft.design/)

## Implementation

### Print Monitoring System
The application uses WMI temporary event consumers to monitor print operations. The central service architecture ensures:
- Single point of WMI event handling
- Centralized object creation and updates

**Print Job Operations:**
- Pause/Resume/Cancel operations via direct WMI method invocation
- Real-time status updates through event subscription

### Document Grouping Logic
Not entirely sure how grouping of documents should work, as I could not get a single document to start 
multiple print jobs. 

Documents are associated with print jobs using these criteria:
1. **First**: Document name and time proximity to other printjobs for that Document
2. **Seccond**: Print job owner

This was tested by printing multiple test pages in a short span of time, assuming that one document creates multiple
print jobs it would make more sense to add checks for printed pages and states of print jobs.

## Known Limitations & Technical Debt

### WMI Event Reliability
Temporary event consumers may miss events, particularly with Microsoft Print to PDF.
implementing [permanent event consumers](https://learn.microsoft.com/en-us/windows/win32/wmisdk/monitoring-events#using-temporary-event-consumers) 
would fix this issue 

### Print Test Functionality
Print test pages bypass WMI events, using direct PrintUI calls
This ensures that when using the Print to PDF the file save dialog pops up.
Using WMI events, this dialog never opens, so the process cannot finish.

#### Direct WMI Method Calls
Print jobs and printers directly invoke WMI methods. 
A better approach would be to Route all WMI interactions 
through PrinterService to centralize WMI interaciton.

#### Navigation Service Design
Registers individual ViewModels for each entity (job/printer/document) this 
Violates DI best practices. This is a workaround 

because all the viewmodels reference each other,  
this creates circular dependencies. This is also why the `FinishedInit` signal exists,
to ensures proper initialization order

#### UI Component Limitations
The side Sidebar implementation is very basic and is not very scalable

## Note
The codebase includes a parallel implementation using the Windows Spooler API for print job and printer management. This was developed as a comparative to check data and functionality.
The WMI-based implementation is the primary approach, with the Spooler API implementation maintained for reference.
