using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    /// <summary>Sent from server to client.</summary>
    public enum ServerPackets
    {
        welcome,
        spawnObject,
        despawnObject,
        updateObject,
        rmi,
        ackSingle,
        sanityCheck,
    }

    /// <summary>Sent from client to server.</summary>
    public enum ClientPackets
    {
        welcomeReceived,
        updateObject,
        ackSingle,
        sanityCheck
    }

    public class Packet : IDisposable
    {
        private List<byte> buffer;
        private byte[] readableBuffer;
        private int readPos;
        UTF8Encoding utf8 = new UTF8Encoding();

        /// <summary>Creates a new empty packet (without an ID).</summary>
        public Packet()
        {
            buffer = new List<byte>(); // Intitialize buffer
            readPos = 0; // Set readPos to 0
        }

        /// <summary>Creates a new packet with a given ID. Used for sending.</summary>
        /// <param name="_id">The packet ID.</param>
        public Packet(int _id)
        {
            buffer = new List<byte>(); // Intitialize buffer
            readPos = 0; // Set readPos to 0

            Write(_id); // Write packet id to the buffer
        }

        /// <summary>Creates a packet from which data can be read. Used for receiving.</summary>
        /// <param name="_data">The bytes to add to the packet.</param>
        public Packet(byte[] _data)
        {
            buffer = new List<byte>(); // Intitialize buffer
            readPos = 0; // Set readPos to 0

            SetBytes(_data);
        }

        #region Functions
        /// <summary>Sets the packet's content and prepares it to be read.</summary>
        /// <param name="_data">The bytes to add to the packet.</param>
        public void SetBytes(byte[] _data)
        {
            Write(_data);
            readableBuffer = FlipIfLittleEndian(buffer.ToArray());
        }

        /// <summary>Inserts the length of the packet's content at the start of the buffer.</summary>
        public void WriteLength()
        {
            buffer.InsertRange(0, FlipIfLittleEndian(BitConverter.GetBytes((ushort)buffer.Count))); // Insert the byte length of the packet at the very beginning
        }
        public void InsertUShort(ushort _data)
        {
            buffer.InsertRange(0, FlipIfLittleEndian(BitConverter.GetBytes(_data)));
        }

        public void InsertByte(byte _data)
        {
            buffer.Insert(0, _data);
        }

        /// <summary>Inserts the given int at the start of the buffer.</summary>
        /// <param name="_value">The int to insert.</param>
        public void InsertInt(int _value)
        {
            buffer.InsertRange(0, FlipIfLittleEndian(BitConverter.GetBytes(_value))); // Insert the int at the start of the buffer
        }

        /// <summary>Gets the packet's content in array form.</summary>
        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        /// <summary>Gets the length of the packet's content.</summary>
        public int Length()
        {
            return buffer.Count; // Return the length of buffer
        }

        /// <summary>Gets the length of the unread data contained in the packet.</summary>
        public int UnreadLength()
        {
            return Length() - readPos; // Return the remaining length (unread)
        }

        /// <summary>Resets the packet instance to allow it to be reused.</summary>
        /// <param name="_shouldReset">Whether or not to reset the packet.</param>
        public void Reset(bool _shouldReset = true)
        {
            if (_shouldReset)
            {
                buffer.Clear(); // Clear buffer
                readableBuffer = null;
                readPos = 0; // Reset readPos
            }
            else
            {
                readPos -= 4; // "Unread" the last read int
            }
        }
        #endregion

        #region Write Data
        /// <summary>Adds a byte to the packet.</summary>
        /// <param name="_value">The byte to add.</param>
        public void Write(byte _value)
        {
            buffer.Add(_value);
        }
        /// <summary>Adds an array of bytes to the packet.</summary>
        /// <param name="_value">The byte array to add.</param>
        public void Write(byte[] _value)
        {
            buffer.AddRange(FlipIfLittleEndian(_value));
        }
        /// <summary>Adds a short to the packet.</summary>
        /// <param name="_value">The short to add.</param>
        public void Write(short _value)
        {
            buffer.AddRange(FlipIfLittleEndian(BitConverter.GetBytes(_value)));
        }
        /// <summary>Adds a ushort to the packet.</summary>
        /// <param name="_value">The short to add.</param>
        public void Write(ushort _value)
        {
            buffer.AddRange(FlipIfLittleEndian(BitConverter.GetBytes(_value)));
        }
        /// <summary>Adds an int to the packet.</summary>
        /// <param name="_value">The int to add.</param>
        public void Write(int _value)
        {
            buffer.AddRange(FlipIfLittleEndian(BitConverter.GetBytes(_value)));
        }
        /// <summary>Adds a long to the packet.</summary>
        /// <param name="_value">The long to add.</param>
        public void Write(long _value)
        {
            buffer.AddRange(FlipIfLittleEndian(BitConverter.GetBytes(_value)));
        }
        /// <summary>Adds a float to the packet.</summary>
        /// <param name="_value">The float to add.</param>
        public void Write(float _value)
        {
            buffer.AddRange(FlipIfLittleEndian(BitConverter.GetBytes(_value)));
        }
        /// <summary>Adds a bool to the packet.</summary>
        /// <param name="_value">The bool to add.</param>
        public void Write(bool _value)
        {
            buffer.AddRange(FlipIfLittleEndian(BitConverter.GetBytes(_value)));
        }
        /// <summary>Adds a string to the packet.</summary>
        /// <param name="_value">The string to add.</param>
        public void Write(string _value)
        {
            Write(_value.Length); // Add the length of the string to the packet
            buffer.AddRange(FlipIfLittleEndian(utf8.GetBytes(_value))); // Add the string itself
        }
        public void Write(List<int> intList)
        {
            Write(intList.Count);
            for (int i = 0; i < intList.Count; i++)
            {
                Write(intList[i]);
            }
        }
        public void Write(List<short> shortList)
        {
            Write(shortList.Count);
            for (int i = 0; i < shortList.Count; i++)
            {
                Write(shortList[i]);
            }
        }
        public void Write(List<ushort> ushortList)
        {
            Write(ushortList.Count);
            for (int i = 0; i < ushortList.Count; i++)
            {
                Write(ushortList[i]);
            }
        }
        public void Write(List<string> stringList)
        {
            Write(stringList.Count);
            for (int i = 0; i < stringList.Count; i++)
            {
                Write(stringList[i]);
            }
        }
        public void Write(List<long> longList)
        {
            Write(longList.Count);
            for (int i = 0; i < longList.Count; i++)
            {
                Write(longList[i]);
            }
        }
        public void Write(List<float> floatList)
        {
            Write(floatList.Count);
            for (int i = 0; i < floatList.Count; i++)
            {
                Write(floatList[i]);
            }
        }
        public void Write(UnityEngine.Vector3 vector3)
        {
            Write(vector3.x);
            Write(vector3.y);
            Write(vector3.z);
        }

        #endregion

        #region Read Data
        /// <summary>Reads a byte from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public byte ReadByte(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                byte _value = readableBuffer[readPos]; // Get the byte at readPos' position
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 1; // Increase readPos by 1
                }
                return _value; // Return the byte
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        /// <summary>Reads an array of bytes from the packet.</summary>
        /// <param name="_length">The length of the byte array.</param>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public byte[] ReadBytes(int _length, bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                byte[] _value = FlipIfLittleEndian(buffer.GetRange(readPos, _length).ToArray()); // Get the bytes at readPos' position with a range of _length
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += _length; // Increase readPos by _length
                }
                return _value; // Return the bytes
            }
            else
            {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        /// <summary>Reads a short from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public short ReadShort(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                short _value = BitConverter.ToInt16(FlipIfLittleEndian(readableBuffer), readPos); // Convert the bytes to a short
                if (_moveReadPos)
                {
                    // If _moveReadPos is true and there are unread bytes
                    readPos += 2; // Increase readPos by 2
                }
                return _value; // Return the short
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        /// <summary>Reads a ushort from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public ushort ReadUShort(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                ushort _value = BitConverter.ToUInt16(FlipIfLittleEndian(readableBuffer), readPos); // Convert the bytes to a short
                if (_moveReadPos)
                {
                    // If _moveReadPos is true and there are unread bytes
                    readPos += 2; // Increase readPos by 2
                }
                return _value; // Return the short
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        /// <summary>Reads an int from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public int ReadInt(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                int _value = BitConverter.ToInt32(FlipIfLittleEndian(readableBuffer), readPos); // Convert the bytes to an int
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 4; // Increase readPos by 4
                }
                return _value; // Return the int
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        /// <summary>Reads a long from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public long ReadLong(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                long _value = BitConverter.ToInt64(FlipIfLittleEndian(readableBuffer), readPos); // Convert the bytes to a long
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 8; // Increase readPos by 8
                }
                return _value; // Return the long
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        /// <summary>Reads a float from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public float ReadFloat(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                float _value = BitConverter.ToSingle(FlipIfLittleEndian(readableBuffer), readPos); // Convert the bytes to a float
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 4; // Increase readPos by 4
                }
                return _value; // Return the float
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        /// <summary>Reads a bool from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public bool ReadBool(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                bool _value = BitConverter.ToBoolean(FlipIfLittleEndian(readableBuffer), readPos); // Convert the bytes to a bool
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 1; // Increase readPos by 1
                }
                return _value; // Return the bool
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        /// <summary>Reads a string from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public string ReadString(bool _moveReadPos = true)
        {
            try
            {
                int _length = ReadInt(); // Get the length of the string
                string _value = utf8.GetString(FlipIfLittleEndian(readableBuffer), readPos, _length); // Convert the bytes to a string
                if (_moveReadPos && _value.Length > 0)
                {
                    // If _moveReadPos is true string is not empty
                    readPos += _length; // Increase readPos by the length of the string
                }
                return _value; // Return the string
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }

        public List<int> ReadIntList(bool _moveReadPos = true)
        {
            List<int> retval = new List<int>();
            int elementCount = ReadInt(_moveReadPos);
            for (int i = 0; i < elementCount; i++)
            {
                retval.Add(ReadInt(_moveReadPos));
            }
            return retval;
        }
        public List<short> ReadShortList(bool _moveReadPos = true)
        {
            List<short> retval = new List<short>();
            int elementCount = ReadInt(_moveReadPos);
            for (int i = 0; i < elementCount; i++)
            {
                retval.Add(ReadShort(_moveReadPos));
            }
            return retval;
        }

        public List<ushort> ReadUShortList(bool _moveReadPos = true)
        {
            List<ushort> retval = new List<ushort>();
            int elementCount = ReadInt(_moveReadPos);
            for (int i = 0; i < elementCount; i++)
            {
                retval.Add(ReadUShort(_moveReadPos));
            }
            return retval;
        }
        public List<long> ReadLongList(bool _moveReadPos = true)
        {
            List<long> retval = new List<long>();
            int elementCount = ReadInt(_moveReadPos);
            for (int i = 0; i < elementCount; i++)
            {
                retval.Add(ReadLong(_moveReadPos));
            }
            return retval;
        }
        public List<string> ReadStringList(bool _moveReadPos = true)
        {
            List<string> retval = new List<string>();
            int elementCount = ReadInt(_moveReadPos);
            for (int i = 0; i < elementCount; i++)
            {
                retval.Add(ReadString(_moveReadPos));
            }
            return retval;
        }
        public List<bool> ReadBoolList(bool _moveReadPos = true)
        {
            List<bool> retval = new List<bool>();
            int elementCount = ReadInt(_moveReadPos);
            for (int i = 0; i < elementCount; i++)
            {
                retval.Add(ReadBool(_moveReadPos));
            }
            return retval;
        }
        public List<float> ReadFloatList(bool _moveReadPos = true)
        {
            List<float> retval = new List<float>();
            int elementCount = ReadInt(_moveReadPos);
            for (int i = 0; i < elementCount; i++)
            {
                retval.Add(ReadFloat(_moveReadPos));
            }
            return retval;
        }

        public UnityEngine.Vector3 ReadVector3(bool _moveReadPos = true)
        {
            return new UnityEngine.Vector3(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
        }

        #endregion

        private bool disposed = false;

        protected virtual void Dispose(bool _disposing)
        {
            if (!disposed)
            {
                if (_disposing)
                {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private byte[] FlipIfLittleEndian(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(arr);
            return arr;
        }
    }
}