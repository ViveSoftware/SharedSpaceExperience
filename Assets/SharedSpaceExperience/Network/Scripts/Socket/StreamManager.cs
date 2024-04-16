using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class StreamManager
    {
        private readonly Stream stream;
        private bool isActive = false;
        private CancellationTokenSource cancelSource;

        private readonly SocketCallbacks callbacks;

        public StreamManager(Stream stream, SocketCallbacks callbacks)
        {
            this.stream = stream;
            this.callbacks = callbacks;
        }

        public async Task<bool> StartStreamAsync()
        {
            if (isActive)
            {
                return false;
            }

            int numBytesRead;
            byte[] headerBytes = new byte[SocketDataPack.HEADER_SIZE];

            cancelSource = new CancellationTokenSource();
            CancellationToken cancelToken = cancelSource.Token;

            isActive = true;
            while (isActive)
            {
                try
                {
                    // read header
                    numBytesRead = await stream.ReadAsync(headerBytes, 0, SocketDataPack.HEADER_SIZE, cancelToken).ConfigureAwait(false);
                    if (numBytesRead == 0)
                    {
                        Logger.Log("Stream closed");
                        isActive = false;
                        break;
                    }

                    // parse header
                    SocketDataPack dataPack = new();
                    if (!dataPack.SetHeader(headerBytes))
                    {
                        Logger.LogError("Failed to parse header");
                        continue;
                    }

                    // read data
                    int numDataRead = 0;
                    int dataSize = dataPack.GetDataSize();
                    byte[] dataBytes = new byte[dataSize];

                    callbacks.OnReceivingData?.Invoke(this, numDataRead, dataSize);
                    while (numDataRead < dataSize)
                    {
                        numBytesRead = await stream.ReadAsync(dataBytes, numDataRead, dataSize - numDataRead, cancelToken).ConfigureAwait(false);

                        Logger.Log($"Try read bytes {dataSize - numDataRead}, bytes read {numBytesRead}", false);

                        numDataRead += numBytesRead;
                        if (!isActive || numBytesRead == 0)
                        {
                            Logger.Log("Stream closed");
                            isActive = false;
                            break;
                        }

                        callbacks.OnReceivingData?.Invoke(this, numDataRead, dataSize);
                    }
                    Logger.Log("Total received: " + numDataRead + "/" + dataSize);
                    callbacks.OnReceivingData?.Invoke(this, numDataRead, dataSize);

                    if (!isActive) break;
                    // parse data
                    if (!dataPack.SetData(dataBytes))
                    {
                        Logger.LogError("Failed to parse data");
                        continue;
                    }

                    // invoke OnReceiveData callback
                    callbacks.OnReceiveData?.Invoke(this, dataPack);

                }
                catch (IOException e)
                {
                    Logger.LogWarning("Failed to read stream");
                    Logger.LogWarning("" + e, false);
                    isActive = false;
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to read stream");
                    Logger.LogError("" + e, false);
                    isActive = false;
                }
            }

            return true;
        }

        public void StopStream()
        {
            // NOTE: stream listener may not stop immediately
            isActive = false;
            cancelSource?.Cancel();
            stream?.Close();
        }

        public async Task<bool> SendDataAsync(SocketDataPack pack)
        {
            if (!isActive || stream == null)
            {
                Logger.LogError("Failed to send data: no stream available");
                return false;
            }

            byte[] bytes = SocketDataPack.ToBytes(pack);
            try
            {
                await stream.WriteAsync(bytes, 0, bytes.Length, cancelSource.Token);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to send data: " + e);
            }

            return true;
        }
    }
}