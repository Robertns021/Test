using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
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
            odgovor.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, parameters.OutputAddress), (ushort)dataShort);

            return odgovor;
        }
    }
}