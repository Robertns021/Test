using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            byte[] paket = new byte[12];
            ModbusWriteCommandParameters write = (ModbusWriteCommandParameters)CommandParameters;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)write.TransactionId)), 0, paket, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)write.ProtocolId)), 0, paket, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)write.Length)), 0, paket, 4, 2);
            paket[6] = write.UnitId;
            paket[7] = write.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)write.OutputAddress)), 0, paket, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)write.Value)), 0, paket, 10, 2);
            return paket;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            //Extract all the data bytes from the entire response
            short dataShort = BitConverter.ToInt16(response, 10);
            dataShort = IPAddress.NetworkToHostOrder(dataShort);
            Dictionary<Tuple<PointType, ushort>, ushort> odgovor = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusWriteCommandParameters parameters = (ModbusWriteCommandParameters)CommandParameters;
            odgovor.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, parameters.OutputAddress), (ushort)dataShort);

            return odgovor;


            //// this is the solution for a single bit
            ////byte dataByte = response[9];

            ////ushort dataBit = (ushort)(dataByte & 1);

            //Dictionary<Tuple<PointType, ushort>, ushort> odgovor = new Dictionary<Tuple<PointType, ushort>, ushort>();
            //ModbusWriteCommandParameters parameters = (ModbusWriteCommandParameters)CommandParameters;

            //// used for counting the amount of data in the dictionary and comparing that to the quantity of data in the response
            //ushort bitCounter = 0;

            ////byte for loop, 9 is the first byte of data in the response
            //for (int i = 9; i < response.Length; i++)
            //{
            //    // bit for loop (8 bits in 1 byte)
            //    for (int j = 0; j < 8; j++)
            //    {

            //        // shift the mask to the left j number of times
            //        ushort dataBit = (ushort)(response[i] & (1 << j));

            //        odgovor.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, (ushort)(parameters.OutputAddress + bitCounter)), dataBit);

            //        bitCounter++;
            //    }
            //}

            //return odgovor;
        }
    }
}