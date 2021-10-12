﻿using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;

namespace DigitalMusicAnalysis
{
	public class timefreq
	{
		public float[][] timeFreqData;
		public int wSamp;
		public Complex[] twiddles;

		public timefreq(float[] x, int windowSamp)
		{
			int ii;
			double pi = 3.14159265;
			Complex i = Complex.ImaginaryOne;
			this.wSamp = windowSamp;
			twiddles = new Complex[wSamp];
			for (ii = 0; ii < wSamp; ii++)
			{
				double a = 2 * pi * ii / (double)wSamp;
				twiddles[ii] = Complex.Pow(Complex.Exp(-i), (float)a);
			}

			timeFreqData = new float[wSamp/2][];

			int nearest = (int)Math.Ceiling((double)x.Length / (double)wSamp);
			nearest = nearest * wSamp;

			Complex[] compX = new Complex[nearest];
			for (int kk = 0; kk < nearest; kk++)
			{
				if (kk < x.Length)
				{
					compX[kk] = x[kk];
				}
				else
				{
					compX[kk] = Complex.Zero;
				}
			}


			int cols = 2 * nearest /wSamp;

			for (int jj = 0; jj < wSamp / 2; jj++)
			{
				timeFreqData[jj] = new float[cols];
			}

			timeFreqData = stft(compX, wSamp);
	
		}

		float[][] stft(Complex[] x, int wSamp)
		{
			int ii = 0;
			int kk = 0;
			int ll = 0;
			int N = x.Length;
			float fftMax = 0;
			
			float[][] Y = new float[wSamp / 2][];

			for (ll = 0; ll < wSamp / 2; ll++)
			{
				Y[ll] = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
			}

			//DateTime start = DateTime.Now;
			int max = (int)(2 * Math.Floor((double)N / (double)wSamp) - 1);
			Complex[][] tempFFT = new Complex[max][];

			Parallel.For(0, Environment.ProcessorCount, new ParallelOptions { MaxDegreeOfParallelism = MainWindow.DoP }, workerId =>
			{
				//int start = max * workerId / Environment.ProcessorCount;
				//int end = max * (workerId + 1) / Environment.ProcessorCount;
				for (int i = workerId; i < max; i += Environment.ProcessorCount)
				{
					Complex[] temp = new Complex[wSamp];

					for (int j = 0; j < wSamp; j++)
					{
						temp[j] = x[i * (wSamp / 2) + j];
					}

					tempFFT[i] = new Complex[wSamp];
					tempFFT[i] = fft(temp);
				}
			});
			//Trace.WriteLine("timefreq.stft fftLoop: " + (DateTime.Now - start).ToString());

			for (ii = 0; ii < max; ii++)
			{
				for (kk = 0; kk < wSamp / 2; kk++)
				{
					Y[kk][ii] = (float)Complex.Abs(tempFFT[ii][kk]);

					if (Y[kk][ii] > fftMax)
					{
						fftMax = Y[kk][ii];
					}
				}

			}

			for (ii = 0; ii < max; ii++)
			{
				for (kk = 0; kk < wSamp / 2; kk++)
				{
					Y[kk][ii] /= fftMax;
				}
			}

			return Y;
		}

		Complex[] fft(Complex[] x)
		{
			int ii = 0;
			int kk = 0;
			int N = x.Length;

			Complex[] Y = new Complex[N];

			// NEED TO MEMSET TO ZERO?

			if (N == 1)
			{
				Y[0] = x[0];
			}
			else{

				Complex[] even = new Complex[N/2];
				Complex[] odd = new Complex[N/2];

				for (ii = 0; ii < N; ii++)
				{

					if (ii % 2 == 0)
					{
						even[ii / 2] = x[ii];
					}
					if (ii % 2 == 1)
					{
						odd[(ii - 1) / 2] = x[ii];
					}
				}

				Complex[] E = fft(even);
				Complex[] O = fft(odd);

				for (kk = 0; kk < N; kk++)
				{
					Y[kk] = E[(kk % (N / 2))] + O[(kk % (N / 2))] * twiddles[kk * wSamp / N];
				}
			}

		   return Y;
		}
		
	}
}
