/*
	Audio FX Collection for dotnet core

	Author: Rafael Sabe
	Email: rafaelmsabe@gmail.com
*/

using System;
using System.IO;
using MyLib;

namespace AudioFXColl;

public abstract class AudioBaseClass
{
	protected const string FILETEMP_DIR = "temp.raw";
	protected const string FILEOUT_DIR_DEFAULT = "output.wav";

	//These variables will be set on initialization. They should be considered as constants.
	protected nuint BUFFER_SIZE_FRAMES = 512U;
	protected nuint BUFFER_SIZE_SAMPLES = 0U;
	protected nuint BUFFER_SIZE_BYTES = 0U;

	protected enum Format {
		NULL,
		UNSUPPORTED,
		I16,
		I24,
		FORMAT_MAX
	};

	protected enum Status {
		ERROR_BROKENHEADER,
		ERROR_FORMATNOTSUPPORTED,
		ERROR_FILENOTSUPPORTED,
		ERROR_NOFILE,
		ERROR_GENERIC,
		UNINITIALIZED,
		INITIALIZED,
		STATUS_MAX
	};

	protected Format format = Format.NULL;
	protected Status status = Status.UNINITIALIZED;

	protected string errorMsg = "";

	protected uint sampleRate = 0U;
	protected ushort bitDepth = (ushort) 0U;
	protected ushort nChannels = (ushort) 0U;

	protected long audioDataBegin = 0L;
	protected long audioDataEnd = 0L;

	protected FileStream? fileIn = null;
	protected FileStream? fileOut = null;
	protected FileStream? fileTemp = null;

	protected long fileInSize = 0L;
	protected long fileTempSize = 0L;

	protected long fileInPos = 0L;
	protected long fileOutPos = 0L;
	protected long fileTempPos = 0L;

	public string FileInDir = "";
	public string FileOutDir = "";

	public abstract bool RunDSP();

	public bool Initialize()
	{
		if(!this.fileExtCheck(this.FileInDir))
		{
			this.status = Status.ERROR_FILENOTSUPPORTED;
			return false;
		}

		if(!this.fileInOpen())
		{
			this.status = Status.ERROR_NOFILE;
			return false;
		}

		if(!this.fileInGetParams())
		{
			this.fileInClose();
			return false;
		}

		this.BUFFER_SIZE_SAMPLES = this.BUFFER_SIZE_FRAMES*((nuint) this.nChannels);
		this.BUFFER_SIZE_BYTES = this.BUFFER_SIZE_SAMPLES*((nuint) (this.bitDepth/8U));

		return true;
	}

	public string GetLastErrorMessage()
	{
		switch(this.status)
		{
			case Status.ERROR_BROKENHEADER:
				return "File header is missing information (probably corrupted).";

			case Status.ERROR_FORMATNOTSUPPORTED:
				return "Audio format is not supported.";

			case Status.ERROR_FILENOTSUPPORTED:
				return "File format is not supported.";

			case Status.ERROR_NOFILE:
				return "File does not exist, or it's not accessible.";

			case Status.ERROR_GENERIC:
				return "Something went wrong.";

			case Status.UNINITIALIZED:
				return "Audio object not initialized.";
		}

		return this.errorMsg;
	}

	public uint GetSampleRate()
	{
		return this.sampleRate;
	}

	public ushort GetBitDepth()
	{
		return this.bitDepth;
	}

	public ushort GetNumberChannels()
	{
		return this.nChannels;
	}

	protected bool fileInOpen()
	{
		try
		{
			this.fileIn = File.Open(this.FileInDir, FileMode.Open, FileAccess.Read);
			this.fileInSize = new FileInfo(this.FileInDir).Length;
		}
		catch(Exception e)
		{
			return false;
		}

		return true;
	}

	protected void fileInClose()
	{
		if(this.fileIn == null) return;

		this.fileIn.Close();
		this.fileIn = null;
		this.fileInSize = 0;
	}

	protected bool fileOutCreate()
	{
		try
		{
			if(File.Exists(this.FileOutDir)) File.Delete(this.FileOutDir);
			this.fileOut = File.Create(this.FileOutDir);
		}
		catch(Exception e)
		{
			return false;
		}

		return true;
	}

	protected void fileOutClose()
	{
		if(this.fileOut == null) return;

		this.fileOut.Close();
		this.fileOut = null;
	}

	protected bool fileTempCreate()
	{
		try
		{
			if(File.Exists(FILETEMP_DIR)) File.Delete(FILETEMP_DIR);
			this.fileTemp = File.Create(FILETEMP_DIR);
		}
		catch(Exception e)
		{
			return false;
		}

		return true;
	}

	protected bool fileTempOpen()
	{
		try
		{
			this.fileTemp = File.Open(FILETEMP_DIR, FileMode.Open, FileAccess.Read);
			this.fileTempSize = new FileInfo(FILETEMP_DIR).Length;
		}
		catch(Exception e)
		{
			return false;
		}

		return true;
	}

	protected void fileTempClose()
	{
		if(this.fileTemp == null) return;

		this.fileTemp.Close();
		this.fileTemp = null;
		this.fileTempSize = 0;
	}

	protected bool fileInGetParams()
	{
		const nuint BUFFER_SIZE = 4096U;
		nuint bytepos = 0U;
		byte[] headerInfo = new byte[BUFFER_SIZE];

		this.fileIn.Seek(0, SeekOrigin.Begin);
		this.fileIn.Read(headerInfo, 0, (int) BUFFER_SIZE);

		if(!this.compareSignature("RIFF", headerInfo, 0U))
		{
			this.status = Status.ERROR_BROKENHEADER;
			return false;
		}

		if(!this.compareSignature("WAVE", headerInfo, 8U))
		{
			this.status = Status.ERROR_BROKENHEADER;
			return false;
		}

		bytepos = 12U;

		while(!this.compareSignature("fmt ", headerInfo, bytepos))
		{
			if(bytepos >= (BUFFER_SIZE - 256U))
			{
				this.status = Status.ERROR_BROKENHEADER;
				return false;
			}

			bytepos += (nuint) (NumUtils.BytesToU32(headerInfo, (bytepos + 4U)) + 8U);
		}

		if(NumUtils.BytesToU16(headerInfo, (bytepos + 8U)) != 1U)
		{
			this.status = Status.ERROR_FORMATNOTSUPPORTED;
			return false;
		}

		this.nChannels = NumUtils.BytesToU16(headerInfo, (bytepos + 10U));
		this.sampleRate = NumUtils.BytesToU32(headerInfo, (bytepos + 12U));
		this.bitDepth = NumUtils.BytesToU16(headerInfo, (bytepos + 22U));

		bytepos += (nuint) (NumUtils.BytesToU32(headerInfo, (bytepos + 4U)) + 8U);

		while(!this.compareSignature("data", headerInfo, bytepos))
		{
			if(bytepos >= (BUFFER_SIZE - 256U))
			{
				this.status = Status.ERROR_BROKENHEADER;
				return false;
			}

			bytepos += (nuint) (NumUtils.BytesToU32(headerInfo, (bytepos + 4U)) + 8U);
		}

		this.audioDataBegin = (long) (bytepos + 8U);
		this.audioDataEnd = this.audioDataBegin + ((long) NumUtils.BytesToU32(headerInfo, (bytepos + 4U)));

		switch(this.bitDepth)
		{
			case ((ushort) 16U):
				this.format = Format.I16;
				this.status = Status.INITIALIZED;
				return true;

			case ((ushort) 24U):
				this.format = Format.I24;
				this.status = Status.INITIALIZED;
				return true;
		}

		this.format = Format.UNSUPPORTED;
		this.status = Status.ERROR_FORMATNOTSUPPORTED;
		return false;
	}

	protected void fileOutWriteHeader()
	{
		byte[] headerInfo = new byte[44];
		ushort u16 = (ushort) 0U;
		uint u32 = 0U;

		headerInfo[0] = (byte) 'R';
		headerInfo[1] = (byte) 'I';
		headerInfo[2] = (byte) 'F';
		headerInfo[3] = (byte) 'F';

		NumUtils.U32ToBytes(((uint) (this.fileTempSize + 36)), headerInfo, 4U);

		headerInfo[8] = (byte) 'W';
		headerInfo[9] = (byte) 'A';
		headerInfo[10] = (byte) 'V';
		headerInfo[11] = (byte) 'E';

		headerInfo[12] = (byte) 'f';
		headerInfo[13] = (byte) 'm';
		headerInfo[14] = (byte) 't';
		headerInfo[15] = (byte) ' ';

		NumUtils.U32ToBytes(16U, headerInfo, 16U);
		NumUtils.U16ToBytes((ushort) 1U, headerInfo, 20U);
		NumUtils.U16ToBytes(this.nChannels, headerInfo, 22U);
		NumUtils.U32ToBytes(this.sampleRate, headerInfo, 24U);
		NumUtils.U32ToBytes((this.sampleRate*((uint) (this.nChannels*this.bitDepth/8U))), headerInfo, 28U);
		NumUtils.U16ToBytes((ushort) (this.nChannels*this.bitDepth/8U), headerInfo, 32U);
		NumUtils.U16ToBytes(this.bitDepth, headerInfo, 34U);

		headerInfo[36] = (byte) 'd';
		headerInfo[37] = (byte) 'a';
		headerInfo[38] = (byte) 't';
		headerInfo[39] = (byte) 'a';

		NumUtils.U32ToBytes(((uint) this.fileTempSize), headerInfo, 40U);

		this.fileOut.Seek(0, SeekOrigin.Begin);
		this.fileOut.Write(headerInfo, 0, 44);
		this.fileOutPos = 44L;
	}

	protected bool fileExtCheck(string fileDir)
	{
		fileDir = fileDir.ToLower();
		char[] buf = fileDir.ToCharArray();
		nuint len = (nuint) fileDir.Length;

		if(len < 5U) return false;

		return this.compareSignature(".wav", buf, (len - 4U));
	}

	protected bool compareSignature(string auth, byte[] bytebuf, nuint offset)
	{
		if(auth[0] != ((char) bytebuf[offset])) return false;
		if(auth[1] != ((char) bytebuf[offset + 1U])) return false;
		if(auth[2] != ((char) bytebuf[offset + 2U])) return false;
		if(auth[3] != ((char) bytebuf[offset + 3U])) return false;

		return true;
	}

	protected bool compareSignature(string auth, char[] charbuf, nuint offset)
	{
		if(auth[0] != charbuf[offset]) return false;
		if(auth[1] != charbuf[offset + 1U]) return false;
		if(auth[2] != charbuf[offset + 2U]) return false;
		if(auth[3] != charbuf[offset + 3U]) return false;

		return true;
	}

	protected bool rawToWavProc()
	{
		if(this.status < Status.INITIALIZED) return false;

		if(!this.fileTempOpen())
		{
			this.errorMsg = "Could not open temporary DSP file.";
			return false;
		}

		if(!this.fileOutCreate())
		{
			this.fileTempClose();
			this.errorMsg = "Could not create output file.";
			return false;
		}

		this.fileOutWriteHeader();
		this.fileTempPos = 0L;

		this.rawToWavProcLoop();

		this.fileTempClose();
		this.fileOutClose();
		return true;
	}

	protected void rawToWavProcLoop()
	{
		byte[] bytebuf = new byte[this.BUFFER_SIZE_BYTES];

		while(this.fileTempPos < this.fileTempSize)
		{
			Array.Clear(bytebuf);

			this.fileTemp.Seek(this.fileTempPos, SeekOrigin.Begin);
			this.fileTemp.Read(bytebuf, 0, (int) this.BUFFER_SIZE_BYTES);
			this.fileTempPos += (long) this.BUFFER_SIZE_BYTES;

			this.fileOut.Seek(this.fileOutPos, SeekOrigin.Begin);
			this.fileOut.Write(bytebuf, 0, (int) this.BUFFER_SIZE_BYTES);
			this.fileOutPos += (long) this.BUFFER_SIZE_BYTES;
		}
	}
}
