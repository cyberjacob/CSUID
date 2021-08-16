using System;

namespace CSUID
{
    internal class InvalidSystemClockException : Exception
    {
        internal InvalidSystemClockException(long lastGenerated, long timestamp) :
            base($"Clock moved backwards or wrapped around. Refusing to generate id for {lastGenerated - timestamp} ticks")
        { }
    }
}
