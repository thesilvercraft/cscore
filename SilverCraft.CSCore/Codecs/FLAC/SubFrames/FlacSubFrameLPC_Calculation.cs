using System;

namespace SilverCraft.CSCore.Codecs.FLAC
{
    internal sealed partial class FlacSubFrameLPC
    {
        private static unsafe void RestoreLPCSignal32(ReadOnlySpan<int> residual, Span<int> destination, int length, int order, ReadOnlySpan<int> qlpCoeff, int lpcShiftNeeded)
        {
            if ((uint)length > (uint)residual.Length || (uint)length > (uint)destination.Length || (uint)order > (uint)qlpCoeff.Length)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (length <= order)
                return;

            fixed (int* pRes = residual, pDest = destination, q = qlpCoeff)
            {
                if (order <= 12)
                {
                    int z;
                    switch (order)
                    {
                        case 12:
                        {
                            int q11 = q[11];
                            int q10 = q[10];
                            int q9 = q[9];
                            int q8 = q[8];
                            int q7 = q[7];
                            int q6 = q[6];
                            int q5 = q[5];
                            int q4 = q[4];
                            int q3 = q[3];
                            int q2 = q[2];
                            int q1 = q[1];
                            int q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q11 * pDest[i - 12]) +
                                    (q10 * pDest[i - 11]) +
                                    (q9 * pDest[i - 10]) +
                                    (q8 * pDest[i - 9]) +
                                    (q7 * pDest[i - 8]) +
                                    (q6 * pDest[i - 7]) +
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 11:
                        {
                            int q10 = q[10];
                            int q9 = q[9];
                            int q8 = q[8];
                            int q7 = q[7];
                            int q6 = q[6];
                            int q5 = q[5];
                            int q4 = q[4];
                            int q3 = q[3];
                            int q2 = q[2];
                            int q1 = q[1];
                            int q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q10 * pDest[i - 11]) +
                                    (q9 * pDest[i - 10]) +
                                    (q8 * pDest[i - 9]) +
                                    (q7 * pDest[i - 8]) +
                                    (q6 * pDest[i - 7]) +
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 10:
                        {
                            int q9 = q[9];
                            int q8 = q[8];
                            int q7 = q[7];
                            int q6 = q[6];
                            int q5 = q[5];
                            int q4 = q[4];
                            int q3 = q[3];
                            int q2 = q[2];
                            int q1 = q[1];
                            int q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q9 * pDest[i - 10]) +
                                    (q8 * pDest[i - 9]) +
                                    (q7 * pDest[i - 8]) +
                                    (q6 * pDest[i - 7]) +
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 9:
                        {
                            int q8 = q[8];
                            int q7 = q[7];
                            int q6 = q[6];
                            int q5 = q[5];
                            int q4 = q[4];
                            int q3 = q[3];
                            int q2 = q[2];
                            int q1 = q[1];
                            int q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q8 * pDest[i - 9]) +
                                    (q7 * pDest[i - 8]) +
                                    (q6 * pDest[i - 7]) +
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 8:
                        {
                            int q7 = q[7];
                            int q6 = q[6];
                            int q5 = q[5];
                            int q4 = q[4];
                            int q3 = q[3];
                            int q2 = q[2];
                            int q1 = q[1];
                            int q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q7 * pDest[i - 8]) +
                                    (q6 * pDest[i - 7]) +
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 7:
                        {
                            int q6 = q[6];
                            int q5 = q[5];
                            int q4 = q[4];
                            int q3 = q[3];
                            int q2 = q[2];
                            int q1 = q[1];
                            int q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q6 * pDest[i - 7]) +
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 6:
                        {
                            int q5 = q[5];
                            int q4 = q[4];
                            int q3 = q[3];
                            int q2 = q[2];
                            int q1 = q[1];
                            int q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 5:
                        {
                            int q4 = q[4];
                            int q3 = q[3];
                            int q2 = q[2];
                            int q1 = q[1];
                            int q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 4:
                        {
                            int q3 = q[3];
                            int q2 = q[2];
                            int q1 = q[1];
                            int q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 3:
                        {
                            int q2 = q[2];
                            int q1 = q[1];
                            int q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 2:
                        {
                            int q1 = q[1];
                            int q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 1:
                        {
                            int q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    int z;
                    switch (order)
                    {
                        case 32:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[31] * pDest[i - 32]) +
                                    (q[30] * pDest[i - 31]) +
                                    (q[29] * pDest[i - 30]) +
                                    (q[28] * pDest[i - 29]) +
                                    (q[27] * pDest[i - 28]) +
                                    (q[26] * pDest[i - 27]) +
                                    (q[25] * pDest[i - 26]) +
                                    (q[24] * pDest[i - 25]) +
                                    (q[23] * pDest[i - 24]) +
                                    (q[22] * pDest[i - 23]) +
                                    (q[21] * pDest[i - 22]) +
                                    (q[20] * pDest[i - 21]) +
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 31:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[30] * pDest[i - 31]) +
                                    (q[29] * pDest[i - 30]) +
                                    (q[28] * pDest[i - 29]) +
                                    (q[27] * pDest[i - 28]) +
                                    (q[26] * pDest[i - 27]) +
                                    (q[25] * pDest[i - 26]) +
                                    (q[24] * pDest[i - 25]) +
                                    (q[23] * pDest[i - 24]) +
                                    (q[22] * pDest[i - 23]) +
                                    (q[21] * pDest[i - 22]) +
                                    (q[20] * pDest[i - 21]) +
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 30:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[29] * pDest[i - 30]) +
                                    (q[28] * pDest[i - 29]) +
                                    (q[27] * pDest[i - 28]) +
                                    (q[26] * pDest[i - 27]) +
                                    (q[25] * pDest[i - 26]) +
                                    (q[24] * pDest[i - 25]) +
                                    (q[23] * pDest[i - 24]) +
                                    (q[22] * pDest[i - 23]) +
                                    (q[21] * pDest[i - 22]) +
                                    (q[20] * pDest[i - 21]) +
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 29:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[28] * pDest[i - 29]) +
                                    (q[27] * pDest[i - 28]) +
                                    (q[26] * pDest[i - 27]) +
                                    (q[25] * pDest[i - 26]) +
                                    (q[24] * pDest[i - 25]) +
                                    (q[23] * pDest[i - 24]) +
                                    (q[22] * pDest[i - 23]) +
                                    (q[21] * pDest[i - 22]) +
                                    (q[20] * pDest[i - 21]) +
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 28:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[27] * pDest[i - 28]) +
                                    (q[26] * pDest[i - 27]) +
                                    (q[25] * pDest[i - 26]) +
                                    (q[24] * pDest[i - 25]) +
                                    (q[23] * pDest[i - 24]) +
                                    (q[22] * pDest[i - 23]) +
                                    (q[21] * pDest[i - 22]) +
                                    (q[20] * pDest[i - 21]) +
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 27:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[26] * pDest[i - 27]) +
                                    (q[25] * pDest[i - 26]) +
                                    (q[24] * pDest[i - 25]) +
                                    (q[23] * pDest[i - 24]) +
                                    (q[22] * pDest[i - 23]) +
                                    (q[21] * pDest[i - 22]) +
                                    (q[20] * pDest[i - 21]) +
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 26:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[25] * pDest[i - 26]) +
                                    (q[24] * pDest[i - 25]) +
                                    (q[23] * pDest[i - 24]) +
                                    (q[22] * pDest[i - 23]) +
                                    (q[21] * pDest[i - 22]) +
                                    (q[20] * pDest[i - 21]) +
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 25:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[24] * pDest[i - 25]) +
                                    (q[23] * pDest[i - 24]) +
                                    (q[22] * pDest[i - 23]) +
                                    (q[21] * pDest[i - 22]) +
                                    (q[20] * pDest[i - 21]) +
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 24:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[23] * pDest[i - 24]) +
                                    (q[22] * pDest[i - 23]) +
                                    (q[21] * pDest[i - 22]) +
                                    (q[20] * pDest[i - 21]) +
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 23:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[22] * pDest[i - 23]) +
                                    (q[21] * pDest[i - 22]) +
                                    (q[20] * pDest[i - 21]) +
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 22:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[21] * pDest[i - 22]) +
                                    (q[20] * pDest[i - 21]) +
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 21:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[20] * pDest[i - 21]) +
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 20:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[19] * pDest[i - 20]) +
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 19:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[18] * pDest[i - 19]) +
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 18:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[17] * pDest[i - 18]) +
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 17:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[16] * pDest[i - 17]) +
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 16:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[15] * pDest[i - 16]) +
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 15:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[14] * pDest[i - 15]) +
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 14:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[13] * pDest[i - 14]) +
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 13:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[12] * pDest[i - 13]) +
                                    (q[11] * pDest[i - 12]) +
                                    (q[10] * pDest[i - 11]) +
                                    (q[9] * pDest[i - 10]) +
                                    (q[8] * pDest[i - 9]) +
                                    (q[7] * pDest[i - 8]) +
                                    (q[6] * pDest[i - 7]) +
                                    (q[5] * pDest[i - 6]) +
                                    (q[4] * pDest[i - 5]) +
                                    (q[3] * pDest[i - 4]) +
                                    (q[2] * pDest[i - 3]) +
                                    (q[1] * pDest[i - 2]) +
                                    (q[0] * pDest[i - 1]);
                                pDest[i] = pRes[i] + (z >> lpcShiftNeeded);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private static unsafe void RestoreLPCSignal64(ReadOnlySpan<int> residual, Span<int> destination, int length, int order, ReadOnlySpan<int> qlpCoeff, int lpcShiftNeeded)
        {
            if ((uint)length > (uint)residual.Length || (uint)length > (uint)destination.Length || (uint)order > (uint)qlpCoeff.Length)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (length <= order)
                return;

            fixed (int* pRes = residual, pDest = destination, q = qlpCoeff)
            {
                if (order <= 12)
                {
                    long z;
                    switch (order)
                    {
                        case 12:
                        {
                            long q11 = q[11];
                            long q10 = q[10];
                            long q9 = q[9];
                            long q8 = q[8];
                            long q7 = q[7];
                            long q6 = q[6];
                            long q5 = q[5];
                            long q4 = q[4];
                            long q3 = q[3];
                            long q2 = q[2];
                            long q1 = q[1];
                            long q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q11 * pDest[i - 12]) +
                                    (q10 * pDest[i - 11]) +
                                    (q9 * pDest[i - 10]) +
                                    (q8 * pDest[i - 9]) +
                                    (q7 * pDest[i - 8]) +
                                    (q6 * pDest[i - 7]) +
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 11:
                        {
                            long q10 = q[10];
                            long q9 = q[9];
                            long q8 = q[8];
                            long q7 = q[7];
                            long q6 = q[6];
                            long q5 = q[5];
                            long q4 = q[4];
                            long q3 = q[3];
                            long q2 = q[2];
                            long q1 = q[1];
                            long q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q10 * pDest[i - 11]) +
                                    (q9 * pDest[i - 10]) +
                                    (q8 * pDest[i - 9]) +
                                    (q7 * pDest[i - 8]) +
                                    (q6 * pDest[i - 7]) +
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 10:
                        {
                            long q9 = q[9];
                            long q8 = q[8];
                            long q7 = q[7];
                            long q6 = q[6];
                            long q5 = q[5];
                            long q4 = q[4];
                            long q3 = q[3];
                            long q2 = q[2];
                            long q1 = q[1];
                            long q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q9 * pDest[i - 10]) +
                                    (q8 * pDest[i - 9]) +
                                    (q7 * pDest[i - 8]) +
                                    (q6 * pDest[i - 7]) +
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 9:
                        {
                            long q8 = q[8];
                            long q7 = q[7];
                            long q6 = q[6];
                            long q5 = q[5];
                            long q4 = q[4];
                            long q3 = q[3];
                            long q2 = q[2];
                            long q1 = q[1];
                            long q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q8 * pDest[i - 9]) +
                                    (q7 * pDest[i - 8]) +
                                    (q6 * pDest[i - 7]) +
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 8:
                        {
                            long q7 = q[7];
                            long q6 = q[6];
                            long q5 = q[5];
                            long q4 = q[4];
                            long q3 = q[3];
                            long q2 = q[2];
                            long q1 = q[1];
                            long q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q7 * pDest[i - 8]) +
                                    (q6 * pDest[i - 7]) +
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 7:
                        {
                            long q6 = q[6];
                            long q5 = q[5];
                            long q4 = q[4];
                            long q3 = q[3];
                            long q2 = q[2];
                            long q1 = q[1];
                            long q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q6 * pDest[i - 7]) +
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 6:
                        {
                            long q5 = q[5];
                            long q4 = q[4];
                            long q3 = q[3];
                            long q2 = q[2];
                            long q1 = q[1];
                            long q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q5 * pDest[i - 6]) +
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 5:
                        {
                            long q4 = q[4];
                            long q3 = q[3];
                            long q2 = q[2];
                            long q1 = q[1];
                            long q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q4 * pDest[i - 5]) +
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 4:
                        {
                            long q3 = q[3];
                            long q2 = q[2];
                            long q1 = q[1];
                            long q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q3 * pDest[i - 4]) +
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 3:
                        {
                            long q2 = q[2];
                            long q1 = q[1];
                            long q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q2 * pDest[i - 3]) +
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 2:
                        {
                            long q1 = q[1];
                            long q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q1 * pDest[i - 2]) +
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 1:
                        {
                            long q0 = q[0];
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q0 * pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    long z;
                    switch (order)
                    {
                        case 32:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[31] * (long)pDest[i - 32]) +
                                    (q[30] * (long)pDest[i - 31]) +
                                    (q[29] * (long)pDest[i - 30]) +
                                    (q[28] * (long)pDest[i - 29]) +
                                    (q[27] * (long)pDest[i - 28]) +
                                    (q[26] * (long)pDest[i - 27]) +
                                    (q[25] * (long)pDest[i - 26]) +
                                    (q[24] * (long)pDest[i - 25]) +
                                    (q[23] * (long)pDest[i - 24]) +
                                    (q[22] * (long)pDest[i - 23]) +
                                    (q[21] * (long)pDest[i - 22]) +
                                    (q[20] * (long)pDest[i - 21]) +
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 31:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[30] * (long)pDest[i - 31]) +
                                    (q[29] * (long)pDest[i - 30]) +
                                    (q[28] * (long)pDest[i - 29]) +
                                    (q[27] * (long)pDest[i - 28]) +
                                    (q[26] * (long)pDest[i - 27]) +
                                    (q[25] * (long)pDest[i - 26]) +
                                    (q[24] * (long)pDest[i - 25]) +
                                    (q[23] * (long)pDest[i - 24]) +
                                    (q[22] * (long)pDest[i - 23]) +
                                    (q[21] * (long)pDest[i - 22]) +
                                    (q[20] * (long)pDest[i - 21]) +
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 30:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[29] * (long)pDest[i - 30]) +
                                    (q[28] * (long)pDest[i - 29]) +
                                    (q[27] * (long)pDest[i - 28]) +
                                    (q[26] * (long)pDest[i - 27]) +
                                    (q[25] * (long)pDest[i - 26]) +
                                    (q[24] * (long)pDest[i - 25]) +
                                    (q[23] * (long)pDest[i - 24]) +
                                    (q[22] * (long)pDest[i - 23]) +
                                    (q[21] * (long)pDest[i - 22]) +
                                    (q[20] * (long)pDest[i - 21]) +
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 29:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[28] * (long)pDest[i - 29]) +
                                    (q[27] * (long)pDest[i - 28]) +
                                    (q[26] * (long)pDest[i - 27]) +
                                    (q[25] * (long)pDest[i - 26]) +
                                    (q[24] * (long)pDest[i - 25]) +
                                    (q[23] * (long)pDest[i - 24]) +
                                    (q[22] * (long)pDest[i - 23]) +
                                    (q[21] * (long)pDest[i - 22]) +
                                    (q[20] * (long)pDest[i - 21]) +
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 28:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[27] * (long)pDest[i - 28]) +
                                    (q[26] * (long)pDest[i - 27]) +
                                    (q[25] * (long)pDest[i - 26]) +
                                    (q[24] * (long)pDest[i - 25]) +
                                    (q[23] * (long)pDest[i - 24]) +
                                    (q[22] * (long)pDest[i - 23]) +
                                    (q[21] * (long)pDest[i - 22]) +
                                    (q[20] * (long)pDest[i - 21]) +
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 27:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[26] * (long)pDest[i - 27]) +
                                    (q[25] * (long)pDest[i - 26]) +
                                    (q[24] * (long)pDest[i - 25]) +
                                    (q[23] * (long)pDest[i - 24]) +
                                    (q[22] * (long)pDest[i - 23]) +
                                    (q[21] * (long)pDest[i - 22]) +
                                    (q[20] * (long)pDest[i - 21]) +
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 26:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[25] * (long)pDest[i - 26]) +
                                    (q[24] * (long)pDest[i - 25]) +
                                    (q[23] * (long)pDest[i - 24]) +
                                    (q[22] * (long)pDest[i - 23]) +
                                    (q[21] * (long)pDest[i - 22]) +
                                    (q[20] * (long)pDest[i - 21]) +
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 25:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[24] * (long)pDest[i - 25]) +
                                    (q[23] * (long)pDest[i - 24]) +
                                    (q[22] * (long)pDest[i - 23]) +
                                    (q[21] * (long)pDest[i - 22]) +
                                    (q[20] * (long)pDest[i - 21]) +
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 24:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[23] * (long)pDest[i - 24]) +
                                    (q[22] * (long)pDest[i - 23]) +
                                    (q[21] * (long)pDest[i - 22]) +
                                    (q[20] * (long)pDest[i - 21]) +
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 23:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[22] * (long)pDest[i - 23]) +
                                    (q[21] * (long)pDest[i - 22]) +
                                    (q[20] * (long)pDest[i - 21]) +
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 22:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[21] * (long)pDest[i - 22]) +
                                    (q[20] * (long)pDest[i - 21]) +
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 21:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[20] * (long)pDest[i - 21]) +
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 20:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[19] * (long)pDest[i - 20]) +
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 19:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[18] * (long)pDest[i - 19]) +
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 18:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[17] * (long)pDest[i - 18]) +
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 17:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[16] * (long)pDest[i - 17]) +
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 16:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[15] * (long)pDest[i - 16]) +
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 15:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[14] * (long)pDest[i - 15]) +
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 14:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[13] * (long)pDest[i - 14]) +
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                        case 13:
                        {
                            for (int i = order; i < length; i++)
                            {
                                z =
                                    (q[12] * (long)pDest[i - 13]) +
                                    (q[11] * (long)pDest[i - 12]) +
                                    (q[10] * (long)pDest[i - 11]) +
                                    (q[9] * (long)pDest[i - 10]) +
                                    (q[8] * (long)pDest[i - 9]) +
                                    (q[7] * (long)pDest[i - 8]) +
                                    (q[6] * (long)pDest[i - 7]) +
                                    (q[5] * (long)pDest[i - 6]) +
                                    (q[4] * (long)pDest[i - 5]) +
                                    (q[3] * (long)pDest[i - 4]) +
                                    (q[2] * (long)pDest[i - 3]) +
                                    (q[1] * (long)pDest[i - 2]) +
                                    (q[0] * (long)pDest[i - 1]);
                                pDest[i] = pRes[i] + (int)(z >> lpcShiftNeeded);
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}
