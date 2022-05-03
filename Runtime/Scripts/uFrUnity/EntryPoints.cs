using System;
using System.Text;

namespace uFrUnity
{
	using System.Runtime.InteropServices;
	using static uFrUnity.uFApi;
	using UFR_HANDLE = System.UIntPtr;
	unsafe class EntryPoints
	{
#if UNITY_STANDALONE_WIN
		const string DLL_PATH = "";
        const string NAME_DLL = "uFCoder-x86_64.dll";

#elif UNITY_STANDALONE_OSX
		const string DLL_PATH = "";
		const string NAME_DLL = "libuFCoder-x86_64.dylib";
#endif

		const string DLL_NAME = DLL_PATH + NAME_DLL;

		#region SINGLE_READER
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "LinearRead")]
		public static extern DL_STATUS LinearRead([Out] byte[] data,
												   int linear_address,
												   int data_len,
												   int* bytes_written,
												   byte key_mode,
												   byte key_index);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "ReaderUISignal")]
		public static extern DL_STATUS ReaderUISignal(int light_mode,
													  int sound_mode);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "ReaderOpen")]
		public static extern DL_STATUS ReaderOpen();

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderOpenEx")]
		private static extern DL_STATUS ReaderOpenEx(UInt32 reader_type, [In] byte[] port_name, UInt32 port_interface, [In] byte[] arg);
		public static DL_STATUS ReaderOpenEx(UInt32 reader_type, string port_name, UInt32 port_interface, string arg)
		{

			byte[] port_name_p = Encoding.ASCII.GetBytes(port_name);
			byte[] port_name_param = new byte[port_name_p.Length + 1];
			Array.Copy(port_name_p, 0, port_name_param, 0, port_name_p.Length);
			port_name_param[port_name_p.Length] = 0;

			byte[] arg_p = Encoding.ASCII.GetBytes(arg);
			byte[] arg_param = new byte[arg_p.Length + 1];
			Array.Copy(arg_p, 0, arg_param, 0, arg_p.Length);
			arg_param[arg_p.Length] = 0;

			return ReaderOpenEx(reader_type, port_name_param, port_interface, arg_param);
		}

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "ReaderClose")]
		public static extern DL_STATUS ReaderClose();

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "GetReaderType")]
		public static extern DL_STATUS GetReaderType(ulong* get_reader_type);



		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "GetCardIdEx")]
		public static extern DL_STATUS GetCardIdEx(byte* bCardType,
												   byte* bCardUID,
												   byte* bUidSize);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "GetDlogicCardType")]
		public static extern DL_STATUS GetDlogicCardType(byte* bCardType);
		#endregion

		#region MULTI_READERS


		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_UpdateAndGetCount")]
		internal static extern DL_STATUS ReaderList_UpdateAndGetCount(Int32* NumberOfDevices);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_GetSerialByIndex")]
		internal static extern DL_STATUS ReaderList_GetSerialByIndex(Int32 DeviceIndex, UInt32* lpulSerialNumber);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_GetSerialDescriptionByIndex")]
		internal static extern DL_STATUS ReaderList_GetSerialDescriptionByIndex(Int32 DeviceIndex, [In, Out] byte[] serial_desc); // SERIAL_DESC_LEN = 8

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_GetTypeByIndex")]
		internal static extern DL_STATUS ReaderList_GetTypeByIndex(Int32 DeviceIndex, UInt32* lpulReaderType);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_GetFTDISerialByIndex")]
		internal static extern DL_STATUS ReaderList_GetFTDISerialByIndex(Int32 DeviceIndex, char** Device_Serial);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_GetFTDIDescriptionByIndex")]
		internal static extern DL_STATUS ReaderList_GetFTDIDescriptionByIndex(Int32 DeviceIndex, char** Device_Description);

		//---------------------------------------------------------------------

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderList_OpenByIndex")]
		internal static extern DL_STATUS ReaderList_OpenByIndex(Int32 DeviceIndex, UFR_HANDLE* hndUFR);

		//---------------------------------------------------------------------

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderOpenM")]
		internal static extern DL_STATUS ReaderOpen(UFR_HANDLE hndUFR);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderCloseM")]
		internal static extern DL_STATUS ReaderClose(UFR_HANDLE hndUFR);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderStillConnectedM")]
		internal static extern DL_STATUS ReaderStillConnectedM(UFR_HANDLE hndUFR, out UInt32 connected);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetReaderSerialDescriptionM")]
		internal static extern DL_STATUS GetReaderSerialDescriptionM(UFR_HANDLE hndUFR, [In, Out] byte[] serial_desc); // SERIAL_DESC_LEN = 8

		//---------------------------------------------------------------------

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetCardIdExM")]
		internal static extern DL_STATUS GetCardIdEx(UFR_HANDLE hndUFR,
													byte* bCardType,
													byte* bCardUID,
													byte* bUidSize);

		//[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetReaderTypeM")]
		//internal static extern DL_STATUS GetReaderType(UInt32* get_reader_type);

		//---------------------------------------------------------------------

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "BlockRead_PKM")]
		public static extern DL_STATUS BlockRead_PK(UFR_HANDLE hndUFR,
												  byte* data,
												  byte block_address,
												  byte auth_mode,
												  byte* key);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "BlockWrite_PKM")]
		public static extern DL_STATUS BlockWrite_PK(UFR_HANDLE hndUFR,
													byte* data,
												  byte block_address,
												  byte auth_mode,
												  byte* key);

		#endregion
	}
}
