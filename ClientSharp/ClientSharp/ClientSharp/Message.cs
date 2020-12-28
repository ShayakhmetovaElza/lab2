using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ClientSharp
{
	public enum Messages
	{
		M_INIT,
		M_EXIT,
		M_GETDATA,
		M_NODATA,
		M_TEXT,
		M_CONFIRM
	};

	public enum Members
	{
		M_BROKER = 0,
		M_ALL = 10,
		M_USER = 100
	};

	public struct MsgHeader
	{
		public int m_To;
		public int m_From;
		public Messages m_Type;
		public int m_Size;
	};
	class Message
	{
		private MsgHeader M_header;
		private string M_data;

		public Message()
		{
			M_header.m_To = 0;
			M_header.m_From = 0;
			M_header.m_Type = Messages.M_NODATA;
			M_header.m_Size = 0;
		}
		public Message(int to, int from, Messages type = Messages.M_TEXT, string data = "")
		{
			M_header.m_To = to;
			M_header.m_From = from;
			M_header.m_Type = type;
			M_header.m_Size = data.Length;
			M_data = data;
		}

		public string getMsgText()
        {
			return M_data;
        }

		void Send(Socket s)
		{
			s.Send(BitConverter.GetBytes(M_header.m_To), sizeof(int), SocketFlags.None);
			s.Send(BitConverter.GetBytes(M_header.m_From), sizeof(int), SocketFlags.None);
			s.Send(BitConverter.GetBytes((int)M_header.m_Type), sizeof(int), SocketFlags.None);
			s.Send(BitConverter.GetBytes(M_data.Length), sizeof(int), SocketFlags.None);
			if (M_header.m_Size != 0)
			{
				s.Send(Encoding.UTF8.GetBytes(M_data), M_data.Length, SocketFlags.None);
			}
		}

		public MsgHeader Receive(Socket s)
		{
			byte[] my_byte = new byte[4];
			s.Receive(my_byte, sizeof(int), SocketFlags.None);
			M_header.m_To = BitConverter.ToInt32(my_byte, 0);

			my_byte = new byte[4];
			s.Receive(my_byte, sizeof(int), SocketFlags.None);
			M_header.m_From = BitConverter.ToInt32(my_byte, 0);

			my_byte = new byte[4];
			s.Receive(my_byte, sizeof(int), SocketFlags.None);
			M_header.m_Type = (Messages)BitConverter.ToInt32(my_byte, 0);

			my_byte = new byte[4];
			s.Receive(my_byte, sizeof(int), SocketFlags.None);
			M_header.m_Size = BitConverter.ToInt32(my_byte, 0);

			if (M_header.m_Size != 0)
			{
				my_byte = new byte[M_header.m_Size];
				s.Receive(my_byte, M_header.m_Size, SocketFlags.None);
				M_data = Encoding.UTF8.GetString(my_byte, 0, M_header.m_Size);
			}
			return M_header;
		}

		public void SendMessage(Socket s, int To, int From, Messages Type = Messages.M_TEXT, string Data = "")
		{
			Message msg = new Message(To, From, Type, Data);
			msg.Send(s);
		}
	}
}

