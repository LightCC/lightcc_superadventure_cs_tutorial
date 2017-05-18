using System;
using System.Security.Cryptography;

namespace Engine
{
    public static class RandomNumberGenerator
    {
        /* 
        // Simple random numbers - not as random as you'd like, though
        private static Random _generator = new Random();

        public static int NumberBetween(int minimumValue, int maximumValue)
        {
            return _generator.Next(minimumValue, maximumValue + 1)
        }
        */

        // This version is better at not following a pattern when generating
        // random numbers
        private static readonly RNGCryptoServiceProvider _generator =
            new RNGCryptoServiceProvider();

        public static int NumberBetween(int minValue, int maxValue)
        {
            byte[] randomNumber = new byte[1];

            _generator.GetBytes(randomNumber);

            double asciiValueOfRandomCharacter = Convert.ToDouble(randomNumber[0]);

            // We are using Math.Max, and substracting 0.00000000001,
            // to ensure "multiplier" will always be between 0.0 and .99999999999
            // Otherwise, it's possible for it to be "1", which causes problems in our rounding.
            double multiplier = Math.Max(0, (asciiValueOfRandomCharacter / 255d) - 0.00000000001d);
            
            // We need to add one to the range, to allow for the rounding done with Math.Floor
            int range = maxValue - minValue + 1;

            double randomValueInRange = Math.Floor(multiplier * range);

            return (int)(minValue + randomValueInRange);
        }
    }
}
