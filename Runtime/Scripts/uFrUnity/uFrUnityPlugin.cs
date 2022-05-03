using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static uFrUnity.uFApi;
using System.Threading;

namespace uFrUnity
{

	public class uFrUnityPlugin : MonoBehaviour
	{
		public class ReaderConnection
		{
			public uFReader Reader = default;
			public ConnectionInfo Data = new ConnectionInfo();
			public string ReaderSN = default;

			public string LastReadCardUID = default;

			public bool Connected => Reader != null && Reader.opened;
			public bool IsReady => Data != null && Data.CardUID != null;

			public void ResetCardReads()
			{
				Data = null;
				LastReadCardUID = null;
			}

			public void Disconnected()
			{
				Reader?.close();
				Reader = null;
			}
		}

		public event System.Action<SuccessfulRead> OnReadData = default;
		private List<ReaderConnection> m_activeReaders = new List<ReaderConnection>();

		private bool m_isBusy = false;
		private bool m_checkingInfo = false;
		private CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();



		private void Awake()
		{
			Task.Run(Tick, m_cancellationTokenSource.Token);
		}

		private void Update()
		{
			if (m_checkingInfo)
			{
				return;
			}

			lock(m_activeReaders)
			{
				if (Ok(Read(ref m_activeReaders, out var successfulReads, ref m_isBusy), out var status))
				{
					successfulReads.ForEach((ReaderConnection conn) => {
						OnReadData?.Invoke(new SuccessfulRead() { ReaderData = conn.Data.Data, ReaderId = conn.ReaderSN });
					});
				}
				else
				{
					Debug.LogError(GetError(status));
				}
			}

			var devices = DiscoverDevices();
			devices.ForEach((uFReader reader) =>
			{
				if (Ok(reader.open(), out var status))
				{
					var current = m_activeReaders.Find(rc => rc.ReaderSN == reader.reader_sn);
					if (current != null)
					{
						current.Reader = reader;
					}
					else
					{
						m_activeReaders.Add(new ReaderConnection() { Reader = reader, Data = null, ReaderSN = reader.reader_sn });
					}

				}
				else
				{
					Debug.LogError(GetError(status));
				}
			});

		}

		private async Task Tick()
		{
			while(!m_cancellationTokenSource.IsCancellationRequested)
			{
				if (!m_isBusy)
				{
					lock (m_activeReaders)
					{
						try
						{
							CheckConnectionAndCardInfo(ref m_activeReaders, ref m_checkingInfo);
						}
						catch (Exception ex)
						{
							Debug.LogError(ex.Message);
						}
					}
				}

				await Task.Delay(300);
			}
			
		}

		private void OnDestroy()
		{
			m_cancellationTokenSource.Cancel();
			Close(ref m_activeReaders);
		}

#if UNITY_EDITOR || UFR_DEBUG
		private void OnGUI()
		{
			GUI.skin.label.fontSize = 32;

			foreach(var readerConnection in m_activeReaders)
			{
				GUILayout.BeginVertical();

				if (!readerConnection.Connected)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label($"{readerConnection.ReaderSN} No longer Connected");
					GUILayout.EndHorizontal();
					continue;
				}

				GUILayout.BeginHorizontal();
				GUILayout.Label($"{readerConnection.ReaderSN} Connected");
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Card Connected");
				GUILayout.Label(readerConnection.IsReady.ToString());
				GUILayout.EndHorizontal();

				if (readerConnection.IsReady)
				{
					GUILayout.BeginVertical();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Card Type");
					GUILayout.Label(readerConnection.Data?.CardTypeString);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Card UID");
					GUILayout.Label(readerConnection.Data?.CardUID);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Last Read Text");
					GUILayout.Label(readerConnection.Data?.Data);
					GUILayout.EndHorizontal();

					GUILayout.EndVertical();
				}
				
			}
		}
#endif
	}
}
