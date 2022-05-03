namespace uFrUnity
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text.RegularExpressions;

	public class SuccessfulRead
	{
		public string ReaderId = default;
		public string ReaderData = default;
	}

	public class ConnectionInfo
	{
		public string CardTypeString = default;
		public string CardUID = default;
		public byte CardType = default;
		public string Data = default;

		public ConnectionInfo() { }

		public void Clear()
		{
			CardType = 0;
			CardTypeString = null;
			Data = null;
			CardUID = null;
		}
	}

	unsafe public class uFApi
	{
		public enum CARD_SAK
		{
			UNKNOWN = 0x00,
			MIFARE_CLASSIC_1k = 0x08,
			MF1ICS50 = 0x08,
			SLE66R35 = 0x88,
			MIFARE_CLASSIC_4k = 0x18,
			MF1ICS70 = 0x18,
			MIFARE_CLASSIC_MINI = 0x09,
			MF1ICS20 = 0x09,
		}


		public enum DL_STATUS
		{
			UFR_OK = 0x00,

			UFR_COMMUNICATION_ERROR = 0x01,
			UFR_CHKSUM_ERROR = 0x02,
			UFR_READING_ERROR = 0x03,
			UFR_WRITING_ERROR = 0x04,
			UFR_BUFFER_OVERFLOW = 0x05,
			UFR_MAX_ADDRESS_EXCEEDED = 0x06,
			UFR_MAX_KEY_INDEX_EXCEEDED = 0x07,
			UFR_NO_CARD = 0x08,
			UFR_COMMAND_NOT_SUPPORTED = 0x09,
			UFR_FORBIDEN_DIRECT_WRITE_IN_SECTOR_TRAILER = 0x0A,
			UFR_ADDRESSED_BLOCK_IS_NOT_SECTOR_TRAILER = 0x0B,
			UFR_WRONG_ADDRESS_MODE = 0x0C,
			UFR_WRONG_ACCESS_BITS_VALUES = 0x0D,
			UFR_AUTH_ERROR = 0x0E,
			UFR_PARAMETERS_ERROR = 0x0F, // ToDo, tačka 5.
			UFR_MAX_SIZE_EXCEEDED = 0x10,

			UFR_WRITE_VERIFICATION_ERROR = 0x70,
			UFR_BUFFER_SIZE_EXCEEDED = 0x71,
			UFR_VALUE_BLOCK_INVALID = 0x72,
			UFR_VALUE_BLOCK_ADDR_INVALID = 0x73,
			UFR_VALUE_BLOCK_MANIPULATION_ERROR = 0x74,
			UFR_WRONG_UI_MODE = 0x75,
			UFR_KEYS_LOCKED = 0x76,
			UFR_KEYS_UNLOCKED = 0x77,
			UFR_WRONG_PASSWORD = 0x78,
			UFR_CAN_NOT_LOCK_DEVICE = 0x79,
			UFR_CAN_NOT_UNLOCK_DEVICE = 0x7A,
			UFR_DEVICE_EEPROM_BUSY = 0x7B,
			UFR_RTC_SET_ERROR = 0x7C,

			UFR_COMMUNICATION_BREAK = 0x50,
			UFR_NO_MEMORY_ERROR = 0x51,
			UFR_CAN_NOT_OPEN_READER = 0x52,
			UFR_READER_NOT_SUPPORTED = 0x53,
			UFR_READER_OPENING_ERROR = 0x54,
			UFR_READER_PORT_NOT_OPENED = 0x55,
			UFR_CANT_CLOSE_READER_PORT = 0x56,

			UFR_FT_STATUS_ERROR_1 = 0xA0,
			UFR_FT_STATUS_ERROR_A1 = 0xA1,
			UFR_FT_STATUS_ERROR_2 = 0xA1,
			UFR_FT_STATUS_ERROR_3 = 0xA2,
			UFR_FT_STATUS_ERROR_4 = 0xA3,
			UFR_FT_STATUS_ERROR_5 = 0xA4,
			UFR_FT_STATUS_ERROR_6 = 0xA5,
			UFR_FT_STATUS_ERROR_7 = 0xA6,
			UFR_FT_STATUS_ERROR_8 = 0xA7,
			UFR_FT_STATUS_ERROR_9 = 0xA8,
			UFR_FT_STATUS_ERROR_B4 = 0xB4,

			//NDEF error codes
			UFR_WRONG_NDEF_CARD_FORMAT = 0x80,
			UFR_NDEF_MESSAGE_NOT_FOUND = 0x81,
			UFR_NDEF_UNSUPPORTED_CARD_TYPE = 0x82,
			UFR_NDEF_CARD_FORMAT_ERROR = 0x83,
			UFR_MAD_NOT_ENABLED = 0x84,
			UFR_MAD_VERSION_NOT_SUPPORTED = 0x85,

			// multi units
			UFR_DEVICE_WRONG_HANDLE = 0x100,
			UFR_DEVICE_INDEX_OUT_OF_BOUND,
			UFR_DEVICE_ALREADY_OPENED,
			UFR_DEVICE_ALREADY_CLOSED,

			MAX_UFR_STATUS = 10000000, // 0xFFFFFFFF
		};

		// MIFARE CLASSIC Authentication Modes:
		public enum MIFARE_AUTHENTICATION
		{
			MIFARE_AUTHENT1A = 0x60,
			MIFARE_AUTHENT1B = 0x61,
		}



		public const byte DL_OK = 0, KEY_INDEX = 0;

		public const byte DL_MIFARE_ULTRALIGHT = 0x01,
		   DL_MIFARE_ULTRALIGHT_EV1_11 = 0x02,
		   DL_MIFARE_ULTRALIGHT_EV1_21 = 0x03,
		   DL_MIFARE_ULTRALIGHT_C = 0x04,
		   DL_NTAG_203 = 0x05,
		   DL_NTAG_210 = 0x06,
		   DL_NTAG_212 = 0x07,
		   DL_NTAG_213 = 0x08,
		   DL_NTAG_215 = 0x09,
		   DL_NTAG_216 = 0x0A,
		   DL_MIFARE_MINI = 0x20,
		   DL_MIFARE_CLASSIC_1K = 0x21,
		   DL_MIFARE_CLASSIC_4K = 0x22,
		   DL_MIFARE_PLUS_S_2K = 0x23,
		   DL_MIFARE_PLUS_S_4K = 0x24,
		   DL_MIFARE_PLUS_X_2K = 0x25,
		   DL_MIFARE_PLUS_X_4K = 0x26,
		   DL_MIFARE_DESFIRE = 0x27,
		   DL_MIFARE_DESFIRE_EV1_2K = 0x28,
		   DL_MIFARE_DESFIRE_EV1_4K = 0x29,
		   DL_MIFARE_DESFIRE_EV1_8K = 0x2A;

		public const byte FRES_OK_LIGHT = 4,
			 FRES_OK_SOUND = 0,
			 FERR_LIGHT = 2,
			 FERR_SOUND = 0;

		public const byte
			 MAX_SECTORS_1k = 16,
			 MAX_SECTORS_4k = 40,
			 MAX_BYTES_NTAG_203 = 144,
			 MAX_BYTES_ULTRALIGHT = 48,
			 MAX_BYTES_ULTRALIGHT_C = 144;
		public const short
		   MAX_BYTES_CLASSIC_1K = 752,
		   MAX_BYTES_CLASSIC_4k = 3440;


		#region API
		public static bool Ok(DL_STATUS status, out DL_STATUS outStatus)
		{
			SignalFeedback(status);
			outStatus = status;
			return status == DL_OK;
		}

		public static string GetError(DL_STATUS errorCode)
		{
			return errorCode.ToString();
		}

		public static List<uFReader> DiscoverDevices()
		{
			int NumberOfDevices = uFReader.get_reader_count();

			List<uFReader> discovered = new List<uFReader>();
			for (int i = 0; i < NumberOfDevices; i++)
			{
				uFReader ufr = new uFReader(i);
				discovered.Add(ufr);
			}

			return discovered;

		}

		public static uFReader FindDevice(string serialNumber)
		{
			int NumberOfDevices;

			NumberOfDevices = uFReader.get_reader_count();

			for (int i = 0; i < NumberOfDevices; i++)
			{
				uFReader ufr = new uFReader(i);
				if (Ok(ufr.open(), out var status))
				{
					if (ufr.reader_sn == serialNumber)
					{
						return ufr;
					}
				} else
				{
					UnityEngine.Debug.Log(GetError(status));
				}
			}

			return null;

		}

		public static void CheckConnectionAndCardInfo(ref List<uFrUnityPlugin.ReaderConnection> readers, ref bool CheckingInfo)
		{
			if (readers.Count == 0)
			{
				return;
			}

			CheckingInfo = true;
			CARD_SAK Sak = CARD_SAK.UNKNOWN;
			byte[] baUid = new byte[7];
			uint ret_val;

			foreach (var reader in readers)
			{
				var uFReader = reader.Reader;
				if (uFReader != null)
				{
					if (uFReader.opened)
					{
						var status = uFReader.GetCardIdEx(ref Sak, ref baUid);
						if (status != DL_STATUS.UFR_OK)
						{
							if (status == DL_STATUS.UFR_NO_CARD)
							{
								reader.ResetCardReads();
								continue;
							}

							if ((status <= DL_STATUS.UFR_FT_STATUS_ERROR_A1)
								|| (status >= DL_STATUS.UFR_FT_STATUS_ERROR_B4))
							{
								continue;
							}
							else
							{
								status = uFReader.ReaderStillConnected(out ret_val);
								if (status == DL_STATUS.UFR_OK)
								{
									if (ret_val == 0)
									{
										var removeAt = readers.Find(rc => rc.Reader == uFReader);
										removeAt.Disconnected();
										break;
									}
								}
								else
								{
									continue;
								}
							}
						}
						else
						{
							string sBuffer = null;
							for (byte bCounter = 0; bCounter < baUid.Length; bCounter++)
							{
								sBuffer += baUid[bCounter].ToString("X2");
							}
							if (reader.Data == null)
							{
								reader.Data = new ConnectionInfo();
							}
							reader.Data.CardUID = sBuffer;
							reader.Data.CardTypeString = Sak.ToString();
							reader.Data.CardType = (byte)Sak;
						}

					}
				}
				else
				{
					// Try link the same device back
					reader.Reader = FindDevice(reader.ReaderSN);
				}

			}
			CheckingInfo = false;
		}

		public static DL_STATUS Read(ref List<uFrUnityPlugin.ReaderConnection> readers, out List<uFrUnityPlugin.ReaderConnection> successfulReads, ref bool IsBusy)
		{
			successfulReads = new List<uFrUnityPlugin.ReaderConnection>();
			if (readers.Count == 0)
			{
				return DL_STATUS.UFR_READING_ERROR;
			}

			try
			{
				IsBusy = true;

				for (int i = 0; i < readers.Count; i++)
				{
					if (readers[i].Connected && readers[i].IsReady && readers[i].LastReadCardUID != readers[i].Data.CardUID)
					{
						byte[] data = new byte[16];
						if (Ok(readers[i].Reader.read(1, data), out var status))
						{
							readers[i].Data.Data = System.Text.Encoding.Default.GetString(data);
							readers[i].Data.Data = Regex.Replace(readers[i].Data.Data, @"\p{C}+", String.Empty);
							readers[i].LastReadCardUID = readers[i].Data.CardUID;
							successfulReads.Add(readers[i]);
						}
					}
					
				}

				return DL_STATUS.UFR_OK;
			}
			catch (Exception ex)
			{
				return DL_STATUS.UFR_COMMUNICATION_ERROR;
			}
			finally
			{
				IsBusy = false;
			}
		}

		public static bool Close(ref List<uFrUnityPlugin.ReaderConnection> readers)
		{
			if (readers.Count == 0)
			{
				return false;
			}

			DL_STATUS result = DL_STATUS.UFR_OK;
			foreach (var reader in readers)
			{
				if (reader.Reader == null)
				{
					continue;
				}

				if (!Ok(reader.Reader.close(), out var status))
				{
					result |= status;
					UnityEngine.Debug.LogError(GetError(status));
				}
			}

			return result == DL_STATUS.UFR_OK;
		}
		#endregion

		#region IMPLEMENTATION
		private static void SignalFeedback(DL_STATUS status)
		{
			EntryPoints.ReaderUISignal(status == DL_OK ? FRES_OK_LIGHT : FERR_LIGHT, status == DL_OK ? FRES_OK_SOUND : FERR_SOUND);
		}
		#endregion
	}
}