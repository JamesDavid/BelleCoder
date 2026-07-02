using System;

namespace startechplus.ble
{
	public interface IBleBridge
	{
		BluetoothLeDevice Startup(bool asCentral, Action action, Action<string> errorAction, Action<string> stateUpdateAction, Action<string, string> rssiUpdateAction);

		void Shutdown(Action action);

		void Cleanup();

		void PauseWithState(bool isPaused);

		void ScanForPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action);

		void StopScanning();

		void ConnectToPeripheralWithIdentifier(string peripheralId, Action<string, string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string, string, string, string> descriptorAction, Action<string, string> disconnectAction);

		void DisconnectFromPeripheralWithIdentifier(string peripheralId, Action<string, string> action);

		void RetrieveListOfPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action);

		void RetrieveListOfPeripheralsWithUUIDs(string[] uuids, Action<string, string> action);

		void SubscribeToCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> notificationAction, Action<string, string, string, byte[]> action, bool isIndication);

		void UnSubscribeFromCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> action);

		void ReadCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string, byte[]> action);

		void WriteCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, byte[] data, int length, bool withResponse, Action<string, string, string> action);

		void ReadDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, Action<string, string, string, string, byte[]> action);

		void WriteDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, byte[] data, int length, Action<string, string, string, string> action);

		void ReadRssiWithIdentifier(string peripheralId);

		void AddAdvertisementDataListeners(Action<string, string> localNameAction, Action<string, byte[]> manufactureDataAction, Action<string, string, byte[]> serviceDataAction, Action<string, string> serviceAction, Action<string, string> overflowServiceAction, Action<string, string> txPowerLevelAction, Action<string, string> isConnectable, Action<string, string> solicitedServiceAction);
	}
}
