# DirCommandPOC


======================
FAST FILE ENUMERATOR
======================

TO COMPILE:

dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

About:
------
The Fast File Enumerator is a culmination of research and development efforts to discover and measure the fastest method for enumerating and counting every file on large storage volumes. Traditional methods of enumeration, especially on vast filesystems with deep folder structures, can be time-consuming and resource-intensive. Our approach taps into the rapidity of the native `dir` command, combined with the power and flexibility of C# to deliver a performance-oriented solution.

Besides being a part of our research to uncover efficient enumeration techniques, this tool doubles as a quick and reliable means to gauge storage performance on extensive volumes. Storage performance is a crucial metric in many scenarios, from system maintenance to capacity planning, and Fast File Enumerator offers a user-friendly yet robust mechanism to achieve this.

Usage:
------
1. Run the application by launching 'FastFileEnumerator.exe'.
2. When prompted, provide the path of the directory you wish to enumerate.
3. The application will start processing and display running statistics, updating every 10,000 files. These statistics include:
    - Total files processed
    - Files processed per second
    - Total runtime
    - CPU usage
    - Disk usage

Features:
---------
- Efficient enumeration using hybrid techniques.
- Real-time statistics display.
- Quick evaluation of storage performance.
- Intuitive, user-friendly interface.

Requirements:
-------------
- Windows 7 or newer
- .NET Core 3.1 runtime (if not using the self-contained deployment version)

Notes:
------
While the primary objective of this tool is to enumerate files swiftly, it also offers insights into storage performance. The CPU and Disk usage statistics can provide a bird's-eye view of how your storage reacts under load, valuable for IT admins and enthusiasts alike.

Thank you for using the Fast File Enumerator. Your feedback is invaluable in our journey to optimize and innovate.

