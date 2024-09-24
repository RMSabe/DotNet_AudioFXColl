/*
	Audio FX Collection for dotnet core

	Author: Rafael Sabe
	Email: rafaelmsabe@gmail.com
*/

using System;
using System.IO;
using MyLib;

namespace AudioFXColl;

public class AudioBitCrush : AudioBaseClass
{
	private int cutoff = 0;

	public AudioBitCrush()
	{
		this.status = Status.UNINITIALIZED;
		this.FileOutDir = FILEOUT_DIR_DEFAULT;
	}

	public AudioBitCrush(string fileInDir) : this()
	{
		this.FileInDir = fileInDir;
	}

	public AudioBitCrush(string fileInDir, string fileOutDir) : this(fileInDir)
	{
		this.FileOutDir = fileOutDir;
	}

	public bool SetCutoff(byte bitcrush)
	{
		switch(this.format)
		{
			case Format.I16:
				if(bitcrush >= 15U)
				{
					this.errorMsg = "Bit crush exceeds sample limit.";
					return false;
				}
				break;

			case Format.I24:
				if(bitcrush >= 23U)
				{
					this.errorMsg = "Bit crush exceeds sample limit.";
					return false;
				}
				break;
		}

		this.cutoff = 0;

		byte b = (byte) 0U;
		while(b < bitcrush)
		{
			this.cutoff |= (1 << b);
			b++;
		}

		return true;
	}

	public override bool RunDSP()
	{
		if(this.status < Status.INITIALIZED) return false;

		if(this.fileIn == null)
		{
			this.status = Status.ERROR_NOFILE;
			return false;
		}

		if(!this.fileTempCreate())
		{
			this.errorMsg = "Could not create temporary DSP file.";
			return false;
		}

		this.fileInPos = this.audioDataBegin;
		this.fileTempPos = 0L;

		switch(this.format)
		{
			case Format.I16:
				this.dspLoop_i16();
				break;

			case Format.I24:
				this.dspLoop_i24();
				break;
		}

		this.fileTempClose();

		return this.rawToWavProc();
	}

	private void dspLoop_i16()
	{
		short[] buffer_i16 = new short[this.BUFFER_SIZE_SAMPLES];
		byte[] bytebuf = new byte[this.BUFFER_SIZE_BYTES];

		nuint nSample = 0U;
		nuint nByte = 0U;

		while(this.fileInPos < this.audioDataEnd)
		{
			Array.Clear(bytebuf);

			this.fileIn.Seek(this.fileInPos, SeekOrigin.Begin);
			this.fileIn.Read(bytebuf, 0, (int) this.BUFFER_SIZE_BYTES);
			this.fileInPos += (long) this.BUFFER_SIZE_BYTES;

			nByte = 0U;
			for(nSample = 0U; nSample < this.BUFFER_SIZE_SAMPLES; nSample++)
			{
				buffer_i16[nSample] = NumUtils.BytesToI16(bytebuf, nByte);

				buffer_i16[nSample] &= (short) (~this.cutoff);

				NumUtils.I16ToBytes(buffer_i16[nSample], bytebuf, nByte);

				nByte += 2U;
			}

			this.fileTemp.Seek(this.fileTempPos, SeekOrigin.Begin);
			this.fileTemp.Write(bytebuf, 0, (int) this.BUFFER_SIZE_BYTES);
			this.fileTempPos += (long) this.BUFFER_SIZE_BYTES;
		}
	}

	private void dspLoop_i24()
	{
		int[] buffer_i32 = new int[this.BUFFER_SIZE_SAMPLES];
		byte[] bytebuf = new byte[this.BUFFER_SIZE_BYTES];

		nuint nSample = 0U;
		nuint nByte = 0U;

		while(this.fileInPos < this.audioDataEnd)
		{
			Array.Clear(bytebuf);

			this.fileIn.Seek(this.fileInPos, SeekOrigin.Begin);
			this.fileIn.Read(bytebuf, 0, (int) this.BUFFER_SIZE_BYTES);
			this.fileInPos += (long) this.BUFFER_SIZE_BYTES;

			nByte = 0U;
			for(nSample = 0U; nSample < this.BUFFER_SIZE_SAMPLES; nSample++)
			{
				buffer_i32[nSample] = ((bytebuf[nByte + 2U] << 16) | (bytebuf[nByte + 1U] << 8) | (bytebuf[nByte]));

				if((buffer_i32[nSample] & 0x00800000) != 0) buffer_i32[nSample] |= unchecked((int) 0xff800000);
				else buffer_i32[nSample] &= 0x007fffff; //Not really necessary, but just to be safe.

				buffer_i32[nSample] &= ~this.cutoff;

				bytebuf[nByte] = (byte) (buffer_i32[nSample] & 0xff);
				bytebuf[nByte + 1U] = (byte) ((buffer_i32[nSample] >> 8) & 0xff);
				bytebuf[nByte + 2U] = (byte) ((buffer_i32[nSample] >> 16) & 0xff);

				nByte += 3U;
			}

			this.fileTemp.Seek(this.fileTempPos, SeekOrigin.Begin);
			this.fileTemp.Write(bytebuf, 0, (int) this.BUFFER_SIZE_BYTES);
			this.fileTempPos += (long) this.BUFFER_SIZE_BYTES;
		}
	}
}
