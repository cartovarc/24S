## Import WSDK into the Project

-   Firstly, create a folder named "DJIWindowsSDK", and copy all the DJI Windows SDK library files in it.![importWSDKDll](https://devusa.djicdn.com/images/quick-start/WSDKCreateWSDKDllFolder-f02694f86b.png)![importWSDKDll](https://devusa.djicdn.com/images/quick-start/WDSKDllImport-76fb74121b.png)
-   Secondly, add reference of  **DJIWindowsSDK**  to your project.
    -   a. Right-click on 24S project, and select  **Add Reference**.
    -   b. Select  **Browse**, and select  **DJIWindowsSDK.dll**. Click  **Add**.
    -   c. Click  **OK**.![importWSDKDll](https://devusa.djicdn.com/images/quick-start/WSDKDllReference-b5f6c33ca0.png)![importWSDKDll](https://devusa.djicdn.com/images/quick-start/WSDKDllImportResult-246675f85c.png)
-   Thirdly, add dll files of third parties to the project.
    -   a. Copy dll files of third parties(**pthread_dll.dll**  and  **libcrypto-1.1.dll**, for x86) to the root path of the project
    -   b. Right-click on 24S project, select  **Add->Existing Item**, and click the dlls needed.![importThirdPartiesDll](https://devusa.djicdn.com/images/quick-start/WSDKAddThirdPartiesDll-5f6f7bcbff.png)![importThirdPartiesDll](https://devusa.djicdn.com/images/quick-start/WSDKAddThirdPartiesDllResult-b612d94b5a.png)
   
## Import DJIVideoParser into the Project
-   Firstly, copy DJIVideoParser project to the root folder of 24S project. You can find DJIVideoParser project  [here](https://github.com/dji-sdk/Windows-SDK/tree/master/Sample%20Code).
![enter image description here](https://i.ibb.co/kcH0d20/image.png)
-   Secondly, add DJIVideoParser project.
    -   a. Open 24S solution, right-click on the solution, select  **Add->Existing Project**.
    -   b. Locate to the folder where DJIVideoParser.vcxproj exists, select it and click  **Open**.
-   Thirdly, configure DJIVideoParser project.
    -   a. Right-click on the DJIVideoParser project, select  **Properties**.
    -   b. Double check and follow the following settings:  **Configuration, Platform, Target Platform Version**  and  **Output Directory**.![VideoParserProject](https://devusa.djicdn.com/images/quick-start/WSDKDJIVideoParserConfig-7351ca195a.png)
    -   c. Right-click on the DJIVideoParser project, select "Build" to build the project so that you can use the API directly in the code after adding the related reference.
-   Fourthly, add reference to 24S.
    -   a. Right-click on 24S project, and select  **Add Reference**, and then click  **Projects**.
    -   b. Click the checkbox of the DJIVideoParser project to add it.![VideoParserProject](https://devusa.djicdn.com/images/quick-start/WSDKAddVideoParserReference-555d20b0a3.png)
    -   Click  **OK**.
-   Finally, copy FFMpeg dlls to 24S project.
    -   a. Right-click on the 24S project. Select  **Add->Existing Item**.
    -   b. Locate to the path of FFmpeg dll files(under the folder of /DJIVideoParser/ThirdParties/dlls).
    -   c. Select all of them, and then click  **Add**.![VideoParserProject](https://devusa.djicdn.com/images/quick-start/WSDKAddFFMPEGDlls-4ccd2bae84.png)


