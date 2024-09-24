/*
	Audio FX Collection for dotnet core

	Author: Rafael Sabe
	Email: rafaelmsabe@gmail.com
*/

using System;
using System.IO;
using MyLib;

namespace AudioFXColl;

public class AudioChannelSubtract : AudioBaseClass
{
	public AudioChannelSubtract()
	{
		this.status = Status.UNINITIALIZED;
		this.FileOutDir = FILEOUT_DIR_DEFAULT;
	}

	public AudioChannelSubtract(string fileInDir) : this()
	{
		this.FileInDir = fileInDir;
	}

	public AudioChannelSubtract(string fileInDir, string fileOutDir) : this(fileInDir)
	{
		this.FileOutDir = fileOutDir;
	}

	public override bool RunDSP()
	{
		if(this.status < Status.INITIALIZED) return false;

		if(this.fileIn == null)
		{
			this.status = Status.ERROR_NOFILE;
			return false;
		}

		if(this.nChannels < 2U)
		{
			this.errorMsg = "This effect cannot be run on single channel audio.";
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

		nuint nFrame = 0U;
		nuint nSample = 0U;
		nuint nChannel = 0U;
		nuint nByte = 0U;

		const int SAMPLE_MAX_VALUE = 0x7fff;
		const int SAMPLE_MIN_VALUE = -0x8000;

		int monoSample = 0;
		int channelSample = 0;

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
				nByte += 2U;
			}

			for(nFrame = 0U; nFrame < this.BUFFER_SIZE_FRAMES; nFrame++)
			{
				monoSample = 0;

				for(nChannel = 0U; nChannel < ((nuint) this.nChannels); nChannel++)
				{
					nSample = nFrame*((nuint) this.nChannels) + nChannel;
					monoSample += (int) buffer_i16[nSample];
				}

				for(nChannel = 0U; nChannel < ((nuint) this.nChannels); nChannel++)
				{
					nSample = nFrame*((nuint) this.nChannels) + nChannel;

					channelSample = (int) buffer_i16[nSample];
					channelSample *= (int) this.nChannels;
					channelSample -= monoSample;
					channelSample /= (int) this.nChannels;

					if(channelSample >= SAMPLE_MAX_VALUE) buffer_i16[nSample] = (short) SAMPLE_MAX_VALUE;
					else if(channelSample <= SAMPLE_MIN_VALUE) buffer_i16[nSample] = (short) SAMPLE_MIN_VALUE;
					else buffer_i16[nSample] = (short) channelSample;
				}
			}

			nByte = 0U;
			for(nSample = 0U; nSample < this.BUFFER_SIZE_SAMPLES; nSample++)
			{
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

		nuint nFrame = 0U;
		nuint nSample = 0U;
		nuint nChannel = 0U;
		nuint nByte = 0U;

		const int SAMPLE_MAX_VALUE = 0x7fffff;
		const int SAMPLE_MIN_VALUE = -0x800000;

		int monoSample = 0;

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

				nByte += 3U;
			}

			for(nFrame = 0U; nFrame < this.BUFFER_SIZE_FRAMES; nFrame++)
			{
				monoSample = 0;

				for(nChannel = 0U; nChannel < ((nuint) this.nChannels); nChannel++)
				{
					nSample = nFrame*((nuint) this.nChannels) + nChannel;
					monoSample += buffer_i32[nSample];
				}

				for(nChannel = 0U; nChannel < ((nuint) this.nChannels); nChannel++)
				{
					nSample = nFrame*((nuint) this.nChannels) + nChannel;

					buffer_i32[nSample] *= (int) this.nChannels;
					buffer_i32[nSample] -= monoSample;
					buffer_i32[nSample] /= (int) this.nChannels;

					if(buffer_i32[nSample] > SAMPLE_MAX_VALUE) buffer_i32[nSample] = SAMPLE_MAX_VALUE;
					else if(buffer_i32[nSample] < SAMPLE_MIN_VALUE) buffer_i32[nSample] = SAMPLE_MIN_VALUE;
				}
			}

			nByte = 0U;
			for(nSample = 0U; nSample < this.BUFFER_SIZE_SAMPLES; nSample++)
			{
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
