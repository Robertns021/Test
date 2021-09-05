using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            byte[] paket = new byte[12];
            ModbusReadCommandParameters read = (ModbusReadCommandParameters)CommandParameters;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)read.TransactionId)), 0, paket, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)read.ProtocolId)), 0, paket, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)read.Length)), 0, paket, 4, 2);
            paket[6] = read.UnitId;
            paket[7] = read.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)read.StartAddress)), 0, paket, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)read.Quantity)), 0, paket, 10, 2);
            return paket;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            // this is the solution for a single bit
            //byte dataByte = response[9];

            //ushort dataBit = (ushort)(dataByte & 1);

            Dictionary<Tuple<PointType, ushort>, ushort> odgovor = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters parameters = (ModbusReadCommandParameters)CommandParameters;

            // used for counting the amount of data in the dictionary and comparing that to the quantity of data in the response
            ushort bitCounter = 0;

            //byte for loop, 9 is the first byte of data in the response
            for (int i = 9; i < response.Length; i++)
            {
                byte temp = response[i];
                // bit for loop (8 bits in 1 byte)
                for (int j = 0; j < 8; j++)
                {
                    // if there are equal or more bits in the dictionary than the number of bits that the response returned, stop the loop
                    if (bitCounter >= parameters.Quantity)
                    {
                        break;
                    }

                    // shift the mask to the left j number of times
                    ushort dataBit = (ushort)(temp & 1);
                    temp >>= 1;

                    odgovor.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, (ushort)(parameters.StartAddress+bitCounter)), dataBit);

                    bitCounter++;
                }
            }

            return odgovor;
        }


    }
}