# IoT Hub Device

The IoT Hub Device is a tool used to send sample heart rate and blood pressure messages from a simulated vital signs device to an Azure IoT Hub. The following instructions describe how to run this tool.


## Prerequisites

- [An IoT hub](https://docs.microsoft.com/en-us/azure/iot-develop/quickstart-send-telemetry-iot-hub?pivots=programming-language-csharp#create-an-iot-hub)

- [A device registered with your IoT Hub](https://docs.microsoft.com/en-us/azure/iot-develop/quickstart-send-telemetry-iot-hub?pivots=programming-language-csharp#register-a-device)


## Obtain device connection string

The device connection string is needed to authenticate and connect the device to your IoT Hub.

Obtain the device connection string by using one of the following methods - **Azure portal** or **Azure CLI**:

- **Azure portal**

    1. Go to Azure portal.

    1. Navigate to {Your IoT Hub resource} -> IoT devices -> {Your Device ID}.

        The Primary Connection String is your device connection string.

- **Azure CLI**
    1. Run the following command:
        ```
        az iot hub device-identity connection-string show --hub-name {Your IoT Hub name} --device-id {Your Device ID} --output table
        ```

        The ConnectionString is your device connection string.


## Run simulated device

Run the simulated device with your device connection string (obtained in the previous step) by using one of the following methods - **commands** or **Visual Studio**:

- **Commands**

    1. Open a command shell.

    1. Navigate to the directory that contains the *Microsoft.Health.Tools.IotHubDevice.exe* application with the following command:
        ```
        cd {Path to iomt-fhir\tools\iot-hub-device\Microsoft.Health.Tools.IotHubDevice\bin\Debug\net5.0}
        ```

    1. Run the simulated device with the following command:
        ```
        Microsoft.Health.Tools.IotHubDevice.exe {Your device connection string}
        ```

        The simulated device application connects to your IoT Hub as the device you registered and begins sending heart rate and blood pressure (systolic and diastolic) telemetry messages. These telemetry messages appear in the console window.

- **Visual Studio**

    1. Open *iomt-fhir\tools\iot-hub-device\Microsoft.Health.Tools.IotHubDevice.sln* in Visual Studio.

    1. Open *Program.cs* from the Solution Explorer tool window.

    1. Replace `{Your device connection string here}` with your device connection string.

    1. Save your change in *Program.cs*.

    1. Press CTRL + F5 in Visual Studio to run the simulated device.

        The simulated device application connects to your IoT Hub as the device you registered and begins sending heart rate and blood pressure (systolic and diastolic) telemetry messages. These telemetry messages appear in the console window.


## View telemetry

View the device telemetry by following [these instructions](https://docs.microsoft.com/en-us/azure/iot-develop/quickstart-send-telemetry-iot-hub?.pivots=programming-language-csharp&pivots=programming-language-csharp#view-telemetry).
