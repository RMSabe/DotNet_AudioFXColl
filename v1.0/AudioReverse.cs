/*
	Audio FX Collection for dotnet core

	Author: Rafael Sabe
	Email: rafaelmsabe@gmail.com
*/

using System;
using System.IO;
using MyLib;

namespace AudioFXColl;

public class AudioReverse : AudioBaseClass
{
	public AudioReverse()
	{
		this.status = Status.UNINITIALIZED;
		this.FileOutDir = FILEOUT_DIR_DEFAULT;
	}

	public AudioReverse(string fileInDir) : this()
	{
		this.FileInDir = fileInDir;
	}

	public AudioReverse(string fileInDir, string fileOutDir) : this(fileInDir)
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

		if(!this.fileTempCreate())
		{
			this.errorMsg = "Could not create temporary DSP file.";
			return false;
		}

		switch(this.format)
		{
			case Format.I16:
				this.dspInit_i16();
				this.dspLoop_i16();
				break;

			case Format.I24:
				this.dspInit_i24();
				this.dspLoop_i24();
				break;
		}

		this.fileTempClose();

		return this.rawToWavProc();
	}

	private void dspInit_i16()
	{
		long AUDIO_DATALENGTH_BYTES = this.audioDataEnd - this.audioDataBegin;
		long AUDIO_DATALENGTH_SAMPLES = AUDIO_DATALENGTH_BYTES/2L;
		long AUDIO_DATALENGTH_FRAMES = AUDIO_DATALENGTH_SAMPLES/((long) this.nChannels);

		nuint N_FRAMES_REMAINING = (nuint) (AUDIO_DATALENGTH_FRAMES%((long) this.BUFFER_SIZE_FRAMES));
		nuint N_SAMPLES_REMAINING = N_FRAMES_REMAINING*((nuint) this.nChannels);
		nuint N_BYTES_REMAINING = N_SAMPLES_REMAINING*2U;

		short[] bufferin_i16 = new short[this.BUFFER_SIZE_SAMPLES];
		short[] bufferout_i16 = new short[this.BUFFER_SIZE_SAMPLES];
		byte[] bytebuf = new byte[this.BUFFER_SIZE_BYTES];

		nuint nFrame = 0U;
		nuint nCounterFrame = 0U;
		nuint nSample = 0U;
		nuint nCounterSample = 0U;
		nuint nChannel = 0U;
		nuint nByte = 0U;

		this.fileInPos = this.audioDataEnd - ((long) N_BYTES_REMAINING);

		this.fileIn.Seek(this.fileInPos, SeekOrigin.Begin);
		this.fileIn.Read(bytebuf, 0, (int) N_BYTES_REMAINING);
		this.fileInPos -= (long) this.BUFFER_SIZE_BYTES;

		nByte = 0U;
		for(nSample = 0U; nSample < N_SAMPLES_REMAINING; nSample++)
		{
			bufferin_i16[nSample] = NumUtils.BytesToI16(bytebuf, nByte);
			nByte += 2U;
		}

		for(nFrame = 0U; nFrame < N_FRAMES_REMAINING; nFrame++)
		{
			nCounterFrame = N_FRAMES_REMAINING - nFrame - 1U;

			for(nChannel = 0U; nChannel < ((nuint) this.nChannels); nChannel++)
			{
				nSample = nFrame*((nuint) this.nChannels) + nChannel;
				nCounterSample = nCounterFrame*((nuint) this.nChannels) + nChannel;

				bufferout_i16[nSample] = bufferin_i16[nCounterSample];
			}
		}

		nByte = 0U;
		for(nSample = 0U; nSample < N_SAMPLES_REMAINING; nSample++)
		{
			NumUtils.I16ToBytes(bufferout_i16[nSample], bytebuf, nByte);
			nByte += 2U;
		}

		this.fileTemp.Seek(0, SeekOrigin.Begin);
		this.fileTemp.Write(bytebuf, 0, (int) N_BYTES_REMAINING);
		this.fileTempPos = (long) N_BYTES_REMAINING;
	}

	private void dspInit_i24()
	{
		long AUDIO_DATALENGTH_BYTES = this.audioDataEnd - this.audioDataBegin;
		long AUDIO_DATALENGTH_SAMPLES = AUDIO_DATALENGTH_BYTES/3L;
		long AUDIO_DATALENGTH_FRAMES = AUDIO_DATALENGTH_SAMPLES/((long) this.nChannels);

		nuint N_FRAMES_REMAINING = (nuint) (AUDIO_DATALENGTH_FRAMES%((long) this.BUFFER_SIZE_FRAMES));
		nuint N_SAMPLES_REMAINING = N_FRAMES_REMAINING*((nuint) this.nChannels);
		nuint N_BYTES_REMAINING = N_SAMPLES_REMAINING*3U;

		int[] bufferin_i32 = new int[this.BUFFER_SIZE_SAMPLES];
		int[] bufferout_i32 = new int[this.BUFFER_SIZE_SAMPLES];
		byte[] bytebuf = new byte[this.BUFFER_SIZE_BYTES];

		nuint nFrame = 0U;
		nuint nCounterFrame = 0U;
		nuint nSample = 0U;
		nuint nCounterSample = 0U;
		nuint nChannel = 0U;
		nuint nByte = 0U;

		this.fileInPos = this.audioDataEnd - ((long) N_BYTES_REMAINING);

		this.fileIn.Seek(this.fileInPos, SeekOrigin.Begin);
		this.fileIn.Read(bytebuf, 0, (int) N_BYTES_REMAINING);
		this.fileInPos -= (long) this.BUFFER_SIZE_BYTES;

		nByte = 0U;
		for(nSample = 0U; nSample < N_SAMPLES_REMAINING; nSample++)
		{
			bufferin_i32[nSample] = ((bytebuf[nByte + 2U] << 16) | (bytebuf[nByte + 1U] << 8) | (bytebuf[nByte]));

			if((bufferin_i32[nSample] & 0x00800000) != 0) bufferin_i32[nSample] |= unchecked((int) 0xff800000);
			else bufferin_i32[nSample] &= 0x007fffff; //Not really necessary, but just to be safe.

			nByte += 3U;
		}

		for(nFrame = 0U; nFrame < N_FRAMES_REMAINING; nFrame++)
		{
			nCounterFrame = N_FRAMES_REMAINING - nFrame - 1U;

			for(nChannel = 0U; nChannel < ((nuint) this.nChannels); nChannel++)
			{
				nSample = nFrame*((nuint) this.nChannels) + nChannel;
				nCounterSample = nCounterFrame*((nuint) this.nChannels) + nChannel;

				bufferout_i32[nSample] = bufferin_i32[nCounterSample];
			}
		}

		nByte = 0U;
		for(nSample = 0U; nSample < N_SAMPLES_REMAINING; nSample++)
		{
			bytebuf[nByte] = (byte) (bufferout_i32[nSample] & 0xff);
			bytebuf[nByte + 1U] = (byte) ((bufferout_i32[nSample] >> 8) & 0xff);
			bytebuf[nByte + 2U] = (byte) ((bufferout_i32[nSample] >> 16) & 0xff);

			nByte += 3U;
		}

		this.fileTemp.Seek(0, SeekOrigin.Begin);
		this.fileTemp.Write(bytebuf, 0, (int) N_BYTES_REMAINING);
		this.fileTempPos = (long) N_BYTES_REMAINING;
	}

	private void dspLoop_i16()
	{
		short[] bufferin_i16 = new short[this.BUFFER_SIZE_SAMPLES];
		short[] bufferout_i16 = new short[this.BUFFER_SIZE_SAMPLES];
		byte[] bytebuf = new byte[this.BUFFER_SIZE_BYTES];

		nuint nFrame = 0U;
		nuint nCounterFrame = 0U;
		nuint nSample = 0U;
		nuint nCounterSample = 0U;
		nuint nChannel = 0U;
		nuint nByte = 0U;

		while(this.fileInPos >= this.audioDataBegin)
		{
			Array.Clear(bytebuf);

			this.fileIn.Seek(this.fileInPos, SeekOrigin.Begin);
			this.fileIn.Read(bytebuf, 0, (int) this.BUFFER_SIZE_BYTES);

			if(this.fileInPos <= ((long) this.BUFFER_SIZE_BYTES)) this.fileInPos = 0L;
			else this.fileInPos -= (long) this.BUFFER_SIZE_BYTES;

			nByte = 0U;
			for(nSample = 0U; nSample < this.BUFFER_SIZE_SAMPLES; nSample++)
			{
				bufferin_i16[nSample] = NumUtils.BytesToI16(bytebuf, nByte);
				nByte += 2U;
			}

			for(nFrame = 0U; nFrame < this.BUFFER_SIZE_FRAMES; nFrame++)
			{
				nCounterFrame = this.BUFFER_SIZE_FRAMES - nFrame - 1U;

				for(nChannel = 0U; nChannel < ((nuint) this.nChannels); nChannel++)
				{
					nSample = nFrame*((nuint) this.nChannels) + nChannel;
					nCounterSample = nCounterFrame*((nuint) this.nChannels) + nChannel;

					bufferout_i16[nSample] = bufferin_i16[nCounterSample];
				}
			}

			nByte = 0U;
			for(nSample = 0U; nSample < this.BUFFER_SIZE_SAMPLES; nSample++)
			{
				NumUtils.I16ToBytes(bufferout_i16[nSample], bytebuf, nByte);
				nByte += 2U;
			}

			this.fileTemp.Seek(this.fileTempPos, SeekOrigin.Begin);
			this.fileTemp.Write(bytebuf, 0, (int) this.BUFFER_SIZE_BYTES);
			this.fileTempPos += (long) this.BUFFER_SIZE_BYTES;
		}
	}

	private void dspLoop_i24()
	{
		int[] bufferin_i32 = new int[this.BUFFER_SIZE_SAMPLES];
		int[] bufferout_i32 = new int[this.BUFFER_SIZE_SAMPLES];
		byte[] bytebuf = new byte[this.BUFFER_SIZE_BYTES];

		nuint nFrame = 0U;
		nuint nCounterFrame = 0U;
		nuint nSample = 0U;
		nuint nCounterSample = 0U;
		nuint nChannel = 0U;
		nuint nByte = 0U;

		while(this.fileInPos >= this.audioDataBegin)
		{
			Array.Clear(bytebuf);

			this.fileIn.Seek(this.fileInPos, SeekOrigin.Begin);
			this.fileIn.Read(bytebuf, 0, (int) this.BUFFER_SIZE_BYTES);

			if(this.fileInPos <= ((long) this.BUFFER_SIZE_BYTES)) this.fileInPos = 0L;
			else this.fileInPos -= (long) this.BUFFER_SIZE_BYTES;

			nByte = 0U;
			for(nSample = 0U; nSample < this.BUFFER_SIZE_SAMPLES; nSample++)
			{
				bufferin_i32[nSample] = ((bytebuf[nByte + 2U] << 16) | (bytebuf[nByte + 1U] << 8) | (bytebuf[nByte]));

				if((bufferin_i32[nSample] & 0x00800000) != 0) bufferin_i32[nSample] |= unchecked((int) 0xff800000);
				else bufferin_i32[nSample] &= 0x007fffff; //Not really necessary, but just to be safe.

				nByte += 3U;
			}

			for(nFrame = 0U; nFrame < this.BUFFER_SIZE_FRAMES; nFrame++)
			{
				nCounterFrame = this.BUFFER_SIZE_FRAMES - nFrame - 1U;

				for(nChannel = 0U; nChannel < ((nuint) this.nChannels); nChannel++)
				{
					nSample = nFrame*((nuint) this.nChannels) + nChannel;
					nCounterSample = nCounterFrame*((nuint) this.nChannels) + nChannel;

					bufferout_i32[nSample] = bufferin_i32[nCounterSample];
				}
			}

			nByte = 0U;
			for(nSample = 0U; nSample < this.BUFFER_SIZE_SAMPLES; nSample++)
			{
				bytebuf[nByte] = (byte) (bufferout_i32[nSample] & 0xff);
				bytebuf[nByte + 1U] = (byte) ((bufferout_i32[nSample] >> 8) & 0xff);
				bytebuf[nByte + 2U] = (byte) ((bufferout_i32[nSample] >> 16) & 0xff);

				nByte += 3U;
			}

			this.fileTemp.Seek(this.fileTempPos, SeekOrigin.Begin);
			this.fileTemp.Write(bytebuf, 0, (int) this.BUFFER_SIZE_BYTES);
			this.fileTempPos += (long) this.BUFFER_SIZE_BYTES;
		}
	}
}
