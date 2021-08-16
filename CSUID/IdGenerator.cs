using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace CSUID
{
    public class IdGenerator : IEnumerable
    {
        internal const int Epoch = 1400000000;

        private ushort _sequence;
        private long _lastGenerated = -1;

        private readonly object _generatorLock = new object();

        private readonly string _system;
        private readonly string _environment;

        // Gets the Id of the generator.
        public byte[] Id { get; }

        public IdGenerator(string system, string environment)
        {
            // Pre-calculate some values
            var macAddress = NetworkInterface.GetAllNetworkInterfaces()
                                             .First(nic => nic.OperationalStatus == OperationalStatus.Up &&
                                                           nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                                             .GetPhysicalAddress();

            var generatorDataBytes = new List<byte>();
            generatorDataBytes.AddRange(macAddress.GetAddressBytes());
            generatorDataBytes.AddRange(BitConverter.GetBytes((uint) Process.GetCurrentProcess().Id));

            Id = generatorDataBytes.ToArray();

            _system      = system;
            _environment = environment;
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string CreateId()
        {
            lock (_generatorLock)
            {
                var utc       = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var timestamp = (uint) (utc - Epoch);
                if (timestamp < _lastGenerated)
                    throw new InvalidSystemClockException(_lastGenerated, timestamp);

                if (timestamp == _lastGenerated)
                    _sequence++;
                else
                {
                    _sequence      = 0;
                    _lastGenerated = timestamp;
                }

                var stamp = new List<byte>(Id);
                stamp.AddRange(BitConverter.GetBytes(timestamp));
                stamp.AddRange(BitConverter.GetBytes(_sequence));
                return $"{_system}_{_environment}_{Base62.ToBase62(stamp.ToArray())}";
            }
        }

        [ItemNotNull]
        private IEnumerable<string> IdStream()
        {
            while (true)
                yield return CreateId();
            // ReSharper disable once IteratorNeverReturns
        }

        [NotNull]
        public IEnumerator<string> GetEnumerator() => IdStream().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
