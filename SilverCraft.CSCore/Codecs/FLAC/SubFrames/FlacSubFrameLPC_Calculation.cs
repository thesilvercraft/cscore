using System;

namespace SilverCraft.CSCore.Codecs.FLAC
{
    internal sealed partial class FlacSubFrameLPC
    {
       private void RestoreLPCSignal32(ReadOnlySpan<int> residual, Span<int> destination, int length, int order, int[] qlpCoeff, int lpcShiftNeeded)
       {
          int[] q = qlpCoeff;
          if(order <= 12)
          {
        int z;
        switch(order)
        {
            case 12:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 11:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 10:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 9:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 8:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 7:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 6:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 5:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 4:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 3:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 2:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 1:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
        }
          }
          else if(order > 12)
          {
        int z;
        switch(order)
        {
            case 32:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[31] * destination[i - 32]) +
                  (q[30] * destination[i - 31]) +
                  (q[29] * destination[i - 30]) +
                  (q[28] * destination[i - 29]) +
                  (q[27] * destination[i - 28]) +
                  (q[26] * destination[i - 27]) +
                  (q[25] * destination[i - 26]) +
                  (q[24] * destination[i - 25]) +
                  (q[23] * destination[i - 24]) +
                  (q[22] * destination[i - 23]) +
                  (q[21] * destination[i - 22]) +
                  (q[20] * destination[i - 21]) +
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 31:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[30] * destination[i - 31]) +
                  (q[29] * destination[i - 30]) +
                  (q[28] * destination[i - 29]) +
                  (q[27] * destination[i - 28]) +
                  (q[26] * destination[i - 27]) +
                  (q[25] * destination[i - 26]) +
                  (q[24] * destination[i - 25]) +
                  (q[23] * destination[i - 24]) +
                  (q[22] * destination[i - 23]) +
                  (q[21] * destination[i - 22]) +
                  (q[20] * destination[i - 21]) +
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 30:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[29] * destination[i - 30]) +
                  (q[28] * destination[i - 29]) +
                  (q[27] * destination[i - 28]) +
                  (q[26] * destination[i - 27]) +
                  (q[25] * destination[i - 26]) +
                  (q[24] * destination[i - 25]) +
                  (q[23] * destination[i - 24]) +
                  (q[22] * destination[i - 23]) +
                  (q[21] * destination[i - 22]) +
                  (q[20] * destination[i - 21]) +
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 29:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[28] * destination[i - 29]) +
                  (q[27] * destination[i - 28]) +
                  (q[26] * destination[i - 27]) +
                  (q[25] * destination[i - 26]) +
                  (q[24] * destination[i - 25]) +
                  (q[23] * destination[i - 24]) +
                  (q[22] * destination[i - 23]) +
                  (q[21] * destination[i - 22]) +
                  (q[20] * destination[i - 21]) +
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 28:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[27] * destination[i - 28]) +
                  (q[26] * destination[i - 27]) +
                  (q[25] * destination[i - 26]) +
                  (q[24] * destination[i - 25]) +
                  (q[23] * destination[i - 24]) +
                  (q[22] * destination[i - 23]) +
                  (q[21] * destination[i - 22]) +
                  (q[20] * destination[i - 21]) +
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 27:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[26] * destination[i - 27]) +
                  (q[25] * destination[i - 26]) +
                  (q[24] * destination[i - 25]) +
                  (q[23] * destination[i - 24]) +
                  (q[22] * destination[i - 23]) +
                  (q[21] * destination[i - 22]) +
                  (q[20] * destination[i - 21]) +
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 26:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[25] * destination[i - 26]) +
                  (q[24] * destination[i - 25]) +
                  (q[23] * destination[i - 24]) +
                  (q[22] * destination[i - 23]) +
                  (q[21] * destination[i - 22]) +
                  (q[20] * destination[i - 21]) +
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 25:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[24] * destination[i - 25]) +
                  (q[23] * destination[i - 24]) +
                  (q[22] * destination[i - 23]) +
                  (q[21] * destination[i - 22]) +
                  (q[20] * destination[i - 21]) +
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 24:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[23] * destination[i - 24]) +
                  (q[22] * destination[i - 23]) +
                  (q[21] * destination[i - 22]) +
                  (q[20] * destination[i - 21]) +
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 23:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[22] * destination[i - 23]) +
                  (q[21] * destination[i - 22]) +
                  (q[20] * destination[i - 21]) +
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 22:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[21] * destination[i - 22]) +
                  (q[20] * destination[i - 21]) +
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 21:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[20] * destination[i - 21]) +
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 20:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[19] * destination[i - 20]) +
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 19:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[18] * destination[i - 19]) +
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 18:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[17] * destination[i - 18]) +
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 17:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[16] * destination[i - 17]) +
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 16:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[15] * destination[i - 16]) +
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 15:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[14] * destination[i - 15]) +
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 14:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[13] * destination[i - 14]) +
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
            case 13:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[12] * destination[i - 13]) +
                  (q[11] * destination[i - 12]) +
                  (q[10] * destination[i - 11]) +
                  (q[9] * destination[i - 10]) +
                  (q[8] * destination[i - 9]) +
                  (q[7] * destination[i - 8]) +
                  (q[6] * destination[i - 7]) +
                  (q[5] * destination[i - 6]) +
                  (q[4] * destination[i - 5]) +
                  (q[3] * destination[i - 4]) +
                  (q[2] * destination[i - 3]) +
                  (q[1] * destination[i - 2]) +
                  (q[0] * destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (z >> lpcShiftNeeded);
               }
               break;
        }
          }
       }

       private void RestoreLPCSignal64(ReadOnlySpan<int> residual, Span<int> destination, int length, int order, int[] qlpCoeff, int lpcShiftNeeded)
       {
          int[] q = qlpCoeff;
          if(order <= 12)
          {
        long z;
        switch(order)
        {
            case 12:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 11:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 10:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 9:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 8:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 7:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 6:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 5:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 4:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 3:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 2:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 1:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
        }
          }
          else if(order > 12)
          {
        long z;
        switch(order)
        {
            case 32:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[31] * (long)destination[i - 32]) +
                  (q[30] * (long)destination[i - 31]) +
                  (q[29] * (long)destination[i - 30]) +
                  (q[28] * (long)destination[i - 29]) +
                  (q[27] * (long)destination[i - 28]) +
                  (q[26] * (long)destination[i - 27]) +
                  (q[25] * (long)destination[i - 26]) +
                  (q[24] * (long)destination[i - 25]) +
                  (q[23] * (long)destination[i - 24]) +
                  (q[22] * (long)destination[i - 23]) +
                  (q[21] * (long)destination[i - 22]) +
                  (q[20] * (long)destination[i - 21]) +
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 31:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[30] * (long)destination[i - 31]) +
                  (q[29] * (long)destination[i - 30]) +
                  (q[28] * (long)destination[i - 29]) +
                  (q[27] * (long)destination[i - 28]) +
                  (q[26] * (long)destination[i - 27]) +
                  (q[25] * (long)destination[i - 26]) +
                  (q[24] * (long)destination[i - 25]) +
                  (q[23] * (long)destination[i - 24]) +
                  (q[22] * (long)destination[i - 23]) +
                  (q[21] * (long)destination[i - 22]) +
                  (q[20] * (long)destination[i - 21]) +
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 30:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[29] * (long)destination[i - 30]) +
                  (q[28] * (long)destination[i - 29]) +
                  (q[27] * (long)destination[i - 28]) +
                  (q[26] * (long)destination[i - 27]) +
                  (q[25] * (long)destination[i - 26]) +
                  (q[24] * (long)destination[i - 25]) +
                  (q[23] * (long)destination[i - 24]) +
                  (q[22] * (long)destination[i - 23]) +
                  (q[21] * (long)destination[i - 22]) +
                  (q[20] * (long)destination[i - 21]) +
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 29:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[28] * (long)destination[i - 29]) +
                  (q[27] * (long)destination[i - 28]) +
                  (q[26] * (long)destination[i - 27]) +
                  (q[25] * (long)destination[i - 26]) +
                  (q[24] * (long)destination[i - 25]) +
                  (q[23] * (long)destination[i - 24]) +
                  (q[22] * (long)destination[i - 23]) +
                  (q[21] * (long)destination[i - 22]) +
                  (q[20] * (long)destination[i - 21]) +
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 28:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[27] * (long)destination[i - 28]) +
                  (q[26] * (long)destination[i - 27]) +
                  (q[25] * (long)destination[i - 26]) +
                  (q[24] * (long)destination[i - 25]) +
                  (q[23] * (long)destination[i - 24]) +
                  (q[22] * (long)destination[i - 23]) +
                  (q[21] * (long)destination[i - 22]) +
                  (q[20] * (long)destination[i - 21]) +
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 27:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[26] * (long)destination[i - 27]) +
                  (q[25] * (long)destination[i - 26]) +
                  (q[24] * (long)destination[i - 25]) +
                  (q[23] * (long)destination[i - 24]) +
                  (q[22] * (long)destination[i - 23]) +
                  (q[21] * (long)destination[i - 22]) +
                  (q[20] * (long)destination[i - 21]) +
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 26:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[25] * (long)destination[i - 26]) +
                  (q[24] * (long)destination[i - 25]) +
                  (q[23] * (long)destination[i - 24]) +
                  (q[22] * (long)destination[i - 23]) +
                  (q[21] * (long)destination[i - 22]) +
                  (q[20] * (long)destination[i - 21]) +
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 25:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[24] * (long)destination[i - 25]) +
                  (q[23] * (long)destination[i - 24]) +
                  (q[22] * (long)destination[i - 23]) +
                  (q[21] * (long)destination[i - 22]) +
                  (q[20] * (long)destination[i - 21]) +
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 24:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[23] * (long)destination[i - 24]) +
                  (q[22] * (long)destination[i - 23]) +
                  (q[21] * (long)destination[i - 22]) +
                  (q[20] * (long)destination[i - 21]) +
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 23:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[22] * (long)destination[i - 23]) +
                  (q[21] * (long)destination[i - 22]) +
                  (q[20] * (long)destination[i - 21]) +
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 22:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[21] * (long)destination[i - 22]) +
                  (q[20] * (long)destination[i - 21]) +
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 21:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[20] * (long)destination[i - 21]) +
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 20:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[19] * (long)destination[i - 20]) +
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 19:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[18] * (long)destination[i - 19]) +
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 18:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[17] * (long)destination[i - 18]) +
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 17:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[16] * (long)destination[i - 17]) +
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 16:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[15] * (long)destination[i - 16]) +
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 15:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[14] * (long)destination[i - 15]) +
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 14:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[13] * (long)destination[i - 14]) +
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
            case 13:
               for(int i = order; i < length ; i++)
               {
                  z = 
                  (q[12] * (long)destination[i - 13]) +
                  (q[11] * (long)destination[i - 12]) +
                  (q[10] * (long)destination[i - 11]) +
                  (q[9] * (long)destination[i - 10]) +
                  (q[8] * (long)destination[i - 9]) +
                  (q[7] * (long)destination[i - 8]) +
                  (q[6] * (long)destination[i - 7]) +
                  (q[5] * (long)destination[i - 6]) +
                  (q[4] * (long)destination[i - 5]) +
                  (q[3] * (long)destination[i - 4]) +
                  (q[2] * (long)destination[i - 3]) +
                  (q[1] * (long)destination[i - 2]) +
                  (q[0] * (long)destination[i - 1]) 
                  ;
                  destination[i] = residual[i ] + (int)(z >> lpcShiftNeeded);
               }
               break;
        }
          }
       }
    }
}

