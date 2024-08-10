using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using Substrate.NetApi.Model.Extrinsics;
using VaraExt = Substrate.Vara.NET.NetApiExt.Generated;

public class ReadStateTrafficLight : MonoBehaviour
{
    private VaraExt.SubstrateClientExt _clientvara;
    private string url;

    async void Start()
    {
        url = "wss://testnet.vara.network";
        _clientvara = new VaraExt.SubstrateClientExt(new Uri(url), ChargeTransactionPayment.Default());
        await _clientvara.ConnectAsync();

        if (_clientvara != null && _clientvara.IsConnected)
        {
            Debug.Log("Client is connected.");
            string[] parametersArray = new string[] { "0x59d593389507c73c89f2cf1a3478d36c55df13de4b8cf89295fb5cebc8db3f8d", "" };
            var result = await _clientvara.InvokeAsync<string>("gear_readState", parametersArray, CancellationToken.None);
            Debug.Log($"Result: {result}");
            byte[] resultBytes = StringToByteArray(result);
            TrafficLight trafficLight = TrafficLight.Decode(resultBytes);
            Debug.Log($"Decoded Result: {trafficLight}");
        }
        else
        {
            Debug.Log("Client is not connected.");
        }
    }

    public static byte[] StringToByteArray(string hex)
    {
        if (hex.StartsWith("0x"))
        {
            hex = hex.Substring(2);
        }

        int numberChars = hex.Length;
        byte[] bytes = new byte[numberChars / 2];
        for (int i = 0; i < numberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }

    [Serializable]
    public class TrafficLight
    {
        public string CurrentLight { get; set; }
        public List<(string, string)> AllUsers { get; set; }

        public static TrafficLight Decode(byte[] bytes)
        {
            int index = 0;

            var currentLight = DecodeString(bytes, ref index, "current_light");

            List<(string, string)> allUsers = new List<(string, string)>();
            int numberOfUsers = DecodeCompactLength(bytes, ref index);
            for (int i = 0; i < numberOfUsers; i++)
            {
                string actorId = DecodeActorId(bytes, ref index, "actor_id");
                string userName = DecodeString(bytes, ref index, "user_name");
                allUsers.Add((actorId, userName));
            }

            return new TrafficLight
            {
                CurrentLight = currentLight,
                AllUsers = allUsers
            };
        }

        private static string DecodeString(byte[] bytes, ref int index, string fieldName)
        {
            int length = DecodeCompactLength(bytes, ref index);
            if (index + length > bytes.Length)
                throw new ArgumentOutOfRangeException("Index and count must refer to a location within the buffer.");
            string value = Encoding.UTF8.GetString(bytes, index, length);
            index += length;
            return value;
        }

        private static string DecodeActorId(byte[] bytes, ref int index, string fieldName)
        {
            int length = 32;
            if (index + length > bytes.Length)
                throw new ArgumentOutOfRangeException("Index and count must refer to a location within the buffer.");
            string value = "0x" + BitConverter.ToString(bytes, index, length).Replace("-", "").ToLower();
            index += length;
            return value;
        }

        private static int DecodeCompactLength(byte[] bytes, ref int index)
        {
            if (index + 1 > bytes.Length)
                throw new ArgumentOutOfRangeException("Index and count must refer to a location within the buffer.");
            byte firstByte = bytes[index];
            index += 1;

            int length;
            if ((firstByte & 0x03) == 0x00)
            {
                length = firstByte >> 2;
            }
            else if ((firstByte & 0x03) == 0x01)
            {
                if (index + 1 > bytes.Length)
                    throw new ArgumentOutOfRangeException("Index and count must refer to a location within the buffer.");
                length = ((firstByte >> 2) | (bytes[index] << 6)) & 0x3FFF;
                index += 1;
            }
            else if ((firstByte & 0x03) == 0x02)
            {
                if (index + 3 > bytes.Length)
                    throw new ArgumentOutOfRangeException("Index and count must refer to a location within the buffer.");
                length = (firstByte >> 2) |
                         (bytes[index] << 6) |
                         (bytes[index + 1] << 14) |
                         (bytes[index + 2] << 22);
                index += 3;
            }
            else if ((firstByte & 0x03) == 0x03)
            {
                if (index + 4 > bytes.Length)
                    throw new ArgumentOutOfRangeException("Index and count must refer to a location within the buffer.");
                length = BitConverter.ToInt32(bytes, index);
                index += 4;
            }
            else
            {
                throw new Exception("Unsupported compact length encoding");
            }

            return length;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"CurrentLight: {CurrentLight}, AllUsers: [");
            foreach (var user in AllUsers)
            {
                sb.Append($"(ActorId: {user.Item1}, UserName: {user.Item2}), ");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
