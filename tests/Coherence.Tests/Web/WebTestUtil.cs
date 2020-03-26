/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Text;

namespace Tangosol.Web
{
    public abstract class WebTestUtil
    {
        protected static byte[] CreateBlob(int size)
        {
            byte[] blob = new byte[size];

            s_rand.NextBytes(blob);
            return blob;
        }

        protected static PortablePerson CreatePerson()
        {
            PortablePerson person = new PortablePerson(GenerateName(), DateTime.Now);
            person.Address = new Address(GenerateStreet(), GenerateCity(), GenerateCountry(), GenerateZip());
            return person;
        }

        static WebTestUtil()
        {
            s_rand = new Random(4711);
        }


        private static string GenerateZip()
        {
            return RandomizeString(5, 5, 48, 57);
        }

        private static string GenerateCountry()
        {
            return RandomizeString(2, 2, 65, 90);
        }

        private static string GenerateCity()
        {
            return RandomizeString(3, 20, 65, 122);
        }

        private static string GenerateStreet()
        {
            return RandomizeString(1, 4, 48, 57) + " " +
                   RandomizeString(5, 15, 65, 122); ;
        }

        private static string GenerateName()
        {
            return RandomizeString(5, 10, 65, 122) + " " + RandomizeString(5, 10, 65, 122);
        }

        private static string RandomizeString(int min, int max, int start, int end)
        {
            int len = s_rand.Next(max - min) + min;
            StringBuilder builder = new StringBuilder(len);
            for (int i = 0; i < len; i++)
            {
                builder.Append((char)s_rand.Next(start, end));
            }
            return builder.ToString();
        }

        private static readonly Random s_rand;
    }
}