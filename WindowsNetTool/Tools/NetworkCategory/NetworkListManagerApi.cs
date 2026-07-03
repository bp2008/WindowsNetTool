using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WindowsNetTool.Tools.NetworkCategory
{
	// Minimal COM interop for the Windows Network List Manager API (netlistmgr.h).
	// Method order in each interface must match the vtable order from the IDL exactly.

	public enum NlmNetworkCategory
	{
		Public = 0,
		Private = 1,
		DomainAuthenticated = 2
	}

	[Flags]
	public enum NlmEnumNetwork
	{
		Connected = 1,
		Disconnected = 2,
		All = 3
	}

	[ComImport, Guid("DCB00002-570F-4A9B-8D69-199FDBA5723B"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface INetwork
	{
		[return: MarshalAs(UnmanagedType.BStr)]
		string GetName();
		void SetName([MarshalAs(UnmanagedType.BStr)] string szNetworkNewName);
		[return: MarshalAs(UnmanagedType.BStr)]
		string GetDescription();
		void SetDescription([MarshalAs(UnmanagedType.BStr)] string szDescription);
		Guid GetNetworkId();
		int GetDomainType();
		[return: MarshalAs(UnmanagedType.Interface)]
		IEnumNetworkConnections GetNetworkConnections();
		void GetTimeCreatedAndConnected(out uint pdwLowDateTimeCreated, out uint pdwHighDateTimeCreated, out uint pdwLowDateTimeConnected, out uint pdwHighDateTimeConnected);
		bool IsConnectedToInternet { get; }
		bool IsConnected { get; }
		int GetConnectivity();
		NlmNetworkCategory GetCategory();
		void SetCategory(NlmNetworkCategory newCategory);
	}

	[ComImport, Guid("DCB00005-570F-4A9B-8D69-199FDBA5723B"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface INetworkConnection
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		INetwork GetNetwork();
		bool IsConnectedToInternet { get; }
		bool IsConnected { get; }
		int GetConnectivity();
		Guid GetConnectionId();
		Guid GetAdapterId();
		int GetDomainType();
	}

	[ComImport, Guid("DCB00003-570F-4A9B-8D69-199FDBA5723B"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IEnumNetworks
	{
		[return: MarshalAs(UnmanagedType.IUnknown)]
		object GetNewEnum(); // propget _NewEnum
		void Next(uint celt, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] INetwork[] rgelt, ref uint pceltFetched);
		void Skip(uint celt);
		void Reset();
		[return: MarshalAs(UnmanagedType.Interface)]
		IEnumNetworks Clone();
	}

	[ComImport, Guid("DCB00006-570F-4A9B-8D69-199FDBA5723B"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IEnumNetworkConnections
	{
		[return: MarshalAs(UnmanagedType.IUnknown)]
		object GetNewEnum(); // propget _NewEnum
		void Next(uint celt, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] INetworkConnection[] rgelt, ref uint pceltFetched);
		void Skip(uint celt);
		void Reset();
		[return: MarshalAs(UnmanagedType.Interface)]
		IEnumNetworkConnections Clone();
	}

	[ComImport, Guid("DCB00000-570F-4A9B-8D69-199FDBA5723B"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface INetworkListManager
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		IEnumNetworks GetNetworks(NlmEnumNetwork flags);
		[return: MarshalAs(UnmanagedType.Interface)]
		INetwork GetNetwork(Guid networkId);
		[return: MarshalAs(UnmanagedType.IUnknown)]
		object GetNetworkConnections();
		[return: MarshalAs(UnmanagedType.IUnknown)]
		object GetNetworkConnection(Guid networkConnectionId);
		bool IsConnectedToInternet { get; }
		bool IsConnected { get; }
		int GetConnectivity();
	}

	[ComImport, Guid("DCB00C01-570F-4A9B-8D69-199FDBA5723B"), ClassInterface(ClassInterfaceType.None)]
	public class NetworkListManagerClass
	{
	}

	/// <summary>
	/// A snapshot of one network from the Network List Manager, retaining the underlying
	/// COM object so the network can be modified later.
	/// </summary>
	public class NetworkInfo
	{
		private readonly INetwork network;
		public Guid Id { get; private set; }
		public string Name { get; private set; }
		public NlmNetworkCategory Category { get; private set; }
		public bool IsConnectedToInternet { get; private set; }

		public NetworkInfo(INetwork network)
		{
			this.network = network;
			Id = network.GetNetworkId();
			Name = network.GetName();
			Category = network.GetCategory();
			IsConnectedToInternet = network.IsConnectedToInternet;
		}

		public void SetCategory(NlmNetworkCategory category)
		{
			network.SetCategory(category);
			Category = category;
		}

		public void Rename(string newName)
		{
			network.SetName(newName);
			Name = newName;
		}
	}

	/// <summary>
	/// Describes the network which owns a particular network adapter.
	/// </summary>
	public class AdapterNetwork
	{
		public Guid AdapterId;
		public string NetworkName;
		/// <summary>UTC time the network was last connected, or DateTime.MinValue if unknown.</summary>
		public DateTime ConnectedSinceUtc;
	}

	public static class NetworkCategoryService
	{
		/// <summary>
		/// Returns all currently connected networks.
		/// </summary>
		public static List<NetworkInfo> GetConnectedNetworks()
		{
			INetworkListManager nlm = (INetworkListManager)new NetworkListManagerClass();
			IEnumNetworks enumNetworks = nlm.GetNetworks(NlmEnumNetwork.Connected);
			List<NetworkInfo> result = new List<NetworkInfo>();
			INetwork[] buffer = new INetwork[1];
			while (true)
			{
				uint fetched = 0;
				enumNetworks.Next(1, buffer, ref fetched);
				if (fetched == 0 || buffer[0] == null)
					break;
				result.Add(new NetworkInfo(buffer[0]));
				buffer[0] = null;
			}
			return result;
		}

		/// <summary>
		/// Returns a map of adapter ID (see System.Net.NetworkInformation.NetworkInterface.Id) to the
		/// connected network which owns that adapter.
		/// </summary>
		public static Dictionary<Guid, AdapterNetwork> GetAdapterNetworkMap()
		{
			Dictionary<Guid, AdapterNetwork> map = new Dictionary<Guid, AdapterNetwork>();
			INetworkListManager nlm = (INetworkListManager)new NetworkListManagerClass();
			IEnumNetworks enumNetworks = nlm.GetNetworks(NlmEnumNetwork.Connected);
			INetwork[] netBuffer = new INetwork[1];
			while (true)
			{
				uint fetched = 0;
				enumNetworks.Next(1, netBuffer, ref fetched);
				if (fetched == 0 || netBuffer[0] == null)
					break;
				INetwork network = netBuffer[0];
				netBuffer[0] = null;

				string name = network.GetName();
				network.GetTimeCreatedAndConnected(out uint createdLow, out uint createdHigh, out uint connectedLow, out uint connectedHigh);
				DateTime connectedSince = FileTimeToDateTimeUtc(connectedHigh, connectedLow);

				IEnumNetworkConnections enumConnections = network.GetNetworkConnections();
				INetworkConnection[] connBuffer = new INetworkConnection[1];
				while (true)
				{
					uint connFetched = 0;
					enumConnections.Next(1, connBuffer, ref connFetched);
					if (connFetched == 0 || connBuffer[0] == null)
						break;
					Guid adapterId = connBuffer[0].GetAdapterId();
					connBuffer[0] = null;
					map[adapterId] = new AdapterNetwork
					{
						AdapterId = adapterId,
						NetworkName = name,
						ConnectedSinceUtc = connectedSince
					};
				}
			}
			return map;
		}

		private static DateTime FileTimeToDateTimeUtc(uint high, uint low)
		{
			long fileTime = ((long)high << 32) | low;
			if (fileTime <= 0)
				return DateTime.MinValue;
			try
			{
				return DateTime.FromFileTimeUtc(fileTime);
			}
			catch (ArgumentOutOfRangeException)
			{
				return DateTime.MinValue;
			}
		}

		public static string GetCategoryText(NlmNetworkCategory category)
		{
			if (category == NlmNetworkCategory.DomainAuthenticated)
				return "Domain";
			return category.ToString();
		}
	}
}
