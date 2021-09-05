using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read input registers functions/requests.
    /// </summary>
    public class ReadInputRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadInputRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadInputRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
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
            Dictionary<Tuple<PointType, ushort>, ushort> odgovor = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters parameters = (ModbusReadCommandParameters)CommandParameters;


            // used for counting the amount of data in the dictionary and comparing that to the quantity of data in the response
            ushort bitCounter = 0;

            //9 is the first byte of data in the response
            for (int i = 9; i < response.Length; i+=2)
            {
                short dataShort = BitConverter.ToInt16(response, i);
                dataShort = IPAddress.NetworkToHostOrder(dataShort);

                odgovor.Add(new Tuple<PointType, ushort>(PointType.ANALOG_INPUT, (ushort)(parameters.StartAddress+bitCounter)), (ushort)dataShort);
                bitCounter++;

            }

            return odgovor;
        }
    }
}