using NAudio.Wave;
using System;

namespace console_microphone
{
    class Program
    {
        static readonly int ChannelsNumber = 32;
        static readonly int BitRates = 44100;
        static readonly int Bits = 32;

        static void Main(string[] args)
        {
            var waveIn = new WaveInEvent
            {
                DeviceNumber = 0, // customize this to select your microphone device
                WaveFormat = new WaveFormatExtensible(rate: BitRates, bits: Bits, channels: ChannelsNumber),
                BufferMilliseconds = 50
            };
            waveIn.DataAvailable += ShowPeakCustom;
            waveIn.StartRecording();
            while (true) { }
        }

        private static string GetBars(double fraction, int barCount = 35)
        {
            var barsOn = (int)(barCount * fraction);
            var barsOff = barCount - barsOn;
            return new string('#', barsOn) + new string('-', barsOff);
        }

        private static void ShowPeakMono(object sender, WaveInEventArgs args)
        {
            float maxValue = 32767;
            int peakValue = 0;
            int bytesPerSample = 2;
            for (int index = 0; index < args.BytesRecorded; index += bytesPerSample)
            {
                int value = BitConverter.ToInt16(args.Buffer, index);
                peakValue = Math.Max(peakValue, value);
            }

            Console.WriteLine("L=" + GetBars(peakValue / maxValue));
        }

        private static void ShowPeakStereo(object sender, WaveInEventArgs args)
        {
            float maxValue = 32767;
            int peakL = 0;
            int peakR = 0;
            int bytesPerSample = 4;
            for (int index = 0; index < args.BytesRecorded; index += bytesPerSample)
            {
                int valueL = BitConverter.ToInt16(args.Buffer, index);
                peakL = Math.Max(peakL, valueL);
                int valueR = BitConverter.ToInt16(args.Buffer, index + 2);
                peakR = Math.Max(peakR, valueR);
            }

            Console.Write("L=" + GetBars(peakL / maxValue));
            Console.Write(" ");
            Console.Write("R=" + GetBars(peakR / maxValue));
            Console.Write("\n");
        }
        
        private static void ShowPeakCustom(object sender, WaveInEventArgs args)
        {
            float maxValue = 32767;
            var peaks = new int[ChannelsNumber];

            var bytesPerSample = 2 * ChannelsNumber;
            for (var index = 0; index < args.BytesRecorded; index += bytesPerSample)
            {
                for (var channel = 0; channel < ChannelsNumber; channel++)
                {
                    var value = BitConverter.ToInt16(args.Buffer, index + 2 * channel);
                    peaks[channel] = Math.Max(peaks[channel], value);
                }
            }

            for (var channel = 0; channel < ChannelsNumber; ++channel)
            {
                Console.WriteLine($"#{channel + 1}={GetBars(peaks[channel] / maxValue)}");
            }

            Console.WriteLine("");
        }
    }
}
