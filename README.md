## To keep in mind:
- Pictures are taken from DJI tutorials, folder DJIWSDKDemo it's actually 24S.
- Architecture x64 used, for any file needed, x64 dependency is used.
- These DJI necessary files are tracked to avoid additional steps:
    ![necessary tracked files](https://i.ibb.co/3ySts7d/image.png)

## Import WSDK into the Project
-   Download DJI Windows SDK files from https://github.com/dji-sdk/Windows-SDK/tree/master/WSDK%20Libraries/SDK%20dll%20x64 
-   Create a folder named "DJIWindowsSDK" in same level that "24S.sln" file, and copy all the DJI Windows SDK library files in it.![importWSDKDll](https://devusa.djicdn.com/images/quick-start/WSDKCreateWSDKDllFolder-f02694f86b.png)![importWSDKDll](https://devusa.djicdn.com/images/quick-start/WDSKDllImport-76fb74121b.png)
## Import DJIVideoParser into the Project
-   Firstly, copy DJIVideoParser project to the root folder of 24S project. You can find DJIVideoParser project  here: https://github.com/dji-sdk/Windows-SDK/tree/master/Sample%20Code
![enter image description here](https://i.ibb.co/kcH0d20/image.png)
-   Secondly, configure DJIVideoParser project.
    -   a. Right-click on the DJIVideoParser project, select  **Properties**.
    -   b. Double check and follow the following settings:  **Configuration, Platform, Target Platform Version**  and  **Output Directory**.![VideoParserProject](https://devusa.djicdn.com/images/quick-start/WSDKDJIVideoParserConfig-7351ca195a.png)

# Visual Studio Dependencies
Make sure have those dependencies:
- ![enter image description here](https://i.imgur.com/vSFMVIM.png)
- ![enter image description here](https://i.imgur.com/1QCOcV6.png) 
- Open "24S.sln" solution, if it ask for download additional dependency, install it.

## Build and start
With "24S.sln" solution opened:
- In the package manager console execute "Update-Package -reinstall"
- Make sure x64 debug is taken in both recompile
![enter image description here](https://i.ibb.co/xmM1SG7/image.png)
- First recompile DJIVideoParser project
![enter image description here](https://i.ibb.co/wrbR0yS/image.png)
- Second recompile 24S project
![enter image description here](https://i.ibb.co/dfFswKb/image.png)
- Start :)

## Useful commands:
- CheckNetIsolation.exe LoopbackExempt -s
- CheckNetIsolation.exe LoopbackExempt -is -n=com.rosas24s.integrator_p4yrhxa6ey1na
