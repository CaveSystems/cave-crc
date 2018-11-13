#region CopyRight 2018
/*
    Copyright (c) 2003-2018 Andreas Rohleder (andreas@rohleder.cc)
    All rights reserved
*/
#endregion
#region License LGPL-3
/*
    This program/library/sourcecode is free software; you can redistribute it
    and/or modify it under the terms of the GNU Lesser General Public License
    version 3 as published by the Free Software Foundation subsequent called
    the License.

    You may not use this program/library/sourcecode except in compliance
    with the License. The License is included in the LICENSE file
    found at the installation directory or the distribution package.

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the
    "Software"), to deal in the Software without restriction, including
    without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to
    permit persons to whom the Software is furnished to do so, subject to
    the following conditions:

    The above copyright notice and this permission notice shall be included
    in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
#region Authors & Contributors
/*
   Author:
     Andreas Rohleder <andreas@rohleder.cc>

   Contributors:
 */
#endregion

using System;
using System.Security.Cryptography;

namespace Cave
{
    /// <summary>
    /// Provides a managed implementation of the Cyclic Redundancy Checksum with 32 bits.
    /// </summary>
    public class CRC32 : HashAlgorithm, IChecksum<uint>
    {
        /// <summary>
        /// Provides the default polynomial
        /// (*the* standard CRC-32 polynomial, first popularized by Ethernet)
        /// x^32+x^26+x^23+x^22+x^16+x^12+x^11+x^10+x^8+x^7+x^5+x^4+x^2+x^1+x^0
        /// (little endian value)
        /// </summary>
        public static readonly uint DefaultPolynomial = 0x04c11db7;

        /// <summary>
        /// width=32 poly=0x04c11db7 init=0xffffffff refin=true refout=true xorout=0xffffffff check=0xcbf43926 residue=0xdebb20e3 name="CRC-32"
        /// </summary>
        public static CRC32 Default => new CRC32(poly: DefaultPolynomial, init: 0xFFFFFFFF, finalXor: 0xFFFFFFFF, reflectInput: true, reflectOutput: true, name: "CRC-32");

        /// <summary>
        /// width=32 poly=0xf4acfb13 init=0xffffffff refin=true refout=true xorout=0xffffffff check=0x1697d06a residue=0x904cddbf name="CRC-32/AUTOSAR"
        /// </summary>
        public static CRC32 AUTOSAR => new CRC32(poly: 0xf4acfb13, init: 0xFFFFFFFF, finalXor: 0xffffffff, reflectInput: true, reflectOutput: true, name: "CRC-32/AUTOSAR");

        /// <summary>
        /// width=32 poly=0x04c11db7 init=0xffffffff refin=false refout=false xorout=0xffffffff check=0xfc891918 residue=0xc704dd7b name="CRC-32/BZIP2"
        /// </summary>
        public static CRC32 BZIP2 => new CRC32(poly: DefaultPolynomial, init: 0xFFFFFFFF, finalXor: 0, reflectInput: false, reflectOutput: false, name: "CRC-32/BZIP2");

        /// <summary>
        /// width=32 poly=0x1edc6f41 init=0xffffffff refin=true refout=true xorout=0xffffffff check=0xe3069283 residue=0xb798b438 name="CRC-32C"
        /// </summary>
        public static CRC32 C => new CRC32(poly: 0x1edc6f41, init: 0xFFFFFFFF, finalXor: 0xFFFFFFFF, reflectInput: true, reflectOutput: true, name: "CRC-32C");

        /// <summary>
        /// width=32 poly=0xa833982b init=0xffffffff refin=true refout=true xorout=0xffffffff check=0x87315576 residue=0x45270551 name="CRC-32D"
        /// </summary>
        public static CRC32 D => new CRC32(poly: 0xa833982b, init: 0xFFFFFFFF, finalXor: 0xFFFFFFFF, reflectInput: true, reflectOutput: true, name: "CRC-32D");

        /// <summary>
        /// width=32 poly=0x04c11db7 init=0xffffffff refin=false refout=false xorout=0x00000000 check=0x0376e6e7 residue=0x00000000 name="CRC-32/MPEG-2"
        /// </summary>
        public static CRC32 MPEG2 => new CRC32(poly: DefaultPolynomial, init: 0xFFFFFFFF, finalXor: 0xFFFFFFFF, reflectInput: false, reflectOutput: false, name: "CRC-32/MPEG-2");

        /// <summary>
        /// width=32 poly=0x04c11db7 init=0x00000000 refin=false refout=false xorout=0xffffffff check=0x765e7680 residue=0xc704dd7b name="CRC-32/POSIX"
        /// </summary>
        public static CRC32 POSIX => new CRC32(poly: DefaultPolynomial, init: 0x00000000, finalXor: 0xFFFFFFFF, reflectInput: false, reflectOutput: false, name: "CRC-32/POSIX");

        /// <summary>
        /// Alias for <see cref="POSIX"/>
        /// </summary>
        public static CRC32 CKSUM => POSIX;

        /// <summary>
        /// width=32 poly=0x814141ab init=0x00000000 refin=false refout=false xorout=0x00000000 check=0x3010bf7f residue=0x00000000 name="CRC-32Q"
        /// </summary>
        public static CRC32 Q => new CRC32(poly: 0x814141ab, init: 0x00000000, finalXor: 0x00000000, reflectInput: false, reflectOutput: false, name: "CRC-32Q");

        /// <summary>Reflects 32 bits</summary>
        /// <param name="x">The bits</param>
        /// <returns>Returns a center reflection</returns>
        public static uint Reflect32(uint x)
        {
            //move bits
            x = ((x & 0x55555555) << 1) | ((x >> 1) & 0x55555555);
            x = ((x & 0x33333333) << 2) | ((x >> 2) & 0x33333333);
            x = ((x & 0x0F0F0F0F) << 4) | ((x >> 4) & 0x0F0F0F0F);
            //move bytes
            x = (x << 24) | ((x & 0xFF00) << 8) | ((x >> 8) & 0xFF00) | (x >> 24);
            return x;
        }

        #region private funtionality
        uint m_CurrentCRC;
        uint[] m_Table;

        /// <summary>Calculates the table.</summary>
        /// <returns></returns>
        protected void CalculateTable()
        {
            uint[] table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint value = i << 24;
                uint crc = 0;
                for (uint n = 0; n < 8; n++)
                {
                    unchecked
                    {
                        if ((int)(crc ^ value) < 0)
                        {
                            crc = (crc << 1) ^ Polynomial;
                        }
                        else
                        {
                            crc = crc << 1;
                        }
                        value <<= 1;
                    }
                }
                table[i] = crc;
            }
            m_Table = table;
        }

        private void CalculateReflectedTable()
        {
            uint poly = Reflect32(Polynomial);
            uint[] table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                unchecked
                {
                    for (uint n = 0; n < 8; n++)
                    {
                        if (0 != (crc & 1))
                        {
                            crc = (crc >> 1) ^ poly;
                        }
                        else
                        {
                            crc = (crc >> 1);
                        }
                    }
                }
                table[i] = crc;
            }
            m_Table = table;
        }
        #endregion

        /// <summary>The polynomial used to generate the table</summary>
        public readonly uint Polynomial;

        /// <summary>The initializer value</summary>
        public readonly uint Initializer;

        /// <summary>The final xor value</summary>
        public readonly uint FinalXor;

        /// <summary>The reflect input flag</summary>
        public readonly bool ReflectInput;

        /// <summary>The reflect output flag</summary>
        public readonly bool ReflectOutput;

        /// <summary>The name of the hash</summary>
        public readonly string Name;

        /// <summary>Gets the lookup table.</summary>
        /// <value>The table.</value>
        public uint[] Table { get { return (uint[])m_Table.Clone(); } }

        /// <summary>Returns the checksum computed so far.</summary>
        public uint Value
        {
            get
            {
                if (ReflectOutput)
                {
                    return m_CurrentCRC ^ FinalXor;
                }
                else
                {
                    return ~m_CurrentCRC ^ FinalXor;
                }
            }
            set
            {
                m_CurrentCRC = value;
            }
        }

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        public override int HashSize => 32;

        /// <summary>
        /// Gets the value of the computed hash code.
        /// </summary>
#if !NETSTANDARD13 && !NETCOREAPP10
        override
#endif
        public byte[] Hash
		{
			get
            {
                return BitConverter.GetBytes(Value);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="CRC32"/> class.</summary>
        /// <param name="blueprint">The blueprint to copy all properties from.</param>
        /// <exception cref="NotImplementedException"></exception>
        public CRC32(CRC32 blueprint)
        {
            Polynomial = blueprint.Polynomial;
            Initializer = blueprint.Initializer;
            FinalXor = blueprint.FinalXor;
            ReflectInput = blueprint.ReflectInput;
            ReflectOutput = blueprint.ReflectOutput;
            if (ReflectInput != ReflectOutput)
            {
                throw new NotImplementedException("ReflectInput has to match ReflectOutput. Uneven reflection is not implemented!");
            }

            Name = blueprint.Name;
            m_Table = blueprint.m_Table;
            m_CurrentCRC = Initializer;
        }

        /// <summary>
        /// Creates a new CRC32 with the <see cref="DefaultPolynomial"/>
        /// </summary>
        public CRC32()
            : this(poly: DefaultPolynomial, init: 0xFFFFFFFF, reflectInput: true, reflectOutput: true, finalXor: 0xffffffff, name: "CRC-32")
        {
        }

        /// <summary>
        /// Creates a new CRC32 with the specified polynomial
        /// </summary>
        /// <param name="poly">The polynom.</param>
        /// <param name="init">The initialize value.</param>
        /// <param name="reflectInput">if set to <c>true</c> [reflect input value] first.</param>
        /// <param name="reflectOutput">if set to <c>true</c> [reflect output value] first.</param>
        /// <param name="finalXor">The final xor value.</param>
        /// <param name="name">The name of the checksum.</param>
        public CRC32(uint poly, uint init, bool reflectInput, bool reflectOutput, uint finalXor, string name)
        {
            Polynomial = poly;
            Initializer = init;
            FinalXor = finalXor;
            ReflectInput = reflectInput;
            ReflectOutput = reflectOutput;
            if (ReflectInput != ReflectOutput)
            {
                throw new NotImplementedException("ReflectInput has to match ReflectOutput. Uneven reflection is not implemented!");
            }

            Name = name;
            if (ReflectInput)
            {
                CalculateReflectedTable();
            }
            else
            {
                CalculateTable();
            }
            m_CurrentCRC = Initializer;
        }

        /// <summary>
        /// (Re-)initializes the <see cref="CRC32"/>
        /// </summary>
        public override void Initialize()
        {
            m_CurrentCRC = Initializer;
        }

        /// <summary>
        /// directly hashes one byte
        /// </summary>
        /// <param name="b"></param>
        public void HashCore(byte b)
        {
            if (ReflectInput)
            {
                uint i = (m_CurrentCRC ^ b) & 0xFF;
                m_CurrentCRC = (m_CurrentCRC >> 8) ^ m_Table[i];
            }
            else
            {
                uint i = ((m_CurrentCRC >> 24) ^ b) & 0xFF;
                m_CurrentCRC = unchecked((m_CurrentCRC << 8) ^ m_Table[i]);
            }
        }

        /// <summary>
        /// Computes the hash for the specified data. 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="ibStart"></param>
        /// <param name="cbSize"></param>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            for (int i = 0; i < cbSize; i++)
            {
                HashCore(array[ibStart++]);
            }
        }

        /// <summary>
        /// Finalizes the hash computation obtains the resulting hash code in the systems byte order.
        /// </summary>
        /// <returns></returns>
        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(Value);
        }

        /// <summary>Resets the checksum to initialization state</summary>
        public void Reset()
        {
            Initialize();
        }

        /// <summary>Adds one byte to the checksum.</summary>
        /// <param name="value">the byte to add. Only the lowest 8 bits will be used.</param>
        public void Update(int value)
        {
            HashCore((byte)(value & 0xFF));
        }

        /// <summary>Updates the checksum with the specified byte array.</summary>
        /// <param name="buffer">The buffer containing the data</param>
        public void Update(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            HashCore(buffer, 0, buffer.Length);
        }

        /// <summary>Updates the checksum with the specified byte array.</summary>
        /// <param name="buffer">The buffer containing the data</param>
        /// <param name="offset">The offset in the buffer where the data starts</param>
        /// <param name="count">the number of data bytes to add.</param>
        public void Update(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            HashCore(buffer, offset, count);
        }

        /// <summary>Returns a <see cref="string" /> that represents this instance.</summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Name + " width=32 poly=" + Polynomial + " init=" + Initializer + " refin=" + ReflectInput + " refout=" + ReflectOutput + " xorout=" + FinalXor;
        }

        /// <summary>Erstellt ein neues Objekt, das eine Kopie der aktuellen Instanz darstellt.</summary>
        /// <returns>Ein neues Objekt, das eine Kopie dieser Instanz darstellt.</returns>
        public object Clone()
        {
            return new CRC32(this);
        }
    }
}