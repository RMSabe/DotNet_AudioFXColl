/*
	Test routine to test effects.

	Author: Rafael Sabe
	Email: rafaelmsabe@gmail.com
*/

using System;
using MyLib;
using AudioFXColl;

namespace Test;

public static class Program
{
	public static string UserInput = "";

	public static void Main(string[] args)
	{
		if(args.Length < 1)
		{
			Console.WriteLine("Error: missing arguments");
			ListExecArgs();
			WaitKeyPress();
			return;
		}

		args[0] = args[0].ToLower();

		if(args[0].Equals("bitcrush"))
		{
			bitcrushRuntime();
			return;
		}

		if(args[0].Equals("reverse"))
		{
			reverseRuntime();
			return;
		}

		if(args[0].Equals("channelswap"))
		{
			channelswapRuntime();
			return;
		}

		if(args[0].Equals("channelsubtract"))
		{
			channelsubtractRuntime();
			return;
		}

		Console.WriteLine("Error: invalid argument");
		ListExecArgs();
		WaitKeyPress();
	}

	public static void ListExecArgs()
	{
		Console.WriteLine("This executable requires 1 argument: <effect>");
		Console.WriteLine("Valid options are:");
		Console.WriteLine("\"bitcrush\"");
		Console.WriteLine("\"reverse\"");
		Console.WriteLine("\"channelswap\"");
		Console.WriteLine("\"channelsubtract\"");
		Console.Write("\r\n");
	}

	public static void WaitKeyPress()
	{
		Console.WriteLine("Press any key to continue...");
		Console.ReadKey();
	}

	public static string PromptUser(string text)
	{
		Console.Write(text);
		return Console.ReadLine();
	}

	public static void bitcrushRuntime()
	{
		AudioBitCrush audio = new AudioBitCrush();
		byte bitcrush = (byte) 0U;

		UserInput = PromptUser("Enter input file directory: ");

		audio.FileInDir = UserInput;

		UserInput = PromptUser("Enter output file directory: ");

		audio.FileOutDir = UserInput;

		UserInput = PromptUser("Enter bit crush level: ");

		try
		{
			bitcrush = Byte.Parse(UserInput);
		}
		catch(Exception e)
		{
			Console.WriteLine("Error: invalid value for bit crush entered");
			WaitKeyPress();
			return;
		}

		if(!audio.Initialize())
		{
			Console.WriteLine("Error occurred:");
			Console.WriteLine(audio.GetLastErrorMessage());
			return;
		}

		if(!audio.SetCutoff(bitcrush))
		{
			Console.WriteLine("Error occurred:");
			Console.WriteLine(audio.GetLastErrorMessage());
			return;
		}

		Console.WriteLine("DSP Started...");

		if(!audio.RunDSP())
		{
			Console.WriteLine("DSP Failed.");
			Console.WriteLine(audio.GetLastErrorMessage());
			return;
		}

		Console.WriteLine("DSP Finished.");
	}

	public static void reverseRuntime()
	{
		AudioReverse audio = new AudioReverse();

		UserInput = PromptUser("Enter input file directory: ");

		audio.FileInDir = UserInput;

		UserInput = PromptUser("Enter output file directory: ");

		audio.FileOutDir = UserInput;

		if(!audio.Initialize())
		{
			Console.WriteLine("Error Occurred:");
			Console.WriteLine(audio.GetLastErrorMessage());
			return;
		}

		Console.WriteLine("DSP Started...");

		if(!audio.RunDSP())
		{
			Console.WriteLine("DSP Failed.");
			Console.WriteLine(audio.GetLastErrorMessage());
			return;
		}

		Console.WriteLine("DSP Finished.");
	}

	public static void channelswapRuntime()
	{
		AudioChannelSwap audio = new AudioChannelSwap();

		UserInput = PromptUser("Enter input file directory: ");

		audio.FileInDir = UserInput;

		UserInput = PromptUser("Enter output file directory: ");

		audio.FileOutDir = UserInput;

		if(!audio.Initialize())
		{
			Console.WriteLine("Error Occurred:");
			Console.WriteLine(audio.GetLastErrorMessage());
			return;
		}

		Console.WriteLine("DSP Started...");

		if(!audio.RunDSP())
		{
			Console.WriteLine("DSP Failed.");
			Console.WriteLine(audio.GetLastErrorMessage());
			return;
		}

		Console.WriteLine("DSP Finished.");
	}

	public static void channelsubtractRuntime()
	{
		AudioChannelSubtract audio = new AudioChannelSubtract();

		UserInput = PromptUser("Enter input file directory: ");

		audio.FileInDir = UserInput;

		UserInput = PromptUser("Enter output file directory: ");

		audio.FileOutDir = UserInput;

		if(!audio.Initialize())
		{
			Console.WriteLine("Error Occurred:");
			Console.WriteLine(audio.GetLastErrorMessage());
			return;
		}

		Console.WriteLine("DSP Started...");

		if(!audio.RunDSP())
		{
			Console.WriteLine("DSP Failed.");
			Console.WriteLine(audio.GetLastErrorMessage());
			return;
		}

		Console.WriteLine("DSP Finished.");
	}
}