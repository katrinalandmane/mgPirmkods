using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _41cite
{
    class Program
    {
        static void Main(string[] args)
        {
            int BoardWidth = 200;
            int BoradHeight = 300;
            string resultFileName = "results.txt";
            if (File.Exists(resultFileName)){File.Delete(resultFileName);}
            string dataFileName = "data";
            int counter = 1;
            Level a=null;
            List<Piece> b = new List<Piece>();
            
            //genSimplePattern(a,b,BoradHeight,BoardWidth);
            while (File.Exists(dataFileName + "" + counter + ".txt"))
            {
            //    WriteHeader(resultFileName, counter);
                string FileName = dataFileName + "" + counter + ".txt";
                //string FileName = dataFileName + "" + BoradHeight + "x" + BoardWidth + "(HxW).txt";
                #region SAS/SASm generation
                int AlgType = 3; //1 - SASm, 2- SAS, 3 - both
                if (AlgType == 1 || AlgType == 3)
                {
                    if (File.Exists("SASm.txt")) { File.Delete("SASm.txt"); }
                    if (File.Exists("SASmBin.txt")) { File.Delete("SASmBin.txt"); }
                }
                if (AlgType == 2 || AlgType == 3)
                {
                    if (File.Exists("SAS.txt")) { File.Delete("SAS.txt"); }
                    if (File.Exists("SASBin.txt")) { File.Delete("SASBin.txt"); }
                }
                bool several = true;
                int variable = 0;
                int cycleCount = 0;
                while (several)
                {
                    if (AlgType == 1 || (AlgType == 3 && cycleCount == 0)) { variable = 1; }
                    if (AlgType == 2 || (AlgType == 3 && cycleCount == 1)) { variable = 2; }
                    List<Level> FinalSort = new List<Level>();
                    List<Bin> FinalBins = new List<Bin>();
                    List<Piece> Pieces = ReadFromFile(FileName);
                    List<Level> primarySort = new List<Level>();
                    if (variable == 1) { primarySort = SASm(BoardWidth, Pieces, true); }
                    if (variable == 2) { primarySort = SAS(BoardWidth, Pieces, true); }
                    List<Bin> primaryBin = FindBKP(primarySort, BoradHeight, BoardWidth);
                    //if (variable == 1) { WriteStatistics(resultFileName, primaryBin, primarySort, BoradHeight, BoardWidth, "SASm"); }// WriteResultListToFile("SASm.txt", "Results", primarySort, primaryBin); }//{ WriteResultListToFile("SASm.txt", "Results", primarySort, primaryBin); }
                    //if (variable == 2) { WriteStatistics(resultFileName, primaryBin, primarySort, BoradHeight, BoardWidth, "SAS"); }//{ WriteResultListToFile("SAS.txt", "Results", primarySort, primaryBin); }
                    if (variable == 1) { WriteResultListToFile("SASmresults_" + BoradHeight + "x" + BoardWidth + "(HxW)"+counter+".txt", "", primarySort, primaryBin); }//{ WriteResultListToFile("SASm.txt", "Results", primarySort, primaryBin); }
                    if (variable == 2) { WriteResultListToFile("SASresults_" + BoradHeight + "x" + BoardWidth + "(HxW)" + counter + ".txt", "", primarySort, primaryBin); }
                    int binNum = 1;
                    bool cycle = true;
                    while (cycle)
                    {
                        if (primaryBin.Count > 0 && primaryBin.Count >= binNum)
                        {
                            Level newSlip = SAS_SASm(BoardWidth, BoradHeight, primaryBin[binNum - 1], primarySort, Pieces, variable);
                            if (newSlip == null)
                            {
                                //better slip was not found. bin not changed. go to next bin.
                                List<ItemAndPosition> temp = new List<ItemAndPosition>();
                                List<int> binLevels = new List<int>();
                                foreach (int num in primaryBin[binNum - 1].Levels)
                                {
                                    temp = new List<ItemAndPosition>(primarySort.Where(x => x.LevelNum == num).FirstOrDefault().Contents.ConvertAll(y => new ItemAndPosition(y.Item, y.yFrom, y.yTo, y.xFrom, y.xTo)));
                                    FinalSort.Add(new Level(FinalSort.Count + 1, temp));
                                    binLevels.Add(FinalSort.Count);
                                }
                                if (binLevels.Count > 0)
                                {
                                    FinalBins.Add(new Bin(FinalBins.Count + 1, binLevels));
                                    List<int> numToRemove = ListToRemove(FinalSort, FinalBins.Where(x => x.BinNum == (FinalBins.Count)).FirstOrDefault(), 0);
                                    Pieces.RemoveAll(x => numToRemove.Contains(x.Number));
                                }
                                binNum++;
                            }
                            else
                            {
                                //better slip for current bin was found. Change it and resort other Bins.
                                int lvlHeight = primarySort.Where(x => primaryBin[binNum - 1].Levels.Contains(x.LevelNum)).Min(x => x.Contents[0].yTo);
                                int slipNum = primarySort.Where(x => x.Contents[0].yTo == lvlHeight).FirstOrDefault().LevelNum;
                                List<int> binLevels = new List<int>();
                                foreach (int num in primaryBin[binNum - 1].Levels)
                                {
                                    List<ItemAndPosition> temp = new List<ItemAndPosition>();
                                    if (num == slipNum)
                                    {
                                        temp = new List<ItemAndPosition>(newSlip.Contents.ConvertAll(y => new ItemAndPosition(y.Item, y.yFrom, y.yTo, y.xFrom, y.xTo)));
                                    }
                                    else
                                    {
                                        //deep copy of level.contents, where levelnum==num
                                        temp = new List<ItemAndPosition>(primarySort.Where(x => x.LevelNum == num).FirstOrDefault().Contents.ConvertAll(y => new ItemAndPosition(y.Item, y.yFrom, y.yTo, y.xFrom, y.xTo)));
                                    }
                                    FinalSort.Add(new Level(FinalSort.Count + 1, temp));
                                    binLevels.Add(FinalSort.Count);
                                }
                                if (binLevels.Count > 0)
                                {
                                    FinalBins.Add(new Bin(FinalBins.Count + 1, binLevels));
                                    List<int> numToRemove = ListToRemove(FinalSort, FinalBins.Where(x => x.BinNum == (FinalBins.Count)).FirstOrDefault(), 0);
                                    Pieces.RemoveAll(x => numToRemove.Contains(x.Number));
                                    if (variable == 1) { primarySort = SASm(BoardWidth, Pieces, true); }
                                    if (variable == 2) { primarySort = SAS(BoardWidth, Pieces, true); }
                                    primaryBin = FindBKP(primarySort, BoradHeight, BoardWidth);
                                    binNum = 1;
                                }
                            }
                        }
                        else { cycle = false; }
                    }
                    //if (variable == 1) { WriteStatistics(resultFileName, FinalBins, FinalSort, BoradHeight, BoardWidth, "SASmMod"); } //WriteResultListToFile("SASmBin.txt", "Results", FinalSort, FinalBins); }//{ WriteResultListToFile("SASmBin.txt", "Results", FinalSort, FinalBins); }
                    //if (variable == 2) { WriteStatistics(resultFileName, FinalBins, FinalSort, BoradHeight, BoardWidth, "SASMod"); }//{ WriteResultListToFile("SASBin.txt", "Results", FinalSort, FinalBins); }
                    if (variable == 1) { WriteResultListToFile("SASmModresults_" + BoradHeight + "x" + BoardWidth + "(HxW)" + counter + ".txt", "", primarySort, primaryBin); }//{ WriteResultListToFile("SASm.txt", "Results", primarySort, primaryBin); }
                    if (variable == 2) { WriteResultListToFile("SASModresults_" + BoradHeight + "x" + BoardWidth + "(HxW)" + counter + ".txt", "", primarySort, primaryBin); }
                    if (AlgType == 1 || AlgType == 2) { several = false; }
                    if (AlgType == 3) { cycleCount++; }
                    if (cycleCount > 1) { several = false; }
                }
                #endregion

                //SAS(BoardWidth);
                //if (File.Exists("SF.txt"))
                //{
                //    File.Delete("SF.txt");
                //}
                //List<Level> SF2temp = SF(BoardWidth, FileName);
                List<Level> SFmtemp = SFm(BoardWidth, BoradHeight, FileName, resultFileName, counter);
                List<Level> SFtemp = SF(BoardWidth, BoradHeight, FileName, resultFileName, counter);
                BinAlg(BoardWidth, BoradHeight, FileName, counter);
                counter++;
            }
        }
        static List<Level> SFm(int BoardWidth, int BoardHeight, string FileName, string resultFileName, int setnum)
        {
            List<Piece> Pieces = ReadFromFile(FileName);
            //select widest item from all items
            Piece widest = Pieces.OrderByDescending(x => x.Width).FirstOrDefault();
            double mDouble = BoardWidth;
            mDouble = mDouble / widest.Width;
            int m = Convert.ToInt32(Math.Round(mDouble));
            List<Level> HalfSorted = new List<Level>();
            List<Level> Sorted = new List<Level>();//Filled space
            List<Piece> WideItems = Pieces.Where(x => x.Width > (BoardWidth / (m + 1))).ToList();
            List<Piece> NarrowItems = Pieces.Where(x => x.Width <= (BoardWidth / (m + 1))).ToList();
            if (WideItems.Count == 0 || NarrowItems.Count == 0)
            {
                List<Bin> primsortBin = null;
                List<Bin> final = null;
                if (WideItems.Count == 0 && NarrowItems.Count > 0)
                {
                    HalfSorted = FFDH(NarrowItems, HalfSorted, BoardWidth, true);
                    primsortBin = FindBKP(HalfSorted, BoardHeight, BoardWidth);
                }
                if (NarrowItems.Count == 0 && WideItems.Count > 0)
                {
                    HalfSorted = FFDH(WideItems, HalfSorted, BoardWidth, true);
                    primsortBin = FindBKP(HalfSorted, BoardHeight, BoardWidth);
                }
                WriteStatistics(resultFileName, primsortBin, HalfSorted, BoardHeight, BoardWidth, "SFm");
                //try resorting the rest
                int binNum = 1;
                bool cycle = true;
                while (cycle)
                {
                    if (primsortBin.Count > 0 && primsortBin.Count >= binNum)
                    {
                        Level newSlip = FFDHSingle(Pieces, BoardWidth, BoardHeight, primsortBin[binNum - 1], HalfSorted);
                        if (newSlip == null)
                        {
                            //better slip was not found. bin not changed. go to next bin.
                            List<ItemAndPosition> temp = new List<ItemAndPosition>();
                            List<int> binLevels = new List<int>();
                            foreach (int num in primsortBin[binNum - 1].Levels)
                            {
                                temp = new List<ItemAndPosition>(HalfSorted.Where(x => x.LevelNum == num).FirstOrDefault().Contents.ConvertAll(y => new ItemAndPosition(y.Item, y.yFrom, y.yTo, y.xFrom, y.xTo)));
                                Sorted.Add(new Level(Sorted.Count + 1, temp));
                                binLevels.Add(Sorted.Count);
                            }
                            if (binLevels.Count > 0)
                            {
                                final.Add(new Bin(final.Count + 1, binLevels));
                                List<int> numToRemove = ListToRemove(Sorted, final.Where(x => x.BinNum == (final.Count)).FirstOrDefault(), 0);
                                Pieces.RemoveAll(x => numToRemove.Contains(x.Number));
                            }
                            binNum++;
                        }
                        else
                        {
                            //better slip for current bin was found. Change it and resort other Bins.
                            int lvlHeight = HalfSorted.Where(x => primsortBin[binNum - 1].Levels.Contains(x.LevelNum)).Min(x => x.Contents[0].yTo);
                            int slipNum = HalfSorted.Where(x => x.Contents[0].yTo == lvlHeight).FirstOrDefault().LevelNum;
                            List<int> binLevels = new List<int>();
                            foreach (int num in primsortBin[binNum - 1].Levels)
                            {
                                List<ItemAndPosition> temp = new List<ItemAndPosition>();
                                if (num == slipNum)
                                {
                                    temp = new List<ItemAndPosition>(newSlip.Contents.ConvertAll(y => new ItemAndPosition(y.Item, y.yFrom, y.yTo, y.xFrom, y.xTo)));
                                }
                                else
                                {
                                    //deep copy of level.contents, where levelnum==num
                                    temp = new List<ItemAndPosition>(HalfSorted.Where(x => x.LevelNum == num).FirstOrDefault().Contents.ConvertAll(y => new ItemAndPosition(y.Item, y.yFrom, y.yTo, y.xFrom, y.xTo)));
                                }
                                Sorted.Add(new Level(Sorted.Count + 1, temp));
                                binLevels.Add(Sorted.Count);
                            }
                            if (binLevels.Count > 0)
                            {
                                final.Add(new Bin(final.Count + 1, binLevels));
                                List<int> numToRemove = ListToRemove(Sorted, final.Where(x => x.BinNum == (final.Count)).FirstOrDefault(), 0);
                                Pieces.RemoveAll(x => numToRemove.Contains(x.Number));
                                HalfSorted.Clear();
                                HalfSorted = FFDH(Pieces, HalfSorted, BoardWidth, false);
                                primsortBin = FindBKP(HalfSorted, BoardHeight, BoardWidth);
                                binNum = 1;
                            }
                        }
                    }
                    else { cycle = false; }
                }
                //WriteStatistics(resultFileName, final, Sorted, BoardHeight, BoardWidth, "SFmMod");
                WriteResultListToFile("SFmModresults_" + BoardHeight + "x" + BoardWidth + "(HxW)" + setnum + ".txt", "", Sorted, final);
            }
            else
            {
                HalfSorted = FFDH(WideItems, Sorted, BoardWidth, true);
                Sorted = new List<Level>(HalfSorted.ConvertAll(x => new Level(x.LevelNum, x.Contents)));
                #region Sort_NotNeededForBins
                //Level small = Sorted.Where(x => x.Contents.Max(y => y.xTo) <= (BoardWidth * (m + 1)) / (m + 2)).FirstOrDefault();
                //if (small != null)
                //{
                //    foreach (Level level in HalfSorted)
                //    {
                //        int currentLvlNum = level.LevelNum;
                //        Level test = Sorted.Where(x => x.LevelNum == currentLvlNum).FirstOrDefault();
                //        if (test.Contents.Max(x => x.xTo) > (BoardWidth * (m + 1)) / (m + 2))
                //        {
                //            test.LevelNum = -2;
                //        }
                //        else
                //        {
                //            test.LevelNum = -1;
                //        }
                //    }
                //    Sorted = Sorted.OrderBy(x => x.LevelNum).ToList();
                //    for (int i = 0; i < Sorted.Count; i++)
                //    {
                //        Sorted[i].LevelNum = i + 1;
                //    }
                //}
                //Sorted=Sorted.OrderBy(x => x.LevelNum).ToList();
                #endregion
                List<Bin> primsortBin = FindBKP(Sorted, BoardHeight, BoardWidth);
                //int Rwidth = BoardWidth / (m + 2);//static value, 1/3 of width
                foreach (Bin bin in primsortBin)
                {//arrange levels in bin based on algorithm (wide strips at bottom, rest at top)
                    List<int> levels = bin.Levels;
                    List<Level> binLevels = Sorted.Where(x => levels.Contains(x.LevelNum)).ToList();
                    List<int> widelevels = binLevels.Where(x => x.Contents.Max(y => y.xTo) > (BoardWidth * (m + 1)) / (m + 2)).Select(z => z.LevelNum).ToList();
                    List<int> newLevels = new List<int>();
                    #region Add Narrow Pieces in area R (right of >2/3)
                    if (widelevels.Count > 0) //add wide items in the bottom (start of list)
                    {
                        int startLvl = widelevels[0];
                        int Rheight = binLevels.Where(x => widelevels.Contains(x.LevelNum)).Sum(y => y.Contents[0].yTo);//calculate narrow level height sum
                        int Rwidth =BoardWidth- binLevels.Where(x => widelevels.Contains(x.LevelNum)).Max(x => x.Contents.Max(y => y.xTo));
                        int startY = 0;
                        List<ItemAndPosition> R = new List<ItemAndPosition>();
                        int slipWidth = 0;
                        int slipHeight = 0;
                        Piece item = NarrowItems.Where(x => x.Length <= Rheight && x.Width <= Rwidth).OrderByDescending(x => x.Length).FirstOrDefault();
                        while (item != null)
                        {
                            slipHeight = item.Length;
                            slipWidth = Rwidth;
                            Piece slipitem = NarrowItems.Where(x => x.Length <= slipHeight && x.Width <= slipWidth).OrderByDescending(x => x.Length).FirstOrDefault();
                            while (slipitem != null)
                            {
                                R.Add(new ItemAndPosition(slipitem, startY, startY + slipitem.Length, BoardWidth - slipWidth, BoardWidth - slipWidth + slipitem.Width));
                                NarrowItems.Remove(slipitem);
                                slipWidth -= slipitem.Width;
                                slipitem = NarrowItems.Where(x => x.Length <= slipHeight && x.Width <= slipWidth).OrderByDescending(x => x.Length).FirstOrDefault();
                            }
                            startY += slipHeight;
                            Rheight -= slipHeight;
                            item = NarrowItems.Where(x => x.Length <= Rheight && x.Width <= Rwidth).OrderByDescending(x => x.Length).FirstOrDefault();
                        }
                        Sorted.Where(x => x.LevelNum == startLvl).First().Contents.AddRange(R);
                        newLevels.AddRange(widelevels);
                    }
                    #endregion
                    #region Add Narrow Pieces in area R (right of <=2/3)
                    List<int> narrowlevels = binLevels.Where(x => x.Contents.Max(y => y.xTo) <= (BoardWidth * (m + 1)) / (m + 2)).Select(z => z.LevelNum).ToList();
                    if (narrowlevels.Count > 0)
                    {
                        int startLvl = narrowlevels[0];
                        int Rheight = binLevels.Where(x => narrowlevels.Contains(x.LevelNum)).Sum(y => y.Contents[0].yTo);//calculate narrow level height sum
                        int Rwidth = BoardWidth - binLevels.Where(x => narrowlevels.Contains(x.LevelNum)).Max(x => x.Contents.Max(y => y.xTo));
                        int startY = 0;
                        List<ItemAndPosition> R = new List<ItemAndPosition>();
                        int slipWidth = 0;
                        int slipHeight = 0;
                        Piece item = NarrowItems.Where(x => x.Length <= Rheight && x.Width <= Rwidth).OrderByDescending(x => x.Length).FirstOrDefault();
                        while (item != null)
                        {
                            slipHeight = item.Length;
                            slipWidth = Rwidth;
                            Piece slipitem = NarrowItems.Where(x => x.Length <= slipHeight && x.Width <= slipWidth).OrderByDescending(x => x.Length).FirstOrDefault();
                            while (slipitem != null)
                            {
                                R.Add(new ItemAndPosition(slipitem, startY, startY + slipitem.Length, BoardWidth - slipWidth, BoardWidth - slipWidth + slipitem.Width));
                                NarrowItems.Remove(slipitem);
                                slipWidth -= slipitem.Width;
                                slipitem = NarrowItems.Where(x => x.Length <= slipHeight && x.Width <= slipWidth).OrderByDescending(x => x.Length).FirstOrDefault();
                            }
                            startY += slipHeight;
                            Rheight -= slipHeight;
                            item = NarrowItems.Where(x => x.Length <= Rheight && x.Width <= Rwidth).OrderByDescending(x => x.Length).FirstOrDefault();
                        }
                        Sorted.Where(x => x.LevelNum == startLvl).First().Contents.AddRange(R);
                        newLevels.AddRange(narrowlevels);
                    }
                    #endregion
                    bin.Levels = newLevels;
                }
                HalfSorted.Clear();
                HalfSorted = FFDH(NarrowItems, HalfSorted, BoardWidth, false);
                //HalfSorted = HalfSorted.Where(x => !Sorted.Any(y => y.LevelNum == x.LevelNum)).ToList();//get only levels which are narrowItem
                //Check, if more levels can be added to bins (FFDH will have to change for SFm)
                foreach (Bin bin in primsortBin)
                {
                    int freeHeight = BoardHeight - Sorted.Where(x => bin.Levels.Contains(x.LevelNum)).Sum(y => y.Contents[0].yTo);
                    Level addable = HalfSorted.Where(x => x.Contents[0].yTo <= freeHeight).FirstOrDefault();
                    if (addable != null)
                    {
                        List<Bin> add = FindBKP(HalfSorted, freeHeight, BoardWidth);
                        if (add != null)
                        {
                            List<int> temp = new List<int>(bin.Levels);
                            foreach (int lvl in add[0].Levels)
                            {
                                int maxlvl = Sorted.Max(x => x.LevelNum) + 1;
                                Sorted.Add(new Level(maxlvl, HalfSorted.Where(x => x.LevelNum == lvl).FirstOrDefault().Contents));
                                temp.Add(maxlvl);
                            }
                            HalfSorted.RemoveAll(x => add[0].Levels.Contains(x.LevelNum));
                            bin.Levels = temp;
                        }
                    }
                }
                
                //remove all piecies used in previous bins from Piece list
                foreach (Bin bin in primsortBin)
                {
                    List<int> removeNums = ListToRemove(Sorted,bin,0);
                    Pieces.RemoveAll(x => removeNums.Contains(x.Number));
                }
                List<Bin> narrowsBin = FindBKP(HalfSorted, BoardHeight, BoardWidth);
                List<Bin> UnionBin = new List<Bin>(primsortBin.ConvertAll(x => new Bin(x.BinNum, x.Levels)));
                List<Level> UnionLvl = new List<Level>(Sorted.ConvertAll(x => new Level(x.LevelNum, x.Contents)));
                foreach (Bin bin in narrowsBin)
                {
                    List<int> newlvl = new List<int>();
                    List<Level> temp = HalfSorted.Where(x => bin.Levels.Contains(x.LevelNum)).ToList();
                    foreach(Level lvl in temp)
                    {
                        UnionLvl.Add(new Level(UnionLvl.Count + 1, lvl.Contents));
                        newlvl.Add(UnionLvl.Count);
                    }
                    if (newlvl.Count > 0)
                    {
                        UnionBin.Add(new Bin(UnionBin.Count + 1, newlvl));
                    }
                }
                //WriteStatistics(resultFileName, UnionBin, UnionLvl, BoardHeight, BoardWidth, "SFm");
                WriteResultListToFile("SFmresults_" + BoardHeight + "x" + BoardWidth + "(HxW)" + setnum + ".txt", "", UnionLvl, UnionBin);
                //try resorting the rest
                int binNum = 1;
                bool cycle = true;
                while (cycle)
                {
                    if (narrowsBin.Count > 0 && narrowsBin.Count >= binNum)
                    {
                        Level newSlip = FFDHSingle(Pieces, BoardWidth, BoardHeight, narrowsBin[binNum - 1], HalfSorted);
                        if (newSlip == null)
                        {
                            //better slip was not found. bin not changed. go to next bin.
                            List<ItemAndPosition> temp = new List<ItemAndPosition>();
                            List<int> binLevels = new List<int>();
                            foreach (int num in narrowsBin[binNum - 1].Levels)
                            {
                                temp = new List<ItemAndPosition>(HalfSorted.Where(x => x.LevelNum == num).FirstOrDefault().Contents.ConvertAll(y => new ItemAndPosition(y.Item, y.yFrom, y.yTo, y.xFrom, y.xTo)));
                                Sorted.Add(new Level(Sorted.Count + 1, temp));
                                binLevels.Add(Sorted.Count);
                            }
                            if (binLevels.Count > 0)
                            {
                                primsortBin.Add(new Bin(primsortBin.Count + 1, binLevels));
                                List<int> numToRemove = ListToRemove(Sorted, primsortBin.Where(x => x.BinNum == (primsortBin.Count)).FirstOrDefault(), 0);
                                Pieces.RemoveAll(x => numToRemove.Contains(x.Number));
                            }
                            binNum++;
                        }
                        else
                        {
                            //better slip for current bin was found. Change it and resort other Bins.
                            int lvlHeight = HalfSorted.Where(x => narrowsBin[binNum - 1].Levels.Contains(x.LevelNum)).Min(x => x.Contents[0].yTo);
                            int slipNum = HalfSorted.Where(x => x.Contents[0].yTo == lvlHeight).FirstOrDefault().LevelNum;
                            List<int> binLevels = new List<int>();
                            foreach (int num in narrowsBin[binNum - 1].Levels)
                            {
                                List<ItemAndPosition> temp = new List<ItemAndPosition>();
                                if (num == slipNum)
                                {
                                    temp = new List<ItemAndPosition>(newSlip.Contents.ConvertAll(y => new ItemAndPosition(y.Item, y.yFrom, y.yTo, y.xFrom, y.xTo)));
                                }
                                else
                                {
                                    //deep copy of level.contents, where levelnum==num
                                    temp = new List<ItemAndPosition>(HalfSorted.Where(x => x.LevelNum == num).FirstOrDefault().Contents.ConvertAll(y => new ItemAndPosition(y.Item, y.yFrom, y.yTo, y.xFrom, y.xTo)));
                                }
                                Sorted.Add(new Level(Sorted.Count + 1, temp));
                                binLevels.Add(Sorted.Count);
                            }
                            if (binLevels.Count > 0)
                            {
                                primsortBin.Add(new Bin(primsortBin.Count + 1, binLevels));
                                List<int> numToRemove = ListToRemove(Sorted, primsortBin.Where(x => x.BinNum == (primsortBin.Count)).FirstOrDefault(), 0);
                                Pieces.RemoveAll(x => numToRemove.Contains(x.Number));
                                HalfSorted.Clear();
                                HalfSorted = FFDH(Pieces, HalfSorted, BoardWidth, false);
                                narrowsBin = FindBKP(HalfSorted, BoardHeight, BoardWidth);
                                binNum = 1;
                            }
                        }
                    }
                    else { cycle = false; }
                }
                //WriteStatistics(resultFileName, primsortBin, Sorted, BoardHeight, BoardWidth, "SFmMod");
                //foreach (Bin bin in narrowsBin)
                //{
                //    List<int> temp = new List<int>();
                //    foreach (int lvl in bin.Levels)
                //    {
                //        int maxlvl = Sorted.Max(x => x.LevelNum) + 1;
                //        Sorted.Add(new Level(maxlvl, HalfSorted.Where(x => x.LevelNum == lvl).FirstOrDefault().Contents));
                //        temp.Add(maxlvl);
                //    }
                //    primsortBin.Add(new Bin(primsortBin.Count + 1, temp));
                //}
                //WriteResultListToFile(resultFileName, "", Sorted, primsortBin);
                WriteResultListToFile("SFmresults_" + BoardHeight + "x" + BoardWidth + "(HxW)" + setnum + ".txt", "", UnionLvl, UnionBin);
                //WriteResultListToFile("FFDH.txt", "Results", Sorted,null);
            }
            return Sorted;
        }
        static List<Level> SF(int BoardWidth, int BoardHeight, string FileName,string resultFileName,int setnum)
        {
            List<Piece> Pieces = ReadFromFile(FileName);
            //select widest item from all items
            Piece widest = Pieces.OrderByDescending(x => x.Width).FirstOrDefault();
            double mDouble = BoardWidth;
            mDouble=mDouble/ widest.Width;
            int m = Convert.ToInt32(Math.Round(mDouble));
            List<Level> HalfSorted = new List<Level>();
            List<Level> Sorted = new List<Level>();//Filled space
            List<Piece> WideItems=Pieces.Where(x => x.Width>(BoardWidth/(m+1))).ToList();
            List<Piece> NarrowItems = Pieces.Where(x => x.Width<=(BoardWidth/(m+1))).ToList();
            if(WideItems.Count==0 || NarrowItems.Count==0)
            {
                if (WideItems.Count == 0&&NarrowItems.Count>0) 
                { 
                    Sorted=FFDH(NarrowItems,Sorted,BoardWidth,true);
                    List<Bin> primsortBin = FindBKP(Sorted, BoardHeight, BoardWidth);
                }
                if (NarrowItems.Count == 0 && WideItems.Count > 0) 
                { 
                    Sorted = FFDH(WideItems, Sorted, BoardWidth, true);
                    List<Bin> primsortBin = FindBKP(Sorted, BoardHeight, BoardWidth);
                }
            }
            else
            {
                HalfSorted = FFDH(WideItems, Sorted, BoardWidth, true);
                Sorted = new List<Level>(HalfSorted.ConvertAll(x => new Level(x.LevelNum,x.Contents)));
                #region Sort_NotNeededForBins
                //Level small = Sorted.Where(x => x.Contents.Max(y => y.xTo) <= (BoardWidth * (m + 1)) / (m + 2)).FirstOrDefault();
                //if (small != null)
                //{
                //    foreach (Level level in HalfSorted)
                //    {
                //        int currentLvlNum = level.LevelNum;
                //        Level test = Sorted.Where(x => x.LevelNum == currentLvlNum).FirstOrDefault();
                //        if (test.Contents.Max(x => x.xTo) > (BoardWidth * (m + 1)) / (m + 2))
                //        {
                //            test.LevelNum = -2;
                //        }
                //        else
                //        {
                //            test.LevelNum = -1;
                //        }
                //    }
                //    Sorted = Sorted.OrderBy(x => x.LevelNum).ToList();
                //    for (int i = 0; i < Sorted.Count; i++)
                //    {
                //        Sorted[i].LevelNum = i + 1;
                //    }
                //}
                //Sorted=Sorted.OrderBy(x => x.LevelNum).ToList();
                #endregion
                List<Bin> primsortBin = FindBKP(Sorted, BoardHeight, BoardWidth);
                int Rwidth = BoardWidth / (m + 2);//static value, 1/3 of width
                foreach (Bin bin in primsortBin)
                {//arrange levels in bin based on algorithm (wide strips at bottom, rest at top)
                    List<int> levels = bin.Levels;
                    List<Level> binLevels = Sorted.Where(x => levels.Contains(x.LevelNum)).ToList();
                    List<int> widelevels = binLevels.Where(x => x.Contents.Max(y => y.xTo) > (BoardWidth * (m + 1)) / (m + 2)).Select(z => z.LevelNum).ToList();
                    List<int> newLevels = new List<int>();
                    if (widelevels.Count > 0) { newLevels.AddRange(widelevels); }//add wide items in the bottom (start of list)

                    //Add narrow piece adding here
                    List<int> narrowlevels = binLevels.Where(x => x.Contents.Max(y => y.xTo) <= (BoardWidth * (m + 1)) / (m + 2)).Select(z => z.LevelNum).ToList();
                    if (narrowlevels.Count > 0)
                    {
                        int startLvl = narrowlevels[0];
                        int Rheight = binLevels.Where(x => narrowlevels.Contains(x.LevelNum)).Sum(y => y.Contents[0].yTo);//calculate narrow level height sum
                        int startY = 0;
                        List<ItemAndPosition> R = new List<ItemAndPosition>();
                        int slipWidth = 0;
                        int slipHeight = 0;
                        Piece item = NarrowItems.Where(x => x.Length <= Rheight && x.Width <= Rwidth).OrderByDescending(x => x.Length).FirstOrDefault();
                        while (item != null)
                        {
                            slipHeight = item.Length;
                            slipWidth = Rwidth;
                            Piece slipitem = NarrowItems.Where(x => x.Length <= slipHeight && x.Width <= slipWidth).OrderByDescending(x => x.Length).FirstOrDefault();
                            while (slipitem != null)
                            {
                                R.Add(new ItemAndPosition(slipitem, startY, startY + slipitem.Length, BoardWidth - slipWidth, BoardWidth - slipWidth + slipitem.Width));
                                NarrowItems.Remove(slipitem);
                                slipWidth -= slipitem.Width;
                                slipitem = NarrowItems.Where(x => x.Length <= slipHeight && x.Width <= slipWidth).OrderByDescending(x => x.Length).FirstOrDefault();
                            }
                            startY += slipHeight;
                            Rheight -= slipHeight;
                            item = NarrowItems.Where(x => x.Length <= Rheight && x.Width <= Rwidth).OrderByDescending(x => x.Length).FirstOrDefault();
                        }
                        Sorted.Where(x => x.LevelNum == startLvl).First().Contents.AddRange(R);
                        newLevels.AddRange(narrowlevels);
                    }
                    bin.Levels = newLevels;
                }
                HalfSorted.Clear();
                HalfSorted = FFDH(NarrowItems, HalfSorted, BoardWidth, false);
                //Check, if more levels can be added to bins (FFDH will have to change for SFm)
                foreach (Bin bin in primsortBin)
                {
                    int freeHeight =BoardHeight- Sorted.Where(x => bin.Levels.Contains(x.LevelNum)).Sum(y => y.Contents[0].yTo);
                    Level addable = HalfSorted.Where(x => x.Contents[0].yTo <= freeHeight).FirstOrDefault();
                    if(addable!=null)
                    {
                        List<Bin> add = FindBKP(HalfSorted, freeHeight, BoardWidth);
                        if (add != null)
                        {
                            List<int> temp = new List<int>(bin.Levels);
                            foreach(int lvl in add[0].Levels)
                            {
                                int maxlvl=Sorted.Max(x => x.LevelNum)+1;
                                Sorted.Add(new Level(maxlvl, HalfSorted.Where(x => x.LevelNum == lvl).FirstOrDefault().Contents));
                                temp.Add(maxlvl);
                            }
                            HalfSorted.RemoveAll(x => add[0].Levels.Contains(x.LevelNum));
                            bin.Levels = temp;
                        }
                    }
                }
                //remove all piecies used in previous bins from Piece list
                foreach (Bin bin in primsortBin)
                {
                    List<int> removeNums = ListToRemove(Sorted, bin, 0);
                    Pieces.RemoveAll(x => removeNums.Contains(x.Number));
                }
                List<Bin> narrowsBin = FindBKP(HalfSorted, BoardHeight, BoardWidth);
                List<Bin> UnionBin = new List<Bin>(primsortBin.ConvertAll(x => new Bin(x.BinNum, x.Levels)));
                List<Level> UnionLvl = new List<Level>(Sorted.ConvertAll(x => new Level(x.LevelNum, x.Contents)));
                foreach (Bin bin in narrowsBin)
                {
                    List<int> newlvl = new List<int>();
                    List<Level> temp = HalfSorted.Where(x => bin.Levels.Contains(x.LevelNum)).ToList();
                    foreach (Level lvl in temp)
                    {
                        UnionLvl.Add(new Level(UnionLvl.Count + 1, lvl.Contents));
                        newlvl.Add(UnionLvl.Count);
                    }
                    if (newlvl.Count > 0)
                    {
                        UnionBin.Add(new Bin(UnionBin.Count + 1, newlvl));
                    }
                }
                //WriteStatistics(resultFileName, UnionBin, UnionLvl, BoardHeight, BoardWidth, "SF");
                WriteResultListToFile("SFresults_" + BoardHeight + "x" + BoardWidth + "(HxW)" + setnum + ".txt", "", UnionLvl, UnionBin);
                //WriteResultListToFile(resultFileName, "", UnionLvl, UnionBin);
                //try resorting the rest
                int binNum = 1;
                bool cycle = true;
                while (cycle)
                {
                    if (narrowsBin.Count > 0 && narrowsBin.Count >= binNum)
                    {
                        Level newSlip = FFDHSingle(Pieces, BoardWidth, BoardHeight, narrowsBin[binNum - 1], HalfSorted);
                        if (newSlip == null)
                        {
                            //better slip was not found. bin not changed. go to next bin.
                            List<ItemAndPosition> temp = new List<ItemAndPosition>();
                            List<int> binLevels = new List<int>();
                            foreach (int num in narrowsBin[binNum - 1].Levels)
                            {
                                temp = new List<ItemAndPosition>(HalfSorted.Where(x => x.LevelNum == num).FirstOrDefault().Contents.ConvertAll(y => new ItemAndPosition(y.Item, y.yFrom, y.yTo, y.xFrom, y.xTo)));
                                Sorted.Add(new Level(Sorted.Count + 1, temp));
                                binLevels.Add(Sorted.Count);
                            }
                            if (binLevels.Count > 0)
                            {
                                primsortBin.Add(new Bin(primsortBin.Count + 1, binLevels));
                                List<int> numToRemove = ListToRemove(Sorted, primsortBin.Where(x => x.BinNum == (primsortBin.Count)).FirstOrDefault(), 0);
                                Pieces.RemoveAll(x => numToRemove.Contains(x.Number));
                            }
                            binNum++;
                        }
                        else
                        {
                            //better slip for current bin was found. Change it and resort other Bins.
                            int lvlHeight = HalfSorted.Where(x => narrowsBin[binNum - 1].Levels.Contains(x.LevelNum)).Min(x => x.Contents[0].yTo);
                            int slipNum = HalfSorted.Where(x => x.Contents[0].yTo == lvlHeight).FirstOrDefault().LevelNum;
                            List<int> binLevels = new List<int>();
                            foreach (int num in narrowsBin[binNum - 1].Levels)
                            {
                                List<ItemAndPosition> temp = new List<ItemAndPosition>();
                                if (num == slipNum)
                                {
                                    temp = new List<ItemAndPosition>(newSlip.Contents.ConvertAll(y => new ItemAndPosition(y.Item, y.yFrom, y.yTo, y.xFrom, y.xTo)));
                                }
                                else
                                {
                                    //deep copy of level.contents, where levelnum==num
                                    temp = new List<ItemAndPosition>(HalfSorted.Where(x => x.LevelNum == num).FirstOrDefault().Contents.ConvertAll(y => new ItemAndPosition(y.Item, y.yFrom, y.yTo, y.xFrom, y.xTo)));
                                }
                                Sorted.Add(new Level(Sorted.Count + 1, temp));
                                binLevels.Add(Sorted.Count);
                            }
                            if (binLevels.Count > 0)
                            {
                                primsortBin.Add(new Bin(primsortBin.Count + 1, binLevels));
                                List<int> numToRemove = ListToRemove(Sorted, primsortBin.Where(x => x.BinNum == (primsortBin.Count)).FirstOrDefault(), 0);
                                Pieces.RemoveAll(x => numToRemove.Contains(x.Number));
                                HalfSorted.Clear();
                                HalfSorted = FFDH(Pieces, HalfSorted, BoardWidth, false);
                                narrowsBin = FindBKP(HalfSorted, BoardHeight, BoardWidth);
                                binNum = 1;
                            }
                        }
                    }
                    else { cycle = false; }
                }
                //WriteStatistics(resultFileName, primsortBin, Sorted, BoardHeight, BoardWidth, "SFMod");
                //List<Bin> narrowsBin = FindBKP(HalfSorted, BoardHeight, BoardWidth);
                //foreach (Bin bin in narrowsBin)
                //{
                //    List<int> temp = new List<int>();
                //    foreach(int lvl in bin.Levels)
                //    {
                //        int maxlvl = Sorted.Max(x => x.LevelNum) + 1;
                //        Sorted.Add(new Level(maxlvl, HalfSorted.Where(x => x.LevelNum == lvl).FirstOrDefault().Contents));
                //        temp.Add(maxlvl);
                //    }
                //    primsortBin.Add(new Bin(primsortBin.Count + 1, temp));
                //}
               // WriteStatistics(resultFileName, primsortBin, Sorted, BoardHeight, BoardWidth, "SF");
                //WriteResultListToFile(resultFileName, "Results", Sorted, primsortBin);
                WriteResultListToFile("SFresults_" + BoardHeight + "x" + BoardWidth + "(HxW)" + setnum + ".txt", "", Sorted, primsortBin);
                //WriteResultListToFile("FFDH.txt", "Results", Sorted,null);
            }
            return Sorted;
        }
        static List<int> ListToRemove(List<Level> LevelInfo, Bin bin, int exclude)//returns list of Pieces (their nums), which are used in bin. Aka can't be used again
        {
            List<int> nums = new List<int>();
            foreach (int level in bin.Levels)
            {
                if (level != exclude)
                {
                    List<ItemAndPosition> levelPieces = LevelInfo.Where(x => x.LevelNum == level).FirstOrDefault().Contents.ToList();
                    foreach (ItemAndPosition itemInfo in levelPieces)
                    {
                        nums.Add(itemInfo.Item.Number);
                    }
                }
            }
            return nums;
        }
        static Level SF(int BoardWidth, int BoradHeight, Bin bin, List<Level> LevelInfo, List<Piece> OrigPieces)
        {
            List<Piece> Pieces = new List<Piece>(OrigPieces.ConvertAll(x => new Piece(x.Number, x.Length, x.Width)));
            int MinHeight = BoradHeight;
            int SmallestLvl = 0;
            int TotalHeight = 0;
            foreach (int level in bin.Levels)
            {
                int lvlHeight = LevelInfo.Where(x => x.LevelNum == level).FirstOrDefault().Contents.Max(y => y.yTo);//get lvl height
                if (lvlHeight <= MinHeight)
                {
                    MinHeight = lvlHeight;
                    SmallestLvl = level;
                }
                TotalHeight += lvlHeight;
            }
            if (SmallestLvl > 0)
            {
                List<int> nums = new List<int>();
                int ResortHeight = BoradHeight - (TotalHeight - MinHeight);
                if (ResortHeight < OrigPieces.Max(x => x.Length))
                {
                    nums = ListToRemove(LevelInfo, bin, SmallestLvl);
                    if (Pieces.Count > 0)
                    {
                        Pieces = Pieces.Where(x => x.Length <= ResortHeight).ToList();
                        Pieces.RemoveAll(x => nums.Contains(x.Number));
                        Level resortedSlip = null;
                        //resortedSlip=FFDH(Pieces,)
                        int oldStripFilled = LevelInfo.Where(z => z.LevelNum == SmallestLvl).Sum(y => y.Contents.Sum(x => (x.xTo - x.xFrom) * (x.yTo - x.yFrom)));
                        int newStripFilled = resortedSlip.Contents.Sum(x => (x.xTo - x.xFrom) * (x.yTo - x.yFrom));
                        if (newStripFilled > oldStripFilled)
                        {
                            return resortedSlip;
                        }
                    }
                }
            }
            return null;
        }
        static Level SAS_SASm(int BoardWidth, int BoradHeight, Bin bin,List<Level> LevelInfo,List<Piece> OrigPieces,int SAS_or_SASm)
        {
            List<Piece> Pieces = new List<Piece>(OrigPieces.ConvertAll(x => new Piece(x.Number,x.Length,x.Width)));
            int MinHeight = BoradHeight;
            int SmallestLvl = 0;
            int TotalHeight = 0;
            foreach(int level in bin.Levels)
            {
                int lvlHeight = LevelInfo.Where(x => x.LevelNum == level).FirstOrDefault().Contents.Max(y => y.yTo);//get lvl height
                if (lvlHeight<=MinHeight)
                {
                    MinHeight = lvlHeight;
                    SmallestLvl = level;
                }
                TotalHeight += lvlHeight;
            }
            if(SmallestLvl>0)
            {
                List<int> nums = new List<int>();
                int ResortHeight = BoradHeight - (TotalHeight - MinHeight);
                if (ResortHeight < OrigPieces.Max(x=>x.Length))
                {
                    nums = ListToRemove(LevelInfo, bin, SmallestLvl);
                    if (Pieces.Count > 0)
                    {
                        Pieces = Pieces.Where(x => x.Length <= ResortHeight).ToList();
                        Pieces.RemoveAll(x => nums.Contains(x.Number));
                        Level resortedSlip=null;
                        if (SAS_or_SASm == 1) { resortedSlip = SASm(BoardWidth, Pieces, false).FirstOrDefault(); }
                        else if(SAS_or_SASm == 2) { resortedSlip = SAS(BoardWidth, Pieces, false).FirstOrDefault(); }
                        int oldStripFilled = LevelInfo.Where(z => z.LevelNum == SmallestLvl).Sum(y => y.Contents.Sum(x => (x.xTo - x.xFrom) * (x.yTo - x.yFrom)));
                        int newStripFilled = resortedSlip.Contents.Sum(x => (x.xTo - x.xFrom) * (x.yTo - x.yFrom));
                        if (newStripFilled > oldStripFilled)
                        {
                            return resortedSlip;
                        }
                    }
                }
            }
            return null; 
        }
        static List<Level> SF(int BoardWidth,string FileName)
        {
            List<Piece> Pieces = ReadFromFile(FileName);
            //select widest item from all items
            Piece widest = Pieces.OrderByDescending(x => x.Width).FirstOrDefault();
            double mDouble = BoardWidth;
            mDouble=mDouble/ widest.Width;
            int m = Convert.ToInt32(Math.Round(mDouble));
            List<Level> HalfSorted = new List<Level>();
            List<Level> Sorted = new List<Level>();//Filled space
            List<Piece> WideItems=Pieces.Where(x => x.Width>(BoardWidth/(m+1))).ToList();
            List<Piece> NarrowItems = Pieces.Where(x => x.Width<=(BoardWidth/(m+1))).ToList();
            if(WideItems.Count==0 || NarrowItems.Count==0)
            {
                if (WideItems.Count == 0&&NarrowItems.Count>0) { Sorted=FFDH(NarrowItems,Sorted,BoardWidth,true); }
                if (NarrowItems.Count == 0 && WideItems.Count > 0) { Sorted = FFDH(WideItems, Sorted, BoardWidth, true); }
            }
            else
            {
                HalfSorted = FFDH(WideItems, Sorted, BoardWidth, true);
                Sorted = new List<Level>(HalfSorted.ConvertAll(x => new Level(x.LevelNum,x.Contents)));
                Level small = Sorted.Where(x => x.Contents.Max(y => y.xTo) <= (BoardWidth * (m + 1)) / (m + 2)).FirstOrDefault();
                if (small != null)
                {
                    foreach (Level level in HalfSorted)
                    {
                        int currentLvlNum = level.LevelNum;
                        Level test = Sorted.Where(x => x.LevelNum == currentLvlNum).FirstOrDefault();
                        if (test.Contents.Max(x => x.xTo) > (BoardWidth * (m + 1)) / (m + 2))
                        {
                            test.LevelNum = -2;
                        }
                        else
                        {
                            test.LevelNum = -1;
                        }
                    }
                    Sorted = Sorted.OrderBy(x => x.LevelNum).ToList();
                    for (int i = 0; i < Sorted.Count; i++)
                    {
                        Sorted[i].LevelNum = i + 1;
                    }
                }
                Sorted=Sorted.OrderBy(x => x.LevelNum).ToList();
                HalfSorted.Clear();
                int Rwidth=0;
                int Rheight=0;
                int startLvl=0;
                small = Sorted.Where(x => x.Contents.Max(y => y.xTo) <= (BoardWidth * (m + 1)) / (m + 2)).OrderBy(z => z.LevelNum).FirstOrDefault();//first level with <2/3 of width
                if(small!=null)
                {
                    Rwidth = BoardWidth / (m + 2);//static, 1/3 of width
                    startLvl = small.LevelNum;//starting level num
                    Rheight = Sorted.Where(x => x.LevelNum >= startLvl).Sum(x => x.Contents[0].yTo);//all levels >= from start lvl are 2/3 or less of width. Need to sum first element of every lvl ('cuz DH)
                }
                if (startLvl > 0)
                {
                    //could not use method
                    //FFDH for Narrow items, which would go in area R
                    int startY = 0;
                    List<ItemAndPosition> R = new List<ItemAndPosition>();
                    int slipWidth = 0;
                    int slipHeight = 0;
                    Piece item = NarrowItems.Where(x => x.Length <= Rheight && x.Width <= Rwidth).OrderByDescending(x => x.Length).FirstOrDefault();
                    while (item != null)
                    {
                        slipHeight = item.Length;
                        slipWidth = Rwidth;
                        Piece slipitem = NarrowItems.Where(x => x.Length <= slipHeight && x.Width <= slipWidth).OrderByDescending(x => x.Length).FirstOrDefault();
                        while(slipitem!=null)
                        {
                            R.Add(new ItemAndPosition(slipitem, startY, startY + slipitem.Length, BoardWidth - slipWidth, BoardWidth - slipWidth + slipitem.Width));
                            NarrowItems.Remove(slipitem);
                            slipWidth -= slipitem.Width;
                            slipitem = NarrowItems.Where(x => x.Length <= slipHeight && x.Width <= slipWidth).OrderByDescending(x => x.Length).FirstOrDefault();
                        }
                        startY += slipHeight;
                        Rheight -= slipHeight;
                        item = NarrowItems.Where(x => x.Length <= Rheight && x.Width <= Rwidth).OrderByDescending(x => x.Length).FirstOrDefault();
                    }
                    Sorted.Where(x => x.LevelNum == startLvl).First().Contents.AddRange(R);
                }
                Sorted = FFDH(NarrowItems, Sorted, BoardWidth, false);
                //WriteResultListToFile("SFresults_" + BoradHeight + "x" + BoardWidth + "(HxW)" + counter + ".txt", "", Sorted, null);
            }
            return Sorted;
        }
        static Level FFDHSingle(List<Piece> items,int BoardWidth,int BoardHeight, Bin bin, List<Level> LevelInfo)
        {
            List<Piece> Pieces = new List<Piece>(items.ConvertAll(x => new Piece(x.Number, x.Length, x.Width)));
            int MinHeight = BoardHeight;
            int SmallestLvl = 0;
            int TotalHeight = 0;
            foreach (int level in bin.Levels)
            {
                int lvlHeight = LevelInfo.Where(x => x.LevelNum == level).FirstOrDefault().Contents.Max(y => y.yTo);//get lvl height
                if (lvlHeight <= MinHeight)
                {
                    MinHeight = lvlHeight;
                    SmallestLvl = level;
                }
                TotalHeight += lvlHeight;
            }
            if (SmallestLvl > 0)
            {
                List<int> nums = new List<int>();
                int ResortHeight = BoardHeight - (TotalHeight - MinHeight);
                if (ResortHeight < items.Max(x => x.Length))
                {
                    nums = ListToRemove(LevelInfo, bin, SmallestLvl);
                    if (Pieces.Count > 0)
                    {
                        Pieces = Pieces.Where(x => x.Length <= ResortHeight).ToList();
                        Pieces.RemoveAll(x => nums.Contains(x.Number));
                        Level resortedSlip = null;
                        int SpaceLeft = BoardWidth;
                        int xFrom = 0;
                        List<ItemAndPosition> levelItems = new List<ItemAndPosition>();
                        Piece item = Pieces.Where(y => y.Width <= SpaceLeft && y.Length <= ResortHeight).OrderByDescending(x => x.Length).FirstOrDefault();
                        while (item != null)
                        {
                            levelItems.Add(new ItemAndPosition(item, 0, item.Length, xFrom, xFrom + item.Width));
                            SpaceLeft -= item.Width;
                            xFrom += item.Width;
                            Pieces.Remove(item);
                            item = Pieces.Where(y => y.Width <= SpaceLeft && y.Length <= ResortHeight).OrderByDescending(x => x.Length).FirstOrDefault();
                        }
                        if (levelItems.Count > 0)
                        {
                            resortedSlip = new Level(0, levelItems);
                        }
                        int oldStripFilled = LevelInfo.Where(z => z.LevelNum == SmallestLvl).Sum(y => y.Contents.Sum(x => (x.xTo - x.xFrom) * (x.yTo - x.yFrom)));
                        int newStripFilled = 0;
                        if(resortedSlip!=null)
                        {
                            newStripFilled = resortedSlip.Contents.Sum(x => (x.xTo - x.xFrom) * (x.yTo - x.yFrom));
                        }
                        if (newStripFilled > oldStripFilled)
                        {
                            return resortedSlip;
                        }
                    }
                }
            }
            return null; 
        }
        static List<Level> FFDH(List<Piece> items,List<Level> Sort, int BoardWidth,bool begin)
        {
            List<Level> Sorted=new List<Level>(Sort.ConvertAll(x=>new Level(x.LevelNum,x.Contents)));
            items = items.OrderByDescending(x => x.Length).ToList();
            List<ItemAndPosition> levelItems = new List<ItemAndPosition>();
            int start = 0;
            if (!begin)//!begin means that narrows can't be added to wide 
            { start = Sorted.Count; }
            while(items.Count>0)
            {
                Piece item = items.OrderByDescending(x => x.Length).FirstOrDefault();
                if (item != null)
                {
                    bool sorted = false;
                    for (int i = start; i < Sorted.Count; i++)
                    {
                        List<ItemAndPosition> workingItemList = Sorted.Where(x => x.LevelNum == i+1).Select(x => x.Contents).FirstOrDefault();
                        int sortedWidth = workingItemList.Max(x => x.xTo);
                        int baseHeight = workingItemList.Min(x => x.yFrom);
                        int maxWidth = BoardWidth - sortedWidth;
                        if (item.Width <= maxWidth)
                        {
                            Sorted[i].Contents.Add(new ItemAndPosition(item, baseHeight, baseHeight + item.Length, sortedWidth, sortedWidth + item.Width));
                            sorted = true;
                            items.Remove(item);
                            break;
                        }
                    }
                    if (sorted == false)
                    {
                        int yFrom = 0;
                        if(Sorted.Count>0)
                        {
                            List<ItemAndPosition> temp = Sorted.Where(x => x.LevelNum == Sorted.Count).Select(x => x.Contents).FirstOrDefault();
                            yFrom = temp.Max(x => x.yTo);
                        }
                        levelItems = new List<ItemAndPosition>();
                        levelItems.Add(new ItemAndPosition(item, 0, item.Length, 0, item.Width));
                        Sorted.Add(new Level(Sorted.Count + 1, levelItems));
                        items.Remove(item);
                    }
                }
            }
            return Sorted;
        }
        static List<Level> SASm(int BoardWidth,List<Piece> Pieces, bool SortAll)
        {
            List<Piece> NarrowItems = Pieces.Where(x => x.Length > x.Width).OrderByDescending(x => x.Length).ThenByDescending(x=>x.Width).ToList();//Items where length(height) > width //Sort DHDW (desc Height(length), desc Width)
            List<Piece> WideItems = Pieces.Where(x => x.Width >= x.Length).OrderByDescending(x => x.Width).ThenByDescending(x => x.Length).ToList();//Items where width >= length(height) //Sort DWDH
            int Level = 1;//Flag for new lvl
            char LastItemType = ' ';
            List<Level> Sorted = new List<Level>();//Filled space
            while (NarrowItems.Count > 0 || WideItems.Count > 0)//Pack items until no items left
            {
                Piece tallestNarrow = NarrowItems.FirstOrDefault();
                Piece tallestWide = WideItems.OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();//need tallest wide item
                Piece identicalHeightPiece;
                int lastWidth;
                List<ItemAndPosition> first = new List<ItemAndPosition>();
                if (WideItems.Count == 0)//no more wide items
                {
                    first.Add(new ItemAndPosition(tallestNarrow, 0, tallestNarrow.Length, 0, tallestNarrow.Width));
                    NarrowItems.Remove(tallestNarrow);
                    lastWidth = tallestNarrow.Width;
                    identicalHeightPiece = NarrowItems.Where(x => x.Length == tallestNarrow.Length).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                    while (identicalHeightPiece != null && lastWidth + identicalHeightPiece.Width <= BoardWidth)
                    {
                        first.Add(new ItemAndPosition(identicalHeightPiece, 0, identicalHeightPiece.Length, lastWidth, lastWidth+identicalHeightPiece.Width));
                        NarrowItems.Remove(identicalHeightPiece);
                        lastWidth += identicalHeightPiece.Width;
                        identicalHeightPiece = NarrowItems.Where(x => x.Length == tallestNarrow.Length).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                    }
                    LastItemType = 'N';
                }
                else if (NarrowItems.Count == 0)
                {
                    first.Add(new ItemAndPosition(tallestWide, 0, tallestWide.Length, 0, tallestWide.Width));
                    WideItems.Remove(tallestWide);
                    lastWidth = tallestWide.Width;
                    identicalHeightPiece = WideItems.Where(x => x.Length == tallestWide.Length).OrderByDescending(x => x.Width).ThenByDescending(x => x.Length).FirstOrDefault();
                    while (identicalHeightPiece != null && lastWidth + identicalHeightPiece.Width <= BoardWidth)
                    {
                        first.Add(new ItemAndPosition(identicalHeightPiece, 0, identicalHeightPiece.Length, lastWidth, lastWidth + identicalHeightPiece.Width));
                        WideItems.Remove(identicalHeightPiece);
                        lastWidth += identicalHeightPiece.Width;
                        identicalHeightPiece = WideItems.Where(x => x.Length == tallestWide.Length).OrderByDescending(x => x.Width).ThenByDescending(x => x.Length).FirstOrDefault();
                    }
                    LastItemType = 'W';
                }
                else
                {
                    if (tallestNarrow.Length >= tallestWide.Length)
                    {
                        first.Add(new ItemAndPosition(tallestNarrow, 0, tallestNarrow.Length, 0, tallestNarrow.Width));
                        NarrowItems.Remove(tallestNarrow);
                        lastWidth = tallestNarrow.Width;
                        identicalHeightPiece = NarrowItems.Where(x => x.Length == tallestNarrow.Length).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                        while (identicalHeightPiece != null && lastWidth + identicalHeightPiece.Width <= BoardWidth)
                        {
                            first.Add(new ItemAndPosition(identicalHeightPiece, 0, identicalHeightPiece.Length, lastWidth, lastWidth + identicalHeightPiece.Width));
                            NarrowItems.Remove(identicalHeightPiece);
                            lastWidth += identicalHeightPiece.Width;
                            identicalHeightPiece = NarrowItems.Where(x => x.Length == tallestNarrow.Length).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                        }
                        LastItemType = 'N';
                    }
                    else
                    {
                        first.Add(new ItemAndPosition(tallestWide, 0, tallestWide.Length, 0, tallestWide.Width));
                        WideItems.Remove(tallestWide);
                        lastWidth = tallestWide.Width;
                        identicalHeightPiece = WideItems.Where(x => x.Length == tallestWide.Length).OrderByDescending(x => x.Width).ThenByDescending(x => x.Length).FirstOrDefault();
                        while (identicalHeightPiece != null && lastWidth + identicalHeightPiece.Width <= BoardWidth)
                        {
                            first.Add(new ItemAndPosition(identicalHeightPiece, 0, identicalHeightPiece.Length, lastWidth, lastWidth + identicalHeightPiece.Width));
                            WideItems.Remove(identicalHeightPiece);
                            lastWidth += identicalHeightPiece.Width;
                            identicalHeightPiece = WideItems.Where(x => x.Length == tallestWide.Length).OrderByDescending(x => x.Width).ThenByDescending(x => x.Length).FirstOrDefault();
                        }
                        LastItemType = 'W';
                    }
                }
                bool cycle = false;
                int H = first.Max(x => x.yTo);//global free H
                int W = BoardWidth - first.Max(x => x.xTo);//global free W

                var tmp1 = NarrowItems.Where(x => (x.Length <= H) && (x.Width <= W)).FirstOrDefault();
                if (tmp1 == null)
                {
                    tmp1 = WideItems.Where(x => (x.Length <= H) && (x.Width <= W)).FirstOrDefault();
                }
                if (tmp1 != null) { cycle = true; }

                while (cycle)
                {
                    H = first.Max(x => x.yTo);//global free H
                    W = BoardWidth - first.Max(x => x.xTo);//global free W
                    int h = H;//height of selected region
                    int w = W;//width of selected region
                    Piece Ni = NarrowItems.Where(x => (x.Length <= h) && (x.Width <= w)).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                    Piece Wi = WideItems.Where(x => (x.Length <= h) && (x.Width <= w)).OrderByDescending(x => x.Width).ThenByDescending(x => x.Length).FirstOrDefault();
                    if (Wi == null && Ni == null) { cycle = false; }
                    else
                    {
                        if ((LastItemType == 'W' || WideItems.Count < 1 || Wi == null) && Ni != null)
                        {
                            //Pack Narrow
                            int floor = 1;
                            while (Ni != null)
                            {
                                if (Ni.Length <= h)
                                {
                                    LastItemType = 'N';
                                    first.Add(new ItemAndPosition(Ni, H - h, H - h + Ni.Length, BoardWidth - W, BoardWidth - W + Ni.Width));
                                    NarrowItems.Remove(Ni);
                                    if (floor == 0)
                                    {
                                        int tempw = w - Ni.Width;
                                        int prevNiWidth = Ni.Width;
                                        int firstNiHeight = Ni.Length;
                                        Ni = NarrowItems.Where(x => (x.Length <= h) && (x.Width <= tempw)).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                                        while (Ni != null)
                                        {
                                            first.Add(new ItemAndPosition(Ni, H - h, H - h + Ni.Length, BoardWidth - W + prevNiWidth, BoardWidth - W + Ni.Width + prevNiWidth));
                                            NarrowItems.Remove(Ni);
                                            tempw -= Ni.Width;
                                            prevNiWidth += Ni.Width;
                                            Ni = NarrowItems.Where(x => (x.Length <= h) && (x.Width <= tempw)).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                                        }
                                        h -= firstNiHeight;
                                    }
                                    else
                                    {
                                        h -= Ni.Length;
                                        w = Ni.Width;
                                    }
                                    Ni = NarrowItems.Where(x => (x.Length <= h) && (x.Width <= w)).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                                    floor = 0;
                                }
                            }
                        }
                        else if ((LastItemType == 'N' || NarrowItems.Count < 1 || Ni == null) && Wi != null)
                        {
                            //Pack Wide
                            if (Wi != null)
                            {
                                if (Wi.Length <= h)
                                {
                                    first.Add(new ItemAndPosition(Wi, H - h, H - h + Wi.Length, BoardWidth - W, BoardWidth - W + Wi.Width));
                                    WideItems.Remove(Wi);
                                    LastItemType = 'W';
                                    while (WideItems.Count > 0 && h > 0)
                                    {
                                        h -= Wi.Length;
                                        w = Wi.Width;
                                        Wi = WideItems.Where(x => (x.Length <= h) && (x.Width <= w)).OrderByDescending(x => x.Width).ThenByDescending(x => x.Length).FirstOrDefault();
                                        if (Wi != null)
                                        {
                                            if (Wi.Width != w)
                                            {
                                                Ni = NarrowItems.Where(x => (x.Length <= h) && (x.Width <= w - Wi.Width)).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                                                int temph = h;
                                                while (Ni != null)
                                                {
                                                    first.Add(new ItemAndPosition(Ni, H - temph, H - temph + Ni.Length, BoardWidth - W + Wi.Width, BoardWidth - W + Wi.Width + Ni.Width));
                                                    NarrowItems.Remove(Ni);
                                                    int tempw = w - Wi.Width- Ni.Width;
                                                    int prevNiWidth = Ni.Width;
                                                    int firstNiHeight = Ni.Length;
                                                    Ni = NarrowItems.Where(x => (x.Length <= temph) && (x.Width <= tempw)).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                                                    while(Ni!=null)
                                                    {
                                                        first.Add(new ItemAndPosition(Ni, H - temph, H - temph + Ni.Length, BoardWidth - W + Wi.Width+prevNiWidth, BoardWidth - W + Wi.Width + Ni.Width+prevNiWidth));
                                                        NarrowItems.Remove(Ni);
                                                        tempw -= Ni.Width;
                                                        prevNiWidth += Ni.Width;
                                                        Ni = NarrowItems.Where(x => (x.Length <= temph) && (x.Width <= tempw)).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                                                    }
                                                    temph -= firstNiHeight;
                                                    Ni = NarrowItems.Where(x => (x.Length <= temph) && (x.Width <= w - Wi.Width)).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                                                }
                                            }
                                            first.Add(new ItemAndPosition(Wi, H - h, H - h + Wi.Length, BoardWidth - W, BoardWidth - W + Wi.Width));
                                            WideItems.Remove(Wi);
                                        }
                                        else 
                                        {
                                            if (h > 0 && w > 0)
                                            {
                                                Ni = NarrowItems.Where(x => (x.Length <= h) && (x.Width <= w)).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                                                int temph = h;
                                                if (Ni == null) { break; }
                                                while (Ni != null)
                                                {
                                                    first.Add(new ItemAndPosition(Ni, H - temph, H - temph + Ni.Length, BoardWidth - W, BoardWidth - W + Ni.Width));
                                                    NarrowItems.Remove(Ni);
                                                    int tempw = w - Ni.Width;
                                                    int prevNiWidth = Ni.Width;
                                                    int firstNiHeight = Ni.Length;
                                                    Ni = NarrowItems.Where(x => (x.Length <= temph) && (x.Width <= tempw)).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                                                    while (Ni != null)
                                                    {
                                                        first.Add(new ItemAndPosition(Ni, H - temph, H - temph + Ni.Length, BoardWidth - W  + prevNiWidth, BoardWidth - W + Ni.Width + prevNiWidth));
                                                        NarrowItems.Remove(Ni);
                                                        tempw -= Ni.Width;
                                                        prevNiWidth += Ni.Width;
                                                        Ni = NarrowItems.Where(x => (x.Length <= temph) && (x.Width <= tempw)).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                                                    }
                                                    temph -= firstNiHeight;
                                                    Ni = NarrowItems.Where(x => (x.Length <= temph) && (x.Width <= prevNiWidth)).OrderByDescending(x => x.Length).ThenByDescending(x => x.Width).FirstOrDefault();
                                                }
                                                break;
                                            }
                                            else { break;} 
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (first.Count > 0)
                {
                    Sorted.Add(new Level(Level, first));
                }
                Level += 1;
                if(!SortAll){ break; }//stop at first level, if method called to resort 1 level
            }
            return Sorted;
        }
        static List<Level> SAS(int BoardWidth, List<Piece> Pieces, bool SortAll)
        {
            List<Piece> NarrowItems = Pieces.Where(x => x.Length > x.Width).OrderByDescending(x => x.Length).ToList();//Items where length(height) > width //Sort DHDW (desc Height(length), desc Width)
            List<Piece> WideItems = Pieces.Where(x => x.Width >= x.Length).OrderByDescending(x => x.Width).ToList();//Items where width >= length(height) //Sort DWDH
            int Level = 1;//Flag for new lvl
            char LastItemType = ' ';
            List<Level> Sorted = new List<Level>();//Filled space
            while (NarrowItems.Count > 0 || WideItems.Count > 0)//Pack items until no items left
            {

                Piece tallestNarrow = NarrowItems.FirstOrDefault();
                Piece tallestWide = WideItems.FirstOrDefault();
                Piece identicalHeightPiece;
                int lastWidth;
                List<ItemAndPosition> first = new List<ItemAndPosition>();
                if (WideItems.Count == 0)//no more wide items
                {
                    first.Add(new ItemAndPosition(tallestNarrow, 0, tallestNarrow.Length, 0, tallestNarrow.Width));
                    NarrowItems.Remove(tallestNarrow);
                    lastWidth = tallestNarrow.Width;
                    identicalHeightPiece = NarrowItems.Where(x => x.Length == tallestNarrow.Length).OrderByDescending(x => x.Length).FirstOrDefault();
                    while (identicalHeightPiece != null && lastWidth + identicalHeightPiece.Width <= BoardWidth)
                    {
                        first.Add(new ItemAndPosition(identicalHeightPiece, 0, identicalHeightPiece.Length, lastWidth, lastWidth+identicalHeightPiece.Width));
                        NarrowItems.Remove(identicalHeightPiece);
                        lastWidth += identicalHeightPiece.Width;
                        identicalHeightPiece = NarrowItems.Where(x => x.Length == tallestNarrow.Length).OrderByDescending(x => x.Length).FirstOrDefault();
                    }
                    LastItemType = 'N';
                }
                else if (NarrowItems.Count == 0)
                {
                    first.Add(new ItemAndPosition(tallestWide, 0, tallestWide.Length, 0, tallestWide.Width));
                    WideItems.Remove(tallestWide);
                    lastWidth = tallestWide.Width;
                    identicalHeightPiece = WideItems.Where(x => x.Length == tallestWide.Length).OrderByDescending(x => x.Width).FirstOrDefault();
                    while (identicalHeightPiece != null && lastWidth + identicalHeightPiece.Width <= BoardWidth)
                    {
                        first.Add(new ItemAndPosition(identicalHeightPiece, 0, identicalHeightPiece.Length, lastWidth, lastWidth + identicalHeightPiece.Width));
                        WideItems.Remove(identicalHeightPiece);
                        lastWidth += identicalHeightPiece.Width;
                        identicalHeightPiece = WideItems.Where(x => x.Length == tallestWide.Length).OrderByDescending(x => x.Width).FirstOrDefault();
                    }
                    LastItemType = 'W';
                }
                else
                {
                    if (tallestNarrow.Length >= tallestWide.Length)
                    {
                        first.Add(new ItemAndPosition(tallestNarrow, 0, tallestNarrow.Length, 0, tallestNarrow.Width));
                        NarrowItems.Remove(tallestNarrow);
                        lastWidth = tallestNarrow.Width;
                        identicalHeightPiece = NarrowItems.Where(x => x.Length == tallestNarrow.Length).OrderByDescending(x => x.Length).FirstOrDefault();
                        while (identicalHeightPiece != null && lastWidth + identicalHeightPiece.Width <= BoardWidth)
                        {
                            first.Add(new ItemAndPosition(identicalHeightPiece, 0, identicalHeightPiece.Length, lastWidth, lastWidth + identicalHeightPiece.Width));
                            NarrowItems.Remove(identicalHeightPiece);
                            lastWidth += identicalHeightPiece.Width;
                            identicalHeightPiece = NarrowItems.Where(x => x.Length == tallestNarrow.Length).OrderByDescending(x => x.Length).FirstOrDefault();
                        }
                        LastItemType = 'N';
                    }
                    else
                    {
                        first.Add(new ItemAndPosition(tallestWide, 0, tallestWide.Length, 0, tallestWide.Width));
                        WideItems.Remove(tallestWide);
                        lastWidth = tallestWide.Width;
                        identicalHeightPiece = WideItems.Where(x => x.Length == tallestWide.Length).OrderByDescending(x => x.Width).FirstOrDefault();
                        while (identicalHeightPiece != null && lastWidth + identicalHeightPiece.Width <= BoardWidth)
                        {
                            first.Add(new ItemAndPosition(identicalHeightPiece, 0, identicalHeightPiece.Length, lastWidth, lastWidth + identicalHeightPiece.Width));
                            WideItems.Remove(identicalHeightPiece);
                            lastWidth += identicalHeightPiece.Width;
                            identicalHeightPiece = WideItems.Where(x => x.Length == tallestWide.Length).OrderByDescending(x => x.Width).FirstOrDefault();
                        }
                        LastItemType = 'W';
                    }
                }
                bool cycle = false;
                int H = first.Max(x => x.yTo);//global free H
                int W = BoardWidth - first.Max(x => x.xTo);//global free W

                var tmp1 = NarrowItems.Where(x => (x.Length <= H) && (x.Width <= W)).FirstOrDefault();
                if (tmp1 == null)
                {
                    tmp1 = WideItems.Where(x => (x.Length <= H) && (x.Width <= W)).FirstOrDefault();
                }
                if (tmp1 != null) { cycle = true; }

                while (cycle)
                {
                    H = first.Max(x => x.yTo);//global free H
                    W = BoardWidth - first.Max(x => x.xTo);//global free W
                    int h = H;//height of selected region
                    int w = W;//width of selected region
                    Piece Ni = NarrowItems.Where(x => (x.Length <= h) && (x.Width <= w)).OrderByDescending(x => x.Length).FirstOrDefault();
                    Piece Wi = WideItems.Where(x => (x.Length <= h) && (x.Width <= w)).OrderByDescending(x => x.Width).FirstOrDefault();
                    if (Wi == null && Ni == null) { cycle = false; }
                    else
                    {
                        if ((LastItemType == 'W' || WideItems.Count < 1 || Wi == null)&&Ni!=null)
                        {
                            //Pack Narrow
                            while (Ni != null)
                            {
                                if (Ni.Length <= h)
                                {
                                    LastItemType = 'N';
                                    first.Add(new ItemAndPosition(Ni, H - h, H - h + Ni.Length, BoardWidth - W, BoardWidth - W + Ni.Width));
                                    NarrowItems.Remove(Ni);
                                    h -= Ni.Length;
                                    w = Ni.Width;
                                    Ni = NarrowItems.Where(x => (x.Length <= h) && (x.Width <= w)).OrderByDescending(x => x.Length).FirstOrDefault();
                                }
                            }
                        }
                        else if ((LastItemType == 'N' || NarrowItems.Count < 1 || Ni == null)&&Wi!=null)
                        {
                            //Pack Wide
                            if (Wi != null)
                            {
                                if (Wi.Length <= h)
                                {
                                    first.Add(new ItemAndPosition(Wi, H - h, H - h + Wi.Length, BoardWidth - W, BoardWidth - W + Wi.Width));
                                    WideItems.Remove(Wi);
                                    LastItemType = 'W';
                                    while (WideItems.Count > 0 && h > 0)
                                    {
                                        h -= Wi.Length;
                                        w = Wi.Width;
                                        Wi = WideItems.Where(x => (x.Length <= h) && (x.Width <= w)).OrderByDescending(x => x.Width).FirstOrDefault();
                                        if (Wi != null)
                                        {
                                            if (Wi.Width != w)
                                            {
                                                Ni = NarrowItems.Where(x => (x.Length <= h) && (x.Width <= w-Wi.Width)).OrderByDescending(x => x.Length).FirstOrDefault();
                                                int temph = h;
                                                while (Ni != null)
                                                {
                                                    first.Add(new ItemAndPosition(Ni, H - temph, H - temph + Ni.Length, BoardWidth - W + Wi.Width, BoardWidth - W + Wi.Width + Ni.Width));
                                                    NarrowItems.Remove(Ni);
                                                    temph -= Ni.Length;
                                                    Ni = NarrowItems.Where(x => (x.Length <= temph) && (x.Width <= w - Wi.Width)).OrderByDescending(x => x.Length).FirstOrDefault();
                                                }
                                            }
                                            first.Add(new ItemAndPosition(Wi, H - h, H - h + Wi.Length, BoardWidth - W, BoardWidth - W + Wi.Width));
                                            WideItems.Remove(Wi);
                                        }
                                        else { break; }
                                    }
                                }
                            }
                        }
                    }
                }
                if (first.Count > 0)
                {
                    Sorted.Add(new Level(Level, first));
                }
                Level += 1;
                if (!SortAll) { break; }
            }
            return Sorted;
        }
        static List<Piece> ReadFromFile(string filename)
        {
            List<Piece> read = new List<Piece>();
            string line;
            int counter = 1;
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            while ((line = file.ReadLine()) != null)
            {
                string[] temp = null;
                if (line.Contains("\t"))
                {
                    temp = line.Split('\t');
                }
                else
                {
                    temp = line.Split(' ');
                }
                temp = temp.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                if (temp.Count() > 0)
                {
                    read.Add(new Piece(counter, Convert.ToInt32(temp[0]), Convert.ToInt32(temp[1])));
                    counter++;
                }
            }
            return read;
        }
        static void WriteStatistics(string FileName,List<Bin> bins, List<Level> levels, int BinHeight,int BinWidth, string AlgType)
        {
            if (FileName.Length > 0)
            {
                double TotalBinArea = BinHeight * BinWidth;
                //All bins without Last
                List<Bin> BinsWoLast = bins.Where(x => x.BinNum < bins.Count).ToList();
                List<int> lvlnums = new List<int>();
                foreach (Bin bin in BinsWoLast)
                {
                    lvlnums.AddRange(bin.Levels);
                }
                double FilledAreaWoLast = 0;
                List<Level> LevelsWoLast = levels.Where(x => lvlnums.Contains(x.LevelNum)).ToList();
                foreach (Level lvl in LevelsWoLast)
                {
                    FilledAreaWoLast += lvl.Contents.Sum(x => (x.xTo - x.xFrom) * (x.yTo - x.yFrom));
                }
                double WoLastPercent = FilledAreaWoLast/(TotalBinArea * (bins.Count - 1))*100;
                //Last Bin
                List<int> Lastlvlnums = bins.Where(x => x.BinNum == bins.Count).FirstOrDefault().Levels;
                double FilledLastArea = 0;
                List<Level> Last = levels.Where(x => Lastlvlnums.Contains(x.LevelNum)).ToList();
                foreach (Level lvl in Last)
                {
                    FilledLastArea += lvl.Contents.Sum(x => (x.xTo - x.xFrom) * (x.yTo - x.yFrom));
                }
                double LastPercent = FilledLastArea / TotalBinArea  * 100;
                if (File.Exists(FileName))
                {
                    
                    using (StreamWriter tw = File.AppendText(FileName))
                    {
                        tw.WriteLine(AlgType+"\t"+BinHeight+"\t"+BinWidth+"\t"+bins.Count+"\t"+WoLastPercent+"\t"+LastPercent);
                    }
                }
                else
                {
                    using (StreamWriter tw = new StreamWriter(FileName))
                    {
                        tw.WriteLine(AlgType + "\t" + BinHeight + "\t" + BinWidth + "\t" + bins.Count + "\t" + WoLastPercent + "\t" + LastPercent);
                    }
                }
            }
        }
        static void WriteHeader(string FileName,int SetNum)
        {
            if (FileName.Length > 0)
            {
                if (File.Exists(FileName))
                {
                    using (StreamWriter tw = File.AppendText(FileName))
                    {
                        tw.WriteLine();
                        tw.WriteLine("Set " + SetNum + "\tBinHeight\tBin Width\tTotal Bins\tBin %(wo Last)\tLast Bin %");
                    }
                }
                else
                {
                    using (StreamWriter tw = new StreamWriter(FileName))
                    {
                        tw.WriteLine("Set " + SetNum + "\tBinHeight\tBin Width\tTotal Bins\tBin %(wo Last)\tLast Bin %");
                    }
                }
            }
        }
        static void WritePiecesListToFile(string FileName, string explanation, List<Piece> pieces)
        {
            if (FileName.Length > 0)
            {
                if (File.Exists(FileName))
                {
                    using (StreamWriter tw = File.AppendText(FileName))
                    {
                        tw.WriteLine();
                        if (explanation.Length > 0)
                        {
                            tw.WriteLine(explanation);
                        }
                        tw.WriteLine("Length \t Width \t Num");
                        foreach (var piece in pieces)
                        {
                            tw.WriteLine(piece.Length + "\t" + piece.Width + "\t" + piece.Number);
                        }
                    }
                }
                else
                {
                    using (StreamWriter tw = new StreamWriter(FileName))
                    {
                        if (explanation.Length > 0)
                        {
                            tw.WriteLine(explanation);
                        }
                        tw.WriteLine("Length \t Width \t Num");
                        foreach (var piece in pieces)
                        {
                            tw.WriteLine(piece.Length + "\t" + piece.Width + "\t" + piece.Number);
                        }
                    }
                }
            }
        }
        static void WriteResultListToFile(string FileName, string explanation, List<Level> levels,List<Bin> bins)
        {
            if (FileName.Length > 0)
            {
                if (File.Exists(FileName))
                {
                    using (StreamWriter tw = File.AppendText(FileName))
                    {
                        tw.WriteLine();
                        if (explanation.Length > 0)
                        {
                            tw.WriteLine(explanation);
                        }
                        //tw.WriteLine("Level \t Item Number \t Start Coord \t End Coord");
                        tw.WriteLine("Level \t Item Number \t Coord x \t Coord y \t Width \t Height");
                        foreach (var level in levels)
                        {
                            foreach (var content in level.Contents)
                            {
                                //tw.WriteLine(level.LevelNum + "\t" + content.Item.Number + "\t (" + content.xFrom + "," + content.yFrom + ") \t (" + content.xTo + "," + content.yTo + ")");
                                tw.WriteLine(level.LevelNum + " \t " + content.Item.Number + " \t " + content.xFrom + "\t " + content.yTo + "\t " + content.Item.Width + "\t " + content.Item.Length);
                            }
                        }
                    }
                }
                else
                {
                    using (StreamWriter tw = new StreamWriter(FileName))
                    {
                        if (explanation.Length > 0)
                        {
                            tw.WriteLine(explanation);
                        }
                        //tw.WriteLine("Level \t Item Number \t Start Coord \t End Coord");
                        //tw.WriteLine("Level \t Item Number \t Coord x \t Coord y \t Width \t Height");
                        if (bins == null)
                        {
                            tw.WriteLine("Level \t Item Number \t Coord x \t Coord y \t Width \t Height");
                            foreach (var level in levels)
                            {
                                foreach (var content in level.Contents)
                                {
                                    //tw.WriteLine(level.LevelNum + "\t" + content.Item.Number + "\t (" + content.xFrom + "," + content.yFrom + ") \t (" + content.xTo + "," + content.yTo + ")");
                                    tw.WriteLine(level.LevelNum + " \t " + content.Item.Number + " \t " + content.xFrom + "\t " + content.yTo + "\t " + content.Item.Width + "\t " + content.Item.Length);
                                }
                            }
                        }
                        else
                        {
                            tw.WriteLine("Level \t Item Number \t Coord x \t Coord y \t Width \t Height\tBin");
                            foreach(Bin bin in bins)
                            {
                                for(int i=0;i<bin.Levels.Count;i++)
                                {
                                    Level level = levels.Where(x => x.LevelNum == bin.Levels[i]).FirstOrDefault();
                                    if (level != null)
                                    {
                                        foreach (var content in level.Contents)
                                        {
                                            //tw.WriteLine(level.LevelNum + "\t" + content.Item.Number + "\t (" + content.xFrom + "," + content.yFrom + ") \t (" + content.xTo + "," + content.yTo + ")");
                                            tw.WriteLine(level.LevelNum + " \t " + content.Item.Number + " \t " + content.xFrom + "\t " + content.yTo + "\t " + content.Item.Width + "\t " + content.Item.Length+"\t"+bin.BinNum);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #region Define Classes
        private class Piece
        {
            public int Number;
            public int Length;
            public int Width;
            public Piece(int number, int length, int width)
            {
                Number = number;
                Length = length;
                Width = width;
            }
        }
        private class genPatternResponse
        {
            public Level P;
            public List<Piece> I;
            public genPatternResponse(Level p,List<Piece> i)
            {
                P = p;
                I = i;
            }

        }
        private class Rect
        {
            public int Identifier;
            public int X;
            public int Y;
            public int Height;
            public int Width;
            public Rect(int identifier,int x, int y, int height, int width)
            {
                X = x;
                Y = y;
                Height = height;
                Width = width;
                Identifier = identifier;
            }
        }
        private class Level
        {
            public int LevelNum;
            public List<ItemAndPosition> Contents;
            public Level(int levelnum, List<ItemAndPosition> contents)
            {
                LevelNum = levelnum;
                Contents = contents;
            }
        }
        private class ItemAndPosition
        {
            public Piece Item;
            public int yFrom;
            public int yTo;
            public int xFrom;
            public int xTo;
            public ItemAndPosition(Piece item, int yfrom, int yto, int xfrom, int xto)
            {
                Item = item;
                xFrom = xfrom;
                xTo = xto;
                yFrom = yfrom;
                yTo = yto;
            }
        }
        static ItemCollection[] FindBKP(int capacity, List<Item> items)
        {
            ItemCollection[] ic = new ItemCollection[capacity + 1];
            for (int i = 0; i <= capacity; i++) ic[i] = new ItemCollection();
            for (int i = 0; i < items.Count; i++)
                for (int j = capacity; j >= 0; j--)
                    if (j >= items[i].Weight)
                    {
                        int quantity = Math.Min(items[i].Quantity, j / items[i].Weight);
                        for (int k = 1; k <= quantity; k++)
                        {
                            ItemCollection lighterCollection = ic[j - k * items[i].Weight];
                            int testValue = lighterCollection.TotalValue + k * items[i].Value;
                            if (testValue > ic[j].TotalValue) (ic[j] = lighterCollection.Copy()).AddItem(items[i], k);
                        }
                    }
            return ic;
        }
        private class Item
        {
            public string Description;
            public int Weight;
            public int Value;
            public int Quantity;
            public Item(string description, int weight, int value, int quantity)
            {
                Description = description;
                Weight = weight;
                Value = value;
                Quantity = quantity;
            }
        }
        private class Bin
        {
            public int BinNum;
            public List<int> Levels;
            public Bin(int binNumber,List<int> levels)
            {
                BinNum = binNumber;
                Levels = levels;
            }
        }
        private class ItemCollection
        {
            public Dictionary<string, int> Contents = new Dictionary<string, int>();
            public int TotalValue;
            public int TotalWeight;
            public void AddItem(Item item, int quantity)
            {
                if (Contents.ContainsKey(item.Description)) Contents[item.Description] += quantity; else Contents[item.Description] = quantity;
                TotalValue += quantity * item.Value;
                TotalWeight += quantity * item.Weight;
            }
            public ItemCollection Copy()
            {
                var ic = new ItemCollection();
                ic.Contents = new Dictionary<string, int>(this.Contents);
                ic.TotalValue = this.TotalValue;
                ic.TotalWeight = this.TotalWeight;
                return ic;
            }
        }
        #endregion
        static List<Bin> FindBKP(List<Level> Sorted,int BinHeight, int BinWidth)
        {
            List<Item> Items4BKP = new List<Item>();
            List<Bin> allBins = new List<Bin>();
            int start = 0;
            int end = 0;
            if(Sorted.Count>0)
            {
                start = Sorted.Min(x => x.LevelNum-1);
                end = Sorted.Max(x => x.LevelNum);
            }
            //int sheetArea = BinHeight * BinWidth;
            for(int i=start;i<end;i++)
            {
                var exist = Sorted.Where(x => x.LevelNum == i + 1).FirstOrDefault();
                if (exist != null)
                {
                    int stripHeight = Sorted.Where(x => x.LevelNum == i + 1).Max(x => x.Contents[0].yTo);
                    double stripArea = stripHeight * BinWidth;
                    double totalFilled = Sorted.Where(z => z.LevelNum == i + 1).Sum(y => y.Contents.Sum(x => (x.xTo - x.xFrom) * (x.yTo - x.yFrom)));
                    int coef = Convert.ToInt32(Math.Round((totalFilled / stripArea) * 100));
                    Items4BKP.Add(new Item(Convert.ToString(i + 1), stripHeight, coef, 1));
                }
            }
            int counter = 0;
            ItemCollection[] result = FindBKP(BinHeight, Items4BKP);//all combinations, even empty
            ItemCollection bestValue = result.Where(x => x.TotalValue > 0).OrderByDescending(c => c.TotalValue).FirstOrDefault();
            while(bestValue!=null)
            {
                counter++;
                List<int> lvlNums = new List<int>();
                foreach (KeyValuePair<string, int> entry in bestValue.Contents)
                {
                    lvlNums.Add(Convert.ToInt32(entry.Key));
                    Items4BKP.RemoveAll(x => x.Description == entry.Key);
                }
                allBins.Add(new Bin(counter, lvlNums));
                result = FindBKP(BinHeight, Items4BKP);//all combinations, even empty
                bestValue = result.Where(x => x.TotalValue > 0).OrderByDescending(c => c.TotalValue).FirstOrDefault();
            }
            return allBins;
        }
        static genPatternResponse genPattern(Level P, List<Piece> I, List<Rect> R, int BinHeight,int BinWidth)
        {
            genPatternResponse response = new genPatternResponse(P, I);
            if (R.Count > 0)
            {
                List<int> removeRs = new List<int>();
                foreach(Rect info in R)
                {
                    Piece fits = I.Where(x => x.Length <= info.Height && x.Width <= info.Width).FirstOrDefault();
                    if(fits==null)
                    {
                        removeRs.Add(info.Identifier);
                    }
                }
                R.RemoveAll(x => removeRs.Contains(x.Identifier));
                if (R.Count > 0)
                {
                    List<Piece> Ppieces = new List<Piece>();
                    if (P.Contents.Count>0)
                    {
                        foreach (ItemAndPosition info in P.Contents)
                        {
                            Ppieces.Add(new Piece(info.Item.Number, info.Item.Length, info.Item.Width));
                        }
                    }
                    R = R.OrderBy(x => x.X).ToList();
                    List<Piece> sequence = new List<Piece>();
                    int rectIdent = 0;
                    int rectIndex = 0;
                    for(int i=0;i<R.Count;i++)
                    {
                        List<Piece> seq = genSimplePattern(P, I, R[i].Height, R[i].Width);
                        List<Piece> p1 = new List<Piece>();
                        List<Piece> p2 = new List<Piece>();
                        List<Piece> PandI = new List<Piece>();
                        p1.AddRange(Ppieces);
                        p2.AddRange(Ppieces);
                        PandI.AddRange(Ppieces);
                        PandI.AddRange(I);
                        if (seq.Count > 0)
                        {
                            p1.AddRange(seq);
                        }
                        if(sequence.Count>0)
                        {
                            p2.AddRange(sequence);
                        }
                        if(sequence.Count<1||fitsBetter(p1,p2,PandI))
                        {
                            sequence = seq;
                            rectIdent = R[i].Identifier;
                            rectIndex = i;
                        }
                    }
                    if(sequence.Count>0)
                    {
                        sequence = sequence.OrderByDescending(x => x.Length).ToList();
                        int currentX = R.Where(x => x.Identifier == rectIdent).FirstOrDefault().X;
                        int staticY=R.Where(x => x.Identifier == rectIdent).FirstOrDefault().Y;
                        int rectWidth = R[rectIndex].Width;
                        List<Rect> Rsub = new List<Rect>();
                        foreach(Piece info in sequence)
                        {
                            ItemAndPosition addToPattern = new ItemAndPosition(new Piece(info.Number, info.Length, info.Width), staticY, staticY + info.Length, currentX, currentX + info.Width);
                            if(R[rectIndex].Height-info.Length>0)
                            {
                                Rsub.Add(new Rect(R.Count + Rsub.Count+1, currentX, staticY + info.Length, R[rectIndex].Height - info.Length, rectWidth));
                            }
                            rectWidth -= info.Width;
                            currentX += info.Width;
                            P.Contents.Add(addToPattern);
                            I.RemoveAll(x => x.Number == info.Number);
                        }
                        List<Rect> Rbelow = new List<Rect>();
                        List<Rect> Rleft = new List<Rect>();
                        if(R.Count>1)
                        {
                            for(int i=0;i<R.Count;i++)
                            {
                                
                                if(i<rectIndex)
                                {
                                    //width needs trim from right
                                    Rleft.Add(new Rect(R[i].Identifier, R[i].X, R[i].Y, R[i].Height, R[rectIndex].X-R[i].X));
                                }
                                else if(i>rectIndex)
                                {
                                    //height needs trim from top
                                    Rbelow.Add(new Rect(R[i].Identifier, R[i].X, R[i].Y, R[rectIndex].Y-R[i].Y, R[i].Width));
                                }
                            }
                        }
                        response = genPattern(P, I, Rsub, BinHeight,BinWidth);
                        P = response.P;
                        I = response.I;
                        if (Rbelow.Count > 0)
                        {
                            int from = Rbelow[0].Y + Rbelow[0].Height;
                            int top = P.Contents.Where(x => x.yFrom >= from).Max(x => x.yTo);
                            if (top < BinHeight)
                            {
                                int change = BinHeight - top;
                                int maxRight = P.Contents.Where(x => x.yFrom >= from).Max(y => y.xTo);
                                P.Contents.Where(x => x.yFrom >= from).Select(y => { y.yTo = y.yTo + change; return y; });
                                P.Contents.Where(x => x.yFrom >= from).Select(y => { y.yFrom = y.yFrom + change; return y; });
                                Rbelow.Select(x => { x.Height = x.Height + change; return x; });
                                for (int i = 0; i < Rbelow.Count; i++)
                                {
                                    if (Rbelow[i].X >= maxRight)
                                    {
                                        Rbelow[i].Height = BinHeight - Rbelow[i].Y;
                                    }
                                }
                            }
                            response = genPattern(P, I, Rbelow, BinHeight, BinWidth);
                            P = response.P;
                            I = response.I;
                        }
                        if (Rleft.Count > 0)
                        {
                            int from = Rleft[0].X + Rleft[0].Width;
                            int right = P.Contents.Where(x => x.xFrom >= from).Max(x => x.xTo);
                            if (right < BinWidth)
                            {
                                int change = BinWidth - right;
                                P.Contents.Where(x => x.xFrom >= from).Select(y => { y.xTo = y.xTo + change; return y; });
                                P.Contents.Where(x => x.xFrom >= from).Select(y => { y.xFrom = y.xFrom + change; return y; });
                                Rleft.Select(x => { x.Width = x.Width + change; return x; });
                            }
                            response = genPattern(P, I, Rleft, BinHeight, BinWidth);
                            P = response.P;
                            I = response.I;
                        }
                    }
                }
                else
                {
                    response.I = I;
                    response.P = P;
                }
            }
            else
            {
                response.I = I;
                response.P = P;
            }
            return response;
        }
        static List<Piece> genSimplePattern(Level P, List<Piece> Pieces, int areaHeight, int areaWidth)
        {
            List<Piece> fit = new List<Piece>();
            fit = Pieces.Where(x => x.Length <= areaHeight && x.Width <= areaWidth).ToList();
            List<Piece> Ppieces = new List<Piece>();
            if (P.Contents.Count>0)
            {
                foreach (ItemAndPosition info in P.Contents)
                {
                    Ppieces.Add(new Piece(info.Item.Number, info.Item.Length, info.Item.Width));
                }
            }
            List<Piece> sequence = new List<Piece>();//???
            double minHeight = fit.Min(x => x.Length);
            double maxHeight = fit.Max(x => x.Length);
            double minArea = fit.Min(x => (x.Length * x.Width));
            double maxArea = fit.Max(x => (x.Length * x.Width));
            double lambda = 0;
            double maxlambda = 1;
            while(lambda<=maxlambda)
            {
                List<Piece> sortedBy = new List<Piece>();
                //lambda*h'+(1-lambda)*a' , where h' normalised height, a' normalised area
                sortedBy=fit.OrderByDescending(x=>(lambda*((x.Length-minHeight)/(maxHeight-minHeight))+(1-lambda)*((x.Length*x.Width-minArea)/(maxArea-minArea)))).ToList();
                int Wleft = areaWidth;
                List<Piece> seq = new List<Piece>();
                foreach(Piece piece in sortedBy)
                {
                    if(piece.Width<=Wleft)
                    {
                        seq.Add(new Piece(piece.Number,piece.Length,piece.Width));
                        Wleft -= piece.Width;
                    }
                }
                //fitsBetter
                //P U S
                List<Piece> pattern1 = new List<Piece>();
                List<Piece> pattern2 = new List<Piece>();
                List<Piece> PplusI = new List<Piece>();
                pattern1.AddRange(Ppieces);
                pattern1.AddRange(seq);
                pattern2.AddRange(Ppieces);
                if(sequence.Count>0)
                {
                    pattern2.AddRange(sequence);
                }
                PplusI.AddRange(Ppieces);
                PplusI.AddRange(Pieces);
                if(fitsBetter(pattern1, pattern2, PplusI))
                {
                    sequence = new List<Piece>(seq);
                }
                lambda += 0.1;
                lambda = Math.Round(lambda, 2);
            }
            return sequence;
        }
        static bool fitsBetter(List<Piece> pattern1, List<Piece> pattern2, List<Piece> unpacked)
        {
            bool answer = false;
            double pat1Area = pattern1.Sum(x => x.Length * x.Width);
            double pat1avgArea = pat1Area / pattern1.Count;
            double pat2Area = 0;
            double pat2avgArea = 0;
            if(pattern2.Count>0)
            {
                pat2Area = pattern2.Sum(x => x.Length * x.Width);
                pat2avgArea = pat2Area / pattern2.Count;
            }
            double unpackedArea = unpacked.Sum(x => x.Length * x.Width);
            double unpackAvgArea = unpackedArea / unpacked.Count;
            if(pat1avgArea>=unpackAvgArea&&pat2avgArea>=unpackAvgArea)
            {
                if(pat1Area>pat2Area)
                {
                    answer = true;
                }
            }
            else if(pat1avgArea<unpackAvgArea&&pat2avgArea<unpackAvgArea)
            {
                if(pat1avgArea>pat2avgArea)
                {
                    answer = true;
                }
            }
            else
            {
                if (pat1avgArea>=unpackAvgArea)
                {
                    answer = true;
                }
            }
            return answer;
        }
        static void BinAlg(int BoardWidth, int BoardHeight, string FileName, int setnum)
        {
            List<Piece> I = ReadFromFile(FileName);
            
            List<Level> L = new List<Level>();
            List<Bin> Bins = new List<Bin>();
            int counter = 1;
            while (I.Count > 0)
            {
                List<ItemAndPosition> itemandpos = new List<ItemAndPosition>();
                Level P = new Level(counter, itemandpos);
                List<Rect> R = new List<Rect>();
                R.Add(new Rect(1, 0, 0, BoardHeight, BoardWidth));
                genPatternResponse response = genPattern(P, I, R, BoardHeight, BoardWidth);
                L.Add(response.P);
                List<int> temp = new List<int>();
                temp.Add(counter);
                Bins.Add(new Bin(counter, temp));
                counter++;
            }
            WriteResultListToFile("2DBPresults_" + BoardHeight + "x" + BoardWidth + "(HxW)" + setnum + ".txt", "", L, Bins);
            //WriteStatistics("results_" + BoardHeight + "x" + BoardWidth + "(HxW).txt", Bins, L, BoardHeight, BoardWidth, "2DBP");
        }
    }
}
