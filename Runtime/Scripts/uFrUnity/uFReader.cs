namespace uFrUnity
{
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;
	using static uFrUnity.uFApi;
	using UFR_HANDLE = System.UIntPtr;

	public class uFReader
	{
		public const Int32 SERIAL_DESC_LEN = 8;
		public int list_idx;
		public bool opened = false;
		UFR_HANDLE hnd;
		public string reader_sn = "";
		public UInt32 reader_type = 0;
		public string ftdi_sn = "";
		public string ftdi_description = "";

		public uFReader()
		{
			// test.
		}

		public uFReader(int list_idx)
		{
			this.list_idx = list_idx;
			this.opened = false;

			// test.
		}

		public DL_STATUS close()
		{
			DL_STATUS status;

			unsafe
			{
				status = EntryPoints.ReaderClose(hnd);
			}

			hnd = UIntPtr.Zero;
			opened = false;

			return status;
		}

		public string get_designator()
		{
			return ftdi_sn;
		}

		public DL_STATUS open()
		{
			return open(list_idx);
		}

		public DL_STATUS open(int index)
		{
			UFR_HANDLE tmp_hnd;
			DL_STATUS status;

			unsafe
			{
				status = EntryPoints.ReaderList_OpenByIndex(index, &tmp_hnd);
			}

			if (status == DL_STATUS.UFR_OK)
			{
				hnd = tmp_hnd;

				try_to_get_all_infos();
			}
			else
			{
				hnd = UIntPtr.Zero;
			}

			if (status == DL_STATUS.UFR_OK || status == DL_STATUS.UFR_DEVICE_ALREADY_OPENED)
				opened = true;
			else
				opened = false;


			return status;
		}

		public DL_STATUS ReaderStillConnected(out UInt32 ret_val)
		{
			DL_STATUS status = DL_STATUS.UFR_OK;

			status = EntryPoints.ReaderStillConnectedM(hnd, out ret_val);

			if (status == DL_STATUS.UFR_OK)
			{
				if (ret_val == 0)
				{
					close();
				}
			}

			return status;
		}

		private void try_to_get_all_infos()
		{
			DL_STATUS status;

			unsafe
			{
				char* tmp_ftdi_serial = null;
				char* tmp_ftdi_desc = null;
				byte[] tmp_rd_sn = Enumerable.Repeat((byte)0, SERIAL_DESC_LEN).ToArray();
				UInt32 tmp_rd_type;

				status = EntryPoints.ReaderList_GetSerialDescriptionByIndex(list_idx, tmp_rd_sn);
				if (status != DL_STATUS.UFR_OK)
				{
					//error_wr("ReaderList_GetSerialDescriptionByIndex()", status);
				}

				status = EntryPoints.ReaderList_GetTypeByIndex(list_idx, &tmp_rd_type);
				if (status != DL_STATUS.UFR_OK)
				{
					//error_wr("ReaderList_GetTypeByIndex()", status);
				}

				status = EntryPoints.ReaderList_GetFTDISerialByIndex(list_idx, &tmp_ftdi_serial);
				if (status != DL_STATUS.UFR_OK)
				{
					//error_wr("ReaderList_GetFTDISerialByIndex()", status);
				}

				status = EntryPoints.ReaderList_GetFTDIDescriptionByIndex(list_idx, &tmp_ftdi_desc);
				if (status != DL_STATUS.UFR_OK)
				{
					//error_wr("ReaderList_GetFTDIDescriptionByIndex()", status);
				}

				reader_sn = System.Text.Encoding.ASCII.GetString(tmp_rd_sn);
				reader_type = tmp_rd_type;

				ftdi_sn = Marshal.PtrToStringAnsi((IntPtr)tmp_ftdi_serial);
				ftdi_description = Marshal.PtrToStringAnsi((IntPtr)tmp_ftdi_desc);
			}
		}

		override
		public string ToString()
		{
			string info = "Reader(" + list_idx.ToString() + ")";
			info += "[";
			info += (ftdi_sn.Length == 0 ? ftdi_sn : " ");
			info += "]";
			info += opened ? "[opened]" : "[closed]";
			info += ":";

			return info;
		}

		public string[] get_info()
		{
			try_to_get_all_infos();

			string[] info = { list_idx.ToString(), reader_sn, reader_type.ToString(), ftdi_sn, ftdi_description, opened.ToString() };

			return info;

		}

		public DL_STATUS GetCardIdEx(ref CARD_SAK Sak, ref byte[] Uid)
		{
			DL_STATUS status = DL_STATUS.UFR_OK;

			byte[] baCardUID = new byte[16];

			byte bUidSize = 0;
			byte bCardType = 0;

			unsafe
			{

				fixed (byte* pData = baCardUID)

					status = EntryPoints.GetCardIdEx(this.hnd, &bCardType, pData, &bUidSize);
			}

			Sak = (CARD_SAK)bCardType;

			//Array.Copy(baCardUID, Uid, (int)bUidSize);

			Array.Resize(ref baCardUID, (int)bUidSize);
			Uid = baCardUID;

			return status;
		}

		public DL_STATUS read(int block_nr, byte[] data)
		{
			DL_STATUS status;
			byte block_address = (byte)block_nr;
			byte auth_mode = (byte)MIFARE_AUTHENTICATION.MIFARE_AUTHENT1B;
			byte[] key = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

			unsafe
			{
				fixed (byte* ptr_data = data)
				fixed (byte* ptr_key = key)

					status = EntryPoints.BlockRead_PK(hnd, ptr_data, block_address, auth_mode,
							ptr_key);
			}

			return status;
		}

		public DL_STATUS write(int block_nr, byte[] data)
		{
			DL_STATUS status;
			byte block_address = (byte)block_nr;
			byte auth_mode = (byte)MIFARE_AUTHENTICATION.MIFARE_AUTHENT1B;
			byte[] key = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

			unsafe
			{
				fixed (byte* ptr_data = data)
				fixed (byte* ptr_key = key)

					status = EntryPoints.BlockWrite_PK(hnd, ptr_data, block_address, auth_mode,
							ptr_key);
			}

			return status;
		}

		//=====================================================================
		static public int get_reader_count()
		{
			int NumberOfDevices;
			DL_STATUS status;

			unsafe
			{
				status = EntryPoints.ReaderList_UpdateAndGetCount(&NumberOfDevices);
				//error_wr("ReaderList_UpdateAndGetCount()", status);
				return (status == DL_STATUS.UFR_OK ? NumberOfDevices : 0);
			}
		}

	}

}