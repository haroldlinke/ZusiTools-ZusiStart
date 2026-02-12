using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZusiCLIProject.Utils
{

    public class ZusiTcpSocket
    {
        public System.Threading.SynchronizationContext SyncContext { set; get; }
        public System.Net.Sockets.TcpClient TcpConnection { set; get; }
        public System.IO.Stream DataStream { private set; get; }
        private IAsyncResult Reading;
        private byte[] CurrentBuffer;
        private int CurrentPos;
        private int CurrentDataLength;
        private List<Int16> CurrentReadNodeI = new List<Int16>();
        private Int16 CurrentAttribute;
        public System.Collections.ObjectModel.ReadOnlyCollection<Int16> CurrentReadNode { private set; get; }
        public ZusiTcpSocket()
        {
            CurrentReadNode = new System.Collections.ObjectModel.ReadOnlyCollection<Int16>(CurrentReadNodeI);
        }
        public void StartRead()
        {
            if (DataStream != null)
                throw new System.InvalidOperationException();
            DataStream = TcpConnection.GetStream();
            CurrentBuffer = new byte[6];
            CurrentPos = 0;
            CurrentDataLength = -1;
            CurrentReadNodeI.Clear();
            Reading = DataStream.BeginRead(CurrentBuffer, 0, 4, ReadDone, null);
        }
        public void StartWriteByFile(System.IO.Stream stream)
		{
			if (DataStream != null)
				throw new System.InvalidOperationException();
			DataStream = stream;
		}
		public void StartReadByFile(System.IO.Stream stream)
		{
			if (DataStream != null)
				throw new System.InvalidOperationException();
			DataStream = stream;
			CurrentBuffer = new byte[6];
			CurrentPos = 0;
			CurrentDataLength = -1;
			CurrentReadNodeI.Clear();
			Reading = DataStream.BeginRead(CurrentBuffer, 0, 4, ReadDone, null);
		}
        //public void EndRead()
        //{
        //    ReadDone(Reading);
        //}
        private int emptyReadingCounter = 0;
        private void ReadDone(IAsyncResult ar)
        {
            //if (ar != Reading)
            //   throw new ArgumentOutOfRangeException();
            try
            {
                int cnt = DataStream.EndRead(ar);
                if (cnt == 0)
                {
                    ++emptyReadingCounter;
                    if (emptyReadingCounter > 100)
                        throw new System.IO.IOException("Socket closed");
                }
                else
                    emptyReadingCounter = 0;
                CurrentPos += cnt;
                if (TcpConnection != null && !TcpConnection.Connected)
                    return;
				if (CurrentDataLength == -1)
                {
                    if (CurrentPos >= 4)
                    {
                        int NodeLng = System.BitConverter.ToInt32(CurrentBuffer, 0);
                        if (NodeLng == -1)
                        {
                            Int16 oldNode = CurrentReadNodeI[CurrentReadNodeI.Count - 1];
                            CurrentReadNodeI.RemoveAt(CurrentReadNodeI.Count - 1);
                            CurrentPos = 0;
                            CurrentBuffer = new byte[6];
                            if (SyncContext == null)
                                NodeClosed?.Invoke(this, new NodeEventArgs(CurrentReadNodeI.ToArray(), oldNode, true));
                            else
                            {
                                SyncContext.Post(delegate (object state)
                                {
                                    NodeClosed?.Invoke(this, (NodeEventArgs)state);
                                }, new NodeEventArgs(CurrentReadNodeI.ToArray(), oldNode, true));
                            }
                            BeginRead(4 - CurrentPos, ReadDone, null);
                            return;
                        }
                        else if (CurrentPos >= 6)
                        {
                            Int16 newNode = System.BitConverter.ToInt16(CurrentBuffer, 4);
                            if (NodeLng == 0)
                            {
                                CurrentReadNodeI.Add(newNode);
                                CurrentPos = 0;
                                CurrentBuffer = new byte[6];
                                if (SyncContext == null)
                                    NodeOpend?.Invoke(this, new NodeEventArgs(CurrentReadNodeI.ToArray(), newNode, false));
                                else
                                {
                                    SyncContext.Post(delegate (object state)
                                    {
                                        NodeOpend?.Invoke(this, (NodeEventArgs)state);
                                    }, new NodeEventArgs(CurrentReadNodeI.ToArray(), newNode, false));
                                }
                                BeginRead(4 - CurrentPos, ReadDone, null);
                                return;
                            }
                            else
                            {
                                CurrentDataLength = NodeLng - 2;
                                CurrentBuffer = new byte[CurrentDataLength];
                                CurrentPos = 0;
                                CurrentAttribute = newNode;
						        BeginRead(CurrentDataLength - CurrentPos, ReadDone, null);
								return;
                            }
                        }
                        else
                        {
                            BeginRead(6 - CurrentPos, ReadDone, null);
                            return;
                        }
                    }
                    else
                    {
                        BeginRead(4 - CurrentPos, ReadDone, null);
                        return;
                    }
                }
                else
                {
                    if (CurrentPos >= CurrentDataLength)
                    {
                        if (SyncContext == null)
                            AttibuteRead?.Invoke(this, new AttributeEventArgs(CurrentReadNodeI.ToArray(), CurrentAttribute, CurrentBuffer));
                        else
                        {
                            SyncContext.Post(delegate (object state)
                            {
                                AttibuteRead?.Invoke(this, (AttributeEventArgs)state);
                            }, new AttributeEventArgs(CurrentReadNodeI.ToArray(), CurrentAttribute, CurrentBuffer));
                        }
                        CurrentDataLength = -1;
                        CurrentPos = 0;
                        CurrentBuffer = new byte[6];
                        BeginRead(4 - CurrentPos, ReadDone, null);
                        return;
                    }
                    else
                    {
                        BeginRead(CurrentDataLength - CurrentPos, ReadDone, null);
                        return;
                    }
                }
            }
            catch(System.Exception ex)
            {
                DataStream = null;
                CurrentBuffer = null;
                Reading = null;
                CurrentReadNodeI.Clear();
                if (SyncContext == null)
                    Closed?.Invoke(this, new System.IO.ErrorEventArgs(ex));
                else
                {
                    SyncContext.Post(delegate (object state)
                    {
                        Closed?.Invoke(this, (System.IO.ErrorEventArgs)state);
                    }, new System.IO.ErrorEventArgs(ex));
                }
            }
		}
        private int StackBuildupPreventerMethodCounter = 0;
		private int StackBuildupPreventerCount = 0;
		private AsyncCallback StackBuildupPreventerCallback = null;
		private object StackBuildupPreventerState = null;
		private void BeginRead(int count, AsyncCallback callback, object state)
		{
            ++StackBuildupPreventerMethodCounter;
            if (StackBuildupPreventerMethodCounter == 1)
            {
                Jumppoint:
                Reading = DataStream.BeginRead(CurrentBuffer, CurrentPos, count, callback, state);
				if (StackBuildupPreventerMethodCounter == 2)
				{
					count = StackBuildupPreventerCount;
					callback = StackBuildupPreventerCallback;
					state = StackBuildupPreventerState;

					StackBuildupPreventerCount = 0;
					StackBuildupPreventerCallback = null;
					StackBuildupPreventerState = null;

					--StackBuildupPreventerMethodCounter;
                    goto Jumppoint;
				}
                else
                    --StackBuildupPreventerMethodCounter;
			}
            else if (StackBuildupPreventerMethodCounter == 2)
			{
				StackBuildupPreventerCount = count;
				StackBuildupPreventerCallback = callback;
				StackBuildupPreventerState = state;
			}
			else
            {
				Reading = DataStream.BeginRead(CurrentBuffer, CurrentPos, count, callback, state);
				--StackBuildupPreventerMethodCounter;
			}
		}

		public event System.IO.ErrorEventHandler Closed;
        public event System.EventHandler<NodeEventArgs> NodeOpend;
        public event System.EventHandler<NodeEventArgs> NodeClosed;
        public event System.EventHandler<AttributeEventArgs> AttibuteRead;




        private List<byte> WriteBuffer;
        private List<Int16> CurrentWriteNodeI = null;
        public System.Collections.ObjectModel.ReadOnlyCollection<Int16> CurrentWriteNode { private set; get; }

        public void WritingBeginBuffering()
        {
            if (CurrentWriteNodeI != null)
                WritingEndBuffering();
            CurrentWriteNodeI = new List<Int16>();
            CurrentWriteNode = new System.Collections.ObjectModel.ReadOnlyCollection<Int16>(CurrentWriteNodeI);
            WriteBuffer = new List<byte>();
        }
        public void WritingEndBuffering()
        {
            if (CurrentWriteNodeI == null)
                return;
            WriteCloseNodes(CurrentWriteNodeI.Count);
            ((DataStream == null) ? TcpConnection.GetStream() : DataStream)
                .Write(WriteBuffer.ToArray(), 0, WriteBuffer.Count);
            WriteBuffer.Clear();
            CurrentWriteNodeI = null;
            CurrentWriteNode = null;
        }
        /// <summary>
        /// Writes a node. If in Bufferd Mode the Node remains open, otherwise it will be closed.
        /// </summary>
        /// <param name="adress"></param>
        public void WriteNode(Int16[] adress)
        {
            bool wasBuffered = (CurrentWriteNodeI != null);
            if (!wasBuffered)
                WritingBeginBuffering();
            int equals = 0;
            for (equals = 0; equals < Math.Min(adress.Length, CurrentWriteNodeI.Count); ++equals)
            {
                if (adress[equals] != CurrentWriteNodeI[equals])
                    break;
            }
            WriteCloseNodes(CurrentWriteNodeI.Count - equals);
            for (int i = equals; i < adress.Length; ++i)
            {
                WriteBuffer.AddRange(System.BitConverter.GetBytes((int)0));
                WriteBuffer.AddRange(System.BitConverter.GetBytes((Int16)adress[i]));
                CurrentWriteNodeI.Add(adress[i]);
            }
            if (!wasBuffered)
                WritingEndBuffering();
        }
        public void WriteCloseNodes(int count)
        {
            if (CurrentWriteNode.Count < count)
                throw new ArgumentOutOfRangeException("count");
            bool wasBuffered = (CurrentWriteNodeI != null);
            if (!wasBuffered)
                WritingBeginBuffering();
            for (int i = 0; i < count; ++i)
            {
                WriteBuffer.AddRange(System.BitConverter.GetBytes((int)-1));
                CurrentWriteNodeI.RemoveAt(CurrentWriteNodeI.Count - 1);
            }
            if (!wasBuffered)
                WritingEndBuffering();
        }
        public void WriteBytes(Int16[] nodeAdress, Int16 attribute, byte[] value)
        {
            bool wasBuffered = (CurrentWriteNodeI != null);
            if (!wasBuffered)
                WritingBeginBuffering();
            WriteNode(nodeAdress);
            WriteBuffer.AddRange(System.BitConverter.GetBytes((int)(value.Length + 2)));
            WriteBuffer.AddRange(System.BitConverter.GetBytes((Int16)attribute));
            WriteBuffer.AddRange(value);
            if (!wasBuffered)
                WritingEndBuffering();
        }
        public void WriteByte(Int16[] nodeAdress, Int16 attribute, byte value)
        {
            WriteBytes(nodeAdress, attribute, new byte[] { value });
        }
        public void WriteShortInt(Int16[] nodeAdress, Int16 attribute, SByte value)
        {
            unchecked
            {
                WriteByte(nodeAdress, attribute, (byte)value);
            }
        }
        public void WriteWord(Int16[] nodeAdress, Int16 attribute, Int16 value)
        {
            WriteBytes(nodeAdress, attribute, System.BitConverter.GetBytes(value));
        }
        public void WriteSmallInt(Int16[] nodeAdress, Int16 attribute, UInt16 value)
        {
            WriteBytes(nodeAdress, attribute, System.BitConverter.GetBytes(value));
        }
        public void WriteInteger(Int16[] nodeAdress, Int16 attribute, Int32 value)
        {
            WriteBytes(nodeAdress, attribute, System.BitConverter.GetBytes(value));
        }
        public void WriteCardinal(Int16[] nodeAdress, Int16 attribute, UInt32 value)
        {
            WriteBytes(nodeAdress, attribute, System.BitConverter.GetBytes(value));
        }
        public void WriteInteger64(Int16[] nodeAdress, Int16 attribute, Int64 value)
        {
            WriteBytes(nodeAdress, attribute, System.BitConverter.GetBytes(value));
        }
        public void WriteSingle(Int16[] nodeAdress, Int16 attribute, Single value)
        {
            WriteBytes(nodeAdress, attribute, System.BitConverter.GetBytes(value));
        }
        public void WriteDouble(Int16[] nodeAdress, Int16 attribute, Double value)
        {
            WriteBytes(nodeAdress, attribute, System.BitConverter.GetBytes(value));
        }
        public void WriteString(Int16[] nodeAdress, Int16 attribute, string value)
        {
            WriteBytes(nodeAdress, attribute, System.Text.Encoding.Default.GetBytes(value));
        }
        public void WriteEnum16<T>(Int16[] nodeAdress, Int16 attribute, T value)
        {
            WriteWord(nodeAdress, attribute, (short)(int)(object)value);
        }
    }
    public class NodeEventArgs : System.EventArgs
    {
        public NodeEventArgs() { }
        public NodeEventArgs(Int16[] openNodes, Int16 opendOrClosedNode, bool closedNode)
        {
            this.OpenNodes = openNodes;
            this.OpendOrClosedNode = opendOrClosedNode;
            this.ClosedNode = closedNode;
        }
        public Int16[] OpenNodes { set; get; }
        public Int16 OpendOrClosedNode { set; get; }
        public bool ClosedNode { set; get; }
    }
    public class AttributeEventArgs : System.EventArgs
    {
        public AttributeEventArgs() { }
        public AttributeEventArgs(Int16[] openNodes, Int16 attribute, byte[] value)
        {
            this.OpenNodes = openNodes;
            this.Attribute = attribute;
            this.Value = value;
        }
        public Int16[] OpenNodes { set; get; }
        public Int16 Attribute { set; get; }
        public byte[] Value { set; get; }
        public byte ToByte()
        {
            if (Value.Length != 1)
                throw new InvalidOperationException();
            return Value[0];
        }
        public SByte ToShortInt()
        {
            if (Value.Length != 1)
                throw new InvalidOperationException();
            unchecked
            {
                return (sbyte)Value[0];
            }
        }
        public UInt16 ToWord()
        {
            if (Value.Length != 2)
                throw new InvalidOperationException();
            return System.BitConverter.ToUInt16(Value, 0);
        }
        public Int16 ToSmallInt()
        {
            if (Value.Length != 2)
                throw new InvalidOperationException();
            return System.BitConverter.ToInt16(Value, 0);
        }
        public Int32 ToInteger()
        {
            if (Value.Length != 4)
                throw new InvalidOperationException();
            return System.BitConverter.ToInt32(Value, 0);
        }
        public UInt32 ToCardinal()
        {
            if (Value.Length != 4)
                throw new InvalidOperationException();
            return System.BitConverter.ToUInt32(Value, 0);
        }
        public Int64 ToInteger64()
        {
            if (Value.Length != 8)
                throw new InvalidOperationException();
            return System.BitConverter.ToInt64(Value, 0);
        }
        public Single ToSingle()
        {
            if (Value.Length != 4)
                throw new InvalidOperationException();
            return System.BitConverter.ToSingle(Value, 0);
        }
        public Double ToDouble()
        {
            if (Value.Length != 8)
                throw new InvalidOperationException();
            return System.BitConverter.ToDouble(Value, 0);
        }
        public override string ToString()
        {
            return System.Text.Encoding.Default.GetString(Value);
        }
        public T ToEnum16<T>()
        {
            return (T)System.Enum.ToObject(typeof(T), (int)ToWord());
        }
    }

}
